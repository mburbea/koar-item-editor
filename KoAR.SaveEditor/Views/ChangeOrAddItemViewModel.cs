﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ChangeOrAddItemViewModel : NotifierBase
    {
        private ArmorType _armorTypeFilter;
        private EquipmentCategory _category;
        private TypeDefinition? _definition;
        private IEnumerable<TypeDefinition>? _definitions;
        private Element _elementFilter;
        private Rarity _rarityFilter;

        public ChangeOrAddItemViewModel(ItemModel? item = null)
        {
            this._definition = (this.Item = item)?.TypeDefinition ?? Amalur.TypeDefinitions.Values.First();
            this._category = item?.Category ?? default;
            this.ProcessCommand = new DelegateCommand(this.Process, this.CanProcess);
            this.OnFilterChanged();
        }

        public ArmorType ArmorTypeFilter
        {
            get => this._armorTypeFilter;
            set
            {
                if (this.SetValue(ref this._armorTypeFilter, value))
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
                if (this.SetValue(ref this._category, value))
                {
                    this._rarityFilter = default;
                    this.OnPropertyChanged(nameof(this.RarityFilter));
                    this._elementFilter = default;
                    this.OnPropertyChanged(nameof(this.ElementFilter));
                    this._armorTypeFilter = default;
                    this.OnPropertyChanged(nameof(this.ArmorTypeFilter));
                    this.OnFilterChanged();
                }
            }
        }

        public TypeDefinition? Definition
        {
            get => this._definition;
            set => this.SetValue(ref this._definition, value);
        }

        public IEnumerable<TypeDefinition>? Definitions
        {
            get => this._definitions;
            private set => this.SetValue(ref this._definitions, value);
        }

        public Element ElementFilter
        {
            get => this._elementFilter;
            set
            {
                if (this.SetValue(ref this._elementFilter, value))
                {
                    this.OnFilterChanged();
                }
            }
        }

        public ItemModel? Item { get; }

        public DelegateCommand ProcessCommand { get; }

        public Rarity RarityFilter
        {
            get => this._rarityFilter;
            set
            {
                if (this.SetValue(ref this._rarityFilter, value))
                {
                    this.OnFilterChanged();
                }
            }
        }

        private bool CanProcess() => this._definition != null;

        private void OnFilterChanged()
        {
            IEnumerable<TypeDefinition> definitions = Amalur.TypeDefinitions.Values.Where(item => item.Category == this._category);
            if (this._elementFilter != default)
            {
                definitions = definitions.Where(item => item.Element == this._elementFilter);
            }
            if (this._armorTypeFilter != default)
            {
                definitions = definitions.Where(item => item.ArmorType == this._armorTypeFilter);
            }
            if (this._rarityFilter != default)
            {
                definitions = definitions.Where(item => item.Rarity == this._rarityFilter);
            }
            this.Definitions = definitions.ToArray();
            if (this._definition == null || !this.Definitions.Contains(this._definition))
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
            Window window = Application.Current.Windows.OfType<ChangeOrAddItemWindow>().Single();
            window.DialogResult = true;
            window.Close();
        }
    }
}