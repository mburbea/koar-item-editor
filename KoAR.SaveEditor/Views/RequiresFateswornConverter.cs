using System;
using System.Globalization;
using System.Windows.Data;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public sealed class RequiresFateswornConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => BooleanBoxes.GetBox(value is IDefinition { RequiresFatesworn: true });

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
