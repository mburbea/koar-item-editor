using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffSelector : Control
    {
        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(BuffSelector.Buffs), typeof(IEnumerable<Buff>), typeof(BuffSelector),
            new PropertyMetadata(BuffSelector.BuffsProperty_ValueChanged));

        public static readonly DependencyProperty BuffTypesProperty = DependencyProperty.Register(nameof(BuffSelector.BuffTypes), typeof(BuffTypes), typeof(BuffSelector),
            new PropertyMetadata((BuffTypes)(-1), BuffSelector.BuffTypesProperty_ValueChanged));

        public static readonly DependencyProperty DataContainersProperty;

        public static readonly DependencyProperty SelectedBuffProperty = DependencyProperty.Register(nameof(BuffSelector.SelectedBuff), typeof(Buff), typeof(BuffSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private static readonly DependencyPropertyKey _dataContainersPropertyKey = DependencyProperty.RegisterReadOnly(nameof(BuffSelector.DataContainers), typeof(IReadOnlyList<DataContainer>), typeof(BuffSelector),
            new PropertyMetadata());

        static BuffSelector()
        {
            BuffSelector.DataContainersProperty = BuffSelector._dataContainersPropertyKey.DependencyProperty;
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffSelector), new FrameworkPropertyMetadata(typeof(BuffSelector)));
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

        public IReadOnlyList<DataContainer>? DataContainers
        {
            get => (IReadOnlyList<DataContainer>?)this.GetValue(BuffSelector.DataContainersProperty);
            private set => this.SetValue(BuffSelector._dataContainersPropertyKey, value);
        }

        public Buff? SelectedBuff
        {
            get => (Buff?)this.GetValue(BuffSelector.SelectedBuffProperty);
            set => this.SetValue(BuffSelector.SelectedBuffProperty, value);
        }

        private static void BuffsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            selector.DataContainers = e.NewValue == null ? null : BuffSelector.GetDataContainers((IEnumerable<Buff>)e.NewValue, selector.BuffTypes);
        }

        private static void BuffTypesProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            if (selector.Buffs != null)
            {
                selector.DataContainers = BuffSelector.GetDataContainers(selector.Buffs, (BuffTypes)e.NewValue);
            }
        }

        private static IReadOnlyList<DataContainer> GetDataContainers(IEnumerable<Buff> buffs, BuffTypes buffTypes)
        {
            return buffs
                .Where(buff => (buff.BuffType & buffTypes) != 0)
                .OrderByDescending(buff => buff.Rarity)
                .ThenBy(buff => buff.Desc.FirstOrDefault())
                .Select(buff => new DataContainer(buff))
                .Prepend(DataContainer.Empty)
                .ToList();
        }
    }
}
