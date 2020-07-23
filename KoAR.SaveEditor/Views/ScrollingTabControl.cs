using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    public sealed class ScrollingTabControl : TabControl
    {
        private Panel? _headerPanel;
        private RepeatButton? _leftButton;
        private RepeatButton? _rightButton;
        private ScrollViewer? _scrollViewer;
        private Window? _window;

        static ScrollingTabControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollingTabControl), new FrameworkPropertyMetadata(typeof(ScrollingTabControl)));

        public override void OnApplyTemplate()
        {
            if (this._leftButton != null)
            {
                this._leftButton.Click -= this.LeftButton_Click;
            }
            if (this._rightButton != null)
            {
                this._rightButton.Click -= this.RightButton_Click;
            }
            if (this._scrollViewer != null)
            {
                this._scrollViewer.Loaded -= this.ScrollViewer_Loaded;
                this._scrollViewer.ScrollChanged -= this.ScrollViewer_ScrollChanged;
            }
            base.OnApplyTemplate();
            this._headerPanel = this.Template.FindName("PART_HeaderPanel", this) as Panel;
            if ((this._leftButton = this.Template.FindName("PART_LeftButton", this) as RepeatButton) != null)
            {
                this._leftButton.Click += this.LeftButton_Click;
            }
            if ((this._rightButton = this.Template.FindName("PART_RightButton", this) as RepeatButton) != null)
            {
                this._rightButton.Click += this.RightButton_Click;
            }
            if ((this._scrollViewer = this.Template.FindName("PART_ScrollViewer", this) as ScrollViewer) != null)
            {
                this._scrollViewer.Loaded += this.ScrollViewer_Loaded;
                this._scrollViewer.ScrollChanged += this.ScrollViewer_ScrollChanged;
            }
            if (this._window != null)
            {
                return;
            }
            this._window = Window.GetWindow(this);
            this._window.PreviewKeyDown += this.Window_PreviewKeyDown;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.ScrollToSelectedItem();
        }

        private TabItem? GetItemByOffset(double offset)
        {
            List<TabItem> tabItems = this.GetTabItems().ToList();
            double width = default;
            foreach (TabItem tabItem in tabItems)
            {
                if (width + tabItem.ActualWidth >= offset)
                {
                    return tabItem;
                }
                width += tabItem.ActualWidth;
            }
            return tabItems.LastOrDefault();
        }

        private IEnumerable<TabItem> GetTabItems()
        {
            foreach (object item in this.Items)
            {
                if (this.ItemContainerGenerator.ContainerFromItem(item) is TabItem tabItem)
                {
                    yield return tabItem;
                }
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._scrollViewer == null || this._headerPanel == null)
            {
                return;
            }
            double offset = Math.Max(this._scrollViewer.HorizontalOffset - this._headerPanel.Margin.Left - 2d, 0d);
            TabItem? leftItem = this.GetItemByOffset(offset);
            if (leftItem != null)
            {
                this.ScrollToItem(leftItem);
            }
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._scrollViewer == null || this._headerPanel == null)
            {
                return;
            }
            double offset = Math.Min(this._scrollViewer.HorizontalOffset + this._scrollViewer.ViewportWidth + this._headerPanel.Margin.Left + 2d, this._scrollViewer.ExtentWidth);
            TabItem? rightItem = this.GetItemByOffset(offset);
            if (rightItem != null)
            {
                this.ScrollToItem(rightItem);
            }
        }

        private void ScrollToItem(TabItem tabItem)
        {
            if (this._scrollViewer == null || this._headerPanel == null)
            {
                return;
            }
            double leftItemsWidth = 0d;
            bool anySelected = false;
            foreach (TabItem leftItem in this.GetTabItems().TakeWhile(item => item != tabItem))
            {
                anySelected = anySelected || leftItem.IsSelected;
                leftItemsWidth += leftItem.ActualWidth;
            }
            if (leftItemsWidth + tabItem.ActualWidth > this._scrollViewer.HorizontalOffset + this._scrollViewer.ViewportWidth)
            {
                double offset = leftItemsWidth + tabItem.ActualWidth - this._scrollViewer.ViewportWidth;
                if (anySelected || tabItem.IsSelected)
                {
                    offset += this._headerPanel.Margin.Left + this._headerPanel.Margin.Right;
                }
                this._scrollViewer.ScrollToHorizontalOffset(offset);
            }
            else if (leftItemsWidth < this._scrollViewer.HorizontalOffset)
            {
                double offset = leftItemsWidth;
                if (anySelected)
                {
                    offset -= (this._headerPanel.Margin.Left + this._headerPanel.Margin.Right);
                }
                this._scrollViewer.ScrollToHorizontalOffset(offset);
            }
        }

        private void ScrollToSelectedItem()
        {
            if (this._scrollViewer != null && this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem) is TabItem tabItem)
            {
                if (tabItem.IsLoaded)
                {
                    this.ScrollToItem(tabItem);
                }
                else
                {
                    tabItem.Loaded += this.TabItem_Loaded;
                }
            }
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.Loaded -= this.ScrollViewer_Loaded;
            this.UpdateScrollButtonsAvailability();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) => this.UpdateScrollButtonsAvailability();

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = (TabItem)sender;
            tabItem.Loaded -= this.TabItem_Loaded;
            this.ScrollToSelectedItem();
        }

        private void UpdateScrollButtonsAvailability()
        {
            if (this._scrollViewer == null)
            {
                return;
            }
            double offset = Math.Max(this._scrollViewer.HorizontalOffset, 0d);
            double width = Math.Max(this._scrollViewer.ScrollableWidth, 0d);
            if (this._leftButton != null)
            {
                this._leftButton.Visibility = width == 0d ? Visibility.Collapsed : Visibility.Visible;
                this._leftButton.IsEnabled = offset > 0d;
            }
            if (this._rightButton != null)
            {
                this._rightButton.Visibility = width == 0d ? Visibility.Collapsed : Visibility.Visible;
                this._rightButton.IsEnabled = offset < width;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.Items.IsEmpty || e.Key != Key.Tab || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }
            int index = this.SelectedIndex;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                this.SelectedIndex = --index < 0 ? this.Items.Count - 1 : index;
            }
            else
            {
                this.SelectedIndex = ++index == this.Items.Count ? 0 : index;
            }
            e.Handled = true;
        }
    }
}
