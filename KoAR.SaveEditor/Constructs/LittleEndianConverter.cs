using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class LittleEndianConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int number ? LittleEndianConverter.Converter(number) : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static string Converter(int x)
        {
            const int magicNumber = 0xFF00FF;
            return ((x = x >> 16 | x << 16) >> 8 & magicNumber | (x & 0xFF00FF) << 8).ToString("X8");
        }
    }
}
