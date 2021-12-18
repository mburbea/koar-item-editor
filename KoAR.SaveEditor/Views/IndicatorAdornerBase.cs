using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public abstract class IndicatorAdornerBase : Adorner, IDisposable
{
    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();

    private readonly UIElement _element;

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
        this._element = new ContentPresenter()
        {
            Content = indicator,
            Opacity = 0.75,
            ContentTemplate = new() { VisualTree = uniformGridFactory },
        };
        this.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(UIElement.IsVisible))
        {
            Source = this.AdornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
        this.ClipToBounds = true;
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
        BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
        GC.SuppressFinalize(this);
    }

    protected static void AttachAdorner<TAdorner>(FrameworkElement element, object? arg = null)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.AttachAdorner(element);

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    protected override Size ArrangeOverride(Size finalSize)
    {
        this._element.Arrange(new(finalSize));
        return finalSize;
    }

    protected override Visual GetVisualChild(int index) => this._element;

    protected override Size MeasureOverride(Size constraint)
    {
        this._element.Measure(this.AdornedElement.RenderSize);
        return this.AdornedElement.RenderSize;
    }

    private static class AdornerAttacher<TAdorner>
        where TAdorner : IndicatorAdornerBase
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(typeof(TAdorner).FullName, typeof(TAdorner), typeof(AdornerAttacher<TAdorner>));
        private static readonly DependencyProperty _factoryProperty = DependencyProperty.RegisterAttached(typeof(Func<TAdorner>).FullName, typeof(Func<TAdorner>), typeof(AdornerAttacher<TAdorner>));

        public static void AttachAdorner(FrameworkElement element, Func<FrameworkElement,TAdorner>? factory = default)
        {
            if (element.IsLoaded)
            {
                TAdorner adorner = (factory?? (e=>(TAdorner)Activator.CreateInstance(typeof(TAdorner), new[] { e })!))(element);
                element.SetValue(AdornerAttacher<TAdorner>._adornerProperty, adorner);
                AdornerLayer.GetAdornerLayer(element).Add(adorner);
            }
            else
            {
                element.SetValue(AdornerAttacher < TAdorner > ._factoryProperty, factory);
                element.Loaded += Element_Loaded;
            }
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            element.Loaded -= Element_Loaded;
            AttachAdorner(element,(Func<FrameworkElement,TAdorner>?)element.GetValue(_factoryProperty));
            element.ClearValue(_factoryProperty);
        }

        public static void DetachAdorner(FrameworkElement element)
        {
            using TAdorner? adorner = (TAdorner?)element.GetValue(AdornerAttacher<TAdorner>._adornerProperty);
            AdornerLayer adornerLayer = element.FindVisualTreeAncestor<AdornerLayer>() ?? AdornerLayer.GetAdornerLayer(element);
            adornerLayer.Remove(adorner);
            element.SetValue(AdornerAttacher<TAdorner>._adornerProperty, null);
            element.Loaded -= Element_Loaded;
        }
    }
}