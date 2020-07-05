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
        public Collection<IValueConverter> Converters { get; } = new Collection<IValueConverter>();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int index = 0; index < this.Converters.Count; index++)
            {
                IValueConverter converter = this.Converters[index];
                value = converter.Convert(value, index == this.Converters.Count - 1 ? targetType : typeof(object), parameter, culture);
            }
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int index = this.Converters.Count - 1; index != -1; index--)
            {
                IValueConverter converter = this.Converters[index];
                value = converter.Convert(value, index == 0 ? targetType : typeof(object), parameter, culture);
            }
            return value;
        }
    }
}
