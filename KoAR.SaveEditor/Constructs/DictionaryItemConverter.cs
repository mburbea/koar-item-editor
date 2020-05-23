using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class DictionaryItemConverter : IMultiValueConverter
    {
        object? IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object key;
            return values != null && values.Length >= 2 && values[0] is IDictionary dictionary && (key = values[1]) != null && dictionary.Contains(key)
                ? dictionary[key]
                : null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
