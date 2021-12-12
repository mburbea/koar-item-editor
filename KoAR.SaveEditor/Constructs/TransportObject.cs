using System.Windows;

namespace KoAR.SaveEditor.Constructs;

public sealed class TransportObject : Freezable
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(TransportObject.Value), typeof(object), typeof(TransportObject));

    public object? Value
    {
        get => this.GetValue(TransportObject.ValueProperty);
        set => this.SetValue(TransportObject.ValueProperty, value);
    }

    protected override Freezable CreateInstanceCore() => new TransportObject();
}
