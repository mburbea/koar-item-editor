using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KoAR.SaveEditor.Constructs
{
    public abstract class NotifierBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }
            field = value;
            this.OnPropertyChanged(propertyName);
        }
    }
}
