using System.Collections.Generic;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class StashItemModel : ItemModelBase<StashItem>
    {
        public StashItemModel(StashItem item)
            : base(item)
        {
        }
        
        public override IReadOnlyList<Buff> ItemBuffs => (IReadOnlyList<Buff>)this.Item.ItemBuffs.List;
        public override IReadOnlyList<Buff> PlayerBuffs => this.Item.PlayerBuffs;
        public override bool UnsupportedFormat => false;
    }
}
