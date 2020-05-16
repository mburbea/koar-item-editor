using System;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class SingleConverter : IValueConverter
    {
        object IValueConverter.Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Convert.ToString(value, culture) : string.Empty;
        }

        object? IValueConverter.ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? default(float?) : Convert.ToSingle(value.ToString().Trim().TrimEnd('.'));
        }
    }
}
