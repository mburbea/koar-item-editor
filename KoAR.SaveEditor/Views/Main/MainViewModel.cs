using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.StashManager;
using Microsoft.Win32;

namespace KoAR.SaveEditor.Views.Main
{
    public sealed class MainViewModel : NotifierBase
    {
        private readonly NotifyingCollection<ItemModel> _items;
        private IReadOnlyList<ItemModel> _filteredItems;
        private GameSave? _gameSave;
        private ItemModel? _selectedItem;
        private bool _unsavedChanges;

        public MainViewModel()
        {
            this._filteredItems = this._items = new NotifyingCollection<ItemModel>();
            this.ItemFilters.FilterChange += this.ItemFilters_FilterChange;
            this.OpenFileCommand = new DelegateCommand(this.OpenFile);
            this.ChangeDefinitionCommand = new DelegateCommand<ItemModel>(this.ChangeDefinition);
            this.AddItemBuffCommand = new DelegateCommand<uint>(this.AddItemBuff, this.CanAddItemBuff);
            this.AddPlayerBuffCommand = new DelegateCommand<uint>(this.AddPlayerBuff, this.CanAddPlayerBuff);
            this.DeleteItemBuffCommand = new DelegateCommand<Buff>(this.DeleteItemBuff, this.CanDeleteItemBuff);
            this.DeletePlayerBuffCommand = new DelegateCommand<Buff>(this.DeletePlayerBuff, this.CanDeletePlayerBuff);
            this.SaveCommand = new DelegateCommand(this.Save, () => this._unsavedChanges);
            this.OpenStashManagerCommand = new DelegateCommand(this.OpenStashManager, () => this._gameSave?.Stash != null);
        }

        public DelegateCommand<uint> AddItemBuffCommand { get; }

        public DelegateCommand<uint> AddPlayerBuffCommand { get; }

        public bool? AllItemsStolen
        {
            get => this.Items.GetAppliesToAll(item => item.IsStolen);
            set
            {
                foreach (ItemModel item in this.FilteredItems)
                {
                    item.IsStolen = value.GetValueOrDefault();
                }
            }
        }

        public bool? AllItemsUnsellable
        {
            get => this.Items.GetAppliesToAll(item => item.IsUnsellable);
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
            get => this.Items.GetAppliesToAll(item => item.IsUnstashable);
            set
            {
                foreach (ItemModel item in this.FilteredItems)
                {
                    item.IsUnstashable = value.GetValueOrDefault();
                }
            }
        }

        public DelegateCommand<ItemModel> ChangeDefinitionCommand { get; }

        public DelegateCommand<Buff> DeleteItemBuffCommand { get; }

        public DelegateCommand<Buff> DeletePlayerBuffCommand { get; }

        public string? FileName => this._gameSave?.FileName;

        public IReadOnlyList<ItemModel> FilteredItems
        {
            get => this._filteredItems;
            private set => this.SetValue(ref this._filteredItems, value);
        }

        public int InventorySize
        {
            get => this._gameSave?.InventorySize ?? 0;
            set
            {
                if (this._gameSave == null || value == this._gameSave.InventorySize)
                {
                    return;
                }
                this._gameSave.InventorySize = value;
                this.OnPropertyChanged();
                this.RegisterUnsavedChange();
            }
        }

        public ItemFilters ItemFilters { get; } = new ItemFilters();

        public IReadOnlyList<ItemModel> Items => this._items;

        public DelegateCommand OpenFileCommand { get; }

        public DelegateCommand OpenStashManagerCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public ItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        public Stash? Stash => this._gameSave?.Stash;

        public bool UnsavedChanges
        {
            get => this._unsavedChanges;
            private set => this.SetValue(ref this._unsavedChanges, value);
        }

        internal void AddStashItem(ItemDefinition definition)
        {
            if (this._gameSave?.Stash == null)
            {
                return;
            }
            this._gameSave.Stash.AddItem(definition);
            this.OnPropertyChanged(nameof(this.Stash));
            this.RepopulateItems(regenerate: true);
            this.RegisterUnsavedChange();
        }

