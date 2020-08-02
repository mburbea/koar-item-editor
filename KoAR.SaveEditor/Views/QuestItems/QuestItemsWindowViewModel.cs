﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.QuestItems
{
    public sealed class QuestItemsWindowViewModel : NotifierBase, IDisposable
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private IReadOnlyList<QuestItemModel> _filteredItems;
        private string _nameFilter = string.Empty;

        public QuestItemsWindowViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this._mainWindowViewModel = mainWindowViewModel;
            GameSave gameSave = this._mainWindowViewModel.GameSave!;
            List<QuestItemModel> items = new List<QuestItemModel>(gameSave.QuestItems.Count);
            foreach (QuestItemModel item in gameSave.QuestItems.Select(item => new QuestItemModel(item)))
            {
                item.IsUnsellableChanged += this.Item_IsUnsellableChanged;
                items.Add(item);
            }
            this.Items = this._filteredItems = items;
            this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);
        }

        public bool? AllItemsUnsellable
        {
            get => this.FilteredItems.GetSelectAllCheckBoxValue(item => item.IsUnsellable);
            set
            {
                foreach (QuestItemModel item in this.FilteredItems)
                {
                    item.IsUnsellable = value.GetValueOrDefault();
                }
            }
        }

        public IReadOnlyList<QuestItemModel> FilteredItems
        {
            get => this._filteredItems;
            private set => this.SetValue(ref this._filteredItems, value);
        }

        public IReadOnlyList<QuestItemModel> Items { get; }

        public string NameFilter
        {
            get => this._nameFilter;
            set
            {
                if (this.SetValue(ref this._nameFilter, value))
                {
                    this.OnFilterChanged();
                }
            }
        }

        public DelegateCommand ResetFiltersCommand { get; }

        public void Dispose()
        {
            foreach (QuestItemModel item in this.Items)
            {
                item.IsUnsellableChanged -= this.Item_IsUnsellableChanged;
            }
        }

        private void Item_IsUnsellableChanged(object sender, EventArgs e)
        {
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
            this._mainWindowViewModel.RegisterUnsavedChange();
        }

        private void OnFilterChanged()
        {
            IEnumerable<QuestItemModel> items = this.Items;
            if (this._nameFilter.Length != 0)
            {
                items = items.Where(item => item.Name.IndexOf(this._nameFilter, StringComparison.InvariantCultureIgnoreCase) != -1);
            }
            this.FilteredItems = object.Equals(this.Items, items) ? this.Items : items.ToList();
            this.OnPropertyChanged(nameof(this.AllItemsUnsellable));
        }

        private void ResetFilters()
        {
            if (Interlocked.Exchange(ref this._nameFilter, string.Empty).Length == 0)
            {
                this.OnPropertyChanged(nameof(this.NameFilter));
            }
            this.OnFilterChanged();
        }
    }
}
