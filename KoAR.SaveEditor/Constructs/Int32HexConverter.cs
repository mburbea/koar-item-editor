using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    class Int32HexConverter : IValueConverter
    {
        object IValueConverter.Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int i ? i.ToString("X6", culture) : string.Empty;
        }
        object? IValueConverter.ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string s ? int.Parse(s, NumberStyles.HexNumber, culture) : default(int?);
        }
    }
}