using System.Runtime.Serialization;
using MFDSkillTracking.Common;

namespace MFDSkillTracking.Models
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(KnownSkill))]
    [KnownType(typeof(UnknownSkill))]
    public abstract class Skill : PropertyChangedBase
    {
        public Skill DeepCopy()
        {
            return Clone(this);
        }

        private static T Clone<T>(T obj) where T : class
        {
            var serializer = new DataContractSerializer(typeof(T), null, int.MaxValue, false, true, null);
            using (var ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                ms.Position = 0;
                return (T)serializer.ReadObject(ms);
            }
        }
    }
}
