using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemsView : Control
    {
        public static readonly DependencyProperty AllItemsUnsellableProperty = DependencyProperty.Register(nameof(ItemsView.AllItemsUnsellable), typeof(bool?), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly RoutedCommand AutoSizeColumnsCommand = new RoutedUICommand("AutoSize Columns", nameof(ItemsView.AutoSizeColumnsCommand), typeof(ItemsView));

        public static readonly DependencyProperty EditItemHexCommandProperty = DependencyProperty.Register(nameof(ItemsView.EditItemHexCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemsView.Items), typeof(IEnumerable<ItemModel>), typeof(ItemsView));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemsView.SelectedItem), typeof(ItemModel), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectRowOnClickProperty = DependencyProperty.RegisterAttached("SelectRowOnClick", typeof(bool), typeof(ItemsView),
            new PropertyMetadata(BooleanBoxes.False, ItemsView.SelectRowOnClickProperty_ValueChanged));

        private ListView? _listView;

        static ItemsView()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsView), new FrameworkPropertyMetadata(typeof(ItemsView)));
            CommandManager.RegisterClassCommandBinding(typeof(ItemsView), new CommandBinding(ItemsView.AutoSizeColumnsCommand, ItemsView.AutoSizeColumns_Executed));
        }

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
                this.AutoSizeAllColumns();
            }
        }

        private static void AutoSizeColumns_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((ItemsView)sender).AutoSizeAllColumns();
            e.Handled = true;
        }

        private static void Element_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            if (element is ListViewItem listViewItem)
            {
                listViewItem.IsSelected = true;
                return;
            }
            ItemsView? view = default;
            DependencyObject? d = element;
            while (d != null && (view = d as ItemsView) == null)
            {
                d = VisualTreeHelper.GetParent(d);
            }
            if (view?._listView?.ItemContainerGenerator.ContainerFromItem(element.DataContext) is ListViewItem item)
            {
                item.IsSelected = true;
            }
        }

        private static void SelectRowOnClickProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement checkBox)
            {
                if ((bool)e.OldValue)
                {
                    WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(checkBox, nameof(FrameworkElement.PreviewMouseLeftButtonDown), ItemsView.Element_PreviewMouseLeftButtonDown);
                }
                if ((bool)e.NewValue)
                {
                    WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(checkBox, nameof(FrameworkElement.PreviewMouseLeftButtonDown), ItemsView.Element_PreviewMouseLeftButtonDown);
                }
            }
        }

        private void AutoSizeAllColumns()
        {
            if (!(this._listView?.View is GridView view))
            {
                return;
            }
            foreach (GridViewColumn column in view.Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }
                column.Width = double.NaN;
            }
        }
    }
}
