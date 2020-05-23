using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class ComparisonConverter : IMultiValueConverter
    {
        private static readonly object _zero = 0;
        private static readonly object _one = 1;
        private static readonly object _negativeOne = -1;

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values == null || values.Length < 2 ? DependencyProperty.UnsetValue : Math.Sign(Comparer.Default.Compare(values[0], values[1])) switch
            {
                1 => ComparisonConverter._one,
                -1 => ComparisonConverter._negativeOne,
                _ => ComparisonConverter._zero
            };
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
