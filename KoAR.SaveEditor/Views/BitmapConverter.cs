using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KoAR.SaveEditor.Views
{
    public sealed class BitmapConverter : IValueConverter, IMultiValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> _bitmaps = new Dictionary<string, BitmapImage>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly BitmapImage _fallback = BitmapConverter.CreateFallback();
        private static readonly Dictionary<string, Uri> _uris = BitmapConverter.DiscoverUris();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? DependencyProperty.UnsetValue : BitmapConverter.GetBitmap(value.ToString(), true)!;
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                string[] joins = new string[values.Length];
                for (int index = 0; index < values.Length; index++)
                {
                    string text = values[index]?.ToString() ?? string.Empty;
                    joins[index] = index == 0 ? text : $"{joins[index - 1]}_{text}";
                }
                for (int index = values.Length - 1; index != -1; index--)
                {
                    string text = joins[index];
                    BitmapImage? bitmap = BitmapConverter.GetBitmap(text, fallback: index == 0);
                    if (bitmap != null)
                    {
                        return bitmap;
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static BitmapImage CreateFallback()
        {
            BitmapImage image = new BitmapImage();
            image.Freeze();
            return image;
        }

        private static Dictionary<string, Uri> DiscoverUris()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            ResourceManager manager = new ResourceManager($"{assembly.GetName().Name}.g", assembly);
            return manager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
                .Cast<DictionaryEntry>()
                .Select(entry => (string)entry.Key)
                .Where(name => name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    Path.GetFileNameWithoutExtension,
                    name => new Uri($"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/{name}"),
                    StringComparer.InvariantCultureIgnoreCase
                );
        }

        private static BitmapImage? GetBitmap(string text, bool fallback = false)
        {
            if (!BitmapConverter._bitmaps.TryGetValue(text, out BitmapImage? bitmap))
            {
                if (BitmapConverter._uris.TryGetValue(text, out Uri? uri))
                {
                    (bitmap = new BitmapImage(uri)).Freeze();
                    BitmapConverter._bitmaps.Add(text, bitmap);
                }
                else if (fallback)
                {
                    BitmapConverter._bitmaps.Add(text, bitmap = BitmapConverter._fallback);
                }
            }
            return bitmap;
        }
    }
}
