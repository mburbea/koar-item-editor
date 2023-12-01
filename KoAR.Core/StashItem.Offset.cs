using System.Diagnostics.CodeAnalysis;

namespace KoAR.Core;

public partial class StashItem
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Offsets class.")]
    protected readonly struct Offset(StashItem item)
    {
        public readonly int TypeId => 0;
        public readonly int Pocket => 6;
        public readonly int Durability => 10;
        public readonly int Quantity => 14;
        public readonly int BuffCount => 18;
        public readonly int FirstBuff => BuffCount + 4;
        public readonly int PostBuffs => FirstBuff + item.BuffCount * 8;
        public readonly int InventoryFlags => PostBuffs;
        public readonly int ExtendedInventoryFlags => InventoryFlags + 1;
        public readonly int NameLength => ExtendedInventoryFlags + 1;
        public readonly int Name => NameLength + 4;
        public readonly int HasItemBuffs => item.HasCustomName ? Name + item.NameLength : ExtendedInventoryFlags + 1;
        public readonly int ItemBuffCount => HasItemBuffs + 3;
        public readonly int FirstItemBuff => ItemBuffCount + 4;
    }
}
