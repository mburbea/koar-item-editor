using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views
{
    public class TitleCaseWordsConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return DependencyProperty.UnsetValue;
            }
            string word = value.ToString();
            int space = -1;
            for (int i = 1; i < word.Length; i++)
            {
                if (char.IsUpper(word, i))
                {
                    space = i;
                    break;
                }
            }
            if (space == -1)
            {
                return word;
            }
            char[] buffer = new char[word.Length + 1];
            word.AsSpan(0, space).CopyTo(buffer);
            buffer[space] = ' ';
            word.AsSpan(space).CopyTo(buffer.AsSpan(space + 1));
            return new string(buffer);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
