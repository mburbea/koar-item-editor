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

        public static readonly RoutedCommand AutoSizeColumnsCommand = new RoutedCommand();

        public static readonly DependencyProperty EditItemHexCommandProperty = DependencyProperty.Register(nameof(ItemsView.EditItemHexCommand), typeof(ICommand), typeof(ItemsView));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemsView.Items), typeof(IEnumerable<ItemModel>), typeof(ItemsView));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemsView.SelectedItem), typeof(ItemModel), typeof(ItemsView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectRowOnClickProperty = DependencyProperty.RegisterAttached("SelectRowOnClick", typeof(bool), typeof(ItemsView),
            new PropertyMetadata(ItemsView.SelectRowOnClickProperty_ValueChanged));

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

        public static bool GetSelectRowOnClick(CheckBox? checkBox)
        {
            return checkBox != null && (bool)checkBox.GetValue(ItemsView.SelectRowOnClickProperty);
        }

        public static void SetSelectRowOnClick(CheckBox? checkBox, bool value)
        {
            checkBox?.SetValue(ItemsView.SelectRowOnClickProperty, BooleanBoxes.GetBox(value));
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

        private static void CheckBox_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            ItemsView? view = default;
            DependencyObject? d = checkBox;
            while (d != null && (view = d as ItemsView) == null)
            {
                d = VisualTreeHelper.GetParent(d);
            }
            if (view?._listView?.ItemContainerGenerator.ContainerFromItem(checkBox.DataContext) is ListViewItem item)
            {
                item.IsSelected = true;
            }
        }

        private static void SelectRowOnClickProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)d;
            if ((bool)e.OldValue)
            {
                WeakEventManager<CheckBox, RoutedEventArgs>.RemoveHandler(checkBox, nameof(CheckBox.PreviewMouseLeftButtonDown), ItemsView.CheckBox_PreviewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<CheckBox, RoutedEventArgs>.AddHandler(checkBox, nameof(CheckBox.PreviewMouseLeftButtonDown), ItemsView.CheckBox_PreviewMouseLeftButtonDown);
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
