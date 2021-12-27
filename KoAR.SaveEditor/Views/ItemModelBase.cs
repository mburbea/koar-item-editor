using System;
using System.Collections.Generic;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public abstract class ItemModelBase : NotifierBase, IDisposable
{
    protected ItemModelBase(IItem item) => this.Item = item;

    public int AffixCount => (this.Prefix == null ? 0 : 1) + (this.Suffix == null ? 0 : 1);

    public EquipmentCategory Category => this.Item.Definition.Category;

    public virtual float CurrentDurability
    {
        get => this.Item.CurrentDurability;
        set => throw new NotSupportedException();
    }

    public string DefinitionDisplayName => this.Definition.AffixableName && (this.Prefix ?? this.Suffix) != null
        ? $"{this.Prefix?.Modifier} {this.Definition.CategoryDisplayName} {this.Suffix?.Modifier}".Trim()
        : this.Definition.Name;

    public string DisplayName => this.HasCustomName ? this.ItemName : this.DefinitionDisplayName;

    public bool HasCustomName => this.Item.HasCustomName;

    public bool IsEquipped => this.Item.IsEquipped;

    public virtual bool IsStolen
    {
        get => this.Item.IsStolen;
        set => throw new NotSupportedException();
    }

    public IItem Item { get; }

    public abstract IReadOnlyList<Buff> ItemBuffs { get; }

    public virtual string ItemName
    {
        get => this.Item.ItemName;
        set => throw new NotSupportedException();
    }

    public virtual byte Level
    {
        get => this.Item.Level;
        set => throw new NotSupportedException();
    }

    public virtual float MaxDurability
    {
        get => this.Item.MaxDurability;
        set => throw new NotSupportedException();
    }

    public abstract IReadOnlyList<Buff> PlayerBuffs { get; }

    public virtual Buff? Prefix
    {
        get => this.Item.ItemBuffs.Prefix;
        set => throw new NotSupportedException();
    }

    public Rarity Rarity => this.Item.Rarity;

    public virtual Buff? Suffix
    {
        get => this.Item.ItemBuffs.Suffix;
        set => throw new NotSupportedException();
    }

    public ItemDefinition Definition => this.Item.Definition;

    public abstract bool UnsupportedFormat { get; }

    public bool IsUnknown => this.Category == EquipmentCategory.Unknown;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}

public abstract class ItemModelBase<TItem> : ItemModelBase
    where TItem : IItem
{
    protected ItemModelBase(TItem item)
        : base(item)
    {
    }

    public new TItem Item => (TItem)base.Item;
}
