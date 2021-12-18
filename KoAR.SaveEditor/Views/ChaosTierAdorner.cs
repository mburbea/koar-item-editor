using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using KoAR.Core;

namespace KoAR.SaveEditor.Views;

public sealed class ChaosTierAdorner : IndicatorAdornerBase
{
    private static readonly Dictionary<string, Func<FrameworkElement, ChaosTierAdorner>> _factories = new();

    public static readonly DependencyProperty ChaosTierProperty = DependencyProperty.RegisterAttached(nameof(ItemDefinition.ChaosTier), typeof(string), typeof(ChaosTierAdorner),
        new PropertyMetadata(null, ChaosTierAdorner.ChaosTierProperty_ValueChanged));

    private ChaosTierAdorner(FrameworkElement adornedElement, string chaosTier)
        : base(adornedElement, AdornerPosition.UpperRight, background: Brushes.CadetBlue, foreground: Brushes.White, chaosTier) => this.IsHitTestVisible = false;

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
            if (ChaosTierAdorner._factories.GetValueOrDefault(tier) is not { } factory)
            {
                ChaosTierAdorner._factories.Add(tier, factory = element => new(element, tier));
            }
            IndicatorAdornerBase.AttachAdorner(element, factory);
        }
        else
        {
            IndicatorAdornerBase.DetachAdorner<ChaosTierAdorner>(element);
        }
    }
}