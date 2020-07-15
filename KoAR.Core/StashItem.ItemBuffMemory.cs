using System.Collections.Generic;

namespace KoAR.Core
{
    public partial class StashItem
    {
        private sealed class ItemBuffMemory : IItemBuffMemory
        {
            private readonly StashItem _stashItem;
            private byte[] Bytes => _stashItem.Bytes;
            private Offset Offsets => _stashItem.Offsets;

            public ItemBuffMemory(StashItem stashItem)
            {
                _stashItem = stashItem;
                List.Capacity = Count;
                var firstBuff = Offsets.FirstItemBuff;
                for(int i = 0; i < List.Capacity; i++)
                {
                    var buffId = MemoryUtilities.Read<uint>(Bytes, firstBuff + (i * 16) + 4);
                    List.Add(Amalur.GetBuff(buffId));
                }
            }

            private int Count => MemoryUtilities.Read<int>(Bytes, Offsets.ItemBuffCount);

            public List<Buff> List { get; } = new List<Buff>();

            public Buff? Prefix => Amalur.BuffMap.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 9));

            public Buff? Suffix => Amalur.BuffMap.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 5));
        }

        private sealed class MissingItemBuffMemory : IItemBuffMemory
        {
            internal static readonly IItemBuffMemory Instance = new MissingItemBuffMemory();

            public List<Buff> List { get; } = new List<Buff>();

            public Buff? Prefix => null;

            public Buff? Suffix => null;

            private MissingItemBuffMemory()
            {
            }
        }
    }
}
