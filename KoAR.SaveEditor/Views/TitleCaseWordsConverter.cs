using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views
{
    public sealed class TitleCaseWordsConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return DependencyProperty.UnsetValue;
            }
            string word = value.ToString();
            return word.Skip(1).Aggregate(new StringBuilder(word.Length + 1).Append(word[0]), (builder, c) =>
            {
                if (Char.IsUpper(c))
                {
                    builder.Append(' ');
                }
                return builder.Append(c);
            }).ToString();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
