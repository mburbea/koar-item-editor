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
        private readonly ItemMemoryInfo _item;

        public ItemModel(ItemMemoryInfo item) => this._item = item;

        public float CurrentDurability
        {
            get => this._item.CurrentDurability;
            set => this.SetItemValue(value, this._item.CurrentDurability, (item, value) => item.CurrentDurability = value);
        }

        public int EffectCount => this._item.EffectCount;

        public bool HasCustomName => this._item.HasCustomName;

        public bool IsUnsellable
        {
            get => this._item.IsUnsellable;
            set => this.SetItemValue(value, this._item.IsUnsellable, (item, value) => item.IsUnsellable = value);
        }

        public int ItemIndex => this._item.ItemIndex;

        public string ItemName
        {
            get => this._item.ItemName;
            set => this.SetItemValue(value, this._item.ItemName, (item, value) => item.ItemName = value);
        }

        public float MaxDurability
        {
            get => this._item.MaxDurability;
            set => this.SetItemValue(value, this._item.MaxDurability, (item, value) => item.MaxDurability = value);
        }

        public ItemMemoryInfo GetItem() => this._item;

        private void SetItemValue<T>(T value, T currentValue, Action<ItemMemoryInfo, T> setValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(value, currentValue))
            {
                return;
            }
            setValue(this._item, value);
            this.OnPropertyChanged(propertyName);
        }
    }
}
