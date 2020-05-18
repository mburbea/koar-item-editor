using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class NegatedBooleanConverter : IValueConverter
    {
        private static readonly object _false = false;
        private static readonly object _true = true;

        object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => NegatedBooleanConverter.Convert(value);

        object? IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => NegatedBooleanConverter.Convert(value);

        private static object? Convert(object value)
        {
            return value switch
            {
                null => null,
                true => NegatedBooleanConverter._false,
                false => NegatedBooleanConverter._true,
                _ => DependencyProperty.UnsetValue,
            };
        }
    }
}
