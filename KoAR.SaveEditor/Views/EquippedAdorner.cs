using System;
using System.Windows;
using System.Windows.Media;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public sealed class EquippedAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty IsEquippedProperty = DependencyProperty.RegisterAttached(nameof(ItemModelBase.IsEquipped), typeof(bool), typeof(EquippedAdorner),
        new(BooleanBoxes.False, EquippedAdorner.IsEquippedProperty_ValueChanged));

    private static readonly DataTemplate _contentTemplate = IndicatorAdornerBase.CreateContentTemplate(background: Brushes.LimeGreen, foreground: Brushes.White, "E");

    [Obsolete($"To be invoked via {nameof(IndicatorAdornerBase.AttachAdorner)} only.")]
    public EquippedAdorner(FrameworkElement adornedElement)
        : base(adornedElement, AdornerPosition.LowerLeft, EquippedAdorner._contentTemplate) => this.ToolTip = "Equipped";

    public static bool GetIsEquipped(FrameworkElement element) => (bool)element.GetValue(EquippedAdorner.IsEquippedProperty);

    public static void SetIsEquipped(FrameworkElement element, bool value) => element.SetValue(EquippedAdorner.IsEquippedProperty, BooleanBoxes.GetBox(value));

    private static void IsEquippedProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }
        if ((bool)e.NewValue)
        {
            IndicatorAdornerBase.AttachAdorner<EquippedAdorner>(element);
        }
        else
        {
            IndicatorAdornerBase.DetachAdorner<EquippedAdorner>(element);
        }
    }
}