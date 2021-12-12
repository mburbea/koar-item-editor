using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class HexConverter : IValueConverter
{
    object IValueConverter.Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is uint number ? number.ToString("X6", culture) : string.Empty;
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string text && uint.TryParse(text, NumberStyles.HexNumber, culture, out uint result)
            ? result
            : default(uint?);
    }
}
