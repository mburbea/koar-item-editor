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
    private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();
    private static readonly Dictionary<Color, Pen> _penCache = new();

    protected static void AttachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.AttachAdorner(element);

    protected static void DetachAdorner<TAdorner>(FrameworkElement element)
        where TAdorner : IndicatorAdornerBase => AdornerAttacher<TAdorner>.DetachAdorner(element);

    private readonly Brush _background;
    private readonly double _fontSize;
    private readonly string _indicator;
    private readonly Brush _foreground;
    private readonly double _radius;
    private readonly Pen _stroke;

    protected AdornerLayer AdornerLayer { get; }

    protected abstract Point EllipseCenter { get; }

    private static Pen CreateFrozenPen(Brush brush)
    {
        Pen pen = new(brush, 0.5);
        pen.Freeze();
        return pen;
    }

    protected IndicatorAdornerBase(FrameworkElement adornedElement, Brush background, Brush foreground, double radius, char indicator, double fontSize)
        : base(adornedElement)
    {
        this.IsHitTestVisible = false;
        this._radius = radius;
        this._fontSize = fontSize;
        this._indicator = char.ToString(indicator);
        this._background = background;
        this._foreground = foreground;
        if (foreground is not SolidColorBrush { Color: Color color })
        {
            this._stroke = IndicatorAdornerBase.CreateFrozenPen(foreground);
        }
        else if (!IndicatorAdornerBase._penCache.TryGetValue(color, out Pen? pen))
        {
            IndicatorAdornerBase._penCache.Add(color, this._stroke = IndicatorAdornerBase.CreateFrozenPen(foreground));
        }
        else
        {
            this._stroke = pen;
        }
        (this.AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement)).Add(this);
        BindingOperations.SetBinding(this, UIElement.VisibilityProperty, new Binding
        {
            Path = new(UIElement.IsVisibleProperty),
            Source = this.AdornedElement,
            Converter = IndicatorAdornerBase._booleanToVisibilityConverter,
        });
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        Point center = this.EllipseCenter;
        drawingContext.DrawEllipse(
            this._background,
            this._stroke,
            center,
            this._radius - this._stroke.Thickness,
            this._radius - this._stroke.Thickness
        );
        FormattedText formattedText = new(
            this._indicator,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new(((Control)PresentationSource.FromVisual(this.AdornedElement).RootVisual).FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            this._fontSize,
            this._foreground,
            1d
        );
        drawingContext.DrawText(
            formattedText,
            new(center.X - formattedText.Width * 0.5, center.Y - formattedText.Height * 0.5)
        );
        base.OnRender(drawingContext);
    }

    public virtual void Dispose()
    {
        BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);
        this.AdornerLayer.Remove(this);
        GC.SuppressFinalize(this);
    }

    private static class AdornerAttacher<TAdorner>
        where TAdorner : IndicatorAdornerBase
    {
        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached(typeof(TAdorner).FullName, typeof(TAdorner), typeof(AdornerAttacher<TAdorner>));
        private static readonly Func<FrameworkElement, TAdorner> _createAdorner = AdornerAttacher<TAdorner>.CreateCreateAdorner();

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

        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            AdornerAttacher<TAdorner>.AttachAdorner(element);
            element.Loaded -= AdornerAttacher<TAdorner>.Element_Loaded;
        }
    }
}