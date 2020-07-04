using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
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
            this.CoreEffects = new EffectCollection(item.CoreEffects.List);
            this.Effects = new EffectCollection(item.Effects);
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

        public Buff? Prefix => Amalur.Buffs.TryGetValue(this.Item.CoreEffects.Prefix, out Buff buff) ? buff : default;

        public Buff? Suffix => Amalur.Buffs.TryGetValue(this.Item.CoreEffects.Suffix, out Buff buff) ? buff : default;

        public TypeDefinition TypeDefinition
        {
            get => this.Item.TypeDefinition;
            set
            {
                this.Item.TypeDefinition = value;
                this.OnPropertyChanged(string.Empty);
            }
        }

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

        private sealed class EffectCollection : Collection<uint>, INotifyCollectionChanged, INotifyPropertyChanged
        {
            private static readonly PropertyChangedEventArgs _countArgs = new PropertyChangedEventArgs(nameof(EffectCollection.Count));
            private static readonly PropertyChangedEventArgs _indexerArgs = new PropertyChangedEventArgs(Binding.IndexerName);

            public EffectCollection(IList<uint> items)
                : base(items)
            {
            }

            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public event PropertyChangedEventHandler? PropertyChanged;

            protected override void InsertItem(int index, uint item)
            {
                base.InsertItem(index, item);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
                this.OnPropertyChanged(EffectCollection._countArgs);
                this.OnPropertyChanged(EffectCollection._indexerArgs);
            }

            protected override void RemoveItem(int index)
            {
                uint item = this.Items[index];
                base.RemoveItem(index);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
                this.OnPropertyChanged(EffectCollection._countArgs);
                this.OnPropertyChanged(EffectCollection._indexerArgs);
            }

            private void OnCollectionChanged(NotifyCollectionChangedAction action, uint item, int index) => this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));

            private void OnPropertyChanged(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);
        }
    }
}
