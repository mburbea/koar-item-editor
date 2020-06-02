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
            int index;
            return value?.ToString() switch
            {
                null => DependencyProperty.UnsetValue,
                string text => (index = text.IndexOfAny(TitleCaseWordsConverter.AllCaps, 1)) != -1 ? $"{text.Substring(0, index)} {text.Substring(index)}" : text
            };
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}