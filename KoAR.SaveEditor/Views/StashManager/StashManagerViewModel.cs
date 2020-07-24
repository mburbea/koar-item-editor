﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.StashManager
{
    public sealed class StashManagerViewModel : NotifierBase, IDisposable
    {
        private readonly NotifyingCollection<StashItemModel> _items;
        private IReadOnlyList<StashItemModel> _filteredItems;
        private MainViewModel _mainViewModel;
        private StashItemModel? _selectedItem;

        public StashManagerViewModel(MainViewModel mainViewModel)
        {
            this._filteredItems = this._items = new NotifyingCollection<StashItemModel>();
            this._mainViewModel = mainViewModel;
            this.ItemFilters.FilterChange += this.ItemFilters_FilterChange;
            this.RepopulateItems();
            this.AddStashItemCommand = new DelegateCommand(this.AddStashItem);
        }

        public DelegateCommand AddStashItemCommand { get; }

        public bool? AllItemsStolen => this.FilteredItems.GetAppliesToAll(item => item.IsStolen);

        public IReadOnlyList<StashItemModel> FilteredItems
        {
            get => this._filteredItems;
            set => this.SetValue(ref this._filteredItems, value);
        }

        public ItemFilters ItemFilters { get; } = new ItemFilters();

        public IReadOnlyList<StashItemModel> Items => this._items;

        public StashItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        public Stash Stash => this._mainViewModel.Stash!;

        public void Dispose()
        {
            this.ItemFilters.FilterChange -= this.ItemFilters_FilterChange;
        }

        private void AddStashItem()
        {
            ChangeOrAddItemViewModel viewModel = new ChangeOrAddItemViewModel();
            ChangeOrAddItemView view = new ChangeOrAddItemView
            {
                Owner = Application.Current.Windows.OfType<StashManagerView>().First(),
                DataContext = viewModel
            };
            if (view.ShowDialog() != true || viewModel.Definition == null)
            {
                return;
            }
            this._mainViewModel.AddStashItem(viewModel.Definition);
            this.OnPropertyChanged(nameof(this.Stash));
            this.RepopulateItems();
        }

        private void ItemFilters_FilterChange(object sender, EventArgs e) => this.OnFilterChange();

        private void OnFilterChange()
        {
            this.FilteredItems = this.ItemFilters.GetFilteredItems(this.Items);
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsStolen));
        }

        private void RepopulateItems()
        {
            using (this._items.CreatePauseEventsScope())
            {
                this._items.ForEach(item => item.Dispose());
                this._items.Clear();
                this.Stash.Items.ForEach(item => this._items.Add(new StashItemModel(item)));
            }
            this.OnFilterChange();
        }
    }
}