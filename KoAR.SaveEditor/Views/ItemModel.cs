#nullable enable

using System.ComponentModel;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    /// <summary>
    /// A wrapper class for <see cref="ItemMemoryInfo"/> that implements <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public sealed class ItemModel : NotifierBase
    {
        private readonly ItemMemoryInfo _item;

        public ItemModel(ItemMemoryInfo item) => this._item = item;

        public float CurrentDurability => this._item.CurrentDurability;

        public int EffectCount => this._item.EffectCount;

        public bool IsUnsellable
        {
            get => this._item.IsUnsellable;
            set
            {
                if (value == this._item.IsUnsellable)
                {
                    return;
                }
                this._item.IsUnsellable = value;
                this.OnPropertyChanged();
            }
        }

        public int ItemIndex => this._item.ItemIndex;

        public string ItemName
        {
            get => this._item.ItemName;
            set
            {
                if (value == this._item.ItemName)
                {
                    return;
                }
                this._item.ItemName = value;
                this.OnPropertyChanged();
            }
        }

        public float MaxDurability => this._item.MaxDurability;

        public static implicit operator ItemModel(ItemMemoryInfo info) => new ItemModel(info);

        public ItemMemoryInfo GetItem() => this._item;
    }
}
