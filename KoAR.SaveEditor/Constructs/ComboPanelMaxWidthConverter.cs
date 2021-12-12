using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class ComboPanelMaxWidthConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double comboBoxWidth && parameter is IConvertible other
            ? Math.Max(comboBoxWidth - 18d, other.ToDouble(culture))
            : DependencyProperty.UnsetValue;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
