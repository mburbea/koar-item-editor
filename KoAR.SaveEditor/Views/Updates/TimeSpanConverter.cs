using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class TimeSpanConverter : IMultiValueConverter, IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is TimeSpan timeSpan
                ? TimeSpanConverter.GetDescriptiveText(timeSpan)
                : DependencyProperty.UnsetValue;
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Length >= 2 && values[0] is DateTime left && values[1] is DateTime right
                ? TimeSpanConverter.GetDescriptiveText(left - right)
                : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static string GetDescriptiveText(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 365d)
            {
                int years = (int)Math.Floor(timeSpan.TotalDays / 365d);
                return years > 1 ? $"{years} years" : "A year";
            }
            if (timeSpan.TotalDays >= 30d)
            {
                int months = (int)Math.Floor(timeSpan.TotalDays / 30d);
                return months > 1 ? $"{months} months" : "A month";
            }
            if (timeSpan.TotalDays >= 1d)
            {
                int days = (int)Math.Floor(timeSpan.TotalDays);
                return days > 1 ? $"{days} days" : "A day";
            }
            if (timeSpan.TotalHours >= 1d)
            {
                int hours = (int)Math.Floor(timeSpan.TotalHours);
                return hours > 1 ? $"{hours} hours" : "An hour";
            }
            if (timeSpan.TotalMinutes >= 1d)
            {
                int minutes = (int)Math.Floor(timeSpan.TotalMinutes);
                return minutes > 1 ? $"{minutes} minutes" : "A minute";
            }
            return "Seconds";
        }
    }
}
