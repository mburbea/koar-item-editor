namespace KoAR.Core
{
    partial class Item
    {
        private readonly struct Offset
        {
            public const int DataLength = 13;
            public const int Owner = 17;
            public const int BuffCount = 21;
            public const int FirstBuff = BuffCount + 4;

            private readonly int _count;

            public Offset(int count) => _count = count;

            public int PostBuffs => FirstBuff + _count * 8; // Pocket, but seems to always be 0.
            public int CurrentDurability => PostBuffs + 4;
            public int MaxDurability => CurrentDurability + 4;
            public int InventoryState => MaxDurability + 8;
            public int HasCustomName => InventoryState + 2;
            public int NameLength => HasCustomName + 1;
            public int Name => NameLength + 4;
        }
    }
}