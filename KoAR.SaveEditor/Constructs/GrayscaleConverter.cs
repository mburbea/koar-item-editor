using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KoAR.SaveEditor.Constructs;

public sealed class GrayscaleConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is BitmapSource source
            ? GrayscaleConverter.Convert(source)
            : DependencyProperty.UnsetValue;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    private static FormatConvertedBitmap Convert(BitmapSource source)
    {
        FormatConvertedBitmap bitmap = new();
        bitmap.BeginInit();
        bitmap.Source = source;
        bitmap.DestinationFormat = PixelFormats.Gray8;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
