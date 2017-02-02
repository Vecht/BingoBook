using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MFDSkillTracking.Common;
using MFDSkillTracking.Models;

namespace MFDSkillTracking.ViewModels
{
    public class VersusViewModel : PropertyChangedBase
    {
        #region Fields

        private Character _selectedCharacter;
        private string _selectedCharacterSkill;
        private int _characterBonusDice;

        private Character _selectedOpponent;
        private string _selectedOpponentSkill;
        private int _opponentBonusDice;

        private bool _isBusy;
        private double _progress;

        #endregion

        #region Properties

        public ObservableCollection<Character> Characters { get; }

        public Character SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                _selectedCharacter = value;
                OnPropertyChanged();

                SelectedCharacterSkill = null;
                OnPropertyChanged(nameof(PossibleCharacterSkills));

                SelectedOpponent = null;
                OnPropertyChanged(nameof(PossibleOpponents));

                _computeCommand.Update();
            }
        }

        public IEnumerable<string> PossibleCharacterSkills
        {
            get
            {
                if (SelectedCharacter == null) return new string[0];
                var characterKnownSkills = SelectedCharacter.KnownSkills.Select(x => x.Name);
                var characterUnknownSkills = SelectedCharacter.UnknownSkills.Select(x => x.Name);
                return characterKnownSkills.Union(characterUnknownSkills).Distinct().ToList();
            }
        }

        public string SelectedCharacterSkill
        {
            get { return _selectedCharacterSkill; }
            set
            {
                _selectedCharacterSkill = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedCharacterSkillLevel));

                _computeCommand.Update();
            }
        }

        public double SelectedCharacterSkillLevel
        {
            get
            {
                if (SelectedCharacter == null || SelectedCharacterSkill == null) return 0;
                var knownSkill = SelectedCharacter.KnownSkills.FirstOrDefault(x => x.Name == SelectedCharacterSkill);
                return knownSkill?.Level ?? SelectedCharacter.UnknownSkills.First(x => x.Name == SelectedCharacterSkill).MeanSkill;
            }
        }

        public IEnumerable<Character> PossibleOpponents
        {
            get
            {
                if (SelectedCharacter == null) return new Character[0];
                return Characters.Where(x => x != SelectedCharacter);
            }
        }

        public Character SelectedOpponent
        {
            get { return _selectedOpponent; }
            set
            {
                _selectedOpponent = value;
                OnPropertyChanged();

                SelectedOpponentSkill = null;
                OnPropertyChanged(nameof(PossibleOpponentSkills));

                _computeCommand.Update();
            }
        }

        public IEnumerable<string> PossibleOpponentSkills
        {
            get
            {
                if (SelectedOpponent == null) return new string[0];
                var opponentKnownSkills = SelectedOpponent.KnownSkills.Select(x => x.Name);
                var opponentUnknownSkills = SelectedOpponent.UnknownSkills.Select(x => x.Name);
                return opponentKnownSkills.Union(opponentUnknownSkills).Distinct().ToList();
            }
        }

        public string SelectedOpponentSkill
        {
            get { return _selectedOpponentSkill; }
            set
            {
                _selectedOpponentSkill = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedOpponentSkillLevel));

                _computeCommand.Update();
            }
        }

        public double SelectedOpponentSkillLevel
        {
            get
            {
                if (SelectedOpponent == null || SelectedOpponentSkill == null) return 0;
                var knownSkill = SelectedOpponent.KnownSkills.FirstOrDefault(x => x.Name == SelectedOpponentSkill);
                return knownSkill?.Level ?? SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill).MeanSkill;
            }
        }

        public int CharacterBonusDice
        {
            get { return _characterBonusDice; }
            set
            {
                _characterBonusDice = value;
                OnPropertyChanged();
            }
        }

        public int OpponentBonusDice
        {
            get { return _opponentBonusDice; }
            set
            {
                _opponentBonusDice = value;
                OnPropertyChanged();
            }
        }

        public double OutcomeProbability { get; private set; }

        public double VictoryAPercent { get; private set; }
        public double VictoryBPercent { get; private set; }
        public double VictoryCPercent { get; private set; }
        public double DefeatAPercent { get; private set; }
        public double DefeatBPercent { get; private set; }
        public double DefeatCPercent { get; private set; }

        private List<Tuple<Outcome, double>> CharacterOutcomes { get; } = new List<Tuple<Outcome, double>>();
        private List<Tuple<Outcome, double>> OpponentOutcomes { get; } = new List<Tuple<Outcome, double>>();

        private double Iterations { get; set; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value; 
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        private double _progressCount;

        #endregion

        #region Constructor

        public VersusViewModel(ObservableCollection<Character> characters)
        {
            Characters = characters;
        }

        #endregion

        #region Methods

        private async void ComputeOutcomeProbability() => await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
                Progress = 0;
            });

            CharacterOutcomes.Clear();
            CharacterOutcomes.Add(new Tuple<Outcome, double>(new Outcome(), 0));
            OpponentOutcomes.Clear();
            OpponentOutcomes.Add(new Tuple<Outcome, double>(new Outcome(), 0));

            OutcomeProbability = GetOutcomeProbability();

            var characterScaledOutcomes = CharacterOutcomes.Select(outcomeSet =>
            {
                var scale = outcomeSet.Item1.ScalingFactor*outcomeSet.Item2;
                return new
                {
                    va = outcomeSet.Item1.VictoryA*scale,
                    vb = outcomeSet.Item1.VictoryB*scale,
                    vc = outcomeSet.Item1.VictoryC*scale,
                    da = outcomeSet.Item1.DefeatA*scale,
                    db = outcomeSet.Item1.DefeatB*scale,
                    dc = outcomeSet.Item1.DefeatC*scale,
                };
            }).Aggregate((current, next) => new
            {
                va = current.va + next.va,
                vb = current.vb + next.vb,
                vc = current.vc + next.vc,
                da = current.da + next.da,
                db = current.db + next.db,
                dc = current.dc + next.dc,
            });

            var opponentScaledOutcomes = OpponentOutcomes.Select(outcomeSet =>
            {
                var scale = outcomeSet.Item1.ScalingFactor*outcomeSet.Item2;
                return new
                {
                    va = outcomeSet.Item1.VictoryA*scale,
                    vb = outcomeSet.Item1.VictoryB*scale,
                    vc = outcomeSet.Item1.VictoryC*scale,
                    da = outcomeSet.Item1.DefeatA*scale,
                    db = outcomeSet.Item1.DefeatB*scale,
                    dc = outcomeSet.Item1.DefeatC*scale,
                };
            }).Aggregate((current, next) => new
            {
                va = current.va + next.va,
                vb = current.vb + next.vb,
                vc = current.vc + next.vc,
                da = current.da + next.da,
                db = current.db + next.db,
                dc = current.dc + next.dc,
            });


            var totalScaledOutcomes = new
            {
                va = characterScaledOutcomes.va + opponentScaledOutcomes.da,
                vb = characterScaledOutcomes.vb + opponentScaledOutcomes.db,
                vc = characterScaledOutcomes.vc + opponentScaledOutcomes.dc,
                da = characterScaledOutcomes.da + opponentScaledOutcomes.va,
                db = characterScaledOutcomes.db + opponentScaledOutcomes.vb,
                dc = characterScaledOutcomes.dc + opponentScaledOutcomes.vc,
            };

            var total = totalScaledOutcomes.va
                        + totalScaledOutcomes.vb
                        + totalScaledOutcomes.vc
                        + totalScaledOutcomes.da
                        + totalScaledOutcomes.db
                        + totalScaledOutcomes.dc;

            VictoryAPercent = 100.0*totalScaledOutcomes.va/total;
            VictoryBPercent = 100.0*totalScaledOutcomes.vb/total;
            VictoryCPercent = 100.0*totalScaledOutcomes.vc/total;
            DefeatAPercent = 100.0*totalScaledOutcomes.da/total;
            DefeatBPercent = 100.0*totalScaledOutcomes.db/total;
            DefeatCPercent = 100.0*totalScaledOutcomes.dc/total;

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
                OnPropertyChanged(nameof(OutcomeProbability));
                OnPropertyChanged(nameof(VictoryAPercent));
                OnPropertyChanged(nameof(VictoryBPercent));
                OnPropertyChanged(nameof(VictoryCPercent));
                OnPropertyChanged(nameof(DefeatAPercent));
                OnPropertyChanged(nameof(DefeatBPercent));
                OnPropertyChanged(nameof(DefeatCPercent));
            });
        });

        private double GetOutcomeProbability()
        {
            var characterKnownSkill = SelectedCharacter.KnownSkills.FirstOrDefault(x => x.Name == SelectedCharacterSkill);
            if (characterKnownSkill == null)
            {
                var characterUnknownSkill = SelectedCharacter.UnknownSkills.First(x => x.Name == SelectedCharacterSkill);

                var opponentKnownSkill = SelectedOpponent.KnownSkills.FirstOrDefault(x => x.Name == SelectedOpponentSkill);
                if (opponentKnownSkill != null) return ComputeOutcomeProbability(characterUnknownSkill, opponentKnownSkill);

                var opponentUnknownSkill = SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill);

                var o = ComputeOutcomeProbability(opponentUnknownSkill, OpponentBonusDice, characterUnknownSkill, CharacterBonusDice, OpponentOutcomes);
                var c = ComputeOutcomeProbability(characterUnknownSkill, CharacterBonusDice, opponentUnknownSkill, OpponentBonusDice, CharacterOutcomes);
                
                return 100.0 * (c/(c+o));
            }

            var opponentSkill = SelectedOpponent.KnownSkills.FirstOrDefault(x => x.Name == SelectedOpponentSkill);
            return opponentSkill == null
                ? ComputeOutcomeProbability(characterKnownSkill, CharacterBonusDice, SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill), CharacterBonusDice, CharacterOutcomes)
                : ComputeOutcomeProbability(characterKnownSkill, opponentSkill);
        }

        private double ComputeOutcomeProbability(KnownSkill characterSkill, KnownSkill opponentSkill)
        {
            _progressCount = 1 / 1000.0;

            var characterDice = characterSkill.Level + CharacterBonusDice;
            var opponentDice = opponentSkill.Level + OpponentBonusDice;

            var outcome = new Outcome();
            var result = SimulateDiceResult(characterDice, opponentDice, 100000, outcome, 1.0) * 100.0;
            CharacterOutcomes.Add(Tuple.Create(outcome, 1.0));

            return result;
        }

        private double ComputeOutcomeProbability(KnownSkill characterSkill, int characterBonusDice, UnknownSkill opponentSkill, int opponentBonusDice, List<Tuple<Outcome, double>> outcomeAggregate)
        {
            _progressCount = 1.0 / 1000.0;

            var characterDice = characterSkill.Level + characterBonusDice;
            var characterWinProbability = 0.0;
            var outcome = new Outcome();
            for (var index = 0; index < Constants.MAX_DICE_LEVEL; index++)
            {
                var opponentDice = index + 1 + opponentBonusDice;
                var opponentDiceProbability = opponentSkill.GetRatingProbability(index);
                characterWinProbability += SimulateDiceResult(characterDice, opponentDice, 1000, outcome, opponentDiceProbability);
            }
            outcomeAggregate.Add(Tuple.Create(outcome, 1.0));
            return characterWinProbability * 100;
        }

        private double ComputeOutcomeProbability(UnknownSkill characterSkill, KnownSkill opponentSkill)
        {
            return 100 - ComputeOutcomeProbability(opponentSkill, OpponentBonusDice, characterSkill, CharacterBonusDice, OpponentOutcomes);
        }

        private double ComputeOutcomeProbability(UnknownSkill characterSkill, int characterDiceBonus, UnknownSkill opponentSkill, int opponentDiceBonus, List<Tuple<Outcome, double>> outcomeAggregate)
        {
            var characterWinProbability = 0.0;
            var characterLikelyDice = characterSkill.TopValues;
            var opponentLikelyDice = opponentSkill.TopValues;
            _progressCount = 100.0/(characterLikelyDice.Count*opponentLikelyDice.Count*2.0);
            
            for (var cIndex = 0; cIndex < characterLikelyDice.Count; cIndex++)
            {
                var characterDice = characterLikelyDice[cIndex].Item1 + characterDiceBonus;
                var characterDiceProbability = characterLikelyDice[cIndex].Item2 / 100.0;

                var characterWinSub = 0.0;

                var outcome = new Outcome();
                for (var oIndex = 0; oIndex < opponentLikelyDice.Count; oIndex++)
                {
                    var opponentDice = opponentLikelyDice[oIndex].Item1 + opponentDiceBonus;
                    var opponentDiceProbability = opponentLikelyDice[oIndex].Item2 / 100.0;
                    characterWinSub += SimulateDiceResult(characterDice, opponentDice, 500, outcome, opponentDiceProbability);
                }

                outcomeAggregate.Add(Tuple.Create(outcome, characterDiceProbability));
                characterWinProbability += characterWinSub*characterDiceProbability;
            }
            return characterWinProbability * 100;
        }

        private static readonly Random Rand = new Random();
        private double SimulateDiceResult(int characterDice, int opponentDice, int iterations, Outcome outcome, double scale)
        {
            characterDice = characterDice < Constants.MIN_DICE_LEVEL ? Constants.MIN_DICE_LEVEL
                : characterDice;

            opponentDice = opponentDice < Constants.MIN_DICE_LEVEL ? Constants.MIN_DICE_LEVEL
                : opponentDice;

            var characterWins = 0;
            for (var i = 0; i < iterations; i++)
            {
                var characterResult = 0;
                for (var c = 0; c < characterDice; c++)
                {
                    characterResult += Rand.Next(1, 101);
                }

                var opponentResult = 0;
                for (var o = 0; o < opponentDice; o++)
                {
                    opponentResult += Rand.Next(1, 101);
                }

                var outcomeResult = (characterResult - opponentResult)/Math.Pow(characterDice + opponentDice, 0.65);
                if (outcomeResult >= 0) characterWins += 1;
                outcome.Update(outcomeResult, scale);
            }
            Progress += _progressCount;
            return scale * characterWins / (double)iterations;
        }

        #endregion

        #region Compute Command

        private Command _computeCommand;

        public ICommand ComputeCommand
        {
            get
            {
                if (_computeCommand == null)
                {
                    _computeCommand = new Command(Compute, CanCompute);
                }
                return _computeCommand;
            }
        }

        private void Compute()
        {
            ComputeOutcomeProbability();
        }

        private bool CanCompute()
        {
            return SelectedCharacter != null
                && SelectedCharacterSkill != null
                && SelectedOpponent != null
                && SelectedOpponentSkill != null;
        }

        #endregion
    }
}
