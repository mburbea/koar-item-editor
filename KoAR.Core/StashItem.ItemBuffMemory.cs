using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public partial class StashItem
    {
        private sealed class ItemBuffMemory : IItemBuffMemory
        {
            private readonly StashItem _stashItem;
            private readonly int _endOfSection;

            private byte[] Bytes => _stashItem.Bytes;
            private Offset Offsets => _stashItem.Offsets;

            public ItemBuffMemory(StashItem stashItem, int endOfSection)
            {
                (_stashItem, _endOfSection) = (stashItem, endOfSection);
                var data = Bytes.AsSpan(Offsets.ItemBuffCount);
                BuffInstance.ReadList(ref data); // activeBuffs
                BuffInstance.ReadList(ref data); // inactiveBuffs
                var selfBuffs = BuffDuration.ReadList(ref data);
                foreach (var (buffId, _) in selfBuffs)
                {
                    List.Add(Amalur.GetBuff(buffId));
                }
            }

            private int Count => MemoryUtilities.Read<int>(Bytes, Offsets.ItemBuffCount);

            public IList<Buff> List { get; } = new List<Buff>();

            public Buff? Prefix => Amalur.Buffs.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, _endOfSection - 8));

            public Buff? Suffix => Amalur.Buffs.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, _endOfSection - 4));
        }
    }
}
