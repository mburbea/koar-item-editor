using System.Collections.Generic;

namespace KoAR.Core;

public interface IItem
{
    float CurrentDurability { get; }
    bool HasCustomName { get; }
    bool IsStolen { get; }
    bool IsEquipped { get; }
    IItemBuffMemory ItemBuffs { get; }
    string ItemName { get; }
    byte Level { get; }
    float MaxDurability { get; }
    List<Buff> PlayerBuffs { get; }
    Rarity Rarity { get; }
    ItemDefinition Definition { get; }
    IEnumerable<Socket> GetSockets();
}