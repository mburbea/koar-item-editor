using System.Collections.Generic;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class StashItemModel : ItemModelBase
    {
        public StashItemModel(StashItem item)
            : base(item)
        {
        }

        public new StashItem Item => (StashItem)base.Item;
        public override IReadOnlyList<Buff> ItemBuffs => this.Item.ItemBuffs.List;
        public override IReadOnlyList<Buff> PlayerBuffs => this.Item.PlayerBuffs;
        public override bool UnsupportedFormat => false;
    }
}
