using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using Microsoft.Win32;

namespace KoAR.SaveEditor.Views
{
    public sealed class MainViewModel : NotifierBase
    {
        private readonly ObservableCollection<ItemModel> _items;
        private EquipmentCategory? _categoryFilter;
        private string _currentDurabilityFilter = string.Empty;
        private string? _fileName;
        private IReadOnlyList<ItemModel> _filteredItems;
        private int _inventorySize;
        private string _itemNameFilter = string.Empty;
        private string _maxDurabilityFilter = string.Empty;
        private ItemModel? _selectedItem;
        private bool _unsavedChanges;

        public MainViewModel()
        {
            this._items = new ObservableCollection<ItemModel>();
            this._filteredItems = this.Items = new ReadOnlyObservableCollection<ItemModel>(this._items);
            this.OpenFileCommand = new DelegateCommand(this.OpenFile);
            this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);
            this.EditItemHexCommand = new DelegateCommand<ItemModel>(this.EditItemHex);
            this.UpdateInventorySizeCommand = new DelegateCommand(this.UpdateInventorySize, this.CanUpdateInventorySize);
            this.AddCoreEffectCommand = new DelegateCommand<uint>(this.AddCoreEffect, this.CanAddCoreEffect);
            this.AddEffectCommand = new DelegateCommand<uint>(this.AddEffect, this.CanAddEffect);
            this.DeleteCoreEffectCommand = new DelegateCommand<uint>(this.DeleteCoreEffect, this.CanDeleteCoreEffect);
            this.DeleteEffectCommand = new DelegateCommand<uint>(this.DeleteEffect, this.CanDeleteEffect);
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
            this.MakeAllItemsDistinctCommand = new DelegateCommand(this.MakeAllItemsDistinct);
        }

        public DelegateCommand<uint> AddCoreEffectCommand
        {
            get;
        }

        public DelegateCommand<uint> AddEffectCommand
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
                if (!Amalur.IsFileOpen)
                {
                    return;
                }
                foreach (ItemModel model in this.FilteredItems)
                {
                    model.IsUnsellable = value.GetValueOrDefault();
                }
                this.OnPropertyChanged();
            }
        }

        public EquipmentCategory? CategoryFilter
        {
            get => this._categoryFilter;
            set
            {
                if (this.SetValue(ref this._categoryFilter, value))
                {
                    this.OnFilterChange();
                }
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

        public DelegateCommand<uint> DeleteCoreEffectCommand
        {
            get;
        }

        public DelegateCommand<uint> DeleteEffectCommand
        {
            get;
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

        public DelegateCommand MakeAllItemsDistinctCommand
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

        public ItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        public bool UnsavedChanges
        {
            get => this._unsavedChanges;
            private set => this.SetValue(ref this._unsavedChanges, value);
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
            this.InventorySize = Amalur.InventorySize;
            this.RepopulateItems();
            this.ResetFilters();
            this._unsavedChanges = false;
            this.OnPropertyChanged(nameof(this.UnsavedChanges));
        }

        internal void Save()
        {
            if (!Amalur.IsFileOpen)
            {
                return;
            }
            Amalur.SaveFile(this._fileName);
            this.UnsavedChanges = false;
            this.RepopulateItems();
            MessageBox.Show($"Save successful! Original save backed up as {this._fileName}.bak.", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCoreEffect(uint code)
        {
            if (this.SelectedItem == null)
            {
                return;
            }
            this.SelectedItem.AddCoreEffect(code);
            this.Refresh();
        }

        private void AddEffect(uint code)
        {
            if (this.SelectedItem == null)
            {
                return;
            }
            this.SelectedItem.AddEffect(code);
            this.Refresh();
        }

        private bool CanAddCoreEffect(uint code) => this.SelectedItem != null && code != 0u;

        private bool CanAddEffect(uint code) => this.SelectedItem != null && code != 0u;

        private bool CanDeleteCoreEffect(uint code) => this.SelectedItem != null && code != 0u;

        private bool CanDeleteEffect(uint code) => this.SelectedItem != null && code != 0u;

        private bool CanSave() => this._unsavedChanges;

        private bool CanUpdateInventorySize() => Amalur.IsFileOpen && Amalur.InventorySize != this.InventorySize;

        private void DeleteCoreEffect(uint code)
        {
            if (this.SelectedItem == null)
            {
                return;
            }
            this.SelectedItem.DeleteCoreEffect(code);
            this.Refresh();
        }

        private void DeleteEffect(uint code)
        {
            if (this.SelectedItem == null)
            {
                return;
            }
            this.SelectedItem.DeleteEffect(code);
            this.Refresh();
        }

        private void EditItemHex(ItemModel model)
        {
            if (!Amalur.IsFileOpen)
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

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Amalur.IsFileOpen)
            {
                return;
            }
            ItemModel model = (ItemModel)sender;
            Amalur.WriteEquipmentBytes(model.Item);
            this.UnsavedChanges = true;
            if (e.PropertyName == nameof(ItemModel.IsUnsellable))
            {
                this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            }
        }

        private void MakeAllItemsDistinct()
        {
            MessageBoxResult result = MessageBox.Show("This operation will assign a unique combination of current and max durability to your inventory and is irreversible.", "KoAR Save Editor", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
            if (result != MessageBoxResult.OK)
            {
                return;
            }
            float maxDurability = 100;
            foreach (IGrouping<EquipmentCategory, ItemModel> grouping in this.Items.Where(model => !model.HasCustomName).GroupBy(model => model.Category))
            {
                float currentDurability = --maxDurability;
                foreach (ItemModel model in grouping)
                {
                    if (currentDurability == 0)
                    {
                        currentDurability = --maxDurability;
                    }
                    model.MaxDurability = maxDurability;
                    model.CurrentDurability = currentDurability--;
                }
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
            if (this._categoryFilter.HasValue)
            {
                items = items.Where(model => model.Category == this._categoryFilter);
            }
            this.FilteredItems = object.ReferenceEquals(items, this.Items)
                ? (IReadOnlyList<ItemModel>)this.Items
                : items.ToList();
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
        }

        private void Refresh()
        {
            //int? selectedItemIndex = this._selectedItem?.ItemIndex;
            //this.RepopulateItems();
            //if (selectedItemIndex.HasValue)
            //{
            //    this.SelectedItem = this._items.FirstOrDefault(item => item.ItemIndex == selectedItemIndex.Value);
            //}
            this.UnsavedChanges = true;
            CommandManager.InvalidateRequerySuggested();
        }

        private void RepopulateItems()
        {
            if (!Amalur.IsFileOpen)
            {
                MessageBox.Show("No save file opened!", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.OpenFile();
                return;
            }
            foreach (ItemModel item in this._items)
            {
                PropertyChangedEventManager.RemoveHandler(item, this.Item_PropertyChanged, string.Empty);
            }
            this._items.Clear();
            foreach (ItemModel item in Amalur.Items.Select(info => new ItemModel(info)))
            {
                PropertyChangedEventManager.AddHandler(item, this.Item_PropertyChanged, string.Empty);
                this._items.Add(item);
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
            if (this._categoryFilter.HasValue)
            {
                this._categoryFilter = default;
                this.OnPropertyChanged(nameof(this.CategoryFilter));
            }
            this.OnFilterChange();
        }

        private void UpdateInventorySize()
        {
            if (!Amalur.IsFileOpen)
            {
                return;
            }
            Amalur.InventorySize = this.InventorySize;
            this.UnsavedChanges = true;
        }
    }
}
