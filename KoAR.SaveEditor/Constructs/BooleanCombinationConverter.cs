using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public abstract class BooleanCombinationConverter : IMultiValueConverter
{
    private static readonly Func<bool, bool> _isTrue = x => x;

    private readonly Func<IEnumerable<bool>, Func<bool, bool>, bool> _combine;

    protected BooleanCombinationConverter(Func<IEnumerable<bool>, Func<bool, bool>, bool> combine) => this._combine = combine;

    object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => BooleanBoxes.GetBox(this._combine(values.OfType<bool>(), BooleanCombinationConverter._isTrue));

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
