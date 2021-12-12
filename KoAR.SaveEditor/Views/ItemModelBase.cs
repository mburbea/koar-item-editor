using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public abstract class ItemModelBase : NotifierBase, IDisposable
    {
        protected static readonly MethodInfo _onPropertyChangedMethod = typeof(NotifierBase).GetMethod(nameof(ItemModelBase.OnPropertyChanged), BindingFlags.NonPublic | BindingFlags.Instance)!;
        protected static readonly char[] _propertyTokens = { '.' };

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

        public bool IsUnknown => Category.IsUnknown();

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
        private static readonly PropertyInfo _itemProperty = typeof(ItemModelBase<TItem>).GetProperty(nameof(ItemModelBase<TItem>.Item), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        private static readonly ParameterExpression _modelParameter = Expression.Parameter(typeof(ItemModelBase<TItem>), "model");

        protected ItemModelBase(TItem item)
            : base(item)
        {
        }

        public new TItem Item => (TItem)base.Item;

        protected bool SetItemValue<TValue>(TValue value, [CallerMemberName] string propertyPath = "", [CallerMemberName] string propertyName = "")
        {
            return ValueSetter<TValue>.SetValue(this, value, propertyPath, propertyName);
        }

        private static class ValueSetter<TValue>
        {
            private static readonly PropertyInfo _defaultProperty = typeof(EqualityComparer<TValue>).GetProperty(nameof(EqualityComparer<TValue>.Default), BindingFlags.Public | BindingFlags.Static)!;
            private static readonly MethodInfo _equalsMethod = typeof(EqualityComparer<TValue>).GetMethod(nameof(EqualityComparer<TValue>.Equals), new[] { typeof(TValue), typeof(TValue) })!;
            private static readonly Dictionary<string, Func<ItemModelBase<TItem>, TValue, bool>> _setters = new();
            private static readonly ParameterExpression _valueParameter = Expression.Parameter(typeof(TValue), "value");

            public static bool SetValue(ItemModelBase<TItem> model, TValue value, string propertyPath, string propertyName)
            {
                if (!ValueSetter<TValue>._setters.TryGetValue(propertyName, out Func<ItemModelBase<TItem>, TValue, bool>? setter))
                {
                    ValueSetter<TValue>._setters.Add(propertyName, setter = ValueSetter<TValue>.CreateSetter(propertyPath, propertyName));
                }
                return setter(model, value);
            }

            private static Func<ItemModelBase<TItem>, TValue, bool> CreateSetter(string propertyPath, string propertyName)
            {
                MemberExpression propertyExpression = propertyPath.Split(ItemModelBase._propertyTokens).Aggregate(
                    Expression.Property(
                        ItemModelBase<TItem>._modelParameter,
                        ItemModelBase<TItem>._itemProperty
                    ),
                    Expression.Property
                );
                Expression<Func<ItemModelBase<TItem>, TValue, bool>> lambdaExpression = Expression.Lambda<Func<ItemModelBase<TItem>, TValue, bool>>(
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
                                ItemModelBase<TItem>._modelParameter,
                                ItemModelBase._onPropertyChangedMethod,
                                Expression.Constant(propertyName)
                            ),
                            Expression.Constant(BooleanBoxes.True)
                        )
                    ),
                    ItemModelBase<TItem>._modelParameter,
                    ValueSetter<TValue>._valueParameter
                );
                return lambdaExpression.Compile();
            }
        }
    }
}
