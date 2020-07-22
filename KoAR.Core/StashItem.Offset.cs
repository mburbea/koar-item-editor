namespace KoAR.Core
{
    public partial class StashItem
    {
        private readonly struct Offset
        {
            private readonly StashItem _item;
            public Offset(StashItem item) => _item = item;
            public readonly int TypeId => 0;
            public readonly int Durability => 10;
            public readonly int Quantity => 14;
            public readonly int BuffCount => 18;
            public readonly int FirstBuff => BuffCount + 4;
            public readonly int PostBuffs => FirstBuff + _item.BuffCount * 8;
            public readonly int IsStolen => PostBuffs;
            public readonly int HasCustomName => IsStolen + 1;
            public readonly int NameLength => HasCustomName + 1;
            public readonly int Name => NameLength + 4;
            public readonly int HasItemBuffs => _item.HasCustomName ? Name + _item.NameLength : HasCustomName + 1;
            public readonly int ItemBuffCount => HasItemBuffs + 3;
            public readonly int FirstItemBuff => ItemBuffCount + 4;
        }

    }
}
