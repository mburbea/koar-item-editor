using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private readonly AmalurSaveEditor _editor;
        private readonly ItemMemoryInfo _item;
        private List<CoreEffectInfo>? _coreEffects;
        private List<EffectInfo>? _effects;
        private EffectInfo? _selectedEffect;

        public ItemModel(AmalurSaveEditor editor, ItemMemoryInfo item)
        {
            this._editor = editor;
            this._item = item;
        }

        public string? CoreEffect0 => this.CoreEffects.FirstOrDefault()?.Code;

        public string? CoreEffect1 => this.CoreEffects.Skip(1).FirstOrDefault()?.Code;
        public string? CoreEffect2 => this.CoreEffects.Skip(2).FirstOrDefault()?.Code;
        public string? CoreEffect3 => this.CoreEffects.Skip(3).FirstOrDefault()?.Code;

        public List<CoreEffectInfo> CoreEffects => this._coreEffects ??= this._item.CoreItemMemory.ReadEffects();

        public float CurrentDurability
        {
            get => this._item.CurrentDurability;
            set => this.SetItemValue(value, this._item.CurrentDurability, value => this._item.CurrentDurability = value);
        }

        public int EffectCount => this._item.EffectCount;

        public List<EffectInfo> Effects => this._effects ??= this._editor.GetEffectList(this._item, MainViewModel.Effects);

        public bool HasCustomName => this._item.HasCustomName;

        public bool IsUnsellable
        {
            get => this._item.IsUnsellable;
            set => this.SetItemValue(value, this._item.IsUnsellable, value => this._item.IsUnsellable = value);
        }

        public int ItemId => this._item.ItemId;

        public int ItemIndex => this._item.ItemIndex;

        public string ItemName
        {
            get => this._item.ItemName;
            set => this.SetItemValue(value, this._item.ItemName, value => this._item.ItemName = value);
        }

        public float MaxDurability
        {
            get => this._item.MaxDurability;
            set => this.SetItemValue(value, this._item.MaxDurability, value => this._item.MaxDurability = value);
        }

        public int MysteryInteger => this._item.CoreItemMemory.MysteryInteger;

        public EffectInfo? SelectedEffect
        {
            get => this._selectedEffect ??= this.Effects.FirstOrDefault();
            set => this.SetValue(ref this._selectedEffect, value ?? this.Effects.FirstOrDefault());
        }

        public void AddEffect(EffectInfo info)
        {
            this.Effects.Add(info);
            this._item.WriteEffects(this.Effects);
            this._editor.WriteEquipmentBytes(this._item, out _);
        }

        public void DeleteEffect(EffectInfo info)
        {
            if (!this.Effects.Remove(info))
            {
                return;
            }
            this._item.WriteEffects(this.Effects);
            this._editor.WriteEquipmentBytes(this._item, out _);
        }

        public ItemMemoryInfo GetItem() => this._item;

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
