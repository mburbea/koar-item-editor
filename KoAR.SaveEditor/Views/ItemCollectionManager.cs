using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemCollectionManager : Control
    {
        public static readonly DependencyProperty AllItemsStolenProperty = DependencyProperty.Register(nameof(ItemCollectionManager.AllItemsStolen), typeof(bool?), typeof(ItemCollectionManager),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AllItemsUnsellableProperty = DependencyProperty.Register(nameof(ItemCollectionManager.AllItemsUnsellable), typeof(bool?), typeof(ItemCollectionManager),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AllItemsUnstashableProperty = DependencyProperty.Register(nameof(ItemCollectionManager.AllItemsUnstashable), typeof(bool?), typeof(ItemCollectionManager),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty CollectionViewProperty;

        public static readonly DependencyProperty DeleteItemCommandProperty = DependencyProperty.Register(nameof(ItemCollectionManager.DeleteItemCommand), typeof(ICommand), typeof(ItemCollectionManager));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemCollectionManager.Items), typeof(IReadOnlyList<ItemModelBase>), typeof(ItemCollectionManager),
            new PropertyMetadata(ItemCollectionManager.ItemsProperty_ValueChanged));

        public static readonly DependencyProperty ModeProperty;

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(ItemCollectionManager),
            new PropertyMetadata());

        public static readonly DependencyProperty RowDoubleClickCommandProperty = DependencyProperty.Register(nameof(ItemCollectionManager.RowDoubleClickCommand), typeof(ICommand), typeof(ItemCollectionManager));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemCollectionManager.SelectedItem), typeof(object), typeof(ItemCollectionManager),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectRowOnClickProperty = DependencyProperty.RegisterAttached("SelectRowOnClick", typeof(bool), typeof(ItemCollectionManager),
            new PropertyMetadata(BooleanBoxes.False, ItemCollectionManager.SelectRowOnClickProperty_ValueChanged));

        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached(nameof(ItemCollectionManager.SortDirection), typeof(ListSortDirection), typeof(ItemCollectionManager),
            new PropertyMetadata());

        public static readonly DependencyProperty SortPropertyProperty = DependencyProperty.RegisterAttached(nameof(ItemCollectionManager.SortProperty), typeof(string), typeof(ItemCollectionManager),
            new PropertyMetadata(nameof(ItemModelBase.Level)));

        private static readonly DependencyPropertyKey _collectionViewPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemCollectionManager.CollectionView), typeof(CollectionView), typeof(ItemCollectionManager),
            new PropertyMetadata(ItemCollectionManager.CollectionView_ValueChanged));

        private static readonly DependencyPropertyKey _modePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemCollectionManager.Mode), typeof(ManagementMode), typeof(ItemCollectionManager),
            new PropertyMetadata());

        private ListView? _listView;

        static ItemCollectionManager()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemCollectionManager), new FrameworkPropertyMetadata(typeof(ItemCollectionManager)));
            ItemCollectionManager.CollectionViewProperty = ItemCollectionManager._collectionViewPropertyKey.DependencyProperty;
            ItemCollectionManager.ModeProperty = ItemCollectionManager._modePropertyKey.DependencyProperty;
        }

        public bool? AllItemsStolen
        {
            get => (bool?)this.GetValue(ItemCollectionManager.AllItemsStolenProperty);
            set => this.SetValue(ItemCollectionManager.AllItemsStolenProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public bool? AllItemsUnsellable
        {
            get => (bool?)this.GetValue(ItemCollectionManager.AllItemsUnsellableProperty);
            set => this.SetValue(ItemCollectionManager.AllItemsUnsellableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public bool? AllItemsUnstashable
        {
            get => (bool?)this.GetValue(ItemCollectionManager.AllItemsUnstashableProperty);
            set => this.SetValue(ItemCollectionManager.AllItemsUnstashableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public ICollectionView? CollectionView
        {
            get => (ICollectionView?)this.GetValue(ItemCollectionManager.CollectionViewProperty);
            private set => this.SetValue(ItemCollectionManager._collectionViewPropertyKey, value);
        }

        public ICommand? DeleteItemCommand
        {
            get => (ICommand?)this.GetValue(ItemCollectionManager.DeleteItemCommandProperty);
            set => this.SetValue(ItemCollectionManager.DeleteItemCommandProperty, value);
        }

        public IReadOnlyList<ItemModelBase>? Items
        {
            get => (IReadOnlyList<ItemModelBase>?)this.GetValue(ItemCollectionManager.ItemsProperty);
            set => this.SetValue(ItemCollectionManager.ItemsProperty, value);
        }

        public ManagementMode Mode
        {
            get => (ManagementMode)this.GetValue(ItemCollectionManager.ModeProperty);
            private set => this.SetValue(ItemCollectionManager._modePropertyKey, value);
        }

        public ICommand? RowDoubleClickCommand
        {
            get => (ICommand?)this.GetValue(ItemCollectionManager.RowDoubleClickCommandProperty);
            set => this.SetValue(ItemCollectionManager.RowDoubleClickCommandProperty, value);
        }

        public object? SelectedItem
        {
            get => (object?)this.GetValue(ItemCollectionManager.SelectedItemProperty);
            set => this.SetValue(ItemCollectionManager.SelectedItemProperty, value);
        }

        public ListSortDirection SortDirection
        {
            get => (ListSortDirection)this.GetValue(ItemCollectionManager.SortDirectionProperty);
            set => this.SetValue(ItemCollectionManager.SortDirectionProperty, value);
        }

        public string SortProperty
        {
            get => (string)this.GetValue(ItemCollectionManager.SortPropertyProperty);
            set => this.SetValue(ItemCollectionManager.SortPropertyProperty, value);
        }

        public static string? GetPropertyName(GridViewColumn column) => (string?)column?.GetValue(ItemCollectionManager.PropertyNameProperty);

        public static bool GetSelectRowOnClick(UIElement? element)
        {
            return element != null && (bool)element.GetValue(ItemCollectionManager.SelectRowOnClickProperty);
        }

        public static void SetPropertyName(GridViewColumn column, string? value) => column?.SetValue(ItemCollectionManager.PropertyNameProperty, value);

        public static void SetSelectRowOnClick(UIElement? element, bool value)
        {
            element?.SetValue(ItemCollectionManager.SelectRowOnClickProperty, BooleanBoxes.GetBox(value));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if ((this._listView = this.Template.FindName("PART_ListView", this) as ListView) == null)
            {
                return;
            }
            ListViewAutoSize.AutoSizeColumns(this._listView);
            this._listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.GridViewColumn_Click));
        }

        private void CollectionView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => this.Dispatcher.InvokeAsync(this.OnViewChanged);

        private void GridViewColumn_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView? view;
            if (!(e.OriginalSource is GridViewColumnHeader header) || (view = this.CollectionView) == null || view.SortDescriptions == null)
            {
                return;
            }
            string? propertyName = ItemCollectionManager.GetPropertyName(header.Column);
            if (propertyName == null)
            {
                return;
            }
            SortDescription current = view.SortDescriptions[1];
            view.SortDescriptions[1] = new SortDescription(
                this.SortProperty = propertyName,
                this.SortDirection = propertyName == current.PropertyName
                    ? (ListSortDirection)((int)current.Direction ^ 1)
                    : ListSortDirection.Ascending
            );
        }

        private void OnViewChanged()
        {
            if (this._listView == null)
            {
                return;
            }
            ListViewAutoSize.AutoSizeColumns(this._listView);
            ListCollectionView collectionView = (ListCollectionView)this._listView.ItemsSource;
            if (!collectionView.IsEmpty)
            {
                this._listView.ScrollIntoView(collectionView.GetItemAt(0));
            }
        }

        private static void CollectionView_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemCollectionManager itemsView = (ItemCollectionManager)d;
            if (e.OldValue != null)
            {
                CollectionChangedEventManager.RemoveHandler((ListCollectionView)e.OldValue, itemsView.CollectionView_CollectionChanged);
            }
            if (e.NewValue != null)
            {
                CollectionChangedEventManager.AddHandler((ListCollectionView)e.NewValue, itemsView.CollectionView_CollectionChanged);
                itemsView.Dispatcher.InvokeAsync(itemsView.OnViewChanged);
            }
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
            ItemCollectionManager itemsView = (ItemCollectionManager)d;
            itemsView.Mode = e.NewValue is IEnumerable<ItemModelBase<StashItem>> ? ManagementMode.StashItem : ManagementMode.Item;
            itemsView.CollectionView = e.NewValue == null ? null : new ListCollectionView((IList)e.NewValue)
            {
                SortDescriptions =
                {
                    new SortDescription(nameof(ItemModelBase.Category), ListSortDirection.Ascending),
                    new SortDescription(itemsView.SortProperty, itemsView.SortDirection),
                    new SortDescription(nameof(ItemModelBase.DisplayName), ListSortDirection.Ascending)
                }
            };
        }

        private static void SelectRowOnClickProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element))
            {
                return;
            }
            if ((bool)e.OldValue)
            {
                WeakEventManager<UIElement, RoutedEventArgs>.RemoveHandler(element, nameof(UIElement.PreviewMouseLeftButtonDown), ItemCollectionManager.Element_PreviewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(element, nameof(UIElement.PreviewMouseLeftButtonDown), ItemCollectionManager.Element_PreviewMouseLeftButtonDown);
            }
        }

        public enum ManagementMode
        {
            Item = 0,
            StashItem = 1
        }
    }
}
