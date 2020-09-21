using System;

namespace KoAR.Core
{
    [Flags]
    internal enum InventoryFlags : byte
    {
        CanBeConvertedToGold = 1 << 0,
        IsLootable = 1 << 1,
        IsEquipment = 1 << 2,
        IsFromStolenSource = 1 << 3,
        Stolen = 1 << 4,
        Salvageable = 1 << 5,
        Unstashable = 1 << 6,
        Unsellable = 1 << 7
    }

    [Flags]
    internal enum ExtendedInventoryFlags : byte
    {
        IsWeapon = 1 << 0,
        IsMagic = 1 << 1,
        IsShield = 1 << 2,
        StashableQuestItem = 1 << 3,
        HasCustomName = 1 << 4
    }
}
