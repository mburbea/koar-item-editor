using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using KoAR.Core;

namespace KoAR.SaveEditor.Views;

public sealed class ChaosTierAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty ChaosTierProperty = DependencyProperty.RegisterAttached(nameof(ItemDefinition.ChaosTier), typeof(string), typeof(ChaosTierAdorner),
        new PropertyMetadata(null, ChaosTierAdorner.ChaosTierProperty_ValueChanged));

    private static readonly Dictionary<string, DataTemplate> _contentTemplates = new();
    private static readonly Dictionary<string, Func<FrameworkElement, ChaosTierAdorner>> _factories = new();

    private ChaosTierAdorner(FrameworkElement adornedElement, string chaosTier)
        : base(adornedElement, AdornerPosition.UpperRight, ChaosTierAdorner._contentTemplates.GetOrAdd(chaosTier, chaosTier => IndicatorAdornerBase.CreateContentTemplate(background: Brushes.CadetBlue, foreground: Brushes.White, chaosTier)))
    {
        this.IsHitTestVisible = false;
    }

    public static string? GetChaosTier(FrameworkElement element) => (string?)element.GetValue(ChaosTierAdorner.ChaosTierProperty);

    public static void SetChaosTier(FrameworkElement element, string? value) => element.SetValue(ChaosTierAdorner.ChaosTierProperty, value);

    private static void ChaosTierProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }
        if (e.NewValue is string tier)
        {
            IndicatorAdornerBase.AttachAdorner(element, ChaosTierAdorner._factories.GetOrAdd(tier, tier => element => new(element, tier)));
        }
        else
        {
            IndicatorAdornerBase.DetachAdorner<ChaosTierAdorner>(element);
        }
    }
}