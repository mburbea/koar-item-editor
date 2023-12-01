using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.ChangeOrAddItem;

public sealed class ChangeOrAddItemViewModel : NotifierBase
{
    /// <summary>
    /// <see cref="EquipmentCategory" /> enumeration does not start at 0.  Instead of just using default, gets the first category.
    /// </summary>
    private static readonly EquipmentCategory _defaultCategory = Enum.GetValues<EquipmentCategory>()[0];

    private int _armorTypeFilter;
    private EquipmentCategory _category;
    private ItemDefinition? _definition;
    private IReadOnlyList<ItemDefinition>? _definitions;
    private int _elementFilter;
    private int _fateswornFilter;
    private int _rarityFilter;
    private bool _retainStats;

    public ChangeOrAddItemViewModel(GameSave gameSave, ItemModelBase? item = null)
    {
        this._category = item?.Category ?? ChangeOrAddItemViewModel._defaultCategory;
        this._definition = (this.Item = item)?.Definition ?? gameSave.ItemDefinitions.First();
        this.GameSave = gameSave;
        this.ProcessCommand = new(this.Process, () => this._definition != null);
        this.OnFilterChanged();
    }

    public ArmorType ArmorTypeFilter
    {
        get => (ArmorType)this._armorTypeFilter;
        set
        {
            if (this.SetValue(ref this._armorTypeFilter, (int)value))
            {
                this.OnFilterChanged();
            }
        }
    }

    public EquipmentCategory Category
    {
        get => this._category;
        set
        {
            if (!this.SetValue(ref this._category, value))
            {
                return;
            }
            if (Interlocked.Exchange(ref this._rarityFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.RarityFilter));
            }
            if (Interlocked.Exchange(ref this._elementFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ElementFilter));
            }
            if (Interlocked.Exchange(ref this._armorTypeFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.ArmorTypeFilter));
            }
            if (Interlocked.Exchange(ref this._fateswornFilter, default) != default)
            {
                this.OnPropertyChanged(nameof(this.FateswornFilter));
            }
            this.OnFilterChanged();
        }
    }

    public ItemDefinition? Definition
    {
        get => this._definition;
        set => this.SetValue(ref this._definition, value);
    }

    public IReadOnlyList<ItemDefinition>? Definitions
    {
        get => this._definitions;
        private set => this.SetValue(ref this._definitions, value);
    }

    public Element ElementFilter
    {
        get => (Element)this._elementFilter;
        set
        {
            if (this.SetValue(ref this._elementFilter, (int)value))
            {
                this.OnFilterChanged();
            }
        }
    }

    public bool FateswornFilter
    {
        get => this._fateswornFilter != 0;
        set
        {
            if (this.SetValue(ref this._fateswornFilter, value ? 1 : 0))
            {
                this.OnFilterChanged();
            }
        }
    }

    public GameSave GameSave { get; }

    public ItemModelBase? Item { get; }

    public DelegateCommand ProcessCommand { get; }

    public Rarity RarityFilter
    {
        get => (Rarity)this._rarityFilter;
        set
        {
            if (this.SetValue(ref this._rarityFilter, (int)value))
            {
                this.OnFilterChanged();
            }
        }
    }

    public bool RetainStats
    {
        get => this._retainStats;
        set => this.SetValue(ref this._retainStats, value);
    }

    private bool IsMatch(ItemDefinition item)
    {
        return this.Category == item.Category &&
            (this.ElementFilter == default || this.ElementFilter == item.Element) &&
            (this.ArmorTypeFilter == default || this.ArmorTypeFilter == item.ArmorType) &&
            (this.RarityFilter == default || this.RarityFilter == item.Rarity) &&
            (this.FateswornFilter == default || this.FateswornFilter == item.RequiresFatesworn);
    }

    private void OnFilterChanged()
    {
        ItemDefinition? selectedItem = this._definition;
        this.Definitions = this.GameSave.ItemDefinitions.Where(this.IsMatch).ToArray();
        if (selectedItem == null || !this.Definitions.Contains(selectedItem))
        {
            this.Definition = this.Definitions.Count > 0 ? this.Definitions[0] : default;
        }
    }

    private void Process()
    {
        if (this._definition == null)
        {
            return;
        }
        Window window = Application.Current.Windows.OfType<ChangeOrAddItemView>().Single();
        window.DialogResult = true;
        window.Close();
    }
}
