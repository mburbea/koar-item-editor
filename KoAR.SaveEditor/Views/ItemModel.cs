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
        public ItemModel(ItemMemoryInfo item)
        {
            this.Item = item;
            this.CoreEffects = new NotifyingCollection<uint>(item.CoreEffects.List);
            this.Effects = new NotifyingCollection<uint>(item.Effects);
        }

        public EquipmentCategory Category => this.Item.TypeDefinition.Category;

        public IList<uint> CoreEffects { get; }

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

        public IList<uint> Effects { get; }

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
            get => Amalur.Buffs.TryGetValue(this.Item.CoreEffects.Prefix, out Buff buff) ? buff : default;
            set => this.SetItemValue(value?.Id ?? 0, this.Item.CoreEffects.Prefix, value => this.Item.CoreEffects.Prefix = value);
        }

        public Rarity Rarity => this.TypeDefinition.Rarity;

        public Buff? Suffix
        {
            get => Amalur.Buffs.TryGetValue(this.Item.CoreEffects.Suffix, out Buff buff) ? buff : default;
            set => this.SetItemValue(value?.Id ?? 0, this.Item.CoreEffects.Suffix, value => this.Item.CoreEffects.Suffix = value);
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

        internal ItemMemoryInfo Item { get; }

        internal void AddCoreEffect(uint code) => this.CoreEffects.Add(code);

        internal void AddEffect(uint code) => this.Effects.Add(code);

        internal void DeleteCoreEffect(uint code) => this.CoreEffects.Remove(code);

        internal void DeleteEffect(uint code) => this.Effects.Remove(code);

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
