using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views;

public sealed class TitleCaseWordsConverter : IValueConverter
{
    private static readonly char[] _allCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    object IValueConverter.Convert([AllowNull] object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?.ToString() is not string text)
        {
            return DependencyProperty.UnsetValue;
        }
        return text.IndexOfAny(TitleCaseWordsConverter._allCaps, 1) is int index and not -1
            ? $"{text[..index]} {text[index..]}"
            : text;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
