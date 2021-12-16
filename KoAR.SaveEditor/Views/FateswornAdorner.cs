using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class FateswornAdorner : Adorner, IDisposable
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(nameof(Adorner), typeof(FateswornAdorner), typeof(FateswornAdorner));
        private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();
        private static readonly Pen _whitePen = FateswornAdorner.CreateWhitePen();

        public static readonly DependencyProperty RequiresFateswornProperty = DependencyProperty.RegisterAttached(nameof(IDefinition.RequiresFatesworn), typeof(bool), typeof(FateswornAdorner),
            new PropertyMetadata(BooleanBoxes.False, FateswornAdorner.RequiresFateswornProperty_ValueChanged));

        private readonly AdornerLayer _adornerLayer;

        private FateswornAdorner(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            (this._adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement)).Add(this);
            this.IsHitTestVisible = false;
            BindingOperations.SetBinding(this, UIElement.VisibilityProperty, new Binding
            { 
                Path = new(UIElement.IsVisibleProperty),
                Source = this.AdornedElement, 
                Converter = FateswornAdorner._booleanToVisibilityConverter,
            });
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
                FateswornAdorner.AttachAdorner(element);
            }
            else
            {
                FateswornAdorner.DetachAdorner(element);
            }
        }

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

        private static Pen CreateWhitePen()
        {
            Pen pen = new(Brushes.White, 0.5);
            pen.Freeze();
            return pen;
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

        public void Dispose()
        {
            BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
            this._adornerLayer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const double radius = 6;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement);
            drawingContext.DrawEllipse(
                Brushes.MediumPurple,
                FateswornAdorner._whitePen,
                new(bounds.Width - radius, bounds.Height - radius),
                radius - FateswornAdorner._whitePen.Thickness,
                radius - FateswornAdorner._whitePen.Thickness
            );
            FormattedText formattedText = new(
                "F",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new(((Control)PresentationSource.FromVisual(this.AdornedElement).RootVisual).FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                10,
                Brushes.White,
                1d
            );
            drawingContext.DrawText(
                formattedText,
                new(bounds.Width - radius - formattedText.Width * 0.5, bounds.Height - radius - formattedText.Height * 0.5)
            );
            base.OnRender(drawingContext);
        }
    }
}
