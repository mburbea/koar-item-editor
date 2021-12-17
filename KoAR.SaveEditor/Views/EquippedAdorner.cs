using KoAR.SaveEditor.Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace KoAR.SaveEditor.Views;

internal class EquippedAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty IsEquippedProperty = DependencyProperty.RegisterAttached(nameof(ItemModelBase.IsEquipped), typeof(bool), typeof(EquippedAdorner),
        new PropertyMetadata(BooleanBoxes.False, EquippedAdorner.IsEquippedProperty_ValueChanged));

    private EquippedAdorner(FrameworkElement adornedElement)
        : base(adornedElement, AdornerPosition.LowerLeft, background: Brushes.LimeGreen, foreground: Brushes.White, "E") => this.ToolTip = "Equipped";

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
