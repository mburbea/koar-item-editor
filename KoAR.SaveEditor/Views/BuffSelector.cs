using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        public static DependencyProperty CollectionViewProperty;

        private static readonly DependencyPropertyKey _collectionViewPropertyKey = DependencyProperty.RegisterReadOnly(nameof(BuffSelector.CollectionView), typeof(ICollectionView), typeof(BuffSelector),
            new PropertyMetadata());

        static BuffSelector()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffSelector), new FrameworkPropertyMetadata(typeof(BuffSelector)));
            CollectionViewProperty = _collectionViewPropertyKey.DependencyProperty;
        }

        public IReadOnlyDictionary<uint, Buff>? Buffs
        {
            get => (IReadOnlyDictionary<uint, Buff>?)this.GetValue(BuffSelector.BuffsProperty);
            set => this.SetValue(BuffSelector.BuffsProperty, value);
        }

        public ICollectionView? CollectionView
        {
            get => (ICollectionView?)this.GetValue(BuffSelector.CollectionViewProperty);
            private set => this.SetValue(BuffSelector._collectionViewPropertyKey, value);
        }

        public BuffsFilter Filter
        {
            get => (BuffsFilter)this.GetValue(BuffSelector.FilterProperty);
            set => this.SetValue(BuffSelector.FilterProperty, value);
        }

        public Buff? SelectedBuff
        {
            get => (Buff?)this.GetValue(BuffSelector.SelectedBuffProperty);
            set => this.SetValue(BuffSelector.SelectedBuffProperty, value);
        }

        private static void BuffsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            selector.CollectionView = e.NewValue == null ? null : new ListCollectionView((IList)e.NewValue)
            {
                Filter = selector.IncludeItem,
                SortDescriptions =
                {
                    new SortDescription(nameof(Buff.ShortDisplayText), ListSortDirection.Ascending)
                }
            };
        }

        private static void FilterProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuffSelector selector = (BuffSelector)d;
            if (selector.CollectionView != null)
            {
                selector.CollectionView.Filter = selector.IncludeItem;
            }
        }

        private bool IncludeItem(object item)
        {
            return item is Buff buff && this.Filter switch
            {
                BuffsFilter.Prefix => (buff.BuffType & BuffTypes.Prefix) == BuffTypes.Prefix,
                BuffsFilter.Suffix => (buff.BuffType & BuffTypes.Suffix) == BuffTypes.Suffix,
                BuffsFilter.Item => buff.ApplyType == ApplyType.OnObject,
                _ => true,
            };
        }
    }
}
