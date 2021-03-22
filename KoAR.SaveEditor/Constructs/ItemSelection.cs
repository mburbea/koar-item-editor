using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs
{
    public static class ItemSelection
    {
        public static readonly DependencyProperty SelectItemOnClickProperty = DependencyProperty.RegisterAttached("SelectItemOnClick", typeof(bool), typeof(ItemSelection),
            new(BooleanBoxes.False, ItemSelection.SelectItemOnClickProperty_ValueChanged));

        public static bool GetSelectItemOnClick(ListBoxItem item)
        {
            return item != null && (bool)item.GetValue(ItemSelection.SelectItemOnClickProperty);
        }

        public static void SetSelectItemOnClick(ListBoxItem item, bool value)
        {
            item?.SetValue(ItemSelection.SelectItemOnClickProperty, BooleanBoxes.GetBox(value));
        }

        private static void Item_PreviewMouseLeftButtonDown(object? sender, RoutedEventArgs e)
        {
            DependencyObject d = (DependencyObject)sender!;
            ListBoxItem? item = d as ListBoxItem ?? d.FindVisualTreeAncestor<ListBoxItem>();
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        private static void SelectItemOnClickProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem item)
            {
                return;
            }
            if ((bool)e.OldValue)
            {
                WeakEventManager<UIElement, RoutedEventArgs>.RemoveHandler(item, nameof(UIElement.PreviewMouseLeftButtonDown), ItemSelection.Item_PreviewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(item, nameof(UIElement.PreviewMouseLeftButtonDown), ItemSelection.Item_PreviewMouseLeftButtonDown);
            }
        }
    }
}
