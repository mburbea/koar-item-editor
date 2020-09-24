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
                int count = Count;
                var selfBuffCount = MemoryUtilities.Read<int>(Bytes, Offsets.FirstItemBuff + 4 + (count * 16));
                var selfBuffs = MemoryMarshal.Cast<byte, uint>(Bytes.AsSpan(Offsets.FirstItemBuff + 8 + (count * 16), selfBuffCount * 8));
                // since we don't support mutation we just need to read in what item buffs are applied.
                for (int i = 0; i < selfBuffs.Length; i += 2)
                {
                    List.Add(Amalur.GetBuff(selfBuffs[i]));
                }
            }

            private int Count => MemoryUtilities.Read<int>(Bytes, Offsets.ItemBuffCount);

            public IList<Buff> List { get; } = new List<Buff>();

            public Buff? Prefix => Amalur.Buffs.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, _endOfSection - 8));

            public Buff? Suffix => Amalur.Buffs.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, _endOfSection - 4));
        }
    }
}
