using System.Windows;
using System.Windows.Media;
using KoAR.Core;

namespace KoAR.SaveEditor.Views;

internal class ChaosTierAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty ChaosTierProperty = DependencyProperty.RegisterAttached(nameof(ItemDefinition.ChaosTier), typeof(char?), typeof(ChaosTierAdorner),
        new PropertyMetadata(null, ChaosTierAdorner.ChaosTierProperty_ValueChanged));

    private ChaosTierAdorner(FrameworkElement adornedElement, char chaosTier)
        : base(adornedElement, AdornerPosition.UpperRight, background: Brushes.CadetBlue, foreground: Brushes.White, chaosTier.ToString()) => this.IsHitTestVisible = false;

    public static char? GetChaosTier(FrameworkElement element) => (char?)element.GetValue(ChaosTierAdorner.ChaosTierProperty);

    public static void SetChaosTier(FrameworkElement element, char? value) => element.SetValue(ChaosTierAdorner.ChaosTierProperty, value);

    private static void ChaosTierProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }
        if (e.NewValue is char tier)
        {
            IndicatorAdornerBase.SetAdorner(element, new ChaosTierAdorner(element, tier));
        }
        else
        {
            IndicatorAdornerBase.DetachAdorner<ChaosTierAdorner>(element);
        }
    }
}
