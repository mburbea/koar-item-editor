using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class GemSocketIcon : Control
    {
        public static readonly DependencyProperty GemSocketProperty = DependencyProperty.Register(nameof(GemSocketIcon.GemSocket), typeof(GemSocket), typeof(GemSocketIcon),
            new PropertyMetadata(GemSocketIcon.GemSocketProperty_ValueChanged));

        public static readonly DependencyProperty IconNameProperty;

        public static readonly IValueConverter PrefixConverter = new SocketPrefixConverter();

        private static readonly DependencyPropertyKey _iconNamePropertyKey = DependencyProperty.RegisterReadOnly(nameof(GemSocketIcon.IconName), typeof(string), typeof(GemSocketIcon),
            new PropertyMetadata());

        static GemSocketIcon()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GemSocketIcon), new FrameworkPropertyMetadata(typeof(GemSocketIcon)));
            GemSocketIcon.IconNameProperty = GemSocketIcon._iconNamePropertyKey.DependencyProperty;
        }

        public GemSocket GemSocket
        {
            get => (GemSocket)this.GetValue(GemSocketIcon.GemSocketProperty);
            set => this.SetValue(GemSocketIcon.GemSocketProperty, value);
        }

        public string? IconName
        {
            get => (string?)this.GetValue(GemSocketIcon.IconNameProperty);
            private set => this.SetValue(GemSocketIcon._iconNamePropertyKey, value);
        }

        private static void GemSocketProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GemSocketIcon icon = (GemSocketIcon)d;
            GemSocket socket = (GemSocket)e.NewValue;
            icon.IconName = $"{socket.SocketType}{(socket.Gem == null ? string.Empty : "_G")}";
        }

        private sealed class SocketPrefixConverter : IValueConverter
        {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"{SocketPrefixConverter.GetPrefix(value)} Socket";

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

            private static string GetPrefix(object value) => value switch
            {
                'W' => "Weapon",
                'A' => "Armor",
                'U' => "Utility",
                'E' => "Epic",
                _ => string.Empty
            };
        }
    }
}
