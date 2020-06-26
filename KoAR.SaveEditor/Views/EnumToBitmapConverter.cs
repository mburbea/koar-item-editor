using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KoAR.SaveEditor.Views
{
    public sealed class EnumToBitmapConverter : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> _dictionary = new Dictionary<string, BitmapImage>();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !value.GetType().IsEnum)
            {
                return DependencyProperty.UnsetValue;
            }
            string uriString = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Resources/{value}.png";
            if (!EnumToBitmapConverter._dictionary.TryGetValue(uriString, out BitmapImage image))
            {
                try
                {
                    image = new BitmapImage(new Uri(uriString));
                }
                catch (Exception)
                {
                    image = new BitmapImage();
                }
                image.Freeze();
                EnumToBitmapConverter._dictionary.Add(uriString, image);
            }
            return image;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
