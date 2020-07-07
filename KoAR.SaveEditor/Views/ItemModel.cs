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
    public sealed class ItemModel : NotifierBase, IDisposable
    {
        private readonly NotifyingCollection<Buff> _effects;
        private readonly NotifyingCollection<Buff> _coreEffects;

        public ItemModel(ItemMemoryInfo item)
        {
            this.Item = item;
            this._coreEffects = new NotifyingCollection<Buff>(item.CoreEffects.List);
            this._coreEffects.CollectionChanged += this.EffectsCollection_CollectionChanged;
            this._effects = new NotifyingCollection<Buff>(item.Effects);
            this._effects.CollectionChanged += this.EffectsCollection_CollectionChanged;
        }

        public EquipmentCategory Category => this.Item.TypeDefinition.Category;

        public IReadOnlyList<Buff> CoreEffects => this._coreEffects;

        public float CurrentDurability
        {
            get => this.Item.CurrentDurability;
            set => this.SetItemValue(value, this.Item.CurrentDurability, value => this.Item.CurrentDurability = value);
        }

        public string DisplayName => this.HasCustomName switch
        {
            true => this.ItemName,
            false when this.TypeDefinition.AffixableName && (this.Prefix ?? this.Suffix) != null => $"{this.Prefix?.Modifier} {this.TypeDefinition.CategoryDisplayName} {this.Suffix?.Modifier}".Trim(),
            false => this.TypeDefinition.Name,
        };

        public IReadOnlyList<Buff> Effects => this._effects;

        public bool HasCustomName => this.Item.HasCustomName;

        public bool IsUnsellable
        {
            get => this.Item.IsUnsellable;
            set => this.SetItemValue(value, this.Item.IsUnsellable, value => this.Item.IsUnsellable = value);
        }

        public bool IsUnstashable
        {
            get => this.Item.IsUnstashable;
            set => this.SetItemValue(value, this.Item.IsUnstashable, value => this.Item.IsUnstashable = value);
        }

        public uint ItemId => this.Item.ItemId;

        public int ItemIndex => this.Item.ItemIndex;

        public string ItemName
        {
            get => this.Item.ItemName;
            set
            {
                if (this.SetItemValue(value, this.Item.ItemName, value => this.Item.ItemName = value))
                {
                    this.OnPropertyChanged(nameof(this.HasCustomName));
                    this.OnPropertyChanged(nameof(this.DisplayName));
                }
            }
        }

        public byte Level
        {
            get => this.Item.Level;
            set => this.SetItemValue(value, this.Item.Level, value => this.Item.Level = value);
        }

        public float MaxDurability
        {
            get => this.Item.MaxDurability;
            set => this.SetItemValue(value, this.Item.MaxDurability, value => this.Item.MaxDurability = value);
        }

        public Buff? Prefix
        {
            get => this.Item.CoreEffects.Prefix;
            set => this.SetItemValue(value, this.Item.CoreEffects.Prefix, value => this.Item.CoreEffects.Prefix = value);
        }

        public Rarity Rarity => this.Item.Rarity;

        public Buff? Suffix
        {
            get => this.Item.CoreEffects.Suffix;
            set => this.SetItemValue(value, this.Item.CoreEffects.Suffix, value => this.Item.CoreEffects.Suffix = value);
        }

        public TypeDefinition TypeDefinition
        {
            get => this.Item.TypeDefinition;
            set
            {
                this.Item.TypeDefinition = value;
                this.OnPropertyChanged(string.Empty);
            }
        }

        public bool UnsupportedFormat => this.Item.CoreEffects.UnsupportedFormat;

        public void Dispose()
        {
            this._effects.CollectionChanged -= this.EffectsCollection_CollectionChanged;
            this._coreEffects.CollectionChanged -= this.EffectsCollection_CollectionChanged;
        }

        internal ItemMemoryInfo Item { get; }

        internal void AddCoreEffect(Buff buff) => this._coreEffects.Add(buff);

        internal void AddEffect(Buff buff) => this._effects.Add(buff);

        internal void DeleteCoreEffect(Buff buff) => this._coreEffects.Remove(buff);

        internal void DeleteEffect(Buff buff) => this._effects.Remove(buff);

        private void EffectsCollection_CollectionChanged(object sender, EventArgs e) => this.OnPropertyChanged(nameof(this.Rarity));

        private bool SetItemValue<T>(T value, T currentValue, Action<T> setValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(value, currentValue))
            {
                return false;
            }
            setValue(value);
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
