using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffDescriptionsConverter : IValueConverter
    {
        private static readonly BuffDescription[] _empty = { BuffDescription.Empty };

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Buff buff))
            {
                return DependencyProperty.UnsetValue;
            }
            return buff.Descriptions.Length != 0 ? buff.Descriptions : BuffDescriptionsConverter._empty;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
