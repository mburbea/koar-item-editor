using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KoAR.SaveEditor.Views;

public abstract class IndicatorAdornerBase : Adorner, IDisposable
{
    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();

    private readonly AdornerPosition _adornerPosition;
    private readonly Brush _background;
    private readonly Brush _foreground;
    private readonly string _indicator;
    private readonly Pen _stroke;
    private readonly Viewbox _viewbox;
    private AdornerLayer? _adornerLayer;

    protected IndicatorAdornerBase(FrameworkElement adornedElement, AdornerPosition adornerPosition, Brush background, Brush foreground, string indicator)
        : base(adornedElement)
    {
        this._adornerPosition = adornerPosition;

        (int row, int column) = adornerPosition switch
        {
            AdornerPosition.UpperLeft => (0, 0),
            AdornerPosition.UpperRight => (0, 1),
            AdornerPosition.LowerLeft => (1, 0),
            AdornerPosition.LowerRight => (1, 1),
            _ => (0, 0),
        };


        Grid grid = new()
        {
            RowDefinitions = { new() { Height = GridLength.Auto }, new() { Height = GridLength.Auto } },
            ColumnDefinitions = { new() { Width = GridLength.Auto }, new() { Width = GridLength.Auto } },
        };
        Ellipse ellipse = new()
        {
            Fill = background,
            Stroke = foreground,
            StrokeThickness = Constants.StrokeThickness,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        Grid.SetRow(ellipse, row);
        Grid.SetColumn(ellipse, column);
        grid.Children.Add(ellipse);
        TextBlock textBlock = new()
        {
            Text = indicator,
            Foreground = foreground,
            FontSize = Constants.FontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetRow(textBlock, row);
        Grid.SetColumn(textBlock, column);
        grid.Children.Add(textBlock);
        this._viewbox = new()
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            Child = grid,
        };
        //this._background = background;
        //this._foreground = foreground;
        //this._indicator = indicator;
        //if (foreground is not SolidColorBrush { Color: Color color })
        //{
        //    this._stroke = IndicatorAdornerBase.CreateFrozenPen(foreground);
        //}
        //else if (IndicatorAdornerBase._penCache.TryGetValue(color, out Pen? pen))
        //{
        //    this._stroke = pen;
        //}
        //else
        //{
        //    IndicatorAdornerBase._penCache.Add(color, this._stroke = IndicatorAdornerBase.CreateFrozenPen(foreground));
        //}
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

    protected override Size ArrangeOverride(Size finalSize)
    {
        this._viewbox.Arrange(new(finalSize));
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        this._viewbox.Measure(this.AdornedElement.RenderSize);
        return this._viewbox.RenderSize;
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => this._viewbox;

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

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    protected static void SetAdorner<TAdorner>(FrameworkElement element, TAdorner adorner)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.SetAdorner(element, adorner);

    /*
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
    */

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
        this.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(UIElement.IsVisible))
        {
            Source = this.AdornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
    }

    private void RemoveLoadedListener() => this.AdornedElement.Loaded -= this.AdornedElement_Loaded;

    private static class AdornerAttacher<TAdorner>
        where TAdorner : IndicatorAdornerBase
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(typeof(TAdorner).FullName, typeof(TAdorner), typeof(AdornerAttacher<TAdorner>));

        public static void DetachAdorner(FrameworkElement element)
        {
            using TAdorner? adorner = (TAdorner?)element.GetValue(AdornerAttacher<TAdorner>._adornerProperty);
            element.ClearValue(AdornerAttacher<TAdorner>._adornerProperty);
        }

        public static void SetAdorner(FrameworkElement element, TAdorner adorner) => element.SetValue(AdornerAttacher<TAdorner>._adornerProperty, adorner);
    }

    private static class Constants
    {
        public const double Dimension = 24;
        public const double FontSize = 10;
        public const double Radius = 6;
        public const double StrokeThickness = 0.5;
    }
}