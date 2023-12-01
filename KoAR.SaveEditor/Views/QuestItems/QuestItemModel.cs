using System;
using KoAR.Core;

namespace KoAR.SaveEditor.Views.QuestItems;

public sealed class QuestItemModel(QuestItem item)
{
    public event EventHandler? IsSellableChanged;

    public bool IsSellable
    {
        get => !item.IsUnsellable;
        set
        {
            if (value != item.IsUnsellable)
            {
                return;
            }
            item.IsUnsellable = !value;
            this.IsSellableChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string Name => item.Name;
}
