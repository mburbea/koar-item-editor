using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace KoAR.SaveEditor.Constructs
{
    [ContentProperty(nameof(CompositeConverter.Converters))]
    public sealed class CompositeConverter : IValueConverter
    {
        public Collection<IValueConverter> Converters { get; } = new();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int index = 0; index < this.Converters.Count; index++)
            {
                value = this.Converters[index].Convert(value, index != this.Converters.Count - 1 ? typeof(object) : targetType, parameter, culture);
            }
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int index = this.Converters.Count - 1; index != -1; index--)
            {
                value = this.Converters[index].ConvertBack(value, index != 0 ? typeof(object) : targetType, parameter, culture);
            }
            return value;
        }
    }
}
