using System.Diagnostics.CodeAnalysis;

namespace KoAR.Core
{
    public sealed partial class Item
    {
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Offsets class.")]
        private readonly struct Offset
        {
            private readonly Item _item;
            public Offset(Item item) => _item = item;

            public readonly int DataLength => 13;
            public readonly int Owner => 17;
            public readonly int BuffCount => 21;
            public readonly int FirstBuff => BuffCount + 4;
            public readonly int PostBuffs => FirstBuff + _item.BuffCount * 8; // Pocket, but seems to always be 0.
            public readonly int CurrentDurability => PostBuffs + 4;
            public readonly int MaxDurability => CurrentDurability + 4;
            public readonly int InventoryFlags => MaxDurability + 8;
            public readonly int ExtendedInventoryFlags => InventoryFlags + 1;
            public readonly int HasCustomName => ExtendedInventoryFlags + 1;
            public readonly int NameLength => HasCustomName + 1;
            public readonly int Name => NameLength + 4;
        }
    }
}