﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using Microsoft.Win32;

namespace KoAR.SaveEditor.Views
{
    public sealed class MainViewModel : NotifierBase
    {
        private readonly EffectInfo[] _attributes;
        private readonly ObservableCollection<ItemModel> _items;
        private string? _currentDurabilityFilter = string.Empty;
        private AmalurSaveEditor? _editor;
        private string? _fileName;
        private IReadOnlyList<ItemModel> _filteredItems;
        private int _inventorySize;
        private string? _itemNameFilter = string.Empty;
        private string? _maxDurabilityFilter = string.Empty;
        private ItemModel? _selectedItem;
        private bool _unsavedChanges;

        public MainViewModel()
        {
            this.OpenFileCommand = new DelegateCommand(this.OpenFile);
            this._filteredItems = this.Items = new ReadOnlyObservableCollection<ItemModel>(this._items = new ObservableCollection<ItemModel>());
            this.MakeAllItemsSellableCommand = new DelegateCommand(this.MakeAllItemsSellable, this.CanMakeAllItemsSellable);
            this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);
            this.EditItemHexCommand = new DelegateCommand<ItemModel>(this.EditItemHex);
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(App).Namespace}.properties.xml");
            this._attributes = XDocument.Load(stream).Root.Elements().Select(element => new EffectInfo
            {
                Code = element.Attribute("id").Value.ToUpper(),
                DisplayText = element.Value.ToUpper()
            }).ToArray();
        }

        public string? CurrentDurabilityFilter
        {
            get => this._currentDurabilityFilter;
            set
            {
                if (this._currentDurabilityFilter == value)
                {
                    return;
                }
                this._currentDurabilityFilter = value;
                this.OnPropertyChanged();
                this.OnFilterChange();
            }
        }

        public DelegateCommand<ItemModel> EditItemHexCommand
        {
            get;
        }

        public string? FileName
        {
            get => this._fileName;
            private set => this.SetValue(ref this._fileName, value);
        }

        public IReadOnlyList<ItemModel> FilteredItems
        {
            get => this._filteredItems;
            private set => this.SetValue(ref this._filteredItems, value);
        }

        public int InventorySize
        {
            get => this._inventorySize;
            set => this.SetValue(ref this._inventorySize, value);
        }

        private void UpdateInventorySize()
        {
            if (this._editor == null)
            {
                return;
            }
            this._editor.EditMaxBagCount(this.InventorySize);
            this.CanSave();
        }

        private bool CanUpdateInventorySize() => this._editor != null && this._editor.GetMaxBagCount() != this.InventorySize;

        public string? ItemNameFilter
        {
            get => this._itemNameFilter;
            set
            {
                if (this._itemNameFilter == value)
                {
                    return;
                }
                this._itemNameFilter = value;
                this.OnPropertyChanged();
                this.OnFilterChange();
            }
        }

        public ReadOnlyObservableCollection<ItemModel> Items
        {
            get;
        }

        public DelegateCommand MakeAllItemsSellableCommand
        {
            get;
        }

        public string? MaxDurabilityFilter
        {
            get => this._maxDurabilityFilter;
            set
            {
                if (this._maxDurabilityFilter == value)
                {
                    return;
                }
                this._maxDurabilityFilter = value;
                this.OnPropertyChanged();
                this.OnFilterChange();
            }
        }

        public DelegateCommand OpenFileCommand
        {
            get;
        }

        public DelegateCommand ResetFiltersCommand
        {
            get;
        }

        public ItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        public bool? UnsavedChanges
        {
            get => this._fileName == null ? default(bool?) : this._unsavedChanges;
            private set => this.SetValue(ref this._unsavedChanges, value.GetValueOrDefault());
        }

        private bool CanMakeAllItemsSellable()
        {
            return this._editor != null && this._fileName != null && this._items.Any(item => item.IsUnsellable);
        }

        private void CanSave()
        {
            int? selectedItemIndex = this._selectedItem?.ItemIndex;
            this.RepopulateItems();
            if (selectedItemIndex.HasValue)
            {
                this.SelectedItem = this._items.FirstOrDefault(item => item.ItemIndex == selectedItemIndex.Value);
            }
            this.UnsavedChanges = true;
            CommandManager.InvalidateRequerySuggested();
        }

        private void EditItemHex(ItemModel item)
        {
            if (this._editor == null)
            {
                return;
            }
            ItemEditorView view = new ItemEditorView
            {
                Owner = Application.Current.MainWindow,
                DataContext = new ItemEditorViewModel(this._editor, item.GetItem())
            };
            if (view.ShowDialog() == true)
            {
                this.CanSave();
            }
        }

        private void MakeAllItemsSellable()
        {
            if (this._editor == null)
            {
                return;
            }
            int count = 0;
            foreach (ItemModel item in this.Items)
            {
                if (!item.IsUnsellable)
                {
                    continue;
                }
                item.IsUnsellable = false;
                this._editor.WriteEquipmentBytes(item.GetItem());
                count++;
            }
            MessageBox.Show($"Modified {count} items.");
            if (count > 0)
            {
                this.CanSave();
            }
        }

        private void OnFilterChange()
        {
            IEnumerable<ItemModel> items = this.Items;
            if (!string.IsNullOrEmpty(this._currentDurabilityFilter) && float.TryParse(this._currentDurabilityFilter, out float single))
            {
                int temp = (int)Math.Floor(single);
                items = items.Where(model => (int)Math.Floor(model.CurrentDurability) == temp);
            }
            if (!string.IsNullOrEmpty(this._maxDurabilityFilter) && float.TryParse(this._maxDurabilityFilter, out single))
            {
                int temp = (int)Math.Floor(single);
                items = items.Where(model => (int)Math.Floor(model.MaxDurability) == temp);
            }
            if (!string.IsNullOrEmpty(this._itemNameFilter))
            {
                items = items.Where(model => model.ItemName.IndexOf(this._itemNameFilter, StringComparison.OrdinalIgnoreCase) != -1);
            }
            this.FilteredItems = object.ReferenceEquals(items, this.Items)
                ? (IReadOnlyList<ItemModel>)this.Items
                : items.ToList();
            this.SelectedItem = null;
        }

        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Load Save File...",
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            this._editor = new AmalurSaveEditor();
            this._editor.ReadFile(this.FileName = dialog.FileName);
            this.InventorySize = this._editor.GetMaxBagCount();
            this.RepopulateItems();
            this.ResetFilters();
        }

        /// <summary>
        /// Formerly called ShowAll or btnShowAll_Click
        /// </summary>
        private void RepopulateItems()
        {
            if (this._editor == null)
            {
                MessageBox.Show("No save file opened!");
                this.OpenFile();
                return;
            }
            this._items.Clear();
            foreach (ItemMemoryInfo info in this._editor.GetAllEquipment())
            {
                this._items.Insert(info.ItemName == "Unknown" ? this._items.Count : 0, info);
            }
        }

        private void ResetFilters()
        {
            this._itemNameFilter = this._currentDurabilityFilter = this._maxDurabilityFilter = string.Empty;
            this.OnFilterChange();
        }
    }
}
