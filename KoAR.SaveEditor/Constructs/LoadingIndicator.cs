using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace KoAR.SaveEditor.Constructs;

public sealed class LoadingIndicator : Control
{
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(LoadingIndicator.IsLoading), typeof(bool), typeof(LoadingIndicator));

    public static readonly IMultiValueConverter LocationConverter = new CircleLocationConverter();

    public static readonly IValueConverter NumericDivisionConverter = new DivisionConverter();

    public static readonly IValueConverter OpacityConverter = new CircleOpacityConverter();

    static LoadingIndicator() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingIndicator), new FrameworkPropertyMetadata(typeof(LoadingIndicator)));

    public bool IsLoading
    {
        get => (bool)this.GetValue(LoadingIndicator.IsLoadingProperty);
        set => this.SetValue(LoadingIndicator.IsLoadingProperty, BooleanBoxes.GetBox(value));
    }

    private static double ParseName(string name, CultureInfo culture) => double.Parse(name[6..], NumberStyles.Float, culture);

    private sealed class CircleLocationConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not DependencyProperty property || values is not [double width, string name])
            {
                return DependencyProperty.UnsetValue;
            }
            Func<double, double> function = property == Canvas.TopProperty ? Math.Cos : Math.Sin;
            return width * (1d + function(Math.PI + LoadingIndicator.ParseName(name, culture) * Math.PI / 5d)) * 2.5;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    private sealed class CircleOpacityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not string name
            ? DependencyProperty.UnsetValue
            : 1d - LoadingIndicator.ParseName(name, culture) / 10d;

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    private sealed class DivisionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is double number && parameter is double divisor
            ? number / divisor
            : DependencyProperty.UnsetValue;

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

public sealed class LoadingEllipseLocationExtension : MarkupExtension
{
    private static readonly Binding _nameBinding = new()
    {
        Path = new(FrameworkElement.NameProperty),
        RelativeSource = RelativeSource.Self,
    };
    private static readonly Binding _tagBinding = new()
    {
        Path = new(Control.TagProperty),
        RelativeSource = new() { AncestorType = typeof(Canvas) }
    };

    public override object ProvideValue(IServiceProvider serviceProvider) => new MultiBinding()
    {
        Converter = LoadingIndicator.LocationConverter,
        ConverterParameter = ((IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!).TargetProperty,
        Bindings =
        {
            LoadingEllipseLocationExtension._tagBinding,
            LoadingEllipseLocationExtension._nameBinding,
        }
    };
}
