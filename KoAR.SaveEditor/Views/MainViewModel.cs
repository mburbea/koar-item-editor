using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using Microsoft.Win32;

namespace KoAR.SaveEditor.Views
{
    public sealed class MainViewModel : NotifierBase
    {
        private readonly NotifyingCollection<ItemModel> _items;
        private int _armorTypeFilter;
        private EquipmentCategory? _categoryFilter;
        private int _elementFilter;
        private string _fileName = string.Empty;
        private IReadOnlyList<ItemModel> _filteredItems;
        private string _itemNameFilter = string.Empty;
        private int _rarityFilter;
        private ItemModel? _selectedItem;
        private bool _unsavedChanges;

        public MainViewModel()
        {
            this._filteredItems = this._items = new NotifyingCollection<ItemModel>();
            this.OpenFileCommand = new DelegateCommand(this.OpenFile);
            this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);
            this.ChangeDefinitionCommand = new DelegateCommand<ItemModel>(this.ChangeDefinition);
            this.AddItemBuffCommand = new DelegateCommand<uint>(this.AddItemBuff, this.CanAddItemBuff);
            this.AddPlayerBuffCommand = new DelegateCommand<uint>(this.AddPlayerBuff, this.CanAddPlayerBuff);
            this.DeleteItemBuffCommand = new DelegateCommand<Buff>(this.DeleteItemBuff, this.CanDeleteItemBuff);
            this.DeletePlayerBuffCommand = new DelegateCommand<Buff>(this.DeletePlayerBuff, this.CanDeletePlayerBuff);
            this.SaveCommand = new DelegateCommand(this.Save, () => this._unsavedChanges);
            this.AddStashItemCommand = new DelegateCommand(this.AddStashItem, () => Amalur.IsFileOpen && Amalur.Stash != null);
        }

        public DelegateCommand<uint> AddItemBuffCommand { get; }

        public DelegateCommand<uint> AddPlayerBuffCommand { get; }

        public DelegateCommand AddStashItemCommand { get; }

        public bool? AllItemsUnsellable
        {
            get => this.GetAppliesToAllItems(item => item.IsUnsellable);
            set
            {
                foreach (ItemModel item in this.FilteredItems)
                {
                    item.IsUnsellable = value.GetValueOrDefault();
                }
            }
        }

        public bool? AllItemsUnstashable
        {
            get => this.GetAppliesToAllItems(item => item.IsUnstashable);
            set
            {
                foreach (ItemModel item in this.FilteredItems)
                {
                    item.IsUnstashable = value.GetValueOrDefault();
                }
            }
        }

