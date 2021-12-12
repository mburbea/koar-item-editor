using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace KoAR.SaveEditor.Constructs;

public static class Watermarking
{
    public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached("Content", typeof(object), typeof(Watermarking),
        new(Watermarking.ContentProperty_ValueChanged));

    public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.RegisterAttached("ContentTemplate", typeof(DataTemplate), typeof(Watermarking),
        new(Watermarking.ContentTemplateProperty_ValueChanged));

    private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached("Adorner", typeof(WatermarkAdorner), typeof(Watermarking),
        new(Watermarking.AdornerProperty_ValueChanged));

    public static object? GetContent(TextBoxBase control) => control?.GetValue(Watermarking.ContentProperty);

    public static DataTemplate? GetContentTemplate(TextBoxBase textBox) => (DataTemplate?)textBox?.GetValue(Watermarking.ContentTemplateProperty);

    public static void SetContent(TextBoxBase control, object? value) => control?.SetValue(Watermarking.ContentProperty, value);

    public static void SetContentTemplate(TextBoxBase textBox, DataTemplate? value) => textBox?.SetValue(Watermarking.ContentTemplateProperty, value);

    private static void AdornerProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TextBoxBase textBox = (TextBoxBase)d;
        if (e.OldValue != null)
        {
            using WatermarkAdorner adorner = (WatermarkAdorner)e.OldValue;
            AdornerLayer adornerLayer = adorner.FindVisualTreeAncestor<AdornerLayer>() ?? AdornerLayer.GetAdornerLayer(textBox);
            adornerLayer.Remove(adorner);
        }
        if (e.NewValue != null)
        {
            AdornerLayer.GetAdornerLayer(textBox).Add((WatermarkAdorner)e.NewValue);
        }
    }

    private static void ContentProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBoxBase textBox)
        {
            return;
        }
        if (textBox.IsLoaded)
        {
            Watermarking.SetAdorner(textBox, e.NewValue == null ? default : new(textBox, e.NewValue, Watermarking.GetContentTemplate(textBox)));
        }
        else if (e.NewValue != null && e.OldValue == null)
        {
            textBox.Loaded += Watermarking.TextBox_Loaded;
        }
        else if (e.NewValue == null && e.OldValue != null)
        {
            textBox.Loaded -= Watermarking.TextBox_Loaded;
        }
    }

    private static void ContentTemplateProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBoxBase textBox && Watermarking.GetAdorner(textBox) is WatermarkAdorner adorner)
        {
            adorner.ContentTemplate = (DataTemplate?)e.NewValue;
        }
    }

    private static WatermarkAdorner? GetAdorner(TextBoxBase textBox) => (WatermarkAdorner?)textBox.GetValue(Watermarking._adornerProperty);

    private static void SetAdorner(TextBoxBase textBox, WatermarkAdorner? adorner) => textBox.SetValue(Watermarking._adornerProperty, adorner);

    private static void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxBase textBox = (TextBoxBase)sender;
        textBox.Loaded -= Watermarking.TextBox_Loaded;
        if (Watermarking.GetContent(textBox) is object content)
        {
            Watermarking.SetAdorner(textBox, new(textBox, content, Watermarking.GetContentTemplate(textBox)));
        }
    }

    private sealed class WatermarkAdorner : Adorner, IDisposable
    {
        private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new();
        private readonly ContentPresenter _contentPresenter;

        public WatermarkAdorner(Control adornedElement, object content, DataTemplate? contentTemplate)
            : base(adornedElement)
        {
            this.IsHitTestVisible = false;
            this._contentPresenter = new()
            {
                Content = content,
                ContentTemplate = contentTemplate,
                Opacity = 0.5,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new(
                    adornedElement.Margin.Left + adornedElement.Padding.Left,
                    adornedElement.Margin.Top + adornedElement.Padding.Top,
                    0,
                    0
                )
            };
            this.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(UIElement.IsVisible))
            {
                Source = adornedElement,
                Converter = WatermarkAdorner._booleanToVisibilityConverter
            });
        }

        public DataTemplate? ContentTemplate
        {
            get => this._contentPresenter.ContentTemplate;
            set => this._contentPresenter.ContentTemplate = value;
        }

        protected override int VisualChildrenCount => 1;

        public void Dispose() => BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);

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
    }
}
