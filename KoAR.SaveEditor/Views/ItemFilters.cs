﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemFilters : NotifierBase
    {
        private int _armorType;
        private EquipmentCategory _category;
        private int _element;
        private bool _isExpanded = true;
        private string _itemName = string.Empty;
        private int _rarity;

        public ItemFilters() => this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);

        public event EventHandler? FilterChange;

        public ArmorType ArmorType
        {
            get => (ArmorType)this._armorType;
            set
            {
                if (this.SetValue(ref this._armorType, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public EquipmentCategory Category
        {
            get => this._category;
            set
            {
                if (this.SetValue(ref this._category, value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public Element Element
        {
            get => (Element)this._element;
            set
            {
                if (this.SetValue(ref this._element, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public bool IsExpanded
        {
            get => this._isExpanded;
            set => this.SetValue(ref this._isExpanded, value);
        }

        public string ItemName
        {
            get => this._itemName;
            set
            {
                if (this.SetValue(ref this._itemName, value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public Rarity Rarity
        {
            get => (Rarity)this._rarity;
            set
            {
                if (this.SetValue(ref this._rarity, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public DelegateCommand ResetFiltersCommand { get; }

        public IReadOnlyList<TItem> GetFilteredItems<TItem>(IReadOnlyList<TItem> items)
            where TItem : ItemModelBase
        {
            return this.Category == default && this.Rarity == default && this.Element == default && this.ArmorType == default && this.ItemName.Length == 0
                ? items
                : items.Where(item => ItemFilters.Match(item, this.Category, this.Rarity, this.Element, this.ArmorType, this.ItemName)).ToList();
        }

        internal static bool Match(ItemModelBase item, EquipmentCategory category, Rarity rarity, Element element, ArmorType armorType, string itemName)
        {
            return (category == default || category == item.Category) &&
                (rarity == default || rarity == item.Rarity) &&
                (element == default || element == item.Definition.Element) &&
                (armorType == default || armorType == item.Definition.ArmorType) &&
                (itemName.Length == 0 || item.DisplayName.IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        private void OnFilterChange() => this.FilterChange?.Invoke(this, EventArgs.Empty);

        private void ResetFilters()
        {
            if (Interlocked.Exchange(ref this._itemName, string.Empty).Length != 0)
            {
                this.OnPropertyChanged(nameof(this.ItemName));
            }
            if (Interlocked.Exchange(ref this._element, default) != default)
            {
                this.OnPropertyChanged(nameof(this.Element));
            }
            if (Interlocked.Exchange(ref this._rarity, default) != default)
            {
                this.OnPropertyChanged(nameof(this.Rarity));
            }
            if (Interlocked.Exchange(ref this._armorType, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ArmorType));
            }
            this.OnFilterChange();
        }
    }
}
