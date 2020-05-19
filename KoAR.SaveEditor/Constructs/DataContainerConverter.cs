using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class DataContainerConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IEnumerable collection
                ? collection.Cast<object>().Select(item => new DataContainer(item)).ToArray()
                : (object)new DataContainer(value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
