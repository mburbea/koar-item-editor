﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemFiltersEditor : Control
    {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(ItemFiltersEditor.IsExpanded), typeof(bool), typeof(ItemFiltersEditor),
            new FrameworkPropertyMetadata(BooleanBoxes.True, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemFiltersProperty = DependencyProperty.Register(nameof(ItemFiltersEditor.ItemFilters), typeof(ItemFilters), typeof(ItemFiltersEditor));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemFiltersEditor.Items), typeof(IReadOnlyCollection<ItemModelBase>), typeof(ItemFiltersEditor));

        static ItemFiltersEditor() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemFiltersEditor), new FrameworkPropertyMetadata(typeof(ItemFiltersEditor)));

        public ItemFilters? ItemFilters
        {
            get => (ItemFilters?)this.GetValue(ItemFiltersEditor.ItemFiltersProperty);
            set => this.SetValue(ItemFiltersEditor.ItemFiltersProperty, value);
        }

        public IReadOnlyCollection<ItemModelBase>? Items
        {
            get => (IReadOnlyCollection<ItemModelBase>?)this.GetValue(ItemFiltersEditor.ItemsProperty);
            set => this.SetValue(ItemFiltersEditor.ItemsProperty, value);
        }

        public bool IsExpanded
        {
            get => (bool)this.GetValue(ItemFiltersEditor.IsExpandedProperty);
            set => this.SetValue(ItemFiltersEditor.IsExpandedProperty, BooleanBoxes.GetBox(value));
        }
    }
}