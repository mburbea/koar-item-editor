using System.Collections;
using System.Collections.Generic;
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
            new PropertyMetadata(nameof(ItemModel.Level)));

        private static readonly DependencyPropertyKey _collectionViewProperty = DependencyProperty.RegisterReadOnly(nameof(ItemsView.CollectionView), typeof(CollectionView), typeof(ItemsView),
            new PropertyMetadata());

        private ListView? _listView;

        static ItemsView() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsView), new FrameworkPropertyMetadata(typeof(ItemsView)));

        public static DependencyProperty CollectionViewProperty => ItemsView._collectionViewProperty.DependencyProperty;

        public bool? AllItemsUnsellable
        {
            get => (bool?)this.GetValue(ItemsView.AllItemsUnsellableProperty);
            set => this.SetValue(ItemsView.AllItemsUnsellableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
        }

        public ICollectionView? CollectionView
        {
            get => (ICollectionView?)this.GetValue(ItemsView.CollectionViewProperty);
            private set => this.SetValue(ItemsView._collectionViewProperty, value);
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
                    new SortDescription(nameof(ItemModel.ItemDisplayName), ListSortDirection.Ascending)
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
