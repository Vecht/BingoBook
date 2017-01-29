using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MFDSkillTracking.Common;
using MFDSkillTracking.Models;
using OxyPlot.Series;

namespace MFDSkillTracking.ViewModels
{
    public class CharacterViewModel : PropertyChangedBase
    {
        #region Fields

        private readonly ObservableCollection<Character> _characters;
        private readonly CollectionViewSource _knownSkillsCVS = new CollectionViewSource();
        private readonly CollectionViewSource _unknownSkillsCVS = new CollectionViewSource();

        private int _computationAccuracy = 5;

        private string _knownSkillSearchString;
        private KnownSkill _selectedKnownSkill;
        private string _newKnownSkillName;

        private string _unknownSkillSearchString;
        private UnknownSkill _selectedUnknownSkill;
        private string _newUnknownSkillName;
        private NinjaLevel? _selectedSkillLevel;

        private Roll _selectedRoll;

        private Character _selectedOpponent;
        private string _selectedOpponentSkill;
        private double _outcome;
        private int _opponentDiceBonusMin;
        private int _opponentDiceBonusMax;
        private int _characterDiceBonus;


        #endregion

        #region Properties

        public Character Character { get; }
        public ICollectionView KnownSkills => _knownSkillsCVS.View;
        public ICollectionView UnknownSkills => _unknownSkillsCVS.View;
        public IEnumerable<NinjaLevel> SkillLevels { get; } = Enum.GetValues(typeof(NinjaLevel)).OfType<NinjaLevel>().ToArray();

        public int ComputationAccuracy
        {
            get { return _computationAccuracy; }
            set
            {
                _computationAccuracy = value;
                Constants.ComputationAccuracy = value;
                OnPropertyChanged();
            }
        }

        public string KnownSkillSearchString
        {
            get { return _knownSkillSearchString; }
            set
            {
                _knownSkillSearchString = value;
                OnPropertyChanged();
                KnownSkills.Refresh();
            }
        }

        public KnownSkill SelectedKnownSkill
        {
            get { return _selectedKnownSkill; }
            set
            {
                _selectedKnownSkill = value;
                OnPropertyChanged();
                _removeKnownSkillCommand?.Update();
            }
        }

        public string NewKnownSkillName
        {
            get { return _newKnownSkillName; }
            set
            {
                _newKnownSkillName = value;
                OnPropertyChanged();
                _addKnownSkillCommand?.Update();
            }
        }

        public string UnknownSkillSearchString
        {
            get { return _unknownSkillSearchString; }
            set
            {
                _unknownSkillSearchString = value;
                OnPropertyChanged();
                UnknownSkills.Refresh();
            }
        }

        public UnknownSkill SelectedUnknownSkill
        {
            get { return _selectedUnknownSkill; }
            set
            {
                _selectedUnknownSkill = value;
                _selectedRoll = null;
                OnPropertyChanged();
                _removeUnknownSkillCommand?.Update();
                _addRollResultCommand?.Update();
            }
        }

        public string NewUnknownSkillName
        {
            get { return _newUnknownSkillName; }
            set
            {
                _newUnknownSkillName = value;
                OnPropertyChanged();
                _addUnknownSkillCommand?.Update();
            }
        }

        public NinjaLevel? SelectedSkillLevel
        {
            get { return _selectedSkillLevel; }
            set
            {
                _selectedSkillLevel = value;
                OnPropertyChanged();
                _addUnknownSkillCommand?.Update();
            }
        }

        public Roll SelectedRoll
        {
            get { return _selectedRoll; }
            set
            {
                _selectedRoll = value;
                OnPropertyChanged();
                _removeRollResultCommand?.Update();
            }
        }

        public IEnumerable<Character> PossibleOpponents => _characters.Where(x => x != Character);

        public Character SelectedOpponent
        {
            get { return _selectedOpponent; }
            set
            {
                _selectedOpponent = value;
                OnPropertyChanged();

                SelectedOpponentSkill = null;
                OnPropertyChanged(nameof(PossibleOpponentSkills));

                OpponentDiceBonusMin = 0;
                OpponentDiceBonusMax = 0;
                CharacterDiceBonus = 0;

                _addRollResultCommand?.Update();
            }
        }

        public IEnumerable<string> PossibleOpponentSkills
        {
            get
            {
                if (SelectedOpponent == null) return new string[0];

                return SelectedOpponent.KnownSkills.Select(x => x.Name)
                    .Union(SelectedOpponent.UnknownSkills.Select(x => x.Name))
                    .OrderBy(x => x)
                    .Distinct();
            }
        }

