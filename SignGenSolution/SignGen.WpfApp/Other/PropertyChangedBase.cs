using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SignGen.WpfApp.Other
{
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Update<T>(T value, ref T prop, [CallerMemberName] string callerName = null)
        {
            prop = value;
            RaisePropertyChanged(callerName);
        }

        protected void RaisePropertyChanged(string propName)
        {
            if (!string.IsNullOrEmpty(propName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
