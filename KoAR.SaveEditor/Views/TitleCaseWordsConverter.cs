using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views
{
    public sealed class TitleCaseWordsConverter : IValueConverter
    {
        private static readonly char[] _allCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        object IValueConverter.Convert([AllowNull] object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() is string text
                ? text.IndexOfAny(TitleCaseWordsConverter._allCaps, 1) switch
                  {
                      int index when index != -1 => $"{text.Substring(0, index)} {text.Substring(index)}",
                      _ => text
                  }
                : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}