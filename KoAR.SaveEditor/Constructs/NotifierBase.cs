using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KoAR.SaveEditor.Constructs
{
    public abstract class NotifierBase : INotifyPropertyChanged
    {
        protected NotifierBase()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new(propertyName));
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
