using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Constructs;

internal sealed class RequiresFateswornConverter: IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is IDefinition { RequiresFatesworn: true } ? BooleanBoxes.True : BooleanBoxes.False;

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
