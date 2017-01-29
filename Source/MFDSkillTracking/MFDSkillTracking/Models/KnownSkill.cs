using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using MFDSkillTracking.Common;

namespace MFDSkillTracking.Models
{
    [DataContract(IsReference = true)]
    public class KnownSkill : Skill
    {
        #region Fields

        [DataMember] private int _level;

        #endregion

        #region Properties

        [DataMember] public string Name { get; private set; }

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public KnownSkill(string name, int level)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Name cannot be null, empty, or whitespace", nameof(name));
            if (level < Constants.MIN_DICE_LEVEL || level > Constants.MAX_DICE_LEVEL) throw new ArgumentException(@"Level is out of bounds", nameof(level));
            Name = name;
            _level = level;
        }

        #endregion

        #region Commands

        #region Increment Command

        private Command _incrementLevelCommand;
        public ICommand IncrementLevelCommand
        {
            get
            {
                if (_incrementLevelCommand == null)
                {
                    _incrementLevelCommand = new Command(IncrementLevel, CanIncrementLevel);
                }
                return _incrementLevelCommand;
            }
        }

        private void IncrementLevel()
        {
            Level += 1;
        }

        private bool CanIncrementLevel()
        {
            return Level < Constants.MAX_DICE_LEVEL;
        }

        #endregion

        #region Decrement Command

        private Command _decrementLevelCommand;
        public ICommand DecrementLevelCommand
        {
            get
            {
                if (_decrementLevelCommand == null)
                {
                    _decrementLevelCommand = new Command(DecrementLevel, CanDecrementLevel);
                }
                return _decrementLevelCommand;
            }
        }

        private void DecrementLevel()
        {
            Level -= 1;
        }

        private bool CanDecrementLevel()
        {
            return Level > Constants.MIN_DICE_LEVEL;
        }

        #endregion

        #endregion
    }
}
