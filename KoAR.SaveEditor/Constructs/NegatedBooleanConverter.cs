using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class NegatedBooleanConverter : IValueConverter
    {
        object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => NegatedBooleanConverter.Convert(value);

        object? IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => NegatedBooleanConverter.Convert(value);

        private static object? Convert(object value) => value switch
        {
            null => null,
            bool boolean => BooleanBoxes.GetBox(!boolean),
            _ => DependencyProperty.UnsetValue,
        };
    }
}
