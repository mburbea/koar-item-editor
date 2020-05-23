using System.Windows;
using System.Windows.Markup;

namespace KoAR.SaveEditor.Constructs
{
    [ContentProperty(nameof(ValueContainer.Value))]
    public sealed class ValueContainer : Freezable
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(ValueContainer.Value), typeof(object), typeof(ValueContainer));

        public object? Value
        {
            get => this.GetValue(ValueContainer.ValueProperty);
            set => this.SetValue(ValueContainer.ValueProperty, value);
        }

        protected override Freezable CreateInstanceCore() => new ValueContainer();
    }
}
