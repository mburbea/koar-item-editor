using System;
using System.Globalization;
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
            string text = value.ToString();
            int space = -1;
            for (int index = 0; index < text.Length; index++)
            {
                if (Char.IsUpper(text, index))
                {
                    space = index;
                    break;
                }
            }
            return space == -1 ? text : $"{text.Substring(0, space)} {text.Substring(space)}";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}