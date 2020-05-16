using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    /// <summary>
    /// A wrapper class for <see cref="ItemMemoryInfo"/> that implements <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public sealed class ItemModel : NotifierBase
    {
        private readonly IReadOnlyList<EffectInfo> _attributes;
        private readonly AmalurSaveEditor _editor;
        private readonly ItemMemoryInfo _item;
        private List<EffectInfo>? _effects;
        private EffectInfo? _selectedEffect;

        public ItemModel(AmalurSaveEditor editor, IReadOnlyList<EffectInfo> attributes, ItemMemoryInfo item)
        {
            this._editor = editor;
            this._attributes = attributes;
            this._item = item;
        }

        public float CurrentDurability
        {
            get => this._item.CurrentDurability;
            set => this.SetItemValue(value, this._item.CurrentDurability, value => this._item.CurrentDurability = value);
        }

        public int EffectCount => this._item.EffectCount;

        public IReadOnlyList<EffectInfo> Effects
        {
            get
            {
                if (this._effects != null)
                {
                    return this._effects;
                }
                try
                {
                    return this._effects = this._editor.GetEffectList(this._item, this._attributes);
                }
                finally
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        this.OnPropertyChanged(nameof(this.SelectedEffect));
                        this.OnPropertyChanged(nameof(this.EffectCount));
                    });
                }
            }
        }

        public bool HasCustomName => this._item.HasCustomName;

        public bool IsUnsellable
        {
            get => this._item.IsUnsellable;
            set => this.SetItemValue(value, this._item.IsUnsellable, value => this._item.IsUnsellable = value);
        }

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

        public EffectInfo? SelectedEffect
        {
            get
            {
                if (this._effects == null || this._effects.Count == 0)
                {
                    return null;
                }
                return this._selectedEffect ??= this._effects.FirstOrDefault();
            }
            set => this.SetValue(ref this._selectedEffect, value ?? this.Effects.FirstOrDefault());
        }

        public void AddAttribute(EffectInfo info)
        {
            if (this._effects == null)
            {
                this._effects = this._editor.GetEffectList(this._item, this._attributes);
            }
            this._effects.Add(info);
            this._item.WriteEffects(this._effects);
            this._editor.WriteEquipmentBytes(this._item);
            this._effects = null;
            this.OnPropertyChanged(nameof(this.Effects));
        }

        public void DeleteAttribute(EffectInfo info)
        {            
            string code = info.Code;
            if (this._effects == null)
            {
                this._effects = this._editor.GetEffectList(this._item, this._attributes);
            }
            bool found = false;
            for (int index = 0; index < this._effects.Count; index++)
            {
                EffectInfo current = this._effects[index];
                if (current.Code == code)
                {
                    this._effects.RemoveAt(index);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return;
            }
            this._item.WriteEffects(this._effects);
            this._editor.WriteEquipmentBytes(this._item);
            this._effects = null;
            this.OnPropertyChanged(nameof(this.Effects));
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
