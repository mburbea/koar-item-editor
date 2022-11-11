using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class AndConverter : IMultiValueConverter
{
    object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => BooleanBoxes.GetBox(Array.TrueForAll(values, value => value is true));

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
