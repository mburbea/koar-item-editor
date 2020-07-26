using System;
using System.Collections.Generic;
using System.ComponentModel;
using KoAR.Core;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.InventoryManager
{
    public sealed class InventoryManagerViewModel : ManagerViewModelBase<Item, ItemModel>
    {
        public InventoryManagerViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel, item => new ItemModel(item))
        {
        }

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
            get => this.FilteredItems.GetAppliesToAll(item => item.IsUnsellable);
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
            get => this.FilteredItems.GetAppliesToAll(item => item.IsUnstashable);
            set
            {
                foreach (ItemModel item in this.FilteredItems)
                {
                    item.IsUnstashable = value.GetValueOrDefault();
                }
            }
        }

        protected override IReadOnlyCollection<Item> GameItems => this.GameSave.Items;

        protected override void OnItemPropertyChanged(ItemModel item, string propertyName)
        {
            base.OnItemPropertyChanged(item, propertyName);
            
        }

        protected override void OnFilterChange()
        {
            base.OnFilterChange();
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            this.OnPropertyChanged(nameof(this.AllItemsUnstashable));
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
