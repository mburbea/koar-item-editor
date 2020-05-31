using System.Collections.Generic;
using System.Windows;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public static class StickyBooleans
    {
        private static readonly Dictionary<object, bool> _values = new Dictionary<object, bool>();

        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached("Key", typeof(object), typeof(StickyBooleans),
            new PropertyMetadata(StickyBooleans.KeyProperty_ValueChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(bool), typeof(StickyBooleans),
            new FrameworkPropertyMetadata(BooleanBoxes.False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, StickyBooleans.ValueProperty_ValueChanged));

        public static object? GetKey(DependencyObject d) => d?.GetValue(StickyBooleans.KeyProperty);

        public static void SetKey(DependencyObject d, object? value) => d?.SetValue(StickyBooleans.KeyProperty, value);

        public static bool GetValue(DependencyObject d) => d != null && (bool)d.GetValue(StickyBooleans.ValueProperty);

        public static void SetValue(DependencyObject d, bool value) => d?.SetValue(StickyBooleans.ValueProperty, BooleanBoxes.GetBox(value));

        private static void KeyProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && StickyBooleans._values.TryGetValue(e.NewValue, out bool value))
            {
                StickyBooleans.SetValue(d, value);
            }
        }

        private static void ValueProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            object? key = StickyBooleans.GetKey(d);
            if (key != null)
            {
                StickyBooleans._values[key] = (bool)e.NewValue;
            }
        }
    }
}