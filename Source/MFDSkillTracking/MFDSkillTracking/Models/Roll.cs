using System;
using System.Runtime.Serialization;

namespace MFDSkillTracking.Models
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(KnownSkill))]
    [KnownType(typeof(UnknownSkill))]
    public class Roll
    {
        [DataMember] public string OpponentName { get; private set; }
        [DataMember] public Skill OpponentSkill { get; private set; }
        [DataMember] public int OpponentDiceBonusMin { get; private set; }
        [DataMember] public int OpponentDiceBonusMax { get; private set; }
        [DataMember] public int CharacterDiceBonus { get; private set; }
        [DataMember] public double Result { get; private set; }

        private string OpponentSkillName => (OpponentSkill as KnownSkill)?.Name ?? ((UnknownSkill) OpponentSkill).Name;
        private string OpponentSkillLevel => (OpponentSkill as KnownSkill)?.Level.ToString() ?? "~" + ((UnknownSkill)OpponentSkill).MeanSkill.ToString("F");

        public string RollText => OpponentDiceBonusMax == 0
            ? $"{OpponentName}\n{OpponentSkillName} ({OpponentSkillLevel})\n{Result}\n"
            : $"{OpponentName}\n{OpponentSkillName} ({OpponentSkillLevel})\n{Result} | {CharacterDiceBonus};{OpponentDiceBonusMin} - {OpponentDiceBonusMax}\n";

        public Roll(string opponentName, Skill opponentSkill, double result, int characterDiceBonus, int opponentDiceBonusMin, int opponentDiceBonusMax)
        {
            if (string.IsNullOrWhiteSpace(opponentName)) throw new ArgumentException(@"Opponent name cannot be null, whitespace, or empty", nameof(opponentName));
            if (!(opponentSkill is KnownSkill) && !(opponentSkill is UnknownSkill)) throw new ArgumentException(@"Opponent skill must be a known skill or an unknown skill", nameof(opponentSkill));
            if (opponentDiceBonusMax < opponentDiceBonusMin) throw new ArgumentException(@"Dice modifier maximum must be greater than or equal to minimum", nameof(opponentDiceBonusMax));

            OpponentName = opponentName;
            OpponentSkill = opponentSkill;
            Result = result;
            CharacterDiceBonus = characterDiceBonus;
            OpponentDiceBonusMin = opponentDiceBonusMin;
            OpponentDiceBonusMax = opponentDiceBonusMax;
        }
    }
}
