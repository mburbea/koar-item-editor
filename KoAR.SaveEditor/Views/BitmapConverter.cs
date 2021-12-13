using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KoAR.SaveEditor.Views;

public sealed class BitmapConverter : IValueConverter, IMultiValueConverter
{
    private static readonly Dictionary<string, BitmapImage> _bitmaps = BitmapConverter.DiscoverBitmaps();
    private static readonly BitmapImage _fallback = BitmapConverter.Freeze(new());

    object IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null && BitmapConverter._bitmaps.TryGetValue(value.ToString()!, out BitmapImage? bitmap)
            ? bitmap
            : BitmapConverter.GetFallback(parameter);
    }

    object IMultiValueConverter.Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values != null)
        {
            List<string> list = new(values.Length);
            foreach (object? value in values)
            {
                if (value == null || value is Enum && value.GetHashCode() == 0)
                {
                    // This tests for null and All/None in the various enumerations.
                    continue;
                }
                string text = value.ToString()!;
                list.Add(list.Count == 0 ? text : $"{list.Last()}_{text}");
            }
            for (int index = list.Count - 1; index != -1; index--)
            {
                string text = list[index];
                if (BitmapConverter._bitmaps.TryGetValue(text, out BitmapImage? bitmap))
                {
                    return bitmap;
                }
            }
        }
        return BitmapConverter.GetFallback(parameter);
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

    private static Dictionary<string, BitmapImage> DiscoverBitmaps()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        ResourceManager manager = new($"{assembly.GetName().Name}.g", assembly);
        return manager.GetResourceSet(CultureInfo.InvariantCulture, true, true)!
            .Cast<DictionaryEntry>()
            .Select(entry => (string)entry.Key)
            .Where(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                name => Path.GetFileNameWithoutExtension(name)!,
                name => BitmapConverter.Freeze(new(new($"pack://application:,,,/{assembly.GetName().Name};component/{name}"))),
                StringComparer.OrdinalIgnoreCase
            );
    }

    private static BitmapImage Freeze(BitmapImage image)
    {
        image.Freeze();
        return image;
    }

    private static BitmapImage GetFallback(object? parameter)
    {
        return parameter is string name && BitmapConverter._bitmaps.TryGetValue(name, out BitmapImage? bitmap)
            ? bitmap
            : BitmapConverter._fallback;
    }
}
