using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemsView : Control
    {
        public static readonly DependencyProperty AllItemsUnsellableProperty = DependencyProperty.Register(nameof(ItemsView.AllItemsUnsellable), typeof(bool?), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty EditItemHexCommandProperty = DependencyProperty.Register(nameof(ItemsView.EditItemHexCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemsView.Items), typeof(IEnumerable<ItemModel>), typeof(ItemsView));

        public static readonly DependencyProperty MakeAllItemsDistinctCommandProperty = DependencyProperty.Register(nameof(ItemsView.MakeAllItemsDistinctCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemsView.SelectedItem), typeof(ItemModel), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectRowOnClickProperty = DependencyProperty.RegisterAttached("SelectRowOnClick", typeof(bool), typeof(ItemsView),
            new PropertyMetadata(BooleanBoxes.False, ItemsView.SelectRowOnClickProperty_ValueChanged));

        private ListView? _listView;

        static ItemsView() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsView), new FrameworkPropertyMetadata(typeof(ItemsView)));

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

        public static bool GetSelectRowOnClick(FrameworkElement? element)
        {
            return element != null && (bool)element.GetValue(ItemsView.SelectRowOnClickProperty);
        }

        public static void SetSelectRowOnClick(FrameworkElement? element, bool value)
        {
            element?.SetValue(ItemsView.SelectRowOnClickProperty, BooleanBoxes.GetBox(value));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if ((this._listView = this.Template.FindName("PART_ListView", this) as ListView) != null)
            {
                ListViewAutoSize.AutoSizeColumns(this._listView);
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
