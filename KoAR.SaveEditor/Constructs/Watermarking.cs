using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace KoAR.SaveEditor.Constructs
{
    public static class Watermarking
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached("Content", typeof(object), typeof(Watermarking),
            new PropertyMetadata(Watermarking.ContentProperty_ValueChanged));

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.RegisterAttached("ContentTemplate", typeof(DataTemplate), typeof(Watermarking),
            new PropertyMetadata(Watermarking.ContentTemplateProperty_ValueChanged));

        private static readonly DependencyProperty _adornerProperty = DependencyProperty.RegisterAttached("Adorner", typeof(WatermarkAdorner), typeof(Watermarking),
            new PropertyMetadata());

        public static object? GetContent(TextBoxBase control) => control?.GetValue(Watermarking.ContentProperty);

        public static DataTemplate? GetContentTemplate(TextBoxBase textBox) => (DataTemplate?)textBox?.GetValue(Watermarking.ContentTemplateProperty);

        public static void SetContent(TextBoxBase control, object? value) => control?.SetValue(Watermarking.ContentProperty, value);

        public static void SetContentTemplate(TextBoxBase textBox, DataTemplate? value) => textBox?.SetValue(Watermarking.ContentTemplateProperty, value);

        private static void ContentProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBoxBase textBox))
            {
                return;
            }
            if (e.OldValue != null)
            {
                WatermarkAdorner? adorner = Watermarking.GetAdorner(textBox);
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(textBox)?? adorner.FindVisualTreeAncestor<AdornerLayer>()!;
                    adornerLayer.Remove(adorner);
                    adorner.ClearVisibilityBinding();
                }
                else
                {
                    textBox.Loaded -= Watermarking.TextBox_Loaded;
                }
                Watermarking.SetAdorner(textBox, default);
            }
            if (e.NewValue != null)
            {
                if (!textBox.IsLoaded)
                {
                    textBox.Loaded += Watermarking.TextBox_Loaded;
                    return;
                }
                WatermarkAdorner adorner = new WatermarkAdorner(textBox, e.NewValue, Watermarking.GetContentTemplate(textBox));
                AdornerLayer.GetAdornerLayer(textBox).Add(adorner);
                Watermarking.SetAdorner(textBox, adorner);
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
            object? content = Watermarking.GetContent(textBox);
            if (content == null)
            {
                return;
            }
            WatermarkAdorner adorner = new WatermarkAdorner(textBox, content, Watermarking.GetContentTemplate(textBox));
            AdornerLayer.GetAdornerLayer(textBox).Add(adorner);
            Watermarking.SetAdorner(textBox, adorner);
        }

        private sealed class WatermarkAdorner : Adorner
        {
            private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
            private readonly ContentPresenter _contentPresenter;

            public WatermarkAdorner(Control adornedElement, object content, DataTemplate? contentTemplate)
                : base(adornedElement)
            {
                this.IsHitTestVisible = false;
                this._contentPresenter = new ContentPresenter
                {
                    Content = content,
                    ContentTemplate = contentTemplate,
                    Opacity = 0.5,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(
                        this.AdornedElement.Margin.Left + this.AdornedElement.Padding.Left,
                        this.AdornedElement.Margin.Top + this.AdornedElement.Padding.Top,
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

            private new Control AdornedElement => (Control)base.AdornedElement;

            public DataTemplate? ContentTemplate
            {
                get => this._contentPresenter.ContentTemplate;
                set => this._contentPresenter.ContentTemplate = value;
            }

            protected override int VisualChildrenCount => 1;

            public void ClearVisibilityBinding() => BindingOperations.ClearBinding(this, UIElement.VisibilityProperty);

            protected override Size ArrangeOverride(Size finalSize)
            {
                this._contentPresenter.Arrange(new Rect(finalSize));
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
}
