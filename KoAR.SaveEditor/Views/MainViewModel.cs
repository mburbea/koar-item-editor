using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly ObservableCollection<ItemModel> _items;
        private string _currentDurabilityFilter = string.Empty;
        private EquipmentType? _equipmentTypeFilter;
        private string? _fileName;
        private IReadOnlyList<ItemModel> _filteredItems;
        private int _inventorySize;
        private string _itemNameFilter = string.Empty;
        private string _maxDurabilityFilter = string.Empty;
        private EffectInfo? _selectedEffect;
        private ItemModel? _selectedItem;
        private bool _unsavedChanges;

        public MainViewModel()
        {
            if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(Window)).DefaultValue)
            {
                Amalur.Initialize();
            }
            _selectedEffect = Amalur.Effects.FirstOrDefault();

            this.OpenFileCommand = new DelegateCommand(this.OpenFile);
            this._filteredItems = this.Items = new ReadOnlyObservableCollection<ItemModel>(this._items = new ObservableCollection<ItemModel>());
            this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);
            this.EditItemHexCommand = new DelegateCommand<ItemModel>(this.EditItemHex);
            this.UpdateInventorySizeCommand = new DelegateCommand(this.UpdateInventorySize, this.CanUpdateInventorySize);
            this.AddEffectCommand = new DelegateCommand<EffectInfo>(this.AddEffect);
            this.DeleteEffectCommand = new DelegateCommand<EffectInfo>(this.DeleteEffect);
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
        }

        public DelegateCommand<EffectInfo> AddEffectCommand
        {
            get;
        }

        public bool? AllItemsUnsellable
        {
            get
            {
                if (this.FilteredItems.Count == 0)
                {
                    return true;
                }
                bool first = this.FilteredItems[0].IsUnsellable;
                for (int index = 1; index < this.FilteredItems.Count; index++)
                {
                    bool current = this.FilteredItems[index].IsUnsellable;
                    if (current != first)
                    {
                        return null;
                    }
                }
                return first;
            }
            set
            {
                if (!Amalur.IsInitialized)
                {
                    return;
                }
                bool newValue = value.GetValueOrDefault();
                foreach (ItemModel model in this.FilteredItems)
                {
                    bool current = model.IsUnsellable;
                    if (current == newValue)
                    {
                        continue;
                    }
                    if (model.Equals(this.SelectedItem))
                    {
                        PropertyChangedEventManager.RemoveHandler(model, this.SelectedItem_IsUnsellableChanged, nameof(model.IsUnsellable));
                    }
                    model.IsUnsellable = newValue;
                    Amalur.WriteEquipmentBytes(model.Item, out _);
                    this.UnsavedChanges = true;
                    if (model.Equals(this.SelectedItem))
                    {
                        PropertyChangedEventManager.AddHandler(model, this.SelectedItem_IsUnsellableChanged, nameof(model.IsUnsellable));
                    }
                }
                this.OnPropertyChanged();
            }
        }

        public string CurrentDurabilityFilter
        {
            get => this._currentDurabilityFilter;
            set
            {
                if (this.SetValue(ref this._currentDurabilityFilter, value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public DelegateCommand<EffectInfo> DeleteEffectCommand
        {
            get;
        }

        public DelegateCommand<ItemModel> EditItemHexCommand
        {
            get;
        }

        public EquipmentType? EquipmentTypeFilter
        {
            get => this._equipmentTypeFilter;
            set
            {
                if (this.SetValue(ref this._equipmentTypeFilter, value))
                {
                    this.OnFilterChange();
                }
            }
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

        public string ItemNameFilter
        {
            get => this._itemNameFilter;
            set
            {
                if (this.SetValue(ref this._itemNameFilter, value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public ReadOnlyObservableCollection<ItemModel> Items
        {
            get;
        }

        public string MaxDurabilityFilter
        {
            get => this._maxDurabilityFilter;
            set
            {
                if (this.SetValue(ref this._maxDurabilityFilter, value))
                {
                    this.OnFilterChange();
                }
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

        public DelegateCommand SaveCommand
        {
            get;
        }

        public EffectInfo? SelectedEffect
        {
            get => this._selectedEffect;
            set => this.SetValue(ref this._selectedEffect, value);
        }

        public ItemModel? SelectedItem
        {
            get => this._selectedItem;
            set
            {
                if (value == this._selectedItem)
                {
                    return;
                }
                if (this._selectedItem != null)
                {
                    PropertyChangedEventManager.RemoveHandler(this._selectedItem, this.SelectedItem_MateriallyChanged, nameof(ItemModel.ItemName));
                    PropertyChangedEventManager.RemoveHandler(this._selectedItem, this.SelectedItem_MateriallyChanged, nameof(ItemModel.CurrentDurability));
                    PropertyChangedEventManager.RemoveHandler(this._selectedItem, this.SelectedItem_MateriallyChanged, nameof(ItemModel.MaxDurability));
                    PropertyChangedEventManager.RemoveHandler(this._selectedItem, this.SelectedItem_IsUnsellableChanged, nameof(ItemModel.IsUnsellable));
                }
                if ((this._selectedItem = value) != null)
                {
                    PropertyChangedEventManager.AddHandler(this._selectedItem, this.SelectedItem_MateriallyChanged, nameof(ItemModel.ItemName));
                    PropertyChangedEventManager.AddHandler(this._selectedItem, this.SelectedItem_MateriallyChanged, nameof(ItemModel.CurrentDurability));
                    PropertyChangedEventManager.AddHandler(this._selectedItem, this.SelectedItem_MateriallyChanged, nameof(ItemModel.MaxDurability));
                    PropertyChangedEventManager.AddHandler(this._selectedItem, this.SelectedItem_IsUnsellableChanged, nameof(ItemModel.IsUnsellable));
                }
                this.OnPropertyChanged();
            }
        }

        public bool? UnsavedChanges
        {
            get => this._fileName == null ? default(bool?) : this._unsavedChanges;
            private set => this.SetValue(ref this._unsavedChanges, value.GetValueOrDefault());
        }

        public DelegateCommand UpdateInventorySizeCommand
        {
            get;
        }

        internal void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open Save File...",
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            Amalur.ReadFile(this.FileName = dialog.FileName);
            this.InventorySize = Amalur.GetMaxBagCount();
            this.RepopulateItems();
            this.ResetFilters();
            this._unsavedChanges = false;
            this.OnPropertyChanged(nameof(this.UnsavedChanges));
        }

        internal void Save()
        {
            if (!Amalur.IsInitialized)
            {
                return;
            }
            File.Copy(this._fileName, $"{this._fileName}.bak", true);
            Amalur.SaveFile(this._fileName);
            this.UnsavedChanges = false;
            this.RepopulateItems();
            MessageBox.Show($"Save successful! Original save backed up as {this._fileName}.bak.", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddEffect(EffectInfo info)
        {
            if (info == null || this.SelectedItem == null)
            {
                return;
            }
            this.SelectedItem.AddEffect(info.Clone());
            this.SelectedEffect = Amalur.Effects[0];
            this.Refresh();
        }

        private bool CanSave() => this._unsavedChanges;

        private bool CanUpdateInventorySize() => Amalur.IsInitialized && Amalur.GetMaxBagCount() != this.InventorySize;

        private void DeleteEffect(EffectInfo info)
        {
            this.SelectedItem?.DeleteEffect(info);
            this.Refresh();
        }

        private void EditItemHex(ItemModel model)
        {
            if (!Amalur.IsInitialized)
            {
                return;
            }
            ItemEditorWindow view = new ItemEditorWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = new ItemEditorViewModel(model.Item)
            };
            if (view.ShowDialog() == true)
            {
                this.Refresh();
            }
        }

        private void OnFilterChange()
        {
            IEnumerable<ItemModel> items = this.Items;
            if (this._currentDurabilityFilter.Length != 0 && float.TryParse(this._currentDurabilityFilter, out float single))
            {
                int temp = (int)Math.Floor(single);
                items = items.Where(model => (int)Math.Floor(model.CurrentDurability) == temp);
            }
            if (this._maxDurabilityFilter.Length != 0 && float.TryParse(this._maxDurabilityFilter, out single))
            {
                int temp = (int)Math.Floor(single);
                items = items.Where(model => (int)Math.Floor(model.MaxDurability) == temp);
            }
            if (this._itemNameFilter.Length != 0)
            {
                items = items.Where(model => model.ItemName.IndexOf(this._itemNameFilter, StringComparison.OrdinalIgnoreCase) != -1);
            }
            if (this._equipmentTypeFilter.HasValue)
            {
                items = items.Where(model => model.EquipmentType == this._equipmentTypeFilter);
            }
            this.FilteredItems = object.ReferenceEquals(items, this.Items)
                ? (IReadOnlyList<ItemModel>)this.Items
                : items.ToList();
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
        }

        private void Refresh()
        {
            int? selectedItemIndex = this._selectedItem?.ItemIndex;
            this.RepopulateItems();
            if (selectedItemIndex.HasValue)
            {
                this.SelectedItem = this._items.FirstOrDefault(item => item.ItemIndex == selectedItemIndex.Value);
            }
            this.UnsavedChanges = true;
            this.SelectedEffect = Amalur.Effects[0];
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Formerly called ShowAll or btnShowAll_Click
        /// </summary>
        private void RepopulateItems()
        {
            if (!Amalur.IsInitialized)
            {
                MessageBox.Show("No save file opened!", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.OpenFile();
                return;
            }
            this._items.Clear();
            foreach (ItemMemoryInfo info in Amalur.GetAllEquipment())
            {
                this._items.Add(new ItemModel(info));
            }
            this.OnFilterChange();
        }


        private void ResetFilters()
        {
            if (Interlocked.Exchange(ref this._itemNameFilter, string.Empty).Length != 0)
            {
                this.OnPropertyChanged(nameof(this.ItemNameFilter));
            }
            if (Interlocked.Exchange(ref this._maxDurabilityFilter, string.Empty).Length != 0)
            {
                this.OnPropertyChanged(nameof(this.MaxDurabilityFilter));
            }
            if (Interlocked.Exchange(ref this._currentDurabilityFilter, string.Empty).Length != 0)
            {
                this.OnPropertyChanged(nameof(this.CurrentDurabilityFilter));
            }
            if (this._equipmentTypeFilter.HasValue)
            {
                this._equipmentTypeFilter = default;
                this.OnPropertyChanged(nameof(this.EquipmentTypeFilter));
            }
            this.OnFilterChange();
        }

        private void SelectedItem_IsUnsellableChanged(object sender, EventArgs e)
        {
            if (!Amalur.IsInitialized)
            {
                return;
            }
            ItemModel model = (ItemModel)sender;
            Amalur.WriteEquipmentBytes(model.Item, out _);
            this.UnsavedChanges = true;
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
        }

        private void SelectedItem_MateriallyChanged(object sender, EventArgs e)
        {
            if (!Amalur.IsInitialized)
            {
                return;
            }
            ItemModel model = (ItemModel)sender;
            Amalur.WriteEquipmentBytes(model.Item, out bool lengthChanged);
            if (lengthChanged)
            {
                this.Refresh();
            }
            else
            {
                this.UnsavedChanges = true;
            }
        }

        private void UpdateInventorySize()
        {
            if (!Amalur.IsInitialized)
            {
                return;
            }
            Amalur.EditMaxBagCount(this.InventorySize);
            this.UnsavedChanges = true;
        }
    }
}
