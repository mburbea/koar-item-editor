using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemsView : Control
    {
        public static readonly DependencyProperty AllItemsUnsellableProperty = DependencyProperty.Register(nameof(ItemsView.AllItemsUnsellable), typeof(bool?), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AllItemsUnstashableProperty = DependencyProperty.Register(nameof(ItemsView.AllItemsUnstashable), typeof(bool?), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(nameof(ItemsView.DoubleClickCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemsView.Items), typeof(IList), typeof(ItemsView),
            new PropertyMetadata(ItemsView.ItemsProperty_ValueChanged));

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(ItemsView),
            new PropertyMetadata());

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemsView.SelectedItem), typeof(ItemModel), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectRowOnClickProperty = DependencyProperty.RegisterAttached("SelectRowOnClick", typeof(bool), typeof(ItemsView),
            new PropertyMetadata(BooleanBoxes.False, ItemsView.SelectRowOnClickProperty_ValueChanged));

        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached(nameof(ItemsView.SortDirection), typeof(ListSortDirection), typeof(ItemsView),
            new PropertyMetadata());

        public static readonly DependencyProperty SortPropertyProperty = DependencyProperty.RegisterAttached(nameof(ItemsView.SortProperty), typeof(string), typeof(ItemsView),
            new PropertyMetadata(nameof(ItemModel.Level)));

        private static readonly DependencyPropertyKey _collectionViewProperty = DependencyProperty.RegisterReadOnly(nameof(ItemsView.CollectionView), typeof(CollectionView), typeof(ItemsView),
            new PropertyMetadata(ItemsView.CollectionView_ValueChanged));

        private ListView? _listView;

        static ItemsView() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsView), new FrameworkPropertyMetadata(typeof(ItemsView)));

        public static DependencyProperty CollectionViewProperty => ItemsView._collectionViewProperty.DependencyProperty;

        public bool? AllItemsUnsellable
        {
            get => (bool?)this.GetValue(ItemsView.AllItemsUnsellableProperty);
            set => this.SetValue(ItemsView.AllItemsUnsellableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public bool? AllItemsUnstashable
        {
            get => (bool?)this.GetValue(ItemsView.AllItemsUnstashableProperty);
            set => this.SetValue(ItemsView.AllItemsUnstashableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public ICollectionView? CollectionView
        {
            get => (ICollectionView?)this.GetValue(ItemsView.CollectionViewProperty);
            private set => this.SetValue(ItemsView._collectionViewProperty, value);
        }

        public ICommand? DoubleClickCommand
        {
            get => (ICommand?)this.GetValue(ItemsView.DoubleClickCommandProperty);
            set => this.SetValue(ItemsView.DoubleClickCommandProperty, value);
        }

        public IEnumerable<ItemModel>? Items
        {
            get => (IEnumerable<ItemModel>?)this.GetValue(ItemsView.ItemsProperty);
            set => this.SetValue(ItemsView.ItemsProperty, value);
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

        public string SortProperty
        {
            get => (string)this.GetValue(ItemsView.SortPropertyProperty);
            set => this.SetValue(ItemsView.SortPropertyProperty, value);
        }

        public static string? GetPropertyName(GridViewColumn column) => (string?)column?.GetValue(ItemsView.PropertyNameProperty);

        public static bool GetSelectRowOnClick(UIElement? element)
        {
            return element != null && (bool)element.GetValue(ItemsView.SelectRowOnClickProperty);
        }

        public static void SetPropertyName(GridViewColumn column, string? value) => column?.SetValue(ItemsView.PropertyNameProperty, value);

        public static void SetSelectRowOnClick(UIElement? element, bool value)
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
            string? propertyName = ItemsView.GetPropertyName(header.Column);
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
            ItemsView itemsView = (ItemsView)d;
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
            ItemsView itemsView = (ItemsView)d;
            itemsView.CollectionView = e.NewValue == null ? null : new ListCollectionView((IList)e.NewValue)
            {
                SortDescriptions =
                {
                    new SortDescription(nameof(ItemModel.Category), ListSortDirection.Ascending),
                    new SortDescription(itemsView.SortProperty, itemsView.SortDirection),
                    new SortDescription(nameof(ItemModel.DisplayName), ListSortDirection.Ascending)
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
                WeakEventManager<UIElement, RoutedEventArgs>.RemoveHandler(element, nameof(UIElement.PreviewMouseLeftButtonDown), ItemsView.Element_PreviewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(element, nameof(UIElement.PreviewMouseLeftButtonDown), ItemsView.Element_PreviewMouseLeftButtonDown);
            }
        }
    }
}
