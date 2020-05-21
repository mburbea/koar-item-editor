using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Constructs
{
    public static class TextBoxUpdateSource
    {
        public static readonly DependencyProperty UpdateOnEnterProperty = DependencyProperty.RegisterAttached("UpdateOnEnter", typeof(bool), typeof(TextBoxUpdateSource),
            new PropertyMetadata(BooleanBoxes.False, TextBoxUpdateSource.UpdateOnEnterProperty_ValueChanged));

        public static bool GetUpdateOnEnter(TextBox textBox) => textBox != null && (bool)textBox.GetValue(TextBoxUpdateSource.UpdateOnEnterProperty);

        public static void SetUpdateOnEnter(TextBox textBox, bool value) => textBox?.SetValue(TextBoxUpdateSource.UpdateOnEnterProperty, BooleanBoxes.GetBox(value));

        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }
            TextBox textBox = (TextBox)sender;
            BindingExpressionBase? expression = BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty);
            if (expression == null || !expression.ValidateWithoutUpdate())
            {
                return;
            }
            BindingMode mode = expression.ParentBindingBase switch
            {
                MultiBinding multiBinding => multiBinding.Mode,
                Binding binding => binding.Mode,
                _ => BindingMode.OneTime,
            };
            if (mode == BindingMode.Default || mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
            {
                expression.UpdateSource();
            }
        }

        private static void UpdateOnEnterProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
            {
                return;
            }
            if ((bool)e.OldValue)
            {
                WeakEventManager<TextBox, KeyEventArgs>.RemoveHandler(textBox, nameof(textBox.PreviewKeyDown), TextBox_PreviewKeyDown);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<TextBox, KeyEventArgs>.AddHandler(textBox, nameof(textBox.PreviewKeyDown), TextBox_PreviewKeyDown);
            }
        }
    }
}
