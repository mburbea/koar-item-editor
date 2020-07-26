using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.InventoryManager
{
    public sealed class InventoryManagerViewModel : ManagerViewModelBase<Item, ItemModel>
    {
        public InventoryManagerViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel, item => new ItemModel(item))
        {
            this.AddItemBuffCommand = new DelegateCommand<uint>(this.AddItemBuff, this.CanAddItemBuff);
            this.AddPlayerBuffCommand = new DelegateCommand<uint>(this.AddPlayerBuff, this.CanAddPlayerBuff);
            this.ChangeDefinitionCommand = new DelegateCommand<ItemModel>(this.ChangeDefinition);
            this.DeleteItemBuffCommand = new DelegateCommand<Buff>(this.DeleteItemBuff, this.CanDeleteItemBuff);
            this.DeletePlayerBuffCommand = new DelegateCommand<Buff>(this.DeletePlayerBuff, this.CanDeletePlayerBuff);
        }

        public DelegateCommand<uint> AddItemBuffCommand { get; }

        public DelegateCommand<uint> AddPlayerBuffCommand { get; }

        public new bool? AllItemsStolen
        {
            get => base.AllItemsStolen;
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
            get => this.GetSelectAllCheckBoxValue(item => item.IsUnsellable);
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
            get => this.GetSelectAllCheckBoxValue(item => item.IsUnstashable);
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

        public int InventorySize
        {
            get => this.GameSave?.InventorySize ?? 0;
            set
            {
                if (this.GameSave == null || value == this.GameSave.InventorySize)
                {
                    return;
                }
                this.GameSave.InventorySize = value;
                this.OnPropertyChanged();
                this.MainWindowViewModel.RegisterUnsavedChange();
            }
        }

        protected override IReadOnlyCollection<Item> GameItems => this.GameSave.Items;

        protected override void OnFilterChange()
        {
            base.OnFilterChange();
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
        }

        protected override void OnItemPropertyChanged(ItemModel item, string propertyName)
        {
            base.OnItemPropertyChanged(item, propertyName);
            this.GameSave.WriteEquipmentBytes(item.Item);
            switch (propertyName)
            {
                case nameof(ItemModel.IsUnsellable):
                    this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
                    break;
                case nameof(ItemModel.IsUnstashable):
                    this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
                    break;
            }
        }

        private void AddItemBuff(uint buffId) => this.SelectedItem?.AddItemBuff(Amalur.GetBuff(buffId));

        private void AddPlayerBuff(uint buffId) => this.SelectedItem?.AddPlayerBuff(Amalur.GetBuff(buffId));

        private bool CanAddItemBuff(uint buffId) => this.SelectedItem != null && buffId != 0u;

        private bool CanAddPlayerBuff(uint buffId) => this.SelectedItem != null && buffId != 0u;

        private bool CanDeleteItemBuff(Buff buff) => this.SelectedItem != null && buff != null;

        private bool CanDeletePlayerBuff(Buff buff) => this.SelectedItem != null && buff != null;

        private void ChangeDefinition(ItemModel item)
        {
            if (this.GameSave == null)
            {
                return;
            }
            ChangeOrAddItemViewModel viewModel = new ChangeOrAddItemViewModel(item);
            ChangeOrAddItemView view = new ChangeOrAddItemView
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            if (view.ShowDialog() == true && viewModel.Definition != null)
            {
                item.Definition = viewModel.Definition;
                this.GameSave.WriteEquipmentBytes(item.Item, true);
            }
        }

        private void DeleteItemBuff(Buff buff) => this.SelectedItem?.RemoveItemBuff(buff);

        private void DeletePlayerBuff(Buff buff) => this.SelectedItem?.RemovePlayerBuff(buff);

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
