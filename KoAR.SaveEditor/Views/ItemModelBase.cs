using System;
using System.Collections.Generic;
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

        public virtual bool IsStolen
        {
            get => this.Item.IsStolen;
            set => throw new NotSupportedException();
        }

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

        protected bool SetItemValue<T>(T value, T currentValue, Action<T> setValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(value, currentValue))
            {
                return false;
            }
            setValue(value);
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
