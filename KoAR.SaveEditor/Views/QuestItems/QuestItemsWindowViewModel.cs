using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.QuestItems;

public sealed class QuestItemsWindowViewModel : NotifierBase, IDisposable
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private IReadOnlyList<QuestItemModel> _filteredItems;
    private string _nameFilter = string.Empty;

    public QuestItemsWindowViewModel(MainWindowViewModel mainWindowViewModel)
    {
        this._mainWindowViewModel = mainWindowViewModel;
        GameSave gameSave = this._mainWindowViewModel.GameSave!;
        List<QuestItemModel> items = new(gameSave.QuestItems.Count);
        foreach (QuestItem questItem in gameSave.QuestItems)
        {
            QuestItemModel item = new(questItem);
            item.IsSellableChanged += this.Item_IsSellableChanged;
            items.Add(item);
        }
        this.Items = this._filteredItems = items;
        this.ResetFiltersCommand = new(this.ResetFilters);
    }

    public bool? AllItemsSellable
    {
        get => this.FilteredItems.GetSelectAllCheckBoxValue(item => item.IsSellable);
        set
        {
            foreach (QuestItemModel item in this.FilteredItems)
            {
                item.IsSellable = value.GetValueOrDefault();
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
            item.IsSellableChanged -= this.Item_IsSellableChanged;
        }
    }

    private void Item_IsSellableChanged(object? sender, EventArgs e)
    {
        this.OnPropertyChanged(nameof(this.AllItemsSellable));
        this._mainWindowViewModel.RegisterUnsavedChange();
    }

    private void OnFilterChanged()
    {
        this.FilteredItems = this._nameFilter.Length != 0
            ? this.Items.Where(item => item.Name.IndexOf(this._nameFilter, StringComparison.InvariantCultureIgnoreCase) != -1).ToList()
            : this.Items;
        this.OnPropertyChanged(nameof(this.AllItemsSellable));
    }

    private void ResetFilters()
    {
        if (Interlocked.Exchange(ref this._nameFilter, string.Empty).Length != 0)
        {
            this.OnPropertyChanged(nameof(this.NameFilter));
        }
        this.OnFilterChanged();
    }
}
