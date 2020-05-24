using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    /// <summary>
    /// A wrapper class for <see cref="ItemMemoryInfo"/> that implements <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public sealed class ItemModel : NotifierBase
    {
        private readonly Lazy<List<uint>> _effects;

        public ItemModel(ItemMemoryInfo item)
        {
            this.Item = item;
            this._effects = new Lazy<List<uint>>(item.ReadEffects);
        }

        public EquipmentCategory Category => this.Item.Category;

        public int CoreEffectCount => this.CoreEffects.Count;

        public List<uint> CoreEffects => this.Item.CoreEffects.List;

        public float CurrentDurability
        {
            get => this.Item.CurrentDurability;
            set => this.SetItemValue(value, this.Item.CurrentDurability, value => this.Item.CurrentDurability = value);
        }

        public int EffectCount => this.Item.EffectCount;

        public List<uint> Effects => this._effects.Value;

        public bool HasCustomName => this.Item.HasCustomName;

        public bool IsUnsellable
        {
            get => this.Item.IsUnsellable;
            set => this.SetItemValue(value, this.Item.IsUnsellable, value => this.Item.IsUnsellable = value);
        }

        public string ItemId => LittleEndianConverter.Convert(this.Item.ItemId);

        public int ItemIndex => this.Item.ItemIndex;

        public string ItemName
        {
            get => this.Item.ItemName;
            set => this.SetItemValue(value, this.Item.ItemName, value => this.Item.ItemName = value);
        }

        public float MaxDurability
        {
            get => this.Item.MaxDurability;
            set => this.SetItemValue(value, this.Item.MaxDurability, value => this.Item.MaxDurability = value);
        }

        public byte MysteryInteger => this.Item.CoreEffects.MysteryInteger;

        public uint TypeId
        {
            get => this.Item.TypeId;
            set => this.SetItemValue(value, this.Item.TypeId, value => this.Item.TypeId = value);
        }

        internal ItemMemoryInfo Item
        {
            get;
        }

        internal void AddCoreEffect(uint code)
        {
            this.CoreEffects.Add(code);
            Amalur.WriteEquipmentBytes(this.Item, out _);
        }

        internal void AddEffect(uint code)
        {
            this.Effects.Add(code);
            this.Item.WriteEffects(this.Effects);
            Amalur.WriteEquipmentBytes(this.Item, out _);
        }

        internal void DeleteCoreEffect(uint code)
        {
            if (this.CoreEffects.Remove(code))
            {
                Amalur.WriteEquipmentBytes(this.Item, out _);
            }
        }

        internal void DeleteEffect(uint code)
        {
            if (!this.Effects.Remove(code))
            {
                return;
            }
            this.Item.WriteEffects(this.Effects);
            Amalur.WriteEquipmentBytes(this.Item, out _);
        }

        private void SetItemValue<T>(T value, T currentValue, Action<T> setValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(value, currentValue))
            {
                return;
            }
            setValue(value);
            this.OnPropertyChanged(propertyName);
        }
    }
}
