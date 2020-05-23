using System.Windows;
using System.Windows.Controls.Primitives;

namespace KoAR.SaveEditor.Constructs
{
    public static class TextBoxSelect
    {
        public static readonly DependencyProperty SelectOnFocusProperty = DependencyProperty.RegisterAttached("SelectOnFocus", typeof(bool), typeof(TextBoxSelect),
            new PropertyMetadata(BooleanBoxes.False, TextBoxSelect.SelectOnFocusProperty_ValueChanged));

        private static readonly RoutedEventHandler _previewMouseLeftButtonDown = (sender, e) =>
        {
            if (!(e.OriginalSource is DependencyObject d))
            {
                return;
            }
            TextBoxBase? textBox = d as TextBoxBase ?? d.FindVisualTreeAncestor<TextBoxBase>();
            if (textBox == null)
            {
                return;
            }
            if (!textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                e.Handled = true;
            }
        };

        private static readonly RoutedEventHandler _textBoxMouseDoubleClick = (sender, e) =>
        {
            ((TextBoxBase)sender).SelectAll();
            e.Handled = true;
        };

        private static readonly RoutedEventHandler _textBoxSelectAll = (sender, e) => ((TextBoxBase)sender).SelectAll();

        public static bool GetSelectOnFocus(TextBoxBase textBox) => textBox != null && (bool)textBox.GetValue(TextBoxSelect.SelectOnFocusProperty);

        public static void SetSelectOnFocus(TextBoxBase textBox, bool value) => textBox?.SetValue(TextBoxSelect.SelectOnFocusProperty, BooleanBoxes.GetBox(value));

        private static void SelectOnFocusProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBoxBase textBox))
            {
                return;
            }
            if ((bool)e.OldValue)
            {
                textBox.RemoveHandler(TextBoxBase.GotKeyboardFocusEvent, TextBoxSelect._textBoxSelectAll);
                textBox.RemoveHandler(TextBoxBase.PreviewMouseDoubleClickEvent, TextBoxSelect._textBoxMouseDoubleClick);
                textBox.RemoveHandler(TextBoxBase.PreviewMouseLeftButtonDownEvent, TextBoxSelect._previewMouseLeftButtonDown);
            }
            if ((bool)e.NewValue)
            {
                textBox.AddHandler(TextBoxBase.GotKeyboardFocusEvent, TextBoxSelect._textBoxSelectAll, true);
                textBox.AddHandler(TextBoxBase.PreviewMouseDoubleClickEvent, TextBoxSelect._textBoxMouseDoubleClick, true);
                textBox.AddHandler(TextBoxBase.PreviewMouseLeftButtonDownEvent, TextBoxSelect._previewMouseLeftButtonDown, true);
            }
        }
    }
}
