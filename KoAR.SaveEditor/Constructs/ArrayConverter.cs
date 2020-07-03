using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class ArrayConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => new[] { value };

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is object[] { Length: 1 } array ? array[0] : value;
        }
    }
}
