using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class ElementIcon : Control
    {
        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(nameof(ElementIcon.Element), typeof(Element), typeof(ElementIcon),
            new PropertyMetadata(Element.None));

        static ElementIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ElementIcon), new FrameworkPropertyMetadata(typeof(ElementIcon)));

        public Element Element
        {
            get => (Element)this.GetValue(ElementIcon.ElementProperty);
            set => this.SetValue(ElementIcon.ElementProperty, value);
        }
    }
}
