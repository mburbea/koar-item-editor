using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs
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
            if (this._window == null && (this._window = Window.GetWindow(this)) != null)
            {
                this._window.PreviewKeyDown += this.Window_PreviewKeyDown;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.ScrollToSelectedItem();
        }

        private TabItem? GetItemByOffset(double offset)
        {
            TabItem? last = default;
            double width = 0d;
            foreach (TabItem tabItem in this.GetTabItems())
            {
                if ((width += (last = tabItem).ActualWidth) >= offset)
                {
                    return tabItem;
                }
            }
            return last;
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
            double offset = Math.Max(this._scrollViewer.HorizontalOffset - 2d, 0d);
            TabItem? tabItem = this.GetItemByOffset(offset);
            if (tabItem != null)
            {
                this.ScrollToItem(tabItem);
            }
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._scrollViewer == null || this._headerPanel == null)
            {
                return;
            }
            double offset = Math.Min(this._scrollViewer.HorizontalOffset + this._scrollViewer.ViewportWidth + 2d, this._scrollViewer.ExtentWidth);
            TabItem? tabItem = this.GetItemByOffset(offset);
            if (tabItem != null)
            {
                this.ScrollToItem(tabItem);
            }
        }

        private void ScrollToItem(TabItem item)
        {
            if (this._scrollViewer == null)
            {
                return;
            }
            double precedingItemsWidth = this.GetTabItems().TakeWhile(tabItem => tabItem != item).Sum(tabItem => tabItem.ActualWidth);
            if (precedingItemsWidth + item.ActualWidth > this._scrollViewer.HorizontalOffset + this._scrollViewer.ViewportWidth)
            {
                this._scrollViewer.ScrollToHorizontalOffset(precedingItemsWidth + item.ActualWidth - this._scrollViewer.ViewportWidth);
            }
            else if (precedingItemsWidth < this._scrollViewer.HorizontalOffset)
            {
                this._scrollViewer.ScrollToHorizontalOffset(precedingItemsWidth);
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
            if (!this.IsVisible || this.Items.IsEmpty || e.Key != Key.Tab || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
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
