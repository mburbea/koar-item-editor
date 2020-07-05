using System.Windows;
using System.Windows.Controls;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffControl : Control
    {
        public static readonly DependencyProperty BuffProperty = DependencyProperty.Register(nameof(BuffControl.Buff), typeof(Buff), typeof(BuffControl));

        public static readonly DependencyProperty ShowModifierProperty = DependencyProperty.Register(nameof(BuffControl.ShowModifier), typeof(bool), typeof(BuffControl),
            new PropertyMetadata(BooleanBoxes.False));

        static BuffControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffControl), new FrameworkPropertyMetadata(typeof(BuffControl)));

        public Buff? Buff
        {
            get => (Buff?)this.GetValue(BuffControl.BuffProperty);
            set => this.SetValue(BuffControl.BuffProperty, value);
        }

        public bool ShowModifier
        {
            get => (bool)this.GetValue(BuffControl.ShowModifierProperty);
            set => this.SetValue(BuffControl.ShowModifierProperty, BooleanBoxes.GetBox(value));
        }
    }
}
