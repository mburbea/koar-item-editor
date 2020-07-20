using System;
using System.Collections.Generic;
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
        protected ItemModelBase(IItem item) => this.Item = item;

        public int AffixCount => (this.Prefix == null ? 0 : 1) + (this.Suffix == null ? 0 : 1);

        public EquipmentCategory Category => this.Item.TypeDefinition.Category;

        public virtual float CurrentDurability
        {
            get => this.Item.CurrentDurability;
            set => throw new NotSupportedException();
        }

        public string DisplayName => this.HasCustomName switch
        {
            true => this.ItemName,
            false when this.TypeDefinition.AffixableName && (this.Prefix ?? this.Suffix) != null => $"{this.Prefix?.Modifier} {this.TypeDefinition.CategoryDisplayName} {this.Suffix?.Modifier}".Trim(),
            false => this.TypeDefinition.Name,
        };

        public bool HasCustomName => this.Item.HasCustomName;

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

        public virtual Buff? Suffix
        {
            get => this.Item.ItemBuffs.Suffix;
            set => throw new NotSupportedException();
        }

        public virtual TypeDefinition TypeDefinition
        {
            get => this.Item.TypeDefinition;
            set => throw new NotSupportedException();
        }

        public abstract bool UnsupportedFormat { get; }

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
        private static readonly PropertyInfo _itemProperty = typeof(ItemModelBase<TItem>).GetProperty(nameof(ItemModelBase<TItem>.Item), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        private static readonly ParameterExpression _modelParameter = Expression.Parameter(typeof(ItemModelBase<TItem>), "model");

        protected ItemModelBase(TItem item)
            : base(item)
        {
        }

        public new TItem Item => (TItem)base.Item;

        protected bool SetItemValue<TValue>(TValue value, [CallerMemberName] string propertyPath = "") => ValueSetter<TValue>.SetValue(this, value, propertyPath);

        private static class ValueSetter<TValue>
        {
            private static readonly MemberExpression _comparerExpression = Expression.Property(null, typeof(EqualityComparer<TValue>).GetProperty(nameof(EqualityComparer<TValue>.Default), BindingFlags.Public | BindingFlags.Static));
            private static readonly MethodInfo _equalsMethod = typeof(EqualityComparer<TValue>).GetMethod(nameof(EqualityComparer<TValue>.Equals), new[] { typeof(TValue), typeof(TValue) });
            private static readonly Dictionary<string, Func<ItemModelBase<TItem>, TValue, bool>> _setters = new Dictionary<string, Func<ItemModelBase<TItem>, TValue, bool>>();

            private static readonly ParameterExpression _valueParameter = Expression.Parameter(typeof(TValue), "value");

            public static bool SetValue(ItemModelBase<TItem> model, TValue value, string propertyPath)
            {
                if (!ValueSetter<TValue>._setters.TryGetValue(propertyPath, out Func<ItemModelBase<TItem>, TValue, bool>? setter))
                {
                    ValueSetter<TValue>._setters.Add(propertyPath, setter = ValueSetter<TValue>.CreateSetter(propertyPath));
                }
                return setter(model, value);
            }

            private static Func<ItemModelBase<TItem>, TValue, bool> CreateSetter(string propertyPath)
            {
                string[] properties = propertyPath.Split('.');
                MemberExpression propertyExpression = properties.Aggregate(
                    Expression.Property(
                        ItemModelBase<TItem>._modelParameter,
                        ItemModelBase<TItem>._itemProperty
                    ),
                    Expression.Property
                );
                Expression<Func<ItemModelBase<TItem>, TValue, bool>> lambdaExpression = Expression.Lambda<Func<ItemModelBase<TItem>, TValue, bool>>(
                    Expression.Condition(
                        Expression.Call(
                            ValueSetter<TValue>._comparerExpression,
                            ValueSetter<TValue>._equalsMethod,
                            ValueSetter<TValue>._valueParameter,
                            propertyExpression
                        ),
                        Expression.Constant(BooleanBoxes.False),
                        Expression.Block(
                            typeof(bool),
                            Expression.Assign(
                                propertyExpression,
                                ValueSetter<TValue>._valueParameter
                            ),
                            Expression.Call(
                                ItemModelBase<TItem>._modelParameter,
                                NotifierBase.OnPropertyChangedMethod,
                                Expression.Constant(properties.Last())
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
