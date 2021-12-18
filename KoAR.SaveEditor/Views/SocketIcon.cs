using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Views;

public sealed class SocketIcon : Control
{
    public static readonly DependencyProperty IconNameProperty;

    public static readonly IValueConverter PrefixConverter = new SocketPrefixConverter();

    public static readonly DependencyProperty SocketProperty = DependencyProperty.Register(nameof(SocketIcon.Socket), typeof(Socket), typeof(SocketIcon),
        new(SocketIcon.SocketProperty_ValueChanged));

    private static readonly DependencyPropertyKey _iconNamePropertyKey = DependencyProperty.RegisterReadOnly(nameof(SocketIcon.IconName), typeof(string), typeof(SocketIcon),
        new());

    static SocketIcon()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SocketIcon), new FrameworkPropertyMetadata(typeof(SocketIcon)));
        SocketIcon.IconNameProperty = SocketIcon._iconNamePropertyKey.DependencyProperty;
    }

    public Socket Socket
    {
        get => (Socket)this.GetValue(SocketIcon.SocketProperty);
        set => this.SetValue(SocketIcon.SocketProperty, value);
    }

    public string? IconName
    {
        get => (string?)this.GetValue(SocketIcon.IconNameProperty);
        private set => this.SetValue(SocketIcon._iconNamePropertyKey, value);
    }

    private static void SocketProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SocketIcon icon = (SocketIcon)d;
        Socket socket = (Socket)e.NewValue;
        icon.IconName = $"{socket.SocketType}{(socket.Gem == null ? string.Empty : "_G")}";
    }

    private sealed class SocketPrefixConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            'W' => "Weapon",
            'A' => "Armor",
            'U' => "Utility",
            'E' => "Epic",
            _ => string.Empty
        };

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
