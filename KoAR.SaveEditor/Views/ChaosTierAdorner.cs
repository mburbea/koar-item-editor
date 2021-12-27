using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KoAR.Core;

namespace KoAR.SaveEditor.Views;

public sealed class ChaosTierAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty ChaosTierProperty = DependencyProperty.RegisterAttached(nameof(ItemDefinition.ChaosTier), typeof(string), typeof(ChaosTierAdorner),
        new PropertyMetadata(null, ChaosTierAdorner.ChaosTierProperty_ValueChanged));
    private static readonly Func<FrameworkElement, ChaosTierAdorner>[] _factories;
    private static readonly DataTemplate[] _contentTemplates = InitializeTemplates(out ChaosTierAdorner._factories);

    private static DataTemplate[] InitializeTemplates(out Func<FrameworkElement, ChaosTierAdorner>[] factories)
    {
        DataTemplate[] templates = new DataTemplate[6];
        factories = new Func<FrameworkElement, ChaosTierAdorner>[6];
        for (char c = 'A'; c < 'G'; c++)
        {
            string tier = c.ToString();
            templates[c - 'A'] = IndicatorAdornerBase.CreateContentTemplate(background: Brushes.CadetBlue, foreground: Brushes.White, tier);
            factories[c - 'A'] = element => new(element, tier);
        }
        return templates;
    }

    private ChaosTierAdorner(FrameworkElement adornedElement, string chaosTier)
        : base(adornedElement, AdornerPosition.UpperRight, ChaosTierAdorner._contentTemplates[chaosTier[0] - 'A']) => this.IsHitTestVisible = false;

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
            IndicatorAdornerBase.AttachAdorner(element, ChaosTierAdorner._factories[tier[0] - 'A']);
        }
        else
        {
            IndicatorAdornerBase.DetachAdorner<ChaosTierAdorner>(element);
        }
    }
}