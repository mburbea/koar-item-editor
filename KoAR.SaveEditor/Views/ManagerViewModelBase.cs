using System;
using System.Collections.Generic;
using System.ComponentModel;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views
{
    public abstract class ManagerViewModelBase<TItem, TItemModel> : NotifierBase, IDisposable
        where TItem : IItem
        where TItemModel : ItemModelBase<TItem>
    {
        private readonly NotifyingCollection<TItemModel> _items;
        private readonly Func<TItem, TItemModel> _modelProjection;
        private IReadOnlyList<TItemModel> _filteredItems;
        private TItemModel? _selectedItem;

        protected ManagerViewModelBase(MainWindowViewModel mainWindowViewModel, Func<TItem, TItemModel> modelProjection)
        {
            this._modelProjection = modelProjection;
            this._filteredItems = this._items = new NotifyingCollection<TItemModel>();
            this.MainWindowViewModel = mainWindowViewModel;
            (this.ItemFilters = new ItemFilters()).FilterChange += this.ItemFilters_FilterChange;
            this.RepopulateItems();
        }

        public bool? AllItemsStolen => this.FilteredItems.GetAppliesToAll(item => item.IsStolen);

        public IReadOnlyList<TItemModel> FilteredItems
        {
            get => this._filteredItems;
            private set => this.SetValue(ref this._filteredItems, value);
        }

        protected GameSave GameSave => this.MainWindowViewModel.GameSave!;

        public ItemFilters ItemFilters { get; }

        public IReadOnlyList<TItemModel> Items => this._items;

        public TItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        protected abstract IReadOnlyCollection<TItem> GameItems { get; }

        protected MainWindowViewModel MainWindowViewModel { get; }

        public virtual void Dispose()
        {
            foreach (TItemModel item in this._items)
            {
                this.DetachEvents(item);
            }
            this.ItemFilters.FilterChange -= this.ItemFilters_FilterChange;
        }

        protected void AddItem(TItemModel item)
        {
            this.AttachEvents(item);
            this._items.Add(item);
        }

        protected virtual void AttachEvents(TItemModel item)
        {
            PropertyChangedEventManager.AddHandler(item, this.Item_PropertyChanged, string.Empty);
        }

        protected virtual void DetachEvents(TItemModel item)
        {
            PropertyChangedEventManager.RemoveHandler(item, this.Item_PropertyChanged, string.Empty);
        }

        protected virtual void OnFilterChange()
        {
            this.FilteredItems = this.ItemFilters.GetFilteredItems(this.Items);
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsStolen));
        }

        protected virtual void OnItemPropertyChanged(TItemModel item, string propertyName)
        {
            this.MainWindowViewModel.RegisterUnsavedChange();
            if (propertyName == nameof(ItemModelBase.IsStolen))
            {
                this.OnPropertyChanged(nameof(this.AllItemsStolen));
            }
        }

        protected virtual void OnRepopulateItemsRequested() => this.RepopulateItems();

        protected void RemoveItem(TItemModel item)
        {
            this.RemoveItemAt(this._items.IndexOf(item));
        }

        protected void RemoveItemAt(int index)
        {
            using TItemModel item = this._items[index];
            this.DetachEvents(item);
            this._items.RemoveAt(index);
        }

        protected void RepopulateItems()
        {
            using (this._items.CreatePauseEventsScope())
            {
                for (int index = this._items.Count - 1; index != -1; index--)
                {
                    this.RemoveItemAt(index);
                }
                foreach (TItem item in this.GameItems)
                {
                    this.AddItem(this._modelProjection(item));
                }
            }
            this.OnFilterChange();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => this.OnItemPropertyChanged((TItemModel)sender, e.PropertyName);

        private void ItemFilters_FilterChange(object sender, EventArgs e) => this.OnFilterChange();

        private void MainWindowViewModel_RepopulateItemsRequested(object sender, EventArgs e) => this.OnRepopulateItemsRequested();
    }
}
