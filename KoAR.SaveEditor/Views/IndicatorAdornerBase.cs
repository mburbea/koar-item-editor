using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KoAR.SaveEditor.Constructs;
using Expression = System.Linq.Expressions.Expression;

namespace KoAR.SaveEditor.Views;

public abstract class IndicatorAdornerBase : Adorner, IDisposable
{
    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();
    private static readonly ParameterExpression _elementParameter = Expression.Parameter(typeof(FrameworkElement));

    private readonly ContentPresenter _contentPresenter;
    private readonly double _heightMultiple;
    private readonly double _widthMultiple;

    protected IndicatorAdornerBase(FrameworkElement adornedElement, AdornerPosition position, DataTemplate contentTemplate)
        : base(adornedElement)
    {
        this._heightMultiple = position is AdornerPosition.LowerLeft or AdornerPosition.LowerRight ? 1d : 0d;
        this._widthMultiple = position is AdornerPosition.UpperRight or AdornerPosition.LowerRight ? 1d : 0d;
        this._contentPresenter = new()
        {
            Content = string.Empty,
            ContentTemplate = contentTemplate,
        };
        this.SetBinding(UIElement.VisibilityProperty, new Binding
        {
            Path = new(UIElement.IsVisibleProperty),
            Source = adornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
    }

    protected enum AdornerPosition
    {
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight,
    }

    public new FrameworkElement AdornedElement => (FrameworkElement)base.AdornedElement;

    public virtual void Dispose()
    {
        BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
        GC.SuppressFinalize(this);
    }

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
        if (this.AdornedElement.RenderSize is not { IsEmpty: false } size)
        {
            return base.GetDesiredTransform(transform);
        }
        return new GeneralTransformGroup
        {
            Children =
            {
                base.GetDesiredTransform(transform),
                new ScaleTransform(0.5, 0.5, size.Width * this._widthMultiple, size.Height * this._heightMultiple),
            }
        };
    }

    protected static void AttachAdorner<TAdorner>(FrameworkElement element, Func<FrameworkElement, TAdorner>? factory = null)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.AttachAdorner(element, factory);

    protected static DataTemplate CreateContentTemplate(Brush background, Brush foreground, string indicator)
    {
        FrameworkElementFactory gridFactory = new(typeof(Grid));
        FrameworkElementFactory ellipseFactory = new(typeof(Ellipse));
        ellipseFactory.SetValue(Shape.FillProperty, background);
        ellipseFactory.SetValue(Shape.StrokeProperty, foreground);
        ellipseFactory.SetValue(Shape.StrokeThicknessProperty, 1d);
        gridFactory.AppendChild(ellipseFactory);
        FrameworkElementFactory viewBoxFactory = new(typeof(Viewbox));
        viewBoxFactory.SetValue(Viewbox.StretchProperty, Stretch.Uniform);
        viewBoxFactory.SetValue(Viewbox.StretchDirectionProperty, StretchDirection.Both);
        FrameworkElementFactory textBlockFactory = new(typeof(TextBlock));
        textBlockFactory.SetValue(TextBlock.TextProperty, indicator);
        textBlockFactory.SetValue(TextBlock.ForegroundProperty, foreground);
        textBlockFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        textBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        viewBoxFactory.AppendChild(textBlockFactory);
        gridFactory.AppendChild(viewBoxFactory);
        return new() { VisualTree = gridFactory };
    }

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    protected override Size ArrangeOverride(Size finalSize)
    {
        this._contentPresenter.Arrange(new(finalSize));
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        this._contentPresenter.Measure(constraint);
        return this.AdornedElement.RenderSize;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (this.AdornedElement.RenderSize is not { IsEmpty: false } size)
        {
            return;
        }
        RenderTargetBitmap bitmap = new((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
        DrawingVisual visual = new();
        using (DrawingContext context = visual.RenderOpen())
        {
            VisualBrush brush = new(this._contentPresenter);
            context.DrawRectangle(brush, null, new(size));
        }
        bitmap.Render(visual);
        bitmap.Freeze();
        drawingContext.DrawImage(bitmap, new(size));
    }

    private static class AdornerAttacher<TAdorner>
        where TAdorner : IndicatorAdornerBase
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(typeof(TAdorner).FullName, typeof(TAdorner), typeof(AdornerAttacher<TAdorner>));
        private static readonly Lazy<Func<FrameworkElement, TAdorner>> _defaultFactory = new(AdornerAttacher<TAdorner>.CreateDefaultFactory, false);
        private static readonly DependencyProperty _factoryProperty = DependencyProperty.RegisterAttached(typeof(Func<FrameworkElement, TAdorner>).FullName, typeof(Func<FrameworkElement, TAdorner>), typeof(AdornerAttacher<TAdorner>));

        public static void AttachAdorner(FrameworkElement element, Func<FrameworkElement, TAdorner>? factory = default)
        {
            if (element.IsLoaded)
            {
                TAdorner adorner = (factory ?? AdornerAttacher<TAdorner>._defaultFactory.Value).Invoke(element);
                element.SetValue(AdornerAttacher<TAdorner>._adornerProperty, adorner);
                AdornerLayer.GetAdornerLayer(element).Add(adorner);
            }
            else
            {
                element.SetValue(AdornerAttacher<TAdorner>._factoryProperty, factory);
                WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(element, nameof(element.Loaded), AdornerAttacher<TAdorner>.Element_Loaded);
            }
        }

        public static void DetachAdorner(FrameworkElement element)
        {
            using TAdorner? adorner = (TAdorner?)element.GetValue(AdornerAttacher<TAdorner>._adornerProperty);
            if (adorner != null)
            {
                (adorner.FindVisualTreeAncestor<AdornerLayer>() ?? element.FindVisualTreeAncestor<AdornerLayer>() ?? AdornerLayer.GetAdornerLayer(element)).Remove(adorner);
                element.ClearValue(AdornerAttacher<TAdorner>._adornerProperty);
            }
            else
            {
                AdornerAttacher<TAdorner>.RemoveLoadedHandler(element);
                element.ClearValue(AdornerAttacher<TAdorner>._factoryProperty);
            }
        }

        private static Func<FrameworkElement, TAdorner> CreateDefaultFactory() => Expression.Lambda<Func<FrameworkElement, TAdorner>>(
            Expression.New(
                typeof(TAdorner).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(FrameworkElement)], null)!,
                IndicatorAdornerBase._elementParameter
            ),
            IndicatorAdornerBase._elementParameter
        ).Compile();

        private static void Element_Loaded(object? sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender!;
            AdornerAttacher<TAdorner>.RemoveLoadedHandler(element);
            AdornerAttacher<TAdorner>.AttachAdorner(element, (Func<FrameworkElement, TAdorner>?)element.GetValue(AdornerAttacher<TAdorner>._factoryProperty));
            element.ClearValue(AdornerAttacher<TAdorner>._factoryProperty);
        }

        private static void RemoveLoadedHandler(FrameworkElement element) => WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(element, nameof(element.Loaded), AdornerAttacher<TAdorner>.Element_Loaded);
    }
}