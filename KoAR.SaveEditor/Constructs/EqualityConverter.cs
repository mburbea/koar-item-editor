using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class EqualityConverter : IMultiValueConverter, IValueConverter
{
    object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values is [var x, var y, .. _] ? BooleanBoxes.GetBox(Object.Equals(x, y)) : DependencyProperty.UnsetValue;
    }

    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BooleanBoxes.GetBox(object.Equals(value, parameter));
    }

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
