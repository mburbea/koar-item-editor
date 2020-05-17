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

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => NegatedBooleanConverter.Convert(value);

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => NegatedBooleanConverter.Convert(value);

        private static object Convert(object value)
        {
            if (value is bool boolean)
            {
                return boolean ? NegatedBooleanConverter._false : NegatedBooleanConverter._true;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
