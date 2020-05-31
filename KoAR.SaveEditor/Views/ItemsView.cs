using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemsView : Control
    {
        public static readonly DependencyProperty AllItemsUnsellableProperty = DependencyProperty.Register(nameof(ItemsView.AllItemsUnsellable), typeof(bool?), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty EditItemHexCommandProperty = DependencyProperty.Register(nameof(ItemsView.EditItemHexCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemsView.Items), typeof(IList), typeof(ItemsView),
            new PropertyMetadata(ItemsView.ItemsProperty_ValueChanged));

        public static readonly DependencyProperty MakeAllItemsDistinctCommandProperty = DependencyProperty.Register(nameof(ItemsView.MakeAllItemsDistinctCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(ItemsView),
            new PropertyMetadata());

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemsView.SelectedItem), typeof(ItemModel), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectRowOnClickProperty = DependencyProperty.RegisterAttached("SelectRowOnClick", typeof(bool), typeof(ItemsView),
            new PropertyMetadata(BooleanBoxes.False, ItemsView.SelectRowOnClickProperty_ValueChanged));

        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached(nameof(ItemsView.SortDirection), typeof(ListSortDirection), typeof(ItemsView),
            new PropertyMetadata());

        public static readonly DependencyProperty SortPropertyProperty = DependencyProperty.RegisterAttached(nameof(ItemsView.SortProperty), typeof(string), typeof(ItemsView),
            new PropertyMetadata());

        private static readonly DependencyPropertyKey _itemsCollectionViewProperty = DependencyProperty.RegisterReadOnly(nameof(ItemsView.ItemsCollectionView), typeof(CollectionView), typeof(ItemsView),
            new PropertyMetadata());

        private ListView? _listView;

        static ItemsView() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsView), new FrameworkPropertyMetadata(typeof(ItemsView)));

        public static DependencyProperty ItemsCollectionViewProperty => ItemsView._itemsCollectionViewProperty.DependencyProperty;

        public bool? AllItemsUnsellable
        {
            get => (bool?)this.GetValue(ItemsView.AllItemsUnsellableProperty);
            set => this.SetValue(ItemsView.AllItemsUnsellableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public ICommand? EditItemHexCommand
        {
            get => (ICommand?)this.GetValue(ItemsView.EditItemHexCommandProperty);
            set => this.SetValue(ItemsView.EditItemHexCommandProperty, value);
        }

        public IEnumerable<ItemModel>? Items
        {
            get => (IEnumerable<ItemModel>?)this.GetValue(ItemsView.ItemsProperty);
            set => this.SetValue(ItemsView.ItemsProperty, value);
        }

        public ListCollectionView? ItemsCollectionView
        {
            get => (ListCollectionView?)this.GetValue(ItemsView.ItemsCollectionViewProperty);
            private set => this.SetValue(ItemsView._itemsCollectionViewProperty, value);
        }

        public ICommand? MakeAllItemsDistinctCommand
        {
            get => (ICommand?)this.GetValue(ItemsView.MakeAllItemsDistinctCommandProperty);
            set => this.SetValue(ItemsView.MakeAllItemsDistinctCommandProperty, value);
        }

        public ItemModel? SelectedItem
        {
            get => (ItemModel?)this.GetValue(ItemsView.SelectedItemProperty);
            set => this.SetValue(ItemsView.SelectedItemProperty, value);
        }

        public ListSortDirection SortDirection
        {
            get => (ListSortDirection)this.GetValue(ItemsView.SortDirectionProperty);
            set => this.SetValue(ItemsView.SortDirectionProperty, value);
        }

        public string? SortProperty
        {
            get => (string?)this.GetValue(ItemsView.SortPropertyProperty);
            set => this.SetValue(ItemsView.SortPropertyProperty, value);
        }

        public static string? GetPropertyName(GridViewColumn column) => (string?)column?.GetValue(ItemsView.PropertyNameProperty);

        public static bool GetSelectRowOnClick(FrameworkElement? element)
        {
            return element != null && (bool)element.GetValue(ItemsView.SelectRowOnClickProperty);
        }

        public static void SetPropertyName(GridViewColumn column, string? value) => column?.SetValue(ItemsView.PropertyNameProperty, value);

        public static void SetSelectRowOnClick(FrameworkElement? element, bool value)
        {
            element?.SetValue(ItemsView.SelectRowOnClickProperty, BooleanBoxes.GetBox(value));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if ((this._listView = this.Template.FindName("PART_ListView", this) as ListView) == null)
            {
                return;
            }
            ListViewAutoSize.AutoSizeColumns(this._listView);
            this._listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(this.GridViewColumn_Click));
        }

        private void GridViewColumn_Click(object sender, RoutedEventArgs e)
        {
            ListCollectionView? view;
            if (!(e.OriginalSource is GridViewColumnHeader header) || (view = this.ItemsCollectionView) == null || view.SortDescriptions == null)
            {
                return;
            }
            string? propertyName = ItemsView.GetPropertyName(header.Column);
            if (propertyName == null)
            {
                return;
            }
            SortDescription sort = view.SortDescriptions[1];
            view.SortDescriptions[1] = new SortDescription(
                this.SortProperty = propertyName,
                this.SortDirection = propertyName == sort.PropertyName ? (ListSortDirection)((int)sort.Direction ^ 1) : ListSortDirection.Ascending
            );
        }

        private static void Element_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ListViewItem? item = element as ListViewItem ?? element.FindVisualTreeAncestor<ListViewItem>();
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        private static void ItemsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsView view = (ItemsView)d;
            if (e.NewValue == null)
            {
                view.ItemsCollectionView = null;
                return;
            }
            view.ItemsCollectionView = new ListCollectionView((IList)e.NewValue)
            {
                GroupDescriptions =
                {
                    new PropertyGroupDescription(nameof(ItemModel.Category))
                },
                SortDescriptions = 
                {
                    new SortDescription(nameof(ItemModel.Category), ListSortDirection.Ascending),
                    view.SortProperty == null
                        ? new SortDescription(nameof(ItemModel.HasCustomName), ListSortDirection.Descending)
                        : new SortDescription(view.SortProperty, view.SortDirection)
                }
            };
        }

        private static void SelectRowOnClickProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element))
            {
                return;
            }
            if ((bool)e.OldValue)
            {
                WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(element, nameof(FrameworkElement.PreviewMouseLeftButtonDown), ItemsView.Element_PreviewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(element, nameof(FrameworkElement.PreviewMouseLeftButtonDown), ItemsView.Element_PreviewMouseLeftButtonDown);
            }
        }
    }
}
