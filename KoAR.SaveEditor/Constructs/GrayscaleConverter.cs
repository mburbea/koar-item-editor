using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class GrayscaleConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is BitmapSource source ? GrayscaleConverter.Convert(source) : DependencyProperty.UnsetValue;
        }

        private static BitmapSource Convert(BitmapSource source)
        {
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap();
            bitmap.BeginInit();
            bitmap.Source = source;
            bitmap.DestinationFormat = PixelFormats.Gray32Float;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
