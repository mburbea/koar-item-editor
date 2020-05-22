using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class Int32HexConverter : IValueConverter
    {
        object IValueConverter.Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int number ? number.ToString("X6", culture) : string.Empty;
        }

        object? IValueConverter.ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string text ? int.Parse(text, NumberStyles.HexNumber, culture) : default(int?);
        }
    }
}