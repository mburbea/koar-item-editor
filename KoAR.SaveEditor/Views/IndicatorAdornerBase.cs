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
    private static readonly ScaleTransform _scaleTransform = IndicatorAdornerBase.CreateScaleTransform();

    private readonly FrameworkElement _element;
    private readonly double _heightMultiple;
    private readonly double _widthMultiple;

    protected IndicatorAdornerBase(FrameworkElement adornedElement, AdornerPosition position, Brush background, Brush foreground, string indicator)
        : base(adornedElement)
    {
        this._heightMultiple = position is AdornerPosition.LowerLeft or AdornerPosition.LowerRight ? 0.5 : default;
        this._widthMultiple = position is AdornerPosition.UpperRight or AdornerPosition.LowerRight ? 0.5 : default;
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
        textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding { Mode = BindingMode.OneTime });
        textBlockFactory.SetValue(TextBlock.ForegroundProperty, foreground);
        textBlockFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        textBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        viewBoxFactory.AppendChild(textBlockFactory);
        gridFactory.AppendChild(viewBoxFactory);
        this._element = new ContentPresenter()
        {
            Content = indicator,
            ContentTemplate = new() { VisualTree = gridFactory },
        };
        this.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(UIElement.IsVisible))
        {
            Source = adornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
    }

    protected enum AdornerPosition
    {
        UpperLeft = 0,
        UpperRight,
        LowerLeft,
        LowerRight,
    }

    public virtual void Dispose()
    {
        BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
        GC.SuppressFinalize(this);
    }

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
        Rect bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement);
        return new GeneralTransformGroup
        {
            Children =
            {
                base.GetDesiredTransform(transform),
                IndicatorAdornerBase._scaleTransform,
                new TranslateTransform(bounds.Width * this._widthMultiple, bounds.Height * this._heightMultiple)
            }
        };
    }

    protected static void AttachAdorner<TAdorner>(FrameworkElement element, Func<FrameworkElement, TAdorner>? factory = null)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.AttachAdorner(element, factory);

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    protected override Size ArrangeOverride(Size finalSize)
    {
        this._element.Arrange(new(finalSize));
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        this._element.Measure(constraint);
        return this.AdornedElement.RenderSize;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        Rect bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement);
        if (bounds.IsEmpty)
        {
            return;
        }
        RenderTargetBitmap bitmap = new((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
        DrawingVisual visual = new();
        using (DrawingContext context = visual.RenderOpen())
        {
            VisualBrush brush = new(this._element);
            context.DrawRectangle(brush, null, new(default, bounds.Size));
        }
        bitmap.Render(visual);
        bitmap.Freeze();
        drawingContext.DrawImage(bitmap, new(default, bounds.Size));
    }

    private static ScaleTransform CreateScaleTransform()
    {
        ScaleTransform scaleTransform = new(0.5, 0.5);
        scaleTransform.Freeze();
        return scaleTransform;
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
                element.Loaded += AdornerAttacher<TAdorner>.Element_Loaded;
            }
        }

        public static void DetachAdorner(FrameworkElement element)
        {
            using TAdorner? adorner = (TAdorner?)element.GetValue(AdornerAttacher<TAdorner>._adornerProperty);
            if (adorner != null)
            {
                (element.FindVisualTreeAncestor<AdornerLayer>() ?? AdornerLayer.GetAdornerLayer(element)).Remove(adorner);
                element.ClearValue(AdornerAttacher<TAdorner>._adornerProperty);
            }
            else
            {
                element.Loaded -= AdornerAttacher<TAdorner>.Element_Loaded;
                element.ClearValue(AdornerAttacher<TAdorner>._factoryProperty);
            }
        }

        private static Func<FrameworkElement, TAdorner> CreateDefaultFactory() => Expression.Lambda<Func<FrameworkElement, TAdorner>>(
            Expression.New(
                typeof(TAdorner).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(FrameworkElement) }, null)!,
                IndicatorAdornerBase._elementParameter
            ),
            IndicatorAdornerBase._elementParameter
        ).Compile();

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            element.Loaded -= AdornerAttacher<TAdorner>.Element_Loaded;
            AdornerAttacher<TAdorner>.AttachAdorner(element, (Func<FrameworkElement, TAdorner>?)element.GetValue(AdornerAttacher<TAdorner>._factoryProperty));
            element.ClearValue(AdornerAttacher<TAdorner>._factoryProperty);
        }
    }
}