        public ArmorType ArmorTypeFilter
        {
            get => (ArmorType)this._armorTypeFilter;
            set
            {
                if (this.SetValue(ref this._armorTypeFilter, (int)value))
                {
                    this.OnFilterChange();
                }
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

        public DelegateCommand<ItemModel> ChangeDefinitionCommand { get; }

        public DelegateCommand<Buff> DeleteItemBuffCommand { get; }

        public DelegateCommand<Buff> DeletePlayerBuffCommand { get; }

        public Element ElementFilter
        {
            get => (Element)this._elementFilter;
            set
            {
                if (this.SetValue(ref this._elementFilter, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public string FileName
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
            get => Amalur.IsFileOpen ? Amalur.InventorySize : 0;
            set
            {
                if (!Amalur.IsFileOpen || value == Amalur.InventorySize)
                {
                    return;
                }
                Amalur.InventorySize = value;
                this.OnPropertyChanged();
                this.UnsavedChanges = true;
            }
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

        public IReadOnlyList<ItemModel> Items => this._items;

        public DelegateCommand OpenFileCommand { get; }

        public Rarity RarityFilter
        {
            get => (Rarity)this._rarityFilter;
            set
            {
                if (this.SetValue(ref this._rarityFilter, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public DelegateCommand ResetFiltersCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public ItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        public Stash? Stash => Amalur.Stash;

        public bool UnsavedChanges
        {
            get => this._unsavedChanges;
            private set => this.SetValue(ref this._unsavedChanges, value);
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
            this.OnPropertyChanged(nameof(this.InventorySize));
            this.RepopulateItems();
            if (this._categoryFilter.HasValue)
            {
                this._categoryFilter = default;
                this.OnPropertyChanged(nameof(this.CategoryFilter));
            }
            this.ResetFilters();
            this.OnPropertyChanged(nameof(this.Stash));
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

        private void AddItemBuff(uint buffId) => this.SelectedItem?.ItemBuffs.Add(Amalur.GetBuff(buffId));

        private void AddPlayerBuff(uint buffId) => this.SelectedItem?.PlayerBuffs.Add(Amalur.GetBuff(buffId));

        private void AddStashItem()
        {
            if (!Amalur.IsFileOpen || Amalur.Stash == null)
            {
                return;
            }
            ChangeOrAddItemViewModel viewModel = new ChangeOrAddItemViewModel();
            ChangeOrAddItemWindow view = new ChangeOrAddItemWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            if (view.ShowDialog() == true && viewModel.Definition != null)
            {
                Amalur.Stash.AddItem(viewModel.Definition);
                this.UnsavedChanges = true;
                this.OnPropertyChanged(nameof(this.Stash));
                this.RepopulateItems(true);
            }
        }

        private bool CanAddItemBuff(uint buffId) => this.SelectedItem != null && buffId != 0u;

        private bool CanAddPlayerBuff(uint buffId) => this.SelectedItem != null && buffId != 0u;

        private bool CanDeleteItemBuff(Buff buff) => buff != null && this.SelectedItem != null;

        private bool CanDeletePlayerBuff(Buff buff) => buff != null && this.SelectedItem != null;

        private void ChangeDefinition(ItemModel model)
        {
            if (!Amalur.IsFileOpen)
            {
                return;
            }
            ChangeOrAddItemViewModel viewModel = new ChangeOrAddItemViewModel(model);
            ChangeOrAddItemWindow view = new ChangeOrAddItemWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            if (view.ShowDialog() == true && viewModel.Definition != null)
            {
                model.TypeDefinition = viewModel.Definition;
                Amalur.WriteEquipmentBytes(model.Item, true);
            }
        }

        private void DeleteItemBuff(Buff buff) => this.SelectedItem?.ItemBuffs.Remove(buff);

        private void DeletePlayerBuff(Buff buff) => this.SelectedItem?.PlayerBuffs.Remove(buff);

        private bool? GetAppliesToAllItems(Func<ItemModel, bool> projection)
        {
            if (this.FilteredItems.Count == 0)
            {
                return true;
            }
            bool first = projection(this.FilteredItems[0]);
            return this.FilteredItems.Skip(1).Select(projection).Any(value => value != first)
                ? default(bool?)
                : first;
        }

        private IReadOnlyList<ItemModel> GetFilteredItems()
        {
            IEnumerable<ItemModel> items = this.Items;
            if (this.RarityFilter != default)
            {
                items = items.Where(model => model.Rarity == this.RarityFilter);
            }
            if (this.ElementFilter != default)
            {
                items = items.Where(model => model.TypeDefinition.Element == this.ElementFilter);
            }
            if (this.ArmorTypeFilter != default)
            {
                items = items.Where(model => model.TypeDefinition.ArmorType == this.ArmorTypeFilter);
            }
            if (this.CategoryFilter.HasValue)
            {
                items = items.Where(model => model.Category == this.CategoryFilter.GetValueOrDefault());
            }
            if (this.ItemNameFilter.Length != 0)
            {
                items = items.Where(model => model.DisplayName.IndexOf(this.ItemNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1);
            }
            return object.Equals(items, this.Items) ? this.Items : items.ToList();
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
            switch (e.PropertyName)
            {
                case nameof(ItemModel.IsUnsellable):
                    this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
                    break;
                case nameof(ItemModel.IsUnstashable):
                    this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
                    break;
            }
        }

        private void OnFilterChange()
        {
            this.FilteredItems = this.GetFilteredItems();
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
        }

        private void RepopulateItems(bool regenerate = false)
        {
            if (!Amalur.IsFileOpen)
            {
                MessageBox.Show("No save file opened!", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.OpenFile();
                return;
            }
            using (this._items.CreatePauseEventsScope())
            {
                for (int index = this._items.Count - 1; index != -1; index--)
                {
                    using ItemModel item = this._items[index];
                    PropertyChangedEventManager.RemoveHandler(item, this.Item_PropertyChanged, string.Empty);
                    this._items.RemoveAt(index);
                }
                if (regenerate)
                {
                    Amalur.GetAllEquipment();
                }
                foreach (ItemModel item in Amalur.Items.Select(info => new ItemModel(info)))
                {
                    PropertyChangedEventManager.AddHandler(item, this.Item_PropertyChanged, string.Empty);
                    this._items.Add(item);
                }
            }
            this.OnFilterChange();
        }

        private void ResetFilters()
        {
            if (Interlocked.Exchange(ref this._itemNameFilter, string.Empty).Length != 0)
            {
                this.OnPropertyChanged(nameof(this.ItemNameFilter));
            }
            if (Interlocked.Exchange(ref this._elementFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ElementFilter));
            }
            if (Interlocked.Exchange(ref this._rarityFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.RarityFilter));
            }
            if (Interlocked.Exchange(ref this._armorTypeFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ArmorTypeFilter));
            }
            this.OnFilterChange();
        }
    }
}
