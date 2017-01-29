using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MFDSkillTracking.Common;
using MFDSkillTracking.Extensions;
using OxyPlot.Series;

namespace MFDSkillTracking.Models
{
    [CollectionDataContract(Name="RollObservableCollection"), KnownType(typeof(RollObservableCollection))]
    public class RollObservableCollection : ObservableCollection<Roll> { }

    [DataContract(IsReference = true)]
    public class UnknownSkill : Skill
    {
        #region Fields

        [DataMember] private readonly NinjaLevel _approximateSkill;
        [DataMember] private double[] _diceDistribution;
        [DataMember] private readonly RollObservableCollection _rolls = new RollObservableCollection();
        private bool _isBusy;
        private double _progress;
        private static readonly Random Rand = new Random();

        #endregion

        #region Properties

        [DataMember] public string Name { get; private set; }

        public RollObservableCollection Rolls => _rolls;

        public IEnumerable<ScatterPoint> SkillDistribution
        {
            get
            {
                var x = 1;
                return _diceDistribution.Select(y => new ScatterPoint(x++, y)).ToList();
            }
        }

        public double MeanSkill
        {
            get
            {
                var i = 1;
                return Math.Round(_diceDistribution.Select(x => x * i++).Sum(), 2);
            }
        }

        public List<Tuple<int, double>> TopValues
        {
            get
            {
                var i = 1;
                var sum = 0.0;
                return _diceDistribution.Select(d => Tuple.Create(i++, d))
                    .OrderByDescending(x => x.Item2)
                    .TakeWhile(x => (sum += x.Item2) <= 0.95)
                    .Select(x => Tuple.Create(x.Item1, x.Item2*100))
                    .ToList();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get { return Math.Round(_progress, 2); }
            private set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public UnknownSkill(string name, NinjaLevel approximateSkill)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Name cannot be null, empty, or whitespace", nameof(name));
            Name = name;
            _approximateSkill = approximateSkill;

            ConstructPriorDistribution();
        }

        #endregion

        #region Methods

        public double GetRatingProbability(int index) => _diceDistribution[index];

        private void ConstructPriorDistribution()
        {
            //Create a prior distribution based on the approximate skill of the character
            var distribution = new List<double>();
            for (var i = Constants.MIN_DICE_LEVEL; i <= Constants.MAX_DICE_LEVEL; i++)
            {
                var dist = Math.Abs(i - (int) _approximateSkill);
                var val = Math.Pow(Math.E, -dist/10.0);
                distribution.Add(val);
            }
            _diceDistribution = distribution.Normalize();
        }

        private static readonly object Lock = new object();
        public async void Recalculate() => await Task.Run(() =>
        {
            lock (Lock)
            {
                IsBusy = true;

                ConstructPriorDistribution();
                if (!Rolls.Any())
                {
                    OnPropertyChanged(nameof(SkillDistribution));
                    OnPropertyChanged(nameof(MeanSkill));
                    OnPropertyChanged(nameof(TopValues));
                    IsBusy = false;
                    return;
                }
                
                var newDistribution = _diceDistribution.ToArray();
                Rolls.ToList().ForEach(roll =>
                {
                    UpdateOnRoll(newDistribution, roll);
                });

                _diceDistribution = newDistribution.Normalize();

                OnPropertyChanged(nameof(SkillDistribution));
                OnPropertyChanged(nameof(MeanSkill));
                OnPropertyChanged(nameof(TopValues));
                IsBusy = false;
            }
        });

        public async void Update(Roll rollResult) => await Task.Run(() =>
        {
            lock (Lock)
            {
                IsBusy = true;

                var newDistribution = _diceDistribution.ToArray();
                UpdateOnRoll(newDistribution, rollResult);
                _diceDistribution = newDistribution.Normalize();

                OnPropertyChanged(nameof(SkillDistribution));
                OnPropertyChanged(nameof(MeanSkill));
                OnPropertyChanged(nameof(TopValues));

                IsBusy = false;
            }
        });

        private void UpdateOnRoll(double[] newDistribution, Roll roll)
        {
            Progress = 0.0;
            for (var index = 0; index < newDistribution.Length; index++)
            {
                var likelihood = 0.0;
                var diceHypothesis = index + 1 + roll.CharacterDiceBonus;
                for (var diceModifier = roll.OpponentDiceBonusMin; diceModifier <= roll.OpponentDiceBonusMax; diceModifier++)
                {
                    likelihood += Likelihood(diceHypothesis + diceModifier, roll.OpponentSkill, roll.Result);
                }
                newDistribution[index] *= likelihood/(roll.OpponentDiceBonusMax - roll.OpponentDiceBonusMin + 1.0);
            }
        }

        private double Likelihood(int diceHypothesis, object opponentSkill, double result)
        {
            var knownSkill = opponentSkill as KnownSkill;
            if (knownSkill == null) return Likelihood(diceHypothesis, (UnknownSkill) opponentSkill, result);

            Progress += 1;
            return Likelihood(diceHypothesis, knownSkill.Level, result, Constants.ComputationAccuracy * 20);
        }

        private double Likelihood(int diceHypothesis, UnknownSkill opponentSkill, double result)
        {
            var total = 0.0;
            var numIterations = opponentSkill.TopValues.Count;
            opponentSkill.TopValues.ForEach(skillValuePair =>
            {
                Progress += 1.0/numIterations;
                total += Likelihood(diceHypothesis, skillValuePair.Item1, result, Constants.ComputationAccuracy) * skillValuePair.Item2;
            });
            return total;
        }

        /// <summary>
        /// A monte carlo method that simulates rolling the given number of dice some number of times.
        /// The score assigned to the numDice hypothesis is proportional the inverse square of the error.
        /// This is obviously not an exact method, but it is a good approximation and is much faster than anything I could find.
        /// </summary>
        private static double Likelihood(int diceHypothesis, int opponentSkillLevel, double result, int iterations)
        {
            return Enumerable.Repeat(1, iterations).Select(x =>
            {
                var characterRoll = 0.0;
                for (var i = 0; i < diceHypothesis; i++)
                {
                    characterRoll += Rand.Next(1, 101);
                }
                var opponentRoll = 0.0;
                for (var i = 0; i < opponentSkillLevel; i++)
                {
                    opponentRoll += Rand.Next(1, 101);
                }
                return (characterRoll - opponentRoll)/Math.Pow((diceHypothesis + opponentSkillLevel), 0.65);
            }).Sum(x =>
            {
                var dist = Math.Abs(result - x);
                return dist < 0.25 ? 16 : 1/Math.Pow(dist,2);
            });
        }

        #endregion
    }
}
