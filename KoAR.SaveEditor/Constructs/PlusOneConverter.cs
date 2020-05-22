using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class PlusOneConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int integer ? integer + 1 : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int integer ? integer - 1 : DependencyProperty.UnsetValue;
        }
    }
}
