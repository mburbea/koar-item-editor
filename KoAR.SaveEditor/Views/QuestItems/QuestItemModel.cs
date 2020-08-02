using System;
using KoAR.Core;

namespace KoAR.SaveEditor.Views.QuestItems
{
    public sealed class QuestItemModel
    {
        private readonly QuestItem _item;

        public QuestItemModel(QuestItem item) => this._item = item;

        public event EventHandler? IsUnsellableChanged;

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
                this.IsUnsellableChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Name => this._item.Name;
    }
}
