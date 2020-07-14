namespace KoAR.Core
{
    partial class Item
    {
        private readonly struct Offset
        {
            public const int DataLength = 13;
            public const int BuffCount = 21;
            public const int FirstBuff = BuffCount + 4;

            private readonly int _count;

            public Offset(int count) => _count = count;

            public int PostBuffs => FirstBuff + _count * 8;
            public int CurrentDurability => PostBuffs + 4;
            public int MaxDurability => CurrentDurability + 4;
            public int SellableFlag => MaxDurability + 8;
            public int HasCustomName => SellableFlag + 2;
            public int CustomNameLength => HasCustomName + 1;
            public int CustomNameText => CustomNameLength + 4;
        }
    }
}