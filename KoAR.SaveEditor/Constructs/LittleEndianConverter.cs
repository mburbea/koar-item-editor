using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class LittleEndianConverter : IValueConverter
    {
        public static string Convert(uint value, IFormatProvider? provider = default)
        {
            const uint magicNumber = 0xFF00FFu;
            return ((value = value >> 16 | value << 16) >> 8 & magicNumber | (value & magicNumber) << 8).ToString("X8", provider);
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is uint number ? LittleEndianConverter.Convert(number, culture) : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
