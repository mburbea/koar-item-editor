using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffSelector : Control
    {
        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(BuffSelector.Buffs), typeof(IReadOnlyList<Buff>), typeof(BuffSelector),
            new PropertyMetadata(BuffSelector.BuffsProperty_ValueChanged));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(BuffSelector.Filter), typeof(BuffsFilter), typeof(BuffSelector),
            new PropertyMetadata(BuffSelector.FilterProperty_ValueChanged));

        public static readonly DependencyProperty SelectedBuffProperty = DependencyProperty.Register(nameof(BuffSelector.SelectedBuff), typeof(Buff), typeof(BuffSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static DependencyProperty FilteredItemsProperty;

        private static readonly DependencyPropertyKey _filteredItemsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(BuffSelector.FilteredItems), typeof(IReadOnlyList<Buff>), typeof(BuffSelector),
            new PropertyMetadata());

        static BuffSelector()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffSelector), new FrameworkPropertyMetadata(typeof(BuffSelector)));
            BuffSelector.FilteredItemsProperty = BuffSelector._filteredItemsPropertyKey.DependencyProperty;
        }

        public IReadOnlyList<Buff>? Buffs
        {
            get => (IReadOnlyList<Buff>?)this.GetValue(BuffSelector.BuffsProperty);
            set => this.SetValue(BuffSelector.BuffsProperty, value);
        }

        public BuffsFilter Filter
        {
            get => (BuffsFilter)this.GetValue(BuffSelector.FilterProperty);
            set => this.SetValue(BuffSelector.FilterProperty, value);
        }

        public IReadOnlyList<Buff>? FilteredItems
        {
            get => (IReadOnlyList<Buff>?)this.GetValue(BuffSelector.FilteredItemsProperty);
            private set => this.SetValue(BuffSelector._filteredItemsPropertyKey, value);
        }

        public Buff? SelectedBuff
        {
            get => (Buff?)this.GetValue(BuffSelector.SelectedBuffProperty);
            set => this.SetValue(BuffSelector.SelectedBuffProperty, value);
        }

        private static void BuffsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            selector.FilteredItems = e.NewValue == null ? null : ((IReadOnlyList<Buff>)e.NewValue).Where(buff => selector.Filter.Matches(buff)).ToList();
        }

        private static void FilterProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            if (selector.Buffs == null)
            {
                return;
            }
            BuffsFilter filter = (BuffsFilter)e.NewValue;
            selector.FilteredItems = selector.Buffs.Where(buff => filter.Matches(buff)).ToList();
        }
    }
}
