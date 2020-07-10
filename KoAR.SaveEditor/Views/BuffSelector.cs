using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffSelector : Control
    {
        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(BuffSelector.Buffs), typeof(IReadOnlyDictionary<uint, Buff>), typeof(BuffSelector));

        public static readonly DependencyProperty SelectedBuffProperty = DependencyProperty.Register(nameof(BuffSelector.SelectedBuff), typeof(Buff), typeof(BuffSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        static BuffSelector() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffSelector), new FrameworkPropertyMetadata(typeof(BuffSelector)));

        public IReadOnlyDictionary<uint, Buff>? Buffs
        {
            get => (IReadOnlyDictionary<uint, Buff>?)this.GetValue(BuffSelector.BuffsProperty);
            set => this.SetValue(BuffSelector.BuffsProperty, value);
        }

        public Buff? SelectedBuff
        {
            get => (Buff?)this.GetValue(BuffSelector.SelectedBuffProperty);
            set => this.SetValue(BuffSelector.SelectedBuffProperty, value);
        }
    }
}
