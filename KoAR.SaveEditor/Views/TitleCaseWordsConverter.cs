using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views
{
    public sealed class TitleCaseWordsConverter : IValueConverter
    {
        static readonly char[] AllCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                null => DependencyProperty.UnsetValue,
                string text when text.IndexOfAny(TitleCaseWordsConverter.AllCaps, 1) is int index && index != -1 => $"{text.Substring(0, index)} {text.Substring(index)}",
                string text => text
            };
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}