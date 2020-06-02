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
            int index = 1;
            for (; index < text.Length; index++)
            {
                if (char.IsUpper(text, index))
                {
                    break;
                }
            }
            return index==text.Length ? text : $"{text.Substring(0, index)} {text.Substring(index)}";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}