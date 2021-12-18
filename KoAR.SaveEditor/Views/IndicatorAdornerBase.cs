using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KoAR.SaveEditor.Views;

public abstract class IndicatorAdornerBase : Adorner, IDisposable
{
    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();

    private readonly Border _contentPresenter;
    private AdornerLayer? _adornerLayer;

    protected IndicatorAdornerBase(FrameworkElement adornedElement, AdornerPosition adornerPosition, Brush background, Brush foreground, string indicator)
        : base(adornedElement)
    {
        FrameworkElementFactory gridFactory = new(typeof(Grid));
        FrameworkElementFactory ellipseFactory = new(typeof(Ellipse));
        ellipseFactory.SetValue(Shape.FillProperty, background);
        ellipseFactory.SetValue(Shape.StrokeProperty, foreground);
        ellipseFactory.SetValue(Shape.StrokeThicknessProperty, 0.5);
        gridFactory.AppendChild(ellipseFactory);
        FrameworkElementFactory viewBoxFactory = new(typeof(Viewbox));
        viewBoxFactory.SetValue(Viewbox.StretchProperty, Stretch.Uniform);
        viewBoxFactory.SetValue(Viewbox.StretchDirectionProperty, StretchDirection.Both);
        FrameworkElementFactory textBlockFactory = new(typeof(TextBlock));
        textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding { Mode = BindingMode.OneTime });
        textBlockFactory.SetValue(TextBlock.ForegroundProperty, foreground);
        textBlockFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        textBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        viewBoxFactory.AppendChild(textBlockFactory);
        gridFactory.AppendChild(viewBoxFactory);
        FrameworkElementFactory uniformGridFactory = new(typeof(UniformGrid));
        uniformGridFactory.SetValue(UniformGrid.RowsProperty, 2);
        uniformGridFactory.SetValue(UniformGrid.ColumnsProperty, 2);
        for (int i = 0; i < (int)adornerPosition; i++)
        {
            uniformGridFactory.AppendChild(new(typeof(Border)));
        }
        uniformGridFactory.AppendChild(gridFactory);
        this._contentPresenter = new Border
        {
            Background = Brushes.Transparent,
            Child = new ContentPresenter()
            {
                Content = indicator,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ContentTemplate = new() { VisualTree = uniformGridFactory },
            }
        };
        this.AttachToAdornedElement();
    }

    protected enum AdornerPosition
    {
        UpperLeft = 0,
        UpperRight,
        LowerLeft,
        LowerRight,
    }

    public new FrameworkElement AdornedElement => (FrameworkElement)base.AdornedElement;

    protected override int VisualChildrenCount => 1;

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

    protected override Size ArrangeOverride(Size finalSize)
    {
        this._contentPresenter.Arrange(new(finalSize));
        return finalSize;
    }

    protected override Visual GetVisualChild(int index) => this._contentPresenter;

    protected override Size MeasureOverride(Size constraint)
    {
        this._contentPresenter.Measure(this.AdornedElement.RenderSize);
        return this.AdornedElement.RenderSize;
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
}