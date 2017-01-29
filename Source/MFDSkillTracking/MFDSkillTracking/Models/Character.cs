using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using MFDSkillTracking.Common;

namespace MFDSkillTracking.Models
{
    [CollectionDataContract(Name = "KnownSkillObservableCollection"), KnownType(typeof(KnownSkillObservableCollection))]
    public class KnownSkillObservableCollection : ObservableCollection<KnownSkill> { }

    [CollectionDataContract(Name = "UnknownSkillObservableCollection"), KnownType(typeof(UnknownSkillObservableCollection))]
    public class UnknownSkillObservableCollection : ObservableCollection<UnknownSkill> { }

    [DataContract(IsReference = true)]
    [KnownType(typeof(KnownSkill))]
    [KnownType(typeof(UnknownSkill))]
    [KnownType(typeof(Roll))]
    public class Character : PropertyChangedBase
    {
        #region Fields

        [DataMember] private readonly KnownSkillObservableCollection _knownSkills = new KnownSkillObservableCollection();
        [DataMember] private readonly UnknownSkillObservableCollection _unknownSkills = new UnknownSkillObservableCollection();

        #endregion

        #region Properties

        [DataMember] public string Name { get; private set; }
        public KnownSkillObservableCollection KnownSkills => _knownSkills;
        public UnknownSkillObservableCollection UnknownSkills => _unknownSkills;

        #endregion

        #region Constructor

        public Character(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Name cannot be null, empty, or whitespace", nameof(name));
            Name = name;
        }

        #endregion
    }
}
