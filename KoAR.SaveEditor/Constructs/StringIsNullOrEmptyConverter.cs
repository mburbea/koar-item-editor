using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class StringIsNullOrEmptyConverter : IValueConverter
    {
        object IValueConverter.Convert([AllowNull] object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BooleanBoxes.GetBox(string.IsNullOrEmpty(value as string));
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
