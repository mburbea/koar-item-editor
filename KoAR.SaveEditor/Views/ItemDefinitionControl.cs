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
            new PropertyMetadata(ItemDefinitionControl.Definition_ValueChanged));

        public static readonly DependencyProperty GemSocketsProperty;

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(ItemDefinitionControl.Item), typeof(ItemModelBase), typeof(ItemDefinitionControl),
            new PropertyMetadata(ItemDefinitionControl.ItemProperty_ValueChanged));

        private static readonly DependencyPropertyKey _gemSocketsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(GemSockets), typeof(IEnumerable<GemSocket>), typeof(ItemDefinitionControl), new PropertyMetadata());

        static ItemDefinitionControl()
        {
            GemSocketsProperty = _gemSocketsPropertyKey.DependencyProperty;
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemDefinitionControl), new FrameworkPropertyMetadata(typeof(ItemDefinitionControl)));
        }

        public ItemDefinition? Definition
        {
            get => (ItemDefinition?)this.GetValue(ItemDefinitionControl.DefinitionProperty);
            set => this.SetValue(ItemDefinitionControl.DefinitionProperty, value);
        }

        public IEnumerable<GemSocket>? GemSockets
        {
            get => (IEnumerable<GemSocket>?)this.GetValue(ItemDefinitionControl.GemSocketsProperty);
            private set => this.SetValue(ItemDefinitionControl._gemSocketsPropertyKey, value);
        }

        public ItemModelBase? Item
        {
            get => (ItemModelBase?)this.GetValue(ItemDefinitionControl.ItemProperty);
            set => this.SetValue(ItemDefinitionControl.ItemProperty, value);
        }

        private static void Definition_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemDefinitionControl control = (ItemDefinitionControl)d;
            if (control.Item == null)
            {
                control.GemSockets = ((ItemDefinition?)e.NewValue)?.GetGemSockets();
            }
        }

        private static void ItemProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemDefinitionControl control = (ItemDefinitionControl)d;
            if (e.OldValue != null)
            {
                ItemModelBase oldItem = (ItemModelBase)e.OldValue;
                PropertyChangedEventManager.RemoveHandler(oldItem, control.Item_DefinitionChanged, nameof(oldItem.Definition));
            }
            ItemModelBase? item = (ItemModelBase)e.NewValue;
            if (item != null)
            {
                control.Definition = item.Definition;
                PropertyChangedEventManager.AddHandler(item, control.Item_DefinitionChanged, nameof(item.Definition));
                control.GemSockets = item.Item.GetGemSockets();
            }
            else
            {
                control.GemSockets = null;
            }
        }

        private void Item_DefinitionChanged(object sender, EventArgs e)
        {
            this.Definition = ((ItemModelBase)sender).Definition;
        }

        
    }
}
