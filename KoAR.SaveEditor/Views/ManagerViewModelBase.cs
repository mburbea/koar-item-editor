using System;
using System.Collections.Generic;
using System.ComponentModel;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views;

public abstract class ManagerViewModelBase<TItem> : NotifierBase, IDisposable
    where TItem : ItemModelBase
{
    private readonly NotifyingCollection<TItem> _items;
    private readonly Func<GameSave, IEnumerable<TItem>> _itemsProjection;
    private IReadOnlyList<TItem> _filteredItems;
    private TItem? _selectedItem;

    protected ManagerViewModelBase(MainWindowViewModel mainWindowViewModel, Func<GameSave, IEnumerable<TItem>> itemsProjection)
    {
        this.MainWindowViewModel = mainWindowViewModel;
        this._itemsProjection = itemsProjection;
        this._filteredItems = this._items = new NotifyingCollection<TItem>();
        this.ItemFilters.FilterChange += this.ItemFilters_FilterChange;
        this.RepopulateItems();
    }

    public bool? AllItemsStolen => this.FilteredItems.GetSelectAllCheckBoxValue(item => item.IsStolen);

    public IReadOnlyList<TItem> FilteredItems
    {
        get => this._filteredItems;
        private set => this.SetValue(ref this._filteredItems, value);
    }

    public GameSave GameSave => this.MainWindowViewModel.GameSave!;

    public ItemFilters ItemFilters { get; } = new();

    public IReadOnlyList<TItem> Items => this._items;

    public TItem? SelectedItem
    {
        get => this._selectedItem;
        set => this.SetValue(ref this._selectedItem, value);
    }

    protected MainWindowViewModel MainWindowViewModel { get; }

    public virtual void Dispose()
    {
        foreach (TItem item in this._items)
        {
            this.DetachEvents(item);
            item.Dispose();
        }
        this.ItemFilters.FilterChange -= this.ItemFilters_FilterChange;
        GC.SuppressFinalize(this);
    }

    protected void AddItem(TItem item)
    {
        this.AttachEvents(item);
        this._items.Add(item);
        this.FilteredItems = this.Items.GetFilteredItems(this.ItemFilters);
    }

    protected virtual void AttachEvents(TItem item) => PropertyChangedEventManager.AddHandler(item, this.Item_PropertyChanged, string.Empty);

    protected virtual void DetachEvents(TItem item) => PropertyChangedEventManager.RemoveHandler(item, this.Item_PropertyChanged, string.Empty);

    protected virtual void OnFilterChange()
    {
        this.FilteredItems = this.Items.GetFilteredItems(this.ItemFilters);
        this.SelectedItem = null;
        this.OnPropertyChanged(nameof(this.AllItemsStolen));
    }

    protected virtual void OnItemPropertyChanged(TItem item, string? propertyName)
    {
        this.MainWindowViewModel.RegisterUnsavedChange();
        if (propertyName == nameof(ItemModelBase.IsStolen))
        {
            this.OnPropertyChanged(nameof(this.AllItemsStolen));
        }
    }

    protected void RemoveItem(TItem item) => this.RemoveItemAt(this._items.IndexOf(item));

    protected void RepopulateItems()
    {
        using (this._items.CreatePauseEventsScope())
        {
            for (int index = this._items.Count - 1; index != -1; index--)
            {
                this.RemoveItemAt(index);
            }
            foreach (TItem item in this._itemsProjection(this.GameSave))
            {
                this.AddItem(item);
            }
        }
        this.OnFilterChange();
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) => this.OnItemPropertyChanged((TItem)sender!, e.PropertyName);

    private void ItemFilters_FilterChange(object? sender, EventArgs e) => this.OnFilterChange();

    private void RemoveItemAt(int index)
    {
        using TItem item = this._items[index];
        this.DetachEvents(item);
        this._items.RemoveAt(index);
        this.FilteredItems = this.Items.GetFilteredItems(this.ItemFilters);
    }
}
