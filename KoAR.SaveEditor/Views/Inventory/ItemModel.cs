using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.Inventory;

public sealed class ItemModel : ItemModelBase<Item>
{
    private static readonly PropertyInfo _itemProperty = typeof(ItemModelBase<Item>).GetProperty(nameof(ItemModel.Item), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    private static readonly ParameterExpression _modelParameter = Expression.Parameter(typeof(ItemModel), "model");

    private readonly NotifyingCollection<Buff> _itemBuffs;
    private readonly NotifyingCollection<Buff> _playerBuffs;

    public ItemModel(Item item)
        : base(item)
    {
        this._itemBuffs = new(item.ItemBuffs.List);
        this._itemBuffs.CollectionChanged += this.Buffs_CollectionChanged;
        this._playerBuffs = new(item.PlayerBuffs);
        this._playerBuffs.CollectionChanged += this.Buffs_CollectionChanged;
    }

    public override float CurrentDurability
    {
        get => base.CurrentDurability;
        set => this.SetItemValue(value);
    }

    public override bool IsStolen
    {
        get => base.IsStolen;
        set => this.SetItemValue(value);
    }

    public bool IsUnsellable
    {
        get => this.Item.IsUnsellable;
        set => this.SetItemValue(value);
    }

    public bool IsUnstashable
    {
        get => this.Item.IsUnstashable;
        set => this.SetItemValue(value);
    }

    public override IReadOnlyList<Buff> ItemBuffs => this._itemBuffs;

    public int ItemId => this.Item.ItemId;

    public int ItemIndex => this.Item.ItemOffset;

    public override string ItemName
    {
        get => base.ItemName;
        set
        {
            if (this.SetItemValue(value))
            {
                this.OnPropertyChanged(nameof(this.HasCustomName));
                this.OnPropertyChanged(nameof(this.DisplayName));
            }
        }
    }

    public override byte Level
    {
        get => base.Level;
        set => this.SetItemValue(value);
    }

    public override float MaxDurability
    {
        get => base.MaxDurability;
        set => this.SetItemValue(value);
    }

    public override IReadOnlyList<Buff> PlayerBuffs => this._playerBuffs;

    public override Buff? Prefix
    {
        get => base.Prefix;
        set
        {
            if (!this.SetItemValue(value, $"{nameof(IItem.ItemBuffs)}.{nameof(IItemBuffMemory.Prefix)}"))
            {
                return;
            }
            this.OnPropertyChanged(nameof(this.AffixCount));
            this.OnPropertyChanged(nameof(this.Rarity));
            if (this.Item.Definition.AffixableName)
            {
                this.OnPropertyChanged(nameof(DisplayName));
                this.OnPropertyChanged(nameof(DefinitionDisplayName));
            }
        }
    }

    public override Buff? Suffix
    {
        get => base.Suffix;
        set
        {
            if (!this.SetItemValue(value, $"{nameof(IItem.ItemBuffs)}.{nameof(IItemBuffMemory.Suffix)}"))
            {
                return;
            }
            this.OnPropertyChanged(nameof(this.AffixCount));
            this.OnPropertyChanged(nameof(this.Rarity));
            if (this.Item.Definition.AffixableName)
            {
                this.OnPropertyChanged(nameof(DisplayName));
                this.OnPropertyChanged(nameof(DefinitionDisplayName));
            }
        }
    }

    public override bool UnsupportedFormat => this.Item.ItemBuffs.UnsupportedFormat || this.IsUnknown;

    public void AddItemBuff(Buff buff) => this._itemBuffs.Add(buff);

    public void AddPlayerBuff(Buff buff) => this._playerBuffs.Add(buff);

    public void ChangeDefinition(ItemDefinition definition, bool retainStats)
    {
        this.Item.ChangeDefinition(definition, retainStats);
        this.OnPropertyChanged(string.Empty);
        this._playerBuffs.OnReset();
        this._itemBuffs.OnReset();
    }

    public void RemoveItemBuff(Buff buff) => this._itemBuffs.Remove(buff);

    public void RemovePlayerBuff(Buff buff) => this._playerBuffs.Remove(buff);

    protected override void Dispose(bool disposing)
    {
        this._playerBuffs.CollectionChanged -= this.Buffs_CollectionChanged;
        this._itemBuffs.CollectionChanged -= this.Buffs_CollectionChanged;
        base.Dispose(disposing);
    }

    private void Buffs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => this.OnPropertyChanged(nameof(this.Rarity));

    private bool SetItemValue<TValue>(TValue value, [CallerMemberName] string propertyPath = "", [CallerMemberName] string propertyName = "")
    {
        return ValueSetter<TValue>.SetValue(this, value, propertyPath, propertyName);
    }

    private static class ValueSetter<TValue>
    {
        private static readonly PropertyInfo _defaultProperty = typeof(EqualityComparer<TValue>).GetProperty(nameof(EqualityComparer<TValue>.Default), BindingFlags.Public | BindingFlags.Static)!;
        private static readonly MethodInfo _equalsMethod = typeof(EqualityComparer<TValue>).GetMethod(nameof(EqualityComparer<TValue>.Equals), new[] { typeof(TValue), typeof(TValue) })!;
        private static readonly Dictionary<string, Func<ItemModel, TValue, bool>> _setters = new();
        private static readonly ParameterExpression _valueParameter = Expression.Parameter(typeof(TValue), "value");

        public static bool SetValue(ItemModel model, TValue value, string propertyPath, string propertyName)
        {
            if (!ValueSetter<TValue>._setters.TryGetValue(propertyName, out Func<ItemModel, TValue, bool>? setter))
            {
                ValueSetter<TValue>._setters.Add(propertyName, setter = ValueSetter<TValue>.CreateSetter(propertyPath, propertyName));
            }
            return setter(model, value);
        }

        private static Func<ItemModel, TValue, bool> CreateSetter(string propertyPath, string propertyName)
        {
            MemberExpression propertyExpression = propertyPath.Split(ItemModelBase._propertyTokens).Aggregate(
                Expression.Property(
                    ItemModel._modelParameter,
                    ItemModel._itemProperty
                ),
                Expression.Property
            );
            Expression<Func<ItemModel, TValue, bool>> lambdaExpression = Expression.Lambda<Func<ItemModel, TValue, bool>>(
                Expression.Condition(
                    Expression.Call(
                        Expression.Property(
                            null,
                            ValueSetter<TValue>._defaultProperty
                        ),
                        ValueSetter<TValue>._equalsMethod,
                        propertyExpression,
                        ValueSetter<TValue>._valueParameter
                    ),
                    Expression.Constant(BooleanBoxes.False),
                    Expression.Block(
                        Expression.Assign(
                            propertyExpression,
                            ValueSetter<TValue>._valueParameter
                        ),
                        Expression.Call(
                            ItemModel._modelParameter,
                            ItemModelBase._onPropertyChangedMethod,
                            Expression.Constant(propertyName)
                        ),
                        Expression.Constant(BooleanBoxes.True)
                    )
                ),
                ItemModel._modelParameter,
                ValueSetter<TValue>._valueParameter
            );
            return lambdaExpression.Compile();
        }
    }
}
