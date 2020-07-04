using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class TypeDefinitionControl : Control
    {
        public static readonly DependencyProperty DefinitionProperty = DependencyProperty.Register(nameof(TypeDefinitionControl.Definition), typeof(TypeDefinition), typeof(TypeDefinitionControl),
            new PropertyMetadata(TypeDefinitionControl.DefinitionProperty_ValueChanged));

        public static readonly DependencyProperty HasCustomNameProperty;

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(TypeDefinitionControl.Item), typeof(ItemModel), typeof(TypeDefinitionControl),
            new PropertyMetadata(TypeDefinitionControl.ItemProperty_ValueChanged));

        public static readonly DependencyProperty RarityProperty;

        private static readonly DependencyPropertyKey _hasCustomNamePropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasCustomName), typeof(bool), typeof(TypeDefinitionControl),
            new PropertyMetadata(BooleanBoxes.False));

        private static readonly DependencyPropertyKey _rarityPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Rarity), typeof(Rarity), typeof(TypeDefinitionControl),
            new PropertyMetadata(default(Rarity)));

        static TypeDefinitionControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TypeDefinitionControl), new FrameworkPropertyMetadata(typeof(TypeDefinitionControl)));
            TypeDefinitionControl.RarityProperty = TypeDefinitionControl._rarityPropertyKey.DependencyProperty;
            TypeDefinitionControl.HasCustomNameProperty = TypeDefinitionControl._hasCustomNamePropertyKey.DependencyProperty;
        }

        public TypeDefinition? Definition
        {
            get => (TypeDefinition?)this.GetValue(TypeDefinitionControl.DefinitionProperty);
            set => this.SetValue(TypeDefinitionControl.DefinitionProperty, value);
        }

        public bool HasCustomName
        {
            get => (bool)this.GetValue(TypeDefinitionControl.HasCustomNameProperty);
            private set => this.SetValue(TypeDefinitionControl._hasCustomNamePropertyKey, BooleanBoxes.GetBox(value));
        }

        public ItemModel? Item
        {
            get => (ItemModel?)this.GetValue(TypeDefinitionControl.ItemProperty);
            set => this.SetValue(TypeDefinitionControl.ItemProperty, value);
        }

        public Rarity Rarity
        {
            get => (Rarity)this.GetValue(TypeDefinitionControl.RarityProperty);
            private set => this.SetValue(TypeDefinitionControl._rarityPropertyKey, value);
        }

        private static void DefinitionProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TypeDefinitionControl control = (TypeDefinitionControl)d;
            control.Rarity = ((TypeDefinition?)e.NewValue)?.Rarity ?? Rarity.Common;
        }

        private static void ItemProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TypeDefinitionControl control = (TypeDefinitionControl)d;
            if (e.OldValue != null)
            {
                ItemModel oldItem = (ItemModel)e.OldValue;
                PropertyChangedEventManager.RemoveHandler(oldItem, control.Item_HasCustomNameChanged, nameof(oldItem.HasCustomName));
            }
            ItemModel? item = (ItemModel)e.NewValue;
            if (item != null)
            {
                control.Definition = item.TypeDefinition;
                PropertyChangedEventManager.AddHandler(item, control.Item_HasCustomNameChanged, nameof(item.HasCustomName));
            }
            control.HasCustomName = item?.HasCustomName ?? false;
        }

        private void Item_HasCustomNameChanged(object sender, EventArgs e) => this.HasCustomName = ((ItemModel)sender).HasCustomName;
    }
}
