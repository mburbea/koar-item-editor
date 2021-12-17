using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace KoAR.SaveEditor.Views;

public abstract class IndicatorAdornerBase : Adorner, IDisposable
{
    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();
    private static readonly Dictionary<Color, Pen> _penCache = new();
    private static readonly Dictionary<(double, double), ScaleTransform> _scaleTransformCache = new();

    private readonly AdornerPosition _adornerPosition;
    private readonly Brush _background;
    private readonly Brush _foreground;
    private readonly string _indicator;
    private readonly Pen _stroke;
    private AdornerLayer? _adornerLayer;

    protected IndicatorAdornerBase(FrameworkElement adornedElement, AdornerPosition adornerPosition, Brush background, Brush foreground, string indicator)
        : base(adornedElement)
    {
        this._adornerPosition = adornerPosition;
        this._background = background;
        this._foreground = foreground;
        this._indicator = indicator;
        if (foreground is not SolidColorBrush { Color: Color color })
        {
            this._stroke = IndicatorAdornerBase.CreateFrozenPen(foreground);
        }
        else if (IndicatorAdornerBase._penCache.TryGetValue(color, out Pen? pen))
        {
            this._stroke = pen;
        }
        else
        {
            IndicatorAdornerBase._penCache.Add(color, this._stroke = IndicatorAdornerBase.CreateFrozenPen(foreground));
        }
        this.AttachToAdornedElement();
    }

    protected enum AdornerPosition
    {
        LowerLeft,
        LowerRight,
        UpperLeft,
        UpperRight,
    }

    public new FrameworkElement AdornedElement => (FrameworkElement)base.AdornedElement;

    public virtual void Dispose()
    {
        if (this._adornerLayer != null)
        {
            BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
            this._adornerLayer.Remove(this);
        }
        else
        {
            this.RemoveLoadedListener();
        }
        GC.SuppressFinalize(this);
    }

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
        Rect bounds;
        if (!this.AdornedElement.IsVisible || (bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement)).Width == Constants.Dimension && bounds.Height == Constants.Dimension)
        {
            return base.GetDesiredTransform(transform);
        }
        return new GeneralTransformGroup
        {
            Children =
            {
                IndicatorAdornerBase.GetScaleTransform(bounds.Width, bounds.Height),
                base.GetDesiredTransform(transform)
            }
        };
    }

    protected static void AttachAdorner<TAdorner>(FrameworkElement element, TAdorner adorner)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.AttachAdorner(element, adorner);

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    protected override void OnRender(DrawingContext drawingContext)
    {
        double centerX = this._adornerPosition is AdornerPosition.LowerRight or AdornerPosition.UpperRight
            ? Constants.Dimension - Constants.Radius
            : Constants.Radius;
        double centerY = this._adornerPosition is AdornerPosition.LowerRight or AdornerPosition.LowerLeft
            ? Constants.Dimension - Constants.Radius
            : Constants.Radius;
        drawingContext.DrawEllipse(
            this._background,
            this._stroke,
            new(centerX, centerY),
            Constants.Radius - Constants.StrokeThickness,
            Constants.Radius - Constants.StrokeThickness
        );
        FormattedText formattedText = new(
            this._indicator,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new(Window.GetWindow(this.AdornedElement).FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            Constants.FontSize,
            this._foreground,
            1d
        );
        drawingContext.DrawText(
            formattedText,
            new(centerX - formattedText.Width * 0.5, centerY - formattedText.Height * 0.5)
        );
        base.OnRender(drawingContext);
    }

    private static Pen CreateFrozenPen(Brush brush) => IndicatorAdornerBase.Freeze(new Pen(brush, Constants.StrokeThickness));

    private static TFreezable Freeze<TFreezable>(TFreezable freezable)
        where TFreezable : Freezable
    {
        freezable.Freeze();
        return freezable;
    }

    private static ScaleTransform GetScaleTransform(double width, double height)
    {
        if (!IndicatorAdornerBase._scaleTransformCache.TryGetValue((width, height), out ScaleTransform? transform))
        {
            IndicatorAdornerBase._scaleTransformCache.Add(
                (width, height),
                transform = IndicatorAdornerBase.Freeze(new ScaleTransform(width / Constants.Dimension, height / Constants.Dimension))
            );
        }
        return transform;
    }

    private void AdornedElement_Loaded(object sender, RoutedEventArgs e)
    {
        this.RemoveLoadedListener();
        this.AttachToAdornedElement();
    }

    private void AttachToAdornedElement()
    {
        if (!this.AdornedElement.IsLoaded)
        {
            this.AdornedElement.Loaded += this.AdornedElement_Loaded;
            return;
        }
        (this._adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement)).Add(this);
        BindingOperations.SetBinding(this, UIElement.VisibilityProperty, new Binding
        {
            Path = new(UIElement.IsVisibleProperty),
            Source = this.AdornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
    }

    private void RemoveLoadedListener() => this.AdornedElement.Loaded -= this.AdornedElement_Loaded;

    private static class AdornerAttacher<TAdorner>
        where TAdorner : IndicatorAdornerBase
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(typeof(TAdorner).FullName, typeof(TAdorner), typeof(AdornerAttacher<TAdorner>));

        public static void AttachAdorner(FrameworkElement element, TAdorner adorner) => element.SetValue(AdornerAttacher<TAdorner>._adornerProperty, adorner);

        public static void DetachAdorner(FrameworkElement element)
        {
            using TAdorner? adorner = (TAdorner?)element.GetValue(AdornerAttacher<TAdorner>._adornerProperty);
            element.ClearValue(AdornerAttacher<TAdorner>._adornerProperty);
        }
    }

    private static class Constants
    {
        public const double Dimension = 24;
        public const double FontSize = 10;
        public const double Radius = 6;
        public const double StrokeThickness = 0.5;
    }
}