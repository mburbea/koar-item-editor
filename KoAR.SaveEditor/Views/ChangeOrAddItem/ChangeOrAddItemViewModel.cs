﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.ChangeOrAddItem
{
    public sealed class ChangeOrAddItemViewModel : NotifierBase
    {
        /// <summary>
        /// <see cref="EquipmentCategory" /> enumeration does not start at 0.  Instead of just using default, gets the first category.
        /// </summary>
        private static readonly EquipmentCategory _defaultCategory = ((EquipmentCategory[])Enum.GetValues(typeof(EquipmentCategory)))[0];

        private int _armorTypeFilter;
        private EquipmentCategory _category;
        private ItemDefinition? _definition;
        private IReadOnlyList<ItemDefinition>? _definitions;
        private int _elementFilter;
        private int _rarityFilter;

        public ChangeOrAddItemViewModel(ItemModelBase? item = null)
        {
            this._definition = (this.Item = item)?.Definition ?? Amalur.ItemDefinitions.Values.First();
            this._category = item?.Category ?? ChangeOrAddItemViewModel._defaultCategory;
            this.ProcessCommand = new DelegateCommand(this.Process, () => this._definition != null);
            this.OnFilterChanged();
        }

        public ArmorType ArmorTypeFilter
        {
            get => (ArmorType)this._armorTypeFilter;
            set
            {
                if (this.SetValue(ref this._armorTypeFilter, (int)value))
                {
                    this.OnFilterChanged();
                }
            }
        }

        public EquipmentCategory Category
        {
            get => this._category;
            set
            {
                if (!this.SetValue(ref this._category, value))
                {
                    return;
                }
                if (Interlocked.Exchange(ref this._rarityFilter, default) != default)
                {
                    this.OnPropertyChanged(nameof(this.RarityFilter));
                }
                if (Interlocked.Exchange(ref this._elementFilter, default) != default)
                {
                    this.OnPropertyChanged(nameof(this.ElementFilter));
                }
                if (Interlocked.Exchange(ref this._armorTypeFilter, default) != default)
                {
                    this.OnPropertyChanged(nameof(this.ArmorTypeFilter));
                }
                this.OnFilterChanged();
            }
        }

        public ItemDefinition? Definition
        {
            get => this._definition;
            set => this.SetValue(ref this._definition, value);
        }

        public IReadOnlyList<ItemDefinition>? Definitions
        {
            get => this._definitions;
            private set => this.SetValue(ref this._definitions, value);
        }

        public Element ElementFilter
        {
            get => (Element)this._elementFilter;
            set
            {
                if (this.SetValue(ref this._elementFilter, (int)value))
                {
                    this.OnFilterChanged();
                }
            }
        }

        public ItemModelBase? Item { get; }

        public DelegateCommand ProcessCommand { get; }

        public Rarity RarityFilter
        {
            get => (Rarity)this._rarityFilter;
            set
            {
                if (this.SetValue(ref this._rarityFilter, (int)value))
                {
                    this.OnFilterChanged();
                }
            }
        }

        private bool IsMatch(ItemDefinition item)
        {
            return this.Category == item.Category &&
                (this.ElementFilter == default || this.ElementFilter == item.Element) &&
                (this.ArmorTypeFilter == default || this.ArmorTypeFilter == item.ArmorType) &&
                (this.RarityFilter == default || this.RarityFilter == item.Rarity);
        }

        private void OnFilterChanged()
        {
            ItemDefinition? selectedItem = this._definition;
            this.Definitions = Amalur.ItemDefinitions.Values.Where(this.IsMatch).ToArray();
            if (selectedItem == null || !this.Definitions.Contains(selectedItem))
            {
                this.Definition = this.Definitions.FirstOrDefault();
            }
        }

        private void Process()
        {
            if (this._definition == null)
            {
                return;
            }
            Window window = Application.Current.Windows.OfType<ChangeOrAddItemView>().Single();
            window.DialogResult = true;
            window.Close();
        }
    }
}