        public string SelectedOpponentSkill
        {
            get
            {
                return _selectedOpponentSkill;
            }
            set
            {
                _selectedOpponentSkill = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedOpponentSkillIsKnown));
                OnPropertyChanged(nameof(SelectedOpponentSkillIsUnknown));
                OnPropertyChanged(nameof(SelectedOpponentSkillLevel));
                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(SelectedOpponentSkillDistribution)));
                });
                

                _addRollResultCommand?.Update();
            }
        }

        public bool SelectedOpponentSkillIsKnown
        {
            get
            {
                if (SelectedOpponent == null || SelectedOpponentSkill == null) return false;
                return SelectedOpponent.KnownSkills.Select(x => x.Name).Contains(SelectedOpponentSkill);
            }
        }

        public bool SelectedOpponentSkillIsUnknown
        {
            get
            {
                if (SelectedOpponent == null || SelectedOpponentSkill == null) return false;
                return SelectedOpponent.UnknownSkills.Select(x => x.Name).Contains(SelectedOpponentSkill);
            }
        }

        public string SelectedOpponentSkillLevel
        {
            get
            {
                if (!SelectedOpponentSkillIsKnown) return "lvl ??";
                return "lvl " + SelectedOpponent.KnownSkills.First(x => x.Name == SelectedOpponentSkill).Level;
            }
        }

        public List<ScatterPoint> SelectedOpponentSkillDistribution
        {
            get
            {
                if (!SelectedOpponentSkillIsUnknown) return new List<ScatterPoint>();
                return SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill).SkillDistribution.ToList();
            }
        }

        public double Outcome
        {
            get { return _outcome; }
            set
            {
                _outcome = value;
                OnPropertyChanged();

                _addRollResultCommand?.Update();
            }
        }

        public int OpponentDiceBonusMin
        {
            get { return _opponentDiceBonusMin; }
            set
            {
                _opponentDiceBonusMin = value;
                OnPropertyChanged();

                _addRollResultCommand?.Update();
            }
        }

        public int OpponentDiceBonusMax
        {
            get { return _opponentDiceBonusMax; }
            set
            {
                _opponentDiceBonusMax = value;
                OnPropertyChanged();

                _addRollResultCommand?.Update();
            }
        }

        public int CharacterDiceBonus
        {
            get { return _characterDiceBonus; }
            set
            {
                _characterDiceBonus = value;
                OnPropertyChanged();

                _addRollResultCommand?.Update();
            }
        }

        #endregion

        #region Constructor

        public CharacterViewModel(Character character, ObservableCollection<Character> characters)
        {
            Character = character;
            _characters = characters;

            _knownSkillsCVS.Source = Character.KnownSkills;
            _knownSkillsCVS.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _knownSkillsCVS.Filter += (sender, e) =>
            {
                var skill = e.Item as KnownSkill;
                if (skill == null || string.IsNullOrWhiteSpace(KnownSkillSearchString)) return;

                e.Accepted = skill.Name.ToUpper().Contains(KnownSkillSearchString.ToUpper());
            };

            _unknownSkillsCVS.Source = Character.UnknownSkills;
            _unknownSkillsCVS.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _unknownSkillsCVS.Filter += (sender, e) =>
            {
                var skill = e.Item as UnknownSkill;
                if (skill == null || string.IsNullOrWhiteSpace(UnknownSkillSearchString)) return;

                e.Accepted = skill.Name.ToUpper().Contains(UnknownSkillSearchString.ToUpper());
            };
        }

        #endregion

        #region Commands

        #region Add Known Skill Command

        private Command _addKnownSkillCommand;
        public ICommand AddKnownSkillCommand
        {
            get
            {
                if (_addKnownSkillCommand == null)
                {
                    _addKnownSkillCommand = new Command(AddKnownSkill, CanAddKnownSkill);
                }
                return _addKnownSkillCommand;
            }
        }

        private void AddKnownSkill()
        {
            Character.KnownSkills.Add(new KnownSkill(NewKnownSkillName, 15));
            NewKnownSkillName = null;
        }

        private bool CanAddKnownSkill()
        {
            return !string.IsNullOrWhiteSpace(NewKnownSkillName)
                && Character.KnownSkills.All(c => c.Name.ToUpper() != NewKnownSkillName.ToUpper())
                && Character.UnknownSkills.All(c => c.Name.ToUpper() != NewKnownSkillName.ToUpper());
        }

        #endregion

        #region Remove Known Skill Command

        private Command _removeKnownSkillCommand;
        public ICommand RemoveKnownSkillCommand
        {
            get
            {
                if (_removeKnownSkillCommand == null)
                {
                    _removeKnownSkillCommand = new Command(RemoveKnownSkill, CanRemoveKnownSkill);
                }
                return _removeKnownSkillCommand;
            }
        }

        private void RemoveKnownSkill()
        {
            var result = MessageBox.Show($"Are you sure you want to remove skill '{SelectedKnownSkill.Name}'?\nThis cannot be undone.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;

            Character.KnownSkills.Remove(SelectedKnownSkill);
        }

        private bool CanRemoveKnownSkill()
        {
            return SelectedKnownSkill != null && KnownSkills.Contains(SelectedKnownSkill);
        }

        #endregion

        #region Add Unknown Skill Command

        private Command _addUnknownSkillCommand;
        public ICommand AddUnknownSkillCommand
        {
            get
            {
                if (_addUnknownSkillCommand == null)
                {
                    _addUnknownSkillCommand = new Command(AddUnknownSkill, CanAddUnknownSkill);
                }
                return _addUnknownSkillCommand;
            }
        }

        private void AddUnknownSkill()
        {
            Character.UnknownSkills.Add(new UnknownSkill(NewUnknownSkillName, SelectedSkillLevel.Value));
            NewUnknownSkillName = null;
            SelectedSkillLevel = null;
        }

        private bool CanAddUnknownSkill()
        {
            return !string.IsNullOrWhiteSpace(NewUnknownSkillName)
                && SelectedSkillLevel != null
                && Character.KnownSkills.All(c => c.Name.ToUpper() != NewUnknownSkillName.ToUpper())
                && Character.UnknownSkills.All(c => c.Name.ToUpper() != NewUnknownSkillName.ToUpper());
        }

        #endregion

        #region Remove Unknown Skill Command

        private Command _removeUnknownSkillCommand;
        public ICommand RemoveUnknownSkillCommand
        {
            get
            {
                if (_removeUnknownSkillCommand == null)
                {
                    _removeUnknownSkillCommand = new Command(RemoveUnknownSkill, CanRemoveUnknownSkill);
                }
                return _removeUnknownSkillCommand;
            }
        }

        private void RemoveUnknownSkill()
        {
            var result = MessageBox.Show($"Are you sure you want to remove skill '{SelectedUnknownSkill.Name}'?\nThis cannot be undone.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;

            Character.UnknownSkills.Remove(SelectedUnknownSkill);
        }

        private bool CanRemoveUnknownSkill()
        {
            return SelectedUnknownSkill != null && UnknownSkills.Contains(SelectedUnknownSkill);
        }

        #endregion

        #region Add Roll Result Command

        private Command _addRollResultCommand;
        public ICommand AddRollResultCommand
        {
            get
            {
                if (_addRollResultCommand == null)
                {
                    _addRollResultCommand = new Command(AddRollResult, CanAddRollResult);
                }
                return _addRollResultCommand;
            }
        }

        private void AddRollResult()
        {
            var opponentSkill = (Skill)SelectedOpponent.KnownSkills.FirstOrDefault(x => x.Name == SelectedOpponentSkill)
                ?? SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill);
            var rollResult = new Roll(SelectedOpponent.Name, opponentSkill.DeepCopy(), Outcome, CharacterDiceBonus, OpponentDiceBonusMin, OpponentDiceBonusMax);

            SelectedUnknownSkill.Rolls.Add(rollResult);
            SelectedUnknownSkill.Update(rollResult);

            Outcome = 0;
            CharacterDiceBonus = 0;
            OpponentDiceBonusMin = 0;
            OpponentDiceBonusMax = 0;
        }

        private bool CanAddRollResult()
        {
            return SelectedUnknownSkill != null
                && !SelectedUnknownSkill.IsBusy
                && SelectedOpponent != null
                && SelectedOpponentSkill != null
                && OpponentDiceBonusMax >= OpponentDiceBonusMin;
        }

        #endregion

        #region Remove Roll Result Command

        private Command _removeRollResultCommand;
        public ICommand RemoveRollResultCommand
        {
            get
            {
                if (_removeRollResultCommand == null)
                {
                    _removeRollResultCommand = new Command(RemoveRollResult, CanRemoveRollResult);
                }
                return _removeRollResultCommand;
            }
        }

        private void RemoveRollResult()
        {
            SelectedUnknownSkill.Rolls.Remove(SelectedRoll);
            SelectedUnknownSkill.Recalculate();
        }

        private bool CanRemoveRollResult()
        {
            return SelectedUnknownSkill != null
                && SelectedRoll != null
                && SelectedUnknownSkill.Rolls.Contains(SelectedRoll);
        }

        #endregion

        #endregion
    }
}
