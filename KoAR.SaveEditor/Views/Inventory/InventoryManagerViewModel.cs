using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.Main;
using KoAR.SaveEditor.Views.QuestItems;

namespace KoAR.SaveEditor.Views.Inventory
{
    public sealed class InventoryManagerViewModel : ManagerViewModelBase<ItemModel>
    {
        public InventoryManagerViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel, gameSave => gameSave.Items.Select(item => new ItemModel(item)))
        {
            this.AddItemBuffCommand = new(this.AddItemBuff, this.CanAddItemBuff);
            this.AddPlayerBuffCommand = new(this.AddPlayerBuff, this.CanAddPlayerBuff);
            this.ChangeDefinitionCommand = new(this.ChangeDefinition, this.CanChangeDefinition);
            this.DeleteItemBuffCommand = new(this.DeleteItemBuff, this.CanDeleteItemBuff);
            this.DeletePlayerBuffCommand = new(this.DeletePlayerBuff, this.CanDeletePlayerBuff);
            this.OpenQuestItemsWindowCommand = new(this.OpenQuestItemsWindow);
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
            get => this.FilteredItems.GetSelectAllCheckBoxValue(item => item.IsUnsellable);
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
            get => this.FilteredItems.GetSelectAllCheckBoxValue(item => item.IsUnstashable);
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

        public DelegateCommand OpenQuestItemsWindowCommand { get; }

        public int QuestItemCount => this.GameSave.QuestItems.Count;

        protected override void OnFilterChange()
        {
            base.OnFilterChange();
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
        }

        protected override void OnItemPropertyChanged(ItemModel item, string? propertyName)
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

        private bool CanChangeDefinition(ItemModel item) => !item.IsEquipped && !item.IsUnknown;

        private bool CanDeleteItemBuff(Buff buff) => this.SelectedItem != null && buff != null;

        private bool CanDeletePlayerBuff(Buff buff) => this.SelectedItem != null && buff != null;

        private void ChangeDefinition(ItemModel item)
        {
            if (this.GameSave == null)
            {
                return;
            }
            ChangeOrAddItemViewModel viewModel = new(item);
            ChangeOrAddItemView view = new()
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            if (view.ShowDialog() == true && viewModel.Definition != null)
            {
                item.ChangeDefinition(viewModel.Definition, viewModel.RetainStats);
                this.GameSave.WriteEquipmentBytes(item.Item, true);
            }
        }

        private void DeleteItemBuff(Buff buff) => this.SelectedItem?.RemoveItemBuff(buff);

        private void DeletePlayerBuff(Buff buff) => this.SelectedItem?.RemovePlayerBuff(buff);

        private void OpenQuestItemsWindow()
        {
            using QuestItemsWindowViewModel viewModel = new(this.MainWindowViewModel);
            QuestItemsWindow window = new()
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow,
            };
            window.ShowDialog();
        }
    }
}
