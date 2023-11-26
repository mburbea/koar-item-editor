using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class IsTypeConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => BooleanBoxes.GetBox(parameter is Type type && type.IsInstanceOfType(value));

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
