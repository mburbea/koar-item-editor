using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class StringIsNullOrEmptyConverter : IValueConverter
    {
        private static readonly object _false = false;
        private static readonly object _true = true;

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? StringIsNullOrEmptyConverter._true : StringIsNullOrEmptyConverter._false;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
