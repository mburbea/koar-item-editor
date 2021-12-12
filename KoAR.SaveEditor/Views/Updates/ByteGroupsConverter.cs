using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views.Updates;

public sealed class ByteGroupsConverter : IValueConverter
{
    private static readonly string[] _groups = { "B", "KB", "MB", "GB", "TB" };

    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IConvertible convertible)
        {
            // Converter receives both Int32 values and Double values.
            return DependencyProperty.UnsetValue;
        }
        double number = convertible.ToDouble(culture);
        foreach (string group in ByteGroupsConverter._groups)
        {
            if (number < 1024d)
            {
                return $"{number:#,##0.##} {group}";
            }
            number /= 1024;
        }
        return $"{convertible} B";
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
