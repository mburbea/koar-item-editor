using System.Collections.Generic;
using System.Collections.Specialized;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemModel : ItemModelBase<Item>
    {
        private readonly NotifyingCollection<Buff> _itemBuffs;
        private readonly NotifyingCollection<Buff> _playerBuffs;

        public ItemModel(Item item)
            : base(item)
        {
            this._itemBuffs = new NotifyingCollection<Buff>(item.ItemBuffs.List);
            this._itemBuffs.CollectionChanged += this.Buffs_CollectionChanged;
            this._playerBuffs = new NotifyingCollection<Buff>(item.PlayerBuffs);
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

        public uint ItemId => this.Item.ItemId;

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
                if (this.SetItemValue(value, $"{nameof(IItem.ItemBuffs)}.{nameof(this.Prefix)}"))
                {
                    this.OnPropertyChanged(nameof(this.AffixCount));
                }
            }
        }

        public override Buff? Suffix
        {
            get => base.Suffix;
            set
            {
                if (this.SetItemValue(value, $"{nameof(IItem.ItemBuffs)}.{nameof(this.Suffix)}"))
                {
                    this.OnPropertyChanged(nameof(this.AffixCount));
                }
            }
        }

        public override ItemDefinition Definition
        {
            get => base.Definition;
            set
            {
                this.Item.Definition = value;
                this.OnPropertyChanged(string.Empty);
                this._playerBuffs.OnReset();
                this._itemBuffs.OnReset();
            }
        }

        public override bool UnsupportedFormat => this.Item.ItemBuffs.UnsupportedFormat;

        public void AddItemBuff(Buff buff) => this._itemBuffs.Add(buff);

        public void AddPlayerBuff(Buff buff) => this._playerBuffs.Add(buff);

        public void RemoveItemBuff(Buff buff) => this._itemBuffs.Remove(buff);

        public void RemovePlayerBuff(Buff buff) => this._playerBuffs.Remove(buff);

        protected override void Dispose(bool disposing)
        {
            this._playerBuffs.CollectionChanged -= this.Buffs_CollectionChanged;
            this._itemBuffs.CollectionChanged -= this.Buffs_CollectionChanged;
            base.Dispose(disposing);
        }

        private void Buffs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => this.OnPropertyChanged(nameof(this.Rarity));
    }
}
