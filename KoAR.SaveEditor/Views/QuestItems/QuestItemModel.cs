using System;
using KoAR.Core;

namespace KoAR.SaveEditor.Views.QuestItems;

public sealed class QuestItemModel(QuestItem item)
{
    private readonly QuestItem _item = item;

    public event EventHandler? IsSellableChanged;

    public bool IsSellable
    {
        get => !this._item.IsUnsellable;
        set
        {
            if (value != this._item.IsUnsellable)
            {
                return;
            }
            this._item.IsUnsellable = !value;
            this.IsSellableChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string Name => this._item.Name;
}
