using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MFDSkillTracking.Properties;

namespace MFDSkillTracking.Common
{
    [DataContract(IsReference = true)]
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) PropertyChanged?.Invoke(this, null);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateProperty(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
