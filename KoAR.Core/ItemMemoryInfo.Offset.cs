namespace KoAR.Core
{
    public partial class ItemMemoryInfo
    {
        public readonly struct Offset
        {
            public const int EffectCount = 21;
            public const int FirstEffect = EffectCount + 4;

            private readonly int _count;
            public Offset(int count) => _count = count;
            public int PostEffect => FirstEffect + _count * 8;
            public int CurrentDurability => PostEffect + 4;
            public int MaxDurability => CurrentDurability + 4;

            public int SellableFlag => MaxDurability + 8;
            public int HasCustomName => SellableFlag + 2;
            public int CustomNameLength => HasCustomName + 1;
            public int CustomNameText => CustomNameLength + 4;
        }
    }
}