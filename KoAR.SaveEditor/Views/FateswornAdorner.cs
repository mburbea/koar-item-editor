using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class FateswornAdorner : Adorner
    {
        private static readonly Pen _whitePen = FateswornAdorner.CreateWhitePen();

        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(nameof(Adorner), typeof(FateswornAdorner), typeof(FateswornAdorner));

        public static readonly DependencyProperty RequiresFateswornProperty = DependencyProperty.RegisterAttached("RequiresFatesworn", typeof(bool), typeof(FateswornAdorner),
            new PropertyMetadata(BooleanBoxes.False, FateswornAdorner.RequiresFateswornProperty_ValueChanged));

        public FateswornAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.IsHitTestVisible = false;
            (this.AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement)).Add(this);
        }

        public AdornerLayer AdornerLayer { get; }

        public static bool GetRequiresFatesworn(FrameworkElement d) => (bool)d.GetValue(FateswornAdorner.RequiresFateswornProperty);

        public static void SetRequiresFatesworn(FrameworkElement d, bool value) => d.SetValue(FateswornAdorner.RequiresFateswornProperty, BooleanBoxes.GetBox(value));

        private static void RequiresFateswornProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                if (element.IsLoaded)
                {
                    FateswornAdorner.AttachAdorner(element);
                }
                else
                {
                    element.Loaded += FateswornAdorner.Element_Loaded;
                }
            }
            else
            {
                if (element.IsLoaded)
                {
                    FateswornAdorner.DetachAdorner(element);
                }
                else
                {
                    element.Loaded -= FateswornAdorner.Element_Loaded;
                }
            }
        }

        private static void AttachAdorner(FrameworkElement element)
        {
            element.SetValue(FateswornAdorner._adornerProperty, new FateswornAdorner(element));
            element.IsVisibleChanged += FateswornAdorner.Element_IsVisibleChanged;
        }

        private static Pen CreateWhitePen()
        {
            Pen pen = new(Brushes.White, 0.5);
            pen.Freeze();
            return pen;
        }

        private static void DetachAdorner(FrameworkElement element)
        {
            FateswornAdorner? adorner = (FateswornAdorner?)element.GetValue(FateswornAdorner._adornerProperty);
            if (adorner == null)
            {
                return;
            }
            element.IsVisibleChanged -= FateswornAdorner.Element_IsVisibleChanged;
            adorner.AdornerLayer.Remove(adorner);
            element.ClearValue(FateswornAdorner._adornerProperty);
        }

        private static void Element_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FateswornAdorner adorner = (FateswornAdorner)((DependencyObject)sender).GetValue(FateswornAdorner._adornerProperty);
            adorner.OnVisibilityChanged();
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            FateswornAdorner.AttachAdorner(element);
            element.Loaded -= FateswornAdorner.Element_Loaded;
        }

        private void OnVisibilityChanged()
        {
            this.Visibility = this.AdornedElement.IsVisible ? Visibility.Visible : Visibility.Hidden;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const double radius = 6;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement);
            drawingContext.DrawEllipse(
                Brushes.MediumPurple,
                FateswornAdorner._whitePen,
                new(bounds.Width - radius, bounds.Height - radius),
                radius,
                radius
            );
            drawingContext.DrawText(
                new("F", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new("Segoe UI"), 10, Brushes.White, 1.25),
                new(bounds.Width - radius - 2, bounds.Height - radius - 4)
            );
            base.OnRender(drawingContext);
        }
    }
}
