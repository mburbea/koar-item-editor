using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class TypeDefinitionControl : Control
    {
        public static readonly DependencyProperty DefinitionProperty = DependencyProperty.Register(nameof(TypeDefinitionControl.Definition), typeof(TypeDefinition), typeof(TypeDefinitionControl));

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(TypeDefinitionControl.Item), typeof(ItemModelBase), typeof(TypeDefinitionControl),
            new PropertyMetadata(TypeDefinitionControl.ItemProperty_ValueChanged));

        public static readonly IValueConverter SocketTextConverter = new SocketLabelConverter();

        static TypeDefinitionControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TypeDefinitionControl), new FrameworkPropertyMetadata(typeof(TypeDefinitionControl)));

        public TypeDefinition? Definition
        {
            get => (TypeDefinition?)this.GetValue(TypeDefinitionControl.DefinitionProperty);
            set => this.SetValue(TypeDefinitionControl.DefinitionProperty, value);
        }

        public ItemModelBase? Item
        {
            get => (ItemModelBase?)this.GetValue(TypeDefinitionControl.ItemProperty);
            set => this.SetValue(TypeDefinitionControl.ItemProperty, value);
        }

        private static void ItemProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TypeDefinitionControl control = (TypeDefinitionControl)d;
            if (e.OldValue != null)
            {
                ItemModelBase oldItem = (ItemModelBase)e.OldValue;
                PropertyChangedEventManager.RemoveHandler(oldItem, control.Item_TypeDefinitionChanged, nameof(oldItem.TypeDefinition));
            }
            ItemModelBase? item = (ItemModelBase)e.NewValue;
            if (item != null)
            {
                control.Definition = item.TypeDefinition;
                PropertyChangedEventManager.AddHandler(item, control.Item_TypeDefinitionChanged, nameof(item.TypeDefinition));
            }
        }

        private void Item_TypeDefinitionChanged(object sender, EventArgs e) => this.Definition = ((ItemModelBase)sender).TypeDefinition;

        private sealed class SocketLabelConverter : IValueConverter
        {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"{SocketLabelConverter.GetPrefix(value)} Socket";

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
