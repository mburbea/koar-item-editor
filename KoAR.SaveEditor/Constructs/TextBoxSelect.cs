using System.Windows;
using System.Windows.Controls.Primitives;

namespace KoAR.SaveEditor.Constructs
{
    public static class TextBoxSelect
    {
        public static readonly DependencyProperty SelectOnFocusProperty = DependencyProperty.RegisterAttached("SelectOnFocus", typeof(bool), typeof(TextBoxSelect),
            new(BooleanBoxes.False, TextBoxSelect.SelectOnFocusProperty_ValueChanged));

        private static readonly RoutedEventHandler _gotKeyboardFocus = (sender, e) => ((TextBoxBase)sender).SelectAll();

        private static readonly RoutedEventHandler _previewMouseDoubleClick = (sender, e) =>
        {
            ((TextBoxBase)sender).SelectAll();
            e.Handled = true;
        };

        private static readonly RoutedEventHandler _previewMouseLeftButtonDown = (sender, e) =>
        {
            if (e.OriginalSource is not DependencyObject d)
            {
                return;
            }
            TextBoxBase? textBox = d as TextBoxBase ?? d.FindVisualTreeAncestor<TextBoxBase>();
            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                e.Handled = true;
            }
        };

        public static bool GetSelectOnFocus(TextBoxBase textBox) => textBox != null && (bool)textBox.GetValue(TextBoxSelect.SelectOnFocusProperty);

        public static void SetSelectOnFocus(TextBoxBase textBox, bool value) => textBox?.SetValue(TextBoxSelect.SelectOnFocusProperty, BooleanBoxes.GetBox(value));

        private static void SelectOnFocusProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBoxBase textBox)
            {
                return;
            }
            if ((bool)e.OldValue)
            {
                textBox.RemoveHandler(TextBoxBase.GotKeyboardFocusEvent, TextBoxSelect._gotKeyboardFocus);
                textBox.RemoveHandler(TextBoxBase.PreviewMouseDoubleClickEvent, TextBoxSelect._previewMouseDoubleClick);
                textBox.RemoveHandler(TextBoxBase.PreviewMouseLeftButtonDownEvent, TextBoxSelect._previewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                textBox.AddHandler(TextBoxBase.GotKeyboardFocusEvent, TextBoxSelect._gotKeyboardFocus, true);
                textBox.AddHandler(TextBoxBase.PreviewMouseDoubleClickEvent, TextBoxSelect._previewMouseDoubleClick, true);
                textBox.AddHandler(TextBoxBase.PreviewMouseLeftButtonDownEvent, TextBoxSelect._previewMouseLeftButtonDown, true);
            }
        }
    }
}