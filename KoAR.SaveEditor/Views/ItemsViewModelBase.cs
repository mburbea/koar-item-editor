using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public abstract class ItemsViewModelBase<TItemModel> : NotifierBase
        where TItemModel : ItemModelBase
    {
        private readonly NotifyingCollection<TItemModel> _items;
        private int _armorTypeFilter;
        private EquipmentCategory? _categoryFilter;
        private int _elementFilter;
        private IReadOnlyList<TItemModel> _filteredItems;
        private string _itemNameFilter = string.Empty;
        private int _rarityFilter;
        private TItemModel? _selectedItem;

        protected ItemsViewModelBase()
        {
            this._filteredItems = this._items = new NotifyingCollection<TItemModel>();
            this.ResetFiltersCommand = new DelegateCommand(this.ResetFilters);
        }

        public ArmorType ArmorTypeFilter
        {
            get => (ArmorType)this._armorTypeFilter;
            set
            {
                if (this.SetValue(ref this._armorTypeFilter, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public EquipmentCategory? CategoryFilter
        {
            get => this._categoryFilter;
            set
            {
                if (this.SetValue(ref this._categoryFilter, value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public Element ElementFilter
        {
            get => (Element)this._elementFilter;
            set
            {
                if (this.SetValue(ref this._elementFilter, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public IReadOnlyList<TItemModel> FilteredItems
        {
            get => this._filteredItems;
            private set => this.SetValue(ref this._filteredItems, value);
        }

        public string ItemNameFilter
        {
            get => this._itemNameFilter;
            set
            {
                if (this.SetValue(ref this._itemNameFilter, value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public IReadOnlyList<TItemModel> Items => this._items;

        public Rarity RarityFilter
        {
            get => (Rarity)this._rarityFilter;
            set
            {
                if (this.SetValue(ref this._rarityFilter, (int)value))
                {
                    this.OnFilterChange();
                }
            }
        }

        public TItemModel? SelectedItem
        {
            get => this._selectedItem;
            set => this.SetValue(ref this._selectedItem, value);
        }

        public DelegateCommand ResetFiltersCommand { get; }

        protected virtual void OnFilterChange()
        {
            this.FilteredItems = this.GetFilteredItems();
            this.SelectedItem = null;
            this.OnPropertyChanged(nameof(this.AllItemsStolen));
        }

        /// <summary>
        /// Resets the <see cref="CategoryFilter"/> property without re-filtering items.
        /// </summary>
        protected void ResetCategoryFilter()
        {
            if (this._categoryFilter.HasValue)
            {
                this._categoryFilter = default;
                this.OnPropertyChanged(nameof(this.CategoryFilter));
            }
        }

        public virtual bool? AllItemsStolen
        {
            get => this.GetAppliesToAllItems(item => item.IsStolen);
            set => throw new NotSupportedException();
        }

        protected bool? GetAppliesToAllItems(Func<TItemModel, bool> projection)
        {
            if (this.FilteredItems.Count == 0)
            {
                return true;
            }
            bool first = projection(this.FilteredItems[0]);
            return this.FilteredItems.Skip(1).Select(projection).Any(value => value != first)
                ? default(bool?)
                : first;
        }

        private IReadOnlyList<TItemModel> GetFilteredItems()
        {
            IEnumerable<TItemModel> items = this.Items;
            if (this.RarityFilter != default)
            {
                items = items.Where(model => model.Rarity == this.RarityFilter);
            }
            if (this.ElementFilter != default)
            {
                items = items.Where(model => model.TypeDefinition.Element == this.ElementFilter);
            }
            if (this.ArmorTypeFilter != default)
            {
                items = items.Where(model => model.TypeDefinition.ArmorType == this.ArmorTypeFilter);
            }
            if (this.CategoryFilter.HasValue)
            {
                items = items.Where(model => model.Category == this.CategoryFilter.GetValueOrDefault());
            }
            if (this.ItemNameFilter.Length != 0)
            {
                items = items.Where(model => model.DisplayName.IndexOf(this.ItemNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1);
            }
            return object.Equals(items, this.Items) ? this.Items : items.ToList();
        }

        private void ResetFilters()
        {
            if (Interlocked.Exchange(ref this._itemNameFilter, string.Empty).Length != 0)
            {
                this.OnPropertyChanged(nameof(this.ItemNameFilter));
            }
            if (Interlocked.Exchange(ref this._elementFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ElementFilter));
            }
            if (Interlocked.Exchange(ref this._rarityFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.RarityFilter));
            }
            if (Interlocked.Exchange(ref this._armorTypeFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ArmorTypeFilter));
            }
            this.OnFilterChange();
        }
    }
}
