﻿using System.Windows;
using System.Windows.Media;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public sealed class EquippedAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty IsEquippedProperty = DependencyProperty.RegisterAttached(nameof(ItemModelBase.IsEquipped), typeof(bool), typeof(EquippedAdorner),
        new PropertyMetadata(BooleanBoxes.False, EquippedAdorner.IsEquippedProperty_ValueChanged));

    public EquippedAdorner(FrameworkElement adornedElement)
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
