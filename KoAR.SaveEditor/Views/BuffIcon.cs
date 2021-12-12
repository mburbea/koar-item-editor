using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views;

public sealed class BuffIcon : Control
{
    public static readonly DependencyProperty BuffProperty = DependencyProperty.Register(nameof(BuffIcon.Buff), typeof(Buff), typeof(BuffIcon));

    static BuffIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffIcon), new FrameworkPropertyMetadata(typeof(BuffIcon)));

    public Buff? Buff
    {
        get => (Buff?)this.GetValue(BuffIcon.BuffProperty);
        set => this.SetValue(BuffIcon.BuffProperty, value);
    }
}
