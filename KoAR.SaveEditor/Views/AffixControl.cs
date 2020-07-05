using System.Collections.Generic;
using System.Linq;
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

    public sealed class BuffSelector : Control
    {
        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(BuffSelector.Buffs), typeof(IEnumerable<Buff>), typeof(BuffSelector),
            new PropertyMetadata(BuffSelector.BuffsProperty_ValueChanged));

        public static readonly DependencyProperty BuffTypesProperty = DependencyProperty.Register(nameof(BuffSelector.BuffTypes), typeof(BuffTypes), typeof(BuffSelector),
            new PropertyMetadata(-1, BuffSelector.BuffTypesProperty_ValueChanged));

        public static readonly DependencyProperty DataContainersProperty;

        private static readonly DependencyPropertyKey _dataContainersPropertyKey = DependencyProperty.RegisterReadOnly(nameof(BuffSelector.DataContainers), typeof(IEnumerable<DataContainer>), typeof(BuffSelector),
            new PropertyMetadata());

        static BuffSelector()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffSelector), new FrameworkPropertyMetadata(typeof(BuffSelector)));
            BuffSelector.DataContainersProperty = BuffSelector._dataContainersPropertyKey.DependencyProperty;
        }

        public IEnumerable<Buff>? Buffs
        {
            get => (IEnumerable<Buff>?)this.GetValue(BuffSelector.BuffsProperty);
            set => this.SetValue(BuffSelector.BuffsProperty, value);
        }

        public BuffTypes BuffTypes
        {
            get => (BuffTypes)this.GetValue(BuffSelector.BuffTypesProperty);
            set => this.SetValue(BuffSelector.BuffTypesProperty, value);
        }

        public IEnumerable<DataContainer>? DataContainers
        {
            get => (IEnumerable<DataContainer>?)this.GetValue(BuffSelector.DataContainersProperty);
            private set => this.SetValue(BuffSelector._dataContainersPropertyKey, value);
        }

        private static void BuffsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            if (e.NewValue == null)
            {
                selector.DataContainers = null;
                return;
            }
            selector.DataContainers = ((IEnumerable<Buff>)e.NewValue).Where(buff => (buff.BuffType & selector.BuffTypes) != 0).Select(buff => new DataContainer(buff))
                .Prepend(DataContainer.Empty)
                .ToList();
        }

        private static void BuffTypesProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            if (selector.Buffs == null)
            {
                return;
            }
            selector.DataContainers = selector.Buffs.Where(buff => (buff.BuffType & (BuffTypes)e.NewValue) != 0).Select(buff => new DataContainer(buff))
                .Prepend(DataContainer.Empty)
                .ToList();
        }
    }
}
