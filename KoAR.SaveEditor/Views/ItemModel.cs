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

        public EquipmentCategory Category => this.Item.Category;

        public int CoreEffectCount => this.CoreEffects.Count;

        public IList<uint> CoreEffects
        {
            get;
        }

        public float CurrentDurability
        {
            get => this.Item.CurrentDurability;
            set => this.SetItemValue(value, this.Item.CurrentDurability, value => this.Item.CurrentDurability = value);
        }

        public int EffectCount => this.Item.Effects.Count;

        public IList<uint> Effects
        {
            get;
        }

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
            set => this.SetItemValue(value, this.Item.ItemName, value => this.Item.ItemName = value);
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
            this.OnPropertyChanged(nameof(this.CoreEffectCount));
            this.OnPropertyChanged(nameof(this.MysteryInteger));
        }

        internal void AddEffect(uint code)
        {
            this.Effects.Add(code);
            this.OnPropertyChanged(nameof(this.EffectCount));
        }

        internal void DeleteCoreEffect(uint code)
        {
            if (this.CoreEffects.Remove(code))
            {
                this.OnPropertyChanged(nameof(this.CoreEffectCount));
                this.OnPropertyChanged(nameof(this.MysteryInteger));
            }
        }

        internal void DeleteEffect(uint code)
        {
            if (this.Effects.Remove(code))
            {
                this.OnPropertyChanged(nameof(this.EffectCount));
            }
        }

        internal void Rematerialize(byte[] bytes)
        {
            this.Item.Rematerialize(bytes);
            this.OnPropertyChanged(string.Empty);
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

        private sealed class EffectCollection : Collection<uint>, INotifyCollectionChanged, INotifyPropertyChanged
        {
            private static readonly PropertyChangedEventArgs _countArgs = new PropertyChangedEventArgs(nameof(IList<uint>.Count));
            private static readonly PropertyChangedEventArgs _indexerArgs = new PropertyChangedEventArgs(Binding.IndexerName);

            public EffectCollection(List<uint> items)
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
