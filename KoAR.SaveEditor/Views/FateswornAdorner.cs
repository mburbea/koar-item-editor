using System.Windows;
using System.Windows.Media;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public sealed class FateswornAdorner : IndicatorAdornerBase
{
    private const double _radius = 6d;

    public static readonly DependencyProperty RequiresFateswornProperty = DependencyProperty.RegisterAttached(nameof(IDefinition.RequiresFatesworn), typeof(bool), typeof(FateswornAdorner),
        new PropertyMetadata(BooleanBoxes.False, FateswornAdorner.RequiresFateswornProperty_ValueChanged));

    private FateswornAdorner(FrameworkElement adornedElement)
        : base(adornedElement, background: Brushes.MediumPurple, foreground: Brushes.White, radius: FateswornAdorner._radius, 'F', 10d)
    {
    }

    protected override Point EllipseCenter
    {
        get
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement);
            return new(bounds.Width - FateswornAdorner._radius, bounds.Height - FateswornAdorner._radius);
        }
    }

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
