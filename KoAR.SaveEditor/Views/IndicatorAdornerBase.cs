using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace KoAR.SaveEditor.Views;

using Expression = System.Linq.Expressions.Expression;

public abstract class IndicatorAdornerBase : Adorner, IDisposable
{
    private const double _fontSize = 10;
    private const double _radius = 6;
    private const double _strokeThickness = 0.5;

    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();
    private static readonly Dictionary<Color, Pen> _penCache = new();

    private readonly AdornerPosition _adornerPosition;
    private readonly Brush _background;
    private readonly Brush _foreground;
    private readonly string _indicator;
    private readonly Pen _stroke;

    protected IndicatorAdornerBase(FrameworkElement adornedElement, AdornerPosition adornerPosition, Brush background, Brush foreground, string indicator)
        : base(adornedElement)
    {
        this.IsHitTestVisible = true;
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
        (this.AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement)).Add(this);
        BindingOperations.SetBinding(this, UIElement.VisibilityProperty, new Binding
        {
            Path = new(UIElement.IsVisibleProperty),
            Source = this.AdornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
    }

    protected AdornerLayer AdornerLayer { get; }

    protected enum AdornerPosition
    {
        LowerLeft,
        LowerRight,
        UpperLeft,
        UpperRight,
    }

    public virtual void Dispose()
    {
        BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
        this.AdornerLayer.Remove(this);
        GC.SuppressFinalize(this);
    }

    protected static void AttachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.AttachAdorner(element);

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    protected override void OnRender(DrawingContext drawingContext)
    {
        Rect bounds = VisualTreeHelper.GetDescendantBounds(this.AdornedElement);
        double centerX = this._adornerPosition is AdornerPosition.LowerRight or AdornerPosition.UpperRight
            ? bounds.Width - IndicatorAdornerBase._radius
            : IndicatorAdornerBase._radius;
        double centerY = this._adornerPosition is AdornerPosition.LowerRight or AdornerPosition.LowerLeft
            ? bounds.Height - IndicatorAdornerBase._radius
            : IndicatorAdornerBase._radius;
        drawingContext.DrawEllipse(
            this._background,
            this._stroke,
            new(centerX, centerY),
            IndicatorAdornerBase._radius - IndicatorAdornerBase._strokeThickness,
            IndicatorAdornerBase._radius - IndicatorAdornerBase._strokeThickness
        );
        FormattedText formattedText = new(
            this._indicator,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new(Window.GetWindow(this.AdornedElement).FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            IndicatorAdornerBase._fontSize,
            this._foreground,
            1d
        );
        drawingContext.DrawText(
            formattedText,
            new(centerX - formattedText.Width * 0.5, centerY - formattedText.Height * 0.5)
        );
        base.OnRender(drawingContext);
    }

    private static Pen CreateFrozenPen(Brush brush)
    {
        Pen pen = new(brush, IndicatorAdornerBase._strokeThickness);
        pen.Freeze();
        return pen;
    }

    private static class AdornerAttacher<TAdorner>
        where TAdorner : IndicatorAdornerBase
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(typeof(TAdorner).FullName, typeof(TAdorner), typeof(AdornerAttacher<TAdorner>));
        private static readonly Func<FrameworkElement, TAdorner> _createAdorner = AdornerAttacher<TAdorner>.CreateCreateAdorner();

        public static void AttachAdorner(FrameworkElement element)
        {
            if (!element.IsLoaded)
            {
                element.Loaded += AdornerAttacher<TAdorner>.Element_Loaded;
            }
            else
            {
                element.SetValue(AdornerAttacher<TAdorner>._adornerProperty, AdornerAttacher<TAdorner>._createAdorner(element));
            }
        }

        public static void DetachAdorner(FrameworkElement element)
        {
            using TAdorner? adorner = (TAdorner?)element.GetValue(AdornerAttacher<TAdorner>._adornerProperty);
            if (adorner == null)
            {
                element.Loaded -= AdornerAttacher<TAdorner>.Element_Loaded;
            }
            else
            {
                element.ClearValue(AdornerAttacher<TAdorner>._adornerProperty);
            }
        }

        private static Func<FrameworkElement, TAdorner> CreateCreateAdorner()
        {
            ParameterExpression parameter = Expression.Parameter(typeof(FrameworkElement), nameof(parameter));
            Expression<Func<FrameworkElement, TAdorner>> lambdaExpression = Expression.Lambda<Func<FrameworkElement, TAdorner>>(
                Expression.New(
                    typeof(TAdorner).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(FrameworkElement) })!,
                    parameter
                ),
                parameter
            );
            return lambdaExpression.Compile();
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            AdornerAttacher<TAdorner>.AttachAdorner(element);
            element.Loaded -= AdornerAttacher<TAdorner>.Element_Loaded;
        }
    }
}