        internal void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open Save File...",
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav",
                CheckFileExists = true,
            };
            if (dialog.ShowDialog(Application.Current.MainWindow) != true)
            {
                return;
            }
            this._gameSave = new GameSave(dialog.FileName);
            this.OnPropertyChanged(nameof(this.FileName));
            this.OnPropertyChanged(nameof(this.InventorySize));
            this.RepopulateItems();
            this.ItemFilters.ResetFilters(true);
            this.OnPropertyChanged(nameof(this.Stash));
            this._unsavedChanges = false;
            this.OnPropertyChanged(nameof(this.UnsavedChanges));
        }

        internal void RegisterUnsavedChange() => this.UnsavedChanges = true;

        internal void Save()
        {
            if (this._gameSave == null)
            {
                return;
            }
            this._gameSave.SaveFile();
            this.UnsavedChanges = false;
            this.RepopulateItems();
            MessageBox.Show($"Save successful! Original save backed up as {this.FileName}.bak.", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddItemBuff(uint buffId) => this._selectedItem?.AddItemBuff(Amalur.GetBuff(buffId));

        private void AddPlayerBuff(uint buffId) => this._selectedItem?.AddPlayerBuff(Amalur.GetBuff(buffId));

        private bool CanAddItemBuff(uint buffId) => this._selectedItem != null && buffId != 0u;

        private bool CanAddPlayerBuff(uint buffId) => this._selectedItem != null && buffId != 0u;

        private bool CanDeleteItemBuff(Buff buff) => this._selectedItem != null && buff != null;

        private bool CanDeletePlayerBuff(Buff buff) => this._selectedItem != null && buff != null;

        private void ChangeDefinition(ItemModel model)
        {
            if (this._gameSave == null)
            {
                return;
            }
            ChangeOrAddItemViewModel viewModel = new ChangeOrAddItemViewModel(model);
            ChangeOrAddItemView view = new ChangeOrAddItemView
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            if (view.ShowDialog() == true && viewModel.Definition != null)
            {
                model.Definition = viewModel.Definition;
                this._gameSave.WriteEquipmentBytes(model.Item, true);
            }
        }

        private void DeleteItemBuff(Buff buff) => this._selectedItem?.RemoveItemBuff(buff);

        private void DeletePlayerBuff(Buff buff) => this._selectedItem?.RemovePlayerBuff(buff);

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this._gameSave == null)
            {
                return;
            }
            ItemModel model = (ItemModel)sender;
            this._gameSave.WriteEquipmentBytes(model.Item);
            this.RegisterUnsavedChange();
            switch (e.PropertyName)
            {
                case nameof(ItemModel.IsStolen):
                    this.OnPropertyChanged(nameof(this.AllItemsStolen));
                    break;
                case nameof(ItemModel.IsUnsellable):
                    this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
                    break;
                case nameof(ItemModel.IsUnstashable):
                    this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
                    break;
            }
        }

        private void ItemFilters_FilterChange(object? sender, EventArgs e) => this.OnFilterChange();

        private void OnFilterChange()
        {
            this.FilteredItems = this.ItemFilters.GetFilteredItems(this.Items);
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsStolen));
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
        }

        private void OpenStashManager()
        {
            using StashManagerViewModel viewModel = new StashManagerViewModel(this);
            StashManagerView view = new StashManagerView
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            view.ShowDialog();
        }

        private void RepopulateItems(bool regenerate = false)
        {
            if (this._gameSave == null)
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
                    this._gameSave.GetAllEquipment();
                    this.OnPropertyChanged(nameof(this.Stash));
                }
                foreach (ItemModel item in this._gameSave.Items.Select(item => new ItemModel(item)))
                {
                    PropertyChangedEventManager.AddHandler(item, this.Item_PropertyChanged, string.Empty);
                    this._items.Add(item);
                }
            }
            this.OnFilterChange();
        }
    }
}
