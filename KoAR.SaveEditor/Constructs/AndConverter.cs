using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class AndConverter : IMultiValueConverter
{
    private static readonly Func<bool, bool> _isTrue = x => x;

    object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => BooleanBoxes.GetBox(values.OfType<bool>().All(_isTrue));

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
