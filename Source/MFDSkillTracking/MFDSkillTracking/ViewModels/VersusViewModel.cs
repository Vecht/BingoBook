using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                ComputeOutcomeProbability();
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
                ComputeOutcomeProbability();
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
                ComputeOutcomeProbability();
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
                ComputeOutcomeProbability();
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
                ComputeOutcomeProbability();
            }
        }

        public int OpponentBonusDice
        {
            get { return _opponentBonusDice; }
            set
            {
                _opponentBonusDice = value;
                OnPropertyChanged();
                ComputeOutcomeProbability();
            }
        }

        private double _outcomeProbability;
        public double OutcomeProbability => _outcomeProbability;

        #endregion

        #region Constructor

        public VersusViewModel(ObservableCollection<Character> characters)
        {
            Characters = characters;
        }

        #endregion

        #region Methods

        private void ComputeOutcomeProbability()
        {
            _outcomeProbability = GetOutcomeProbability();
            OnPropertyChanged(nameof(OutcomeProbability));
        }

        private double GetOutcomeProbability()
        {
            if (SelectedCharacter == null || SelectedCharacterSkill == null || SelectedOpponent == null || SelectedOpponentSkill == null) return 0;

            var characterKnownSkill = SelectedCharacter.KnownSkills.FirstOrDefault(x => x.Name == SelectedCharacterSkill);
            if (characterKnownSkill == null)
            {
                var characterUnknownSkill = SelectedCharacter.UnknownSkills.First(x => x.Name == SelectedCharacterSkill);

                var opponentKnownSkill = SelectedOpponent.KnownSkills.FirstOrDefault(x => x.Name == SelectedOpponentSkill);
                if (opponentKnownSkill != null) return ComputeOutcomeProbability(characterUnknownSkill, opponentKnownSkill);

                var opponentUnknownSkill = SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill);
                var c = ComputeOutcomeProbability(characterUnknownSkill, CharacterBonusDice, opponentUnknownSkill, OpponentBonusDice);
                var o = ComputeOutcomeProbability(opponentUnknownSkill, OpponentBonusDice, characterUnknownSkill, CharacterBonusDice);
                return 100.0 * (c/(c+o));
            }

            var opponentSkill = SelectedOpponent.KnownSkills.FirstOrDefault(x => x.Name == SelectedOpponentSkill);
            return opponentSkill == null
                ? ComputeOutcomeProbability(characterKnownSkill, SelectedOpponent.UnknownSkills.First(x => x.Name == SelectedOpponentSkill))
                : ComputeOutcomeProbability(characterKnownSkill, opponentSkill);
        }

        private double ComputeOutcomeProbability(KnownSkill characterSkill, KnownSkill opponentSkill)
        {
            var characterDice = characterSkill.Level + CharacterBonusDice;
            var opponentDice = opponentSkill.Level + OpponentBonusDice;
            return SimulateDiceResult(characterDice, opponentDice, 100000) * 100;
        }

        private double ComputeOutcomeProbability(KnownSkill characterSkill, UnknownSkill opponentSkill)
        {
            var characterDice = characterSkill.Level + CharacterBonusDice;
            var characterWinProbability = 0.0;
            for (var index = 0; index < Constants.MAX_DICE_LEVEL; index++)
            {
                var opponentDice = index + 1 + OpponentBonusDice;
                var opponentDiceProbability = opponentSkill.GetRatingProbability(index);
                characterWinProbability += SimulateDiceResult(characterDice, opponentDice, 1000) * opponentDiceProbability;
            }
            return characterWinProbability * 100;
        }

        private double ComputeOutcomeProbability(UnknownSkill characterSkill, KnownSkill opponentSkill)
        {
            return 100 - ComputeOutcomeProbability(opponentSkill, characterSkill);
        }

        private double ComputeOutcomeProbability(UnknownSkill characterSkill, int characterDiceBonus, UnknownSkill opponentSkill, int opponentDiceBonus)
        {
            var characterWinProbability = 0.0;
            var characterLikelyDice = characterSkill.TopValues;
            for (var cIndex = 0; cIndex < characterLikelyDice.Count; cIndex++)
            {
                var characterDice = characterLikelyDice[cIndex].Item1 + characterDiceBonus;
                var characterDiceProbability = characterLikelyDice[cIndex].Item2 / 100.0;

                var characterWinSub = 0.0;
                var opponentLikelyDice = opponentSkill.TopValues;
                for (var oIndex = 0; oIndex < opponentLikelyDice.Count; oIndex++)
                {
                    var opponentDice = opponentLikelyDice[oIndex].Item1 + opponentDiceBonus;
                    var opponentDiceProbability = opponentLikelyDice[oIndex].Item2 / 100.0;
                    characterWinSub += SimulateDiceResult(characterDice, opponentDice, 1000)*opponentDiceProbability;
                }

                characterWinProbability += characterWinSub*characterDiceProbability;
            }
            return characterWinProbability * 100;
        }

        private static readonly Random Rand = new Random();
        private static double SimulateDiceResult(int characterDice, int opponentDice, int iterations)
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

                if (characterResult >= opponentResult) characterWins += 1;
            }

            return characterWins/(double)iterations;
        }
    }

    #endregion
}
