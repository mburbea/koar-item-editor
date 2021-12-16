using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class FateswornAdorner : IndicatorAdornerBase
    {
        private const double _radius = 6d;

        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(nameof(Adorner), typeof(FateswornAdorner), typeof(FateswornAdorner));

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

        private static void AttachAdorner(FrameworkElement element)
        {
            if (!element.IsLoaded)
            {
                element.Loaded += FateswornAdorner.Element_Loaded;
            }
            else
            {
                element.SetValue(FateswornAdorner._adornerProperty, new FateswornAdorner(element));
            }
        }

        private static void DetachAdorner(FrameworkElement element)
        {
            using FateswornAdorner? adorner = (FateswornAdorner?)element.GetValue(FateswornAdorner._adornerProperty);
            if (adorner == null)
            {
                element.Loaded -= FateswornAdorner.Element_Loaded;
            }
            else
            {
                element.ClearValue(FateswornAdorner._adornerProperty);
            }
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            FateswornAdorner.AttachAdorner(element);
            element.Loaded -= FateswornAdorner.Element_Loaded;
        }

        private static void RequiresFateswornProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                FateswornAdorner.AttachAdorner(element);
            }
            else
            {
                FateswornAdorner.DetachAdorner(element);
            }
        }
    }
}
