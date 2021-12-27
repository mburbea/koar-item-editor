using System.Windows;
using System.Windows.Media;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public sealed class FateswornAdorner : IndicatorAdornerBase
{
    public static readonly DependencyProperty RequiresFateswornProperty = DependencyProperty.RegisterAttached(nameof(IDefinition.RequiresFatesworn), typeof(bool), typeof(FateswornAdorner),
        new PropertyMetadata(BooleanBoxes.False, FateswornAdorner.RequiresFateswornProperty_ValueChanged));

    private static readonly DataTemplate _contentTemplate = IndicatorAdornerBase.CreateContentTemplate(background: Brushes.MediumPurple, foreground: Brushes.White, "F");

    private FateswornAdorner(FrameworkElement adornedElement)
        : base(adornedElement, AdornerPosition.LowerRight, FateswornAdorner._contentTemplate) => this.ToolTip = "Fatesworn";

    public static bool GetRequiresFatesworn(FrameworkElement element) => (bool)element.GetValue(FateswornAdorner.RequiresFateswornProperty);

    public static void SetRequiresFatesworn(FrameworkElement element, bool value) => element.SetValue(FateswornAdorner.RequiresFateswornProperty, BooleanBoxes.GetBox(value));

    private static void RequiresFateswornProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }
        if ((bool)e.NewValue)
        {
            IndicatorAdornerBase.AttachAdorner<FateswornAdorner>(element);
        }
        else
        {
            IndicatorAdornerBase.DetachAdorner<FateswornAdorner>(element);
        }
    }
}