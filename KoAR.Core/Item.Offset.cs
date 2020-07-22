namespace KoAR.Core
{
    partial class Item
    {
        private readonly struct Offset
        {
            public readonly int DataLength => 13;
            public readonly int Owner => 17;
            public readonly int BuffCount => 21;
            public readonly int FirstBuff => BuffCount + 4;

            private readonly Item _item;

            public Offset(Item item) => _item = item;

            public readonly int PostBuffs => FirstBuff + _item.BuffCount * 8; // Pocket, but seems to always be 0.
            public readonly int CurrentDurability => PostBuffs + 4;
            public readonly int MaxDurability => CurrentDurability + 4;
            public readonly int InventoryState => MaxDurability + 8;
            public readonly int HasCustomName => InventoryState + 2;
            public readonly int NameLength => HasCustomName + 1;
            public readonly int Name => NameLength + 4;
        }
    }
}