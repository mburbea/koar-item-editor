using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public interface IItemFilters
{
    ArmorType ArmorType { get; }

    EquipmentCategory Category { get; }

    Element Element { get; }

    string ItemName { get; }

    Rarity Rarity { get; }
}

public sealed class ItemFilters : NotifierBase, IItemFilters
{
    public static readonly DelegateCommand<ItemFilters> ResetCommand = new(filters => filters.Reset());

    private int _armorType;
    private EquipmentCategory _category;
    private int _element;
    private bool _isExpanded = true;
    private string _itemName = string.Empty;
    private int _rarity;

    public event EventHandler? FilterChange;

    public ArmorType ArmorType
    {
        get => (ArmorType)this._armorType;
        set
        {
            if (this.SetValue(ref this._armorType, (int)value))
            {
                this.OnFilterChange();
            }
        }
    }

    public EquipmentCategory Category
    {
        get => this._category;
        set
        {
            if (this.SetValue(ref this._category, value))
            {
                this.OnFilterChange();
            }
        }
    }

    public Element Element
    {
        get => (Element)this._element;
        set
        {
            if (this.SetValue(ref this._element, (int)value))
            {
                this.OnFilterChange();
            }
        }
    }

    public bool IsExpanded
    {
        get => this._isExpanded;
        set => this.SetValue(ref this._isExpanded, value);
    }

    public string ItemName
    {
        get => this._itemName;
        set
        {
            if (this.SetValue(ref this._itemName, value))
            {
                this.OnFilterChange();
            }
        }
    }

    public Rarity Rarity
    {
        get => (Rarity)this._rarity;
        set
        {
            if (this.SetValue(ref this._rarity, (int)value))
            {
                this.OnFilterChange();
            }
        }
    }

    private void OnFilterChange() => this.FilterChange?.Invoke(this, EventArgs.Empty);

    private void Reset()
    {
        bool changed = false;
        if (Interlocked.Exchange(ref this._itemName, string.Empty).Length != 0)
        {
            this.OnPropertyChanged(nameof(this.ItemName));
            changed = true;
        }
        if (Interlocked.Exchange(ref this._element, default) != default)
        {
            this.OnPropertyChanged(nameof(this.Element));
            changed = true;
        }
        if (Interlocked.Exchange(ref this._rarity, default) != default)
        {
            this.OnPropertyChanged(nameof(this.Rarity));
            changed = true;
        }
        if (Interlocked.Exchange(ref this._armorType, default) != default)
        {
            this.OnPropertyChanged(nameof(this.ArmorType));
            changed = true;
        }
        if (changed)
        {
            this.OnFilterChange();
        }
    }
}

public static class ItemFiltersMethods
{
    public static int GetFilteredItemCount(this IReadOnlyCollection<ItemModelBase> items, IItemFilters filters)
    {
        return filters.IsEmpty() ? items.Count : items.Count(filters.Match);
    }

    public static IReadOnlyList<TItem> GetFilteredItems<TItem>(this IReadOnlyList<TItem> items, IItemFilters filters)
        where TItem : ItemModelBase
    {
        return filters.IsEmpty() ? items : items.Where(filters.Match).ToList();
    }

    private static bool IsEmpty(this IItemFilters filters)
    {
        return filters.Category == default && filters.Rarity == default && filters.Element == default && filters.ArmorType == default && filters.ItemName.Length == 0;
    }

    private static bool Match(this IItemFilters filters, ItemModelBase item)
    {
        return (filters.Category == default || filters.Category == item.Category) &&
            (filters.Rarity == default || filters.Rarity == item.Rarity) &&
            (filters.Element == default || filters.Element == item.Definition.Element) &&
            (filters.ArmorType == default || filters.ArmorType == item.Definition.ArmorType) &&
            (filters.ItemName.Length == 0 || item.DisplayName.Contains(filters.ItemName, StringComparison.InvariantCultureIgnoreCase));
    }
}
