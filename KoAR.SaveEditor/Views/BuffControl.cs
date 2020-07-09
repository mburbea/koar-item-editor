using System.Windows;
using System.Windows.Controls;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffControl : Control
    {
        public static readonly DependencyProperty BuffProperty = DependencyProperty.Register(nameof(BuffControl.Buff), typeof(Buff), typeof(BuffControl));

        public static readonly DependencyProperty ExpandedProperty = DependencyProperty.Register(nameof(BuffControl.Expanded), typeof(bool), typeof(BuffControl),
            new FrameworkPropertyMetadata(BooleanBoxes.False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        static BuffControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffControl), new FrameworkPropertyMetadata(typeof(BuffControl)));

        public Buff? Buff
        {
            get => (Buff?)this.GetValue(BuffControl.BuffProperty);
            set => this.SetValue(BuffControl.BuffProperty, value);
        }

        public bool Expanded
        {
            get => (bool)this.GetValue(BuffControl.ExpandedProperty);
            set => this.SetValue(BuffControl.ExpandedProperty, BooleanBoxes.GetBox(value));
        }
    }
}
