using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public sealed class LoadingIndicator : Control
{
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(LoadingIndicator));

    public static readonly IMultiValueConverter LocationConverter = new CircleLocationConverter();

    public static readonly IValueConverter NumericDivisionConverter = new DivisionConverter();

    public static readonly IValueConverter OpacityConverter = new CircleOpacityConverter();

    static LoadingIndicator() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingIndicator), new FrameworkPropertyMetadata(typeof(LoadingIndicator)));

    public bool IsLoading
    {
        get => (bool)this.GetValue(LoadingIndicator.IsLoadingProperty);
        set => this.SetValue(LoadingIndicator.IsLoadingProperty, BooleanBoxes.GetBox(value));
    }

    private sealed class CircleLocationConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is not { Length: 2 } || parameter is not string text)
            {
                return DependencyProperty.UnsetValue;
            }
            Func<double, double> function = text == "Top" ? Math.Cos : Math.Sin;
            return (1d + function(Math.PI + Convert.ToInt32(values[1], culture) * (Math.PI / 5d))) * Convert.ToDouble(values[0], culture) * 2.5;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    private sealed class CircleOpacityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.TryParse(value.ToString(), NumberStyles.Float, culture, out double index) ? 1d - index / 10d : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    private sealed class DivisionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double number && parameter is IConvertible divisor
                ? number / divisor.ToDouble(culture)
                : DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
