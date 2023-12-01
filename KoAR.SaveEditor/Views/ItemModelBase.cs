using System;
using System.Collections.Generic;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public abstract class ItemModelBase(IItem item) : NotifierBase, IDisposable
{
    public int AffixCount => (this.Prefix == null ? 0 : 1) + (this.Suffix == null ? 0 : 1);

    public EquipmentCategory Category => this.Item.Definition.Category;

    public virtual float CurrentDurability
    {
        get => item.CurrentDurability;
        set => throw new NotSupportedException();
    }

    public string DefinitionDisplayName => this.Definition.AffixableName && (this.Prefix ?? this.Suffix) != null
        ? $"{this.Prefix?.Modifier} {this.Definition.CategoryDisplayName} {this.Suffix?.Modifier}".Trim()
        : this.Definition.Name;

    public string DisplayName => this.HasCustomName ? this.ItemName : this.DefinitionDisplayName;

    public bool HasCustomName => item.HasCustomName;

    public bool IsEquipped => item.IsEquipped;

    public virtual bool IsStolen
    {
        get => this.Item.IsStolen;
        set => throw new NotSupportedException();
    }

    public IItem Item => item;

    public abstract IReadOnlyList<Buff> ItemBuffs { get; }

    public virtual string ItemName
    {
        get => item.ItemName;
        set => throw new NotSupportedException();
    }

    public virtual byte Level
    {
        get => item.Level;
        set => throw new NotSupportedException();
    }

    public virtual float MaxDurability
    {
        get => item.MaxDurability;
        set => throw new NotSupportedException();
    }

    public abstract IReadOnlyList<Buff> PlayerBuffs { get; }

    public virtual Buff? Prefix
    {
        get => item.ItemBuffs.Prefix;
        set => throw new NotSupportedException();
    }

    public Rarity Rarity => item.Rarity;

    public virtual Buff? Suffix
    {
        get => item.ItemBuffs.Suffix;
        set => throw new NotSupportedException();
    }

    public ItemDefinition Definition => item.Definition;

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

public abstract class ItemModelBase<TItem>(TItem item) : ItemModelBase(item)
    where TItem : IItem
{
    public new TItem Item => item;
}
