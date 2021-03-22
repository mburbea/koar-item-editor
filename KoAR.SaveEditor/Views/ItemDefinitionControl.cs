using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemDefinitionControl : Control
    {
        public static readonly DependencyProperty DefinitionProperty = DependencyProperty.Register(nameof(ItemDefinitionControl.Definition), typeof(ItemDefinition), typeof(ItemDefinitionControl),
            new(ItemDefinitionControl.Definition_ValueChanged));

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(ItemDefinitionControl.Item), typeof(ItemModelBase), typeof(ItemDefinitionControl),
            new(ItemDefinitionControl.ItemProperty_ValueChanged));

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(nameof(ItemDefinitionControl.SearchText), typeof(string), typeof(ItemDefinitionControl),
            new());

        public static readonly DependencyProperty SocketsProperty;

        private static readonly DependencyPropertyKey _socketsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Sockets), typeof(IEnumerable<Socket>), typeof(ItemDefinitionControl),
            new());

        static ItemDefinitionControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemDefinitionControl), new FrameworkPropertyMetadata(typeof(ItemDefinitionControl)));
            ItemDefinitionControl.SocketsProperty = ItemDefinitionControl._socketsPropertyKey.DependencyProperty;
        }

        public ItemDefinition? Definition
        {
            get => (ItemDefinition?)this.GetValue(ItemDefinitionControl.DefinitionProperty);
            set => this.SetValue(ItemDefinitionControl.DefinitionProperty, value);
        }

        public ItemModelBase? Item
        {
            get => (ItemModelBase?)this.GetValue(ItemDefinitionControl.ItemProperty);
            set => this.SetValue(ItemDefinitionControl.ItemProperty, value);
        }

        public string? SearchText
        {
            get => (string?)this.GetValue(ItemDefinitionControl.SearchTextProperty);
            set => this.SetValue(ItemDefinitionControl.SearchTextProperty, value);
        }

        public IEnumerable<Socket>? Sockets
        {
            get => (IEnumerable<Socket>?)this.GetValue(ItemDefinitionControl.SocketsProperty);
            private set => this.SetValue(ItemDefinitionControl._socketsPropertyKey, value);
        }

        private static void Definition_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemDefinitionControl control = (ItemDefinitionControl)d;
            if (control.Item == null)
            {
                control.Sockets = ((ItemDefinition?)e.NewValue)?.GetSockets();
            }
        }

        private static void ItemProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemDefinitionControl control = (ItemDefinitionControl)d;
            if (e.OldValue is ItemModelBase oldItem)
            {
                PropertyChangedEventManager.RemoveHandler(oldItem, control.Item_DefinitionChanged, nameof(oldItem.Definition));
            }
            if (e.NewValue is ItemModelBase newItem)
            {
                control.Definition = newItem.Definition;
                PropertyChangedEventManager.AddHandler(newItem, control.Item_DefinitionChanged, nameof(newItem.Definition));
                control.Sockets = newItem.Item.GetSockets();
            }
            else
            {
                control.Sockets = null;
            }
        }

        private void Item_DefinitionChanged(object? sender, EventArgs e)
        {
            ItemModelBase item = (ItemModelBase)sender!;
            this.Definition = item.Definition;
            this.Sockets = item.Item.GetSockets();
        }
    }
}