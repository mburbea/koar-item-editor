using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KoAR.Core;

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

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Useful for debugging")]
        private int Count => BitConverter.ToInt32(Bytes, Offsets.ItemBuffCount);

        public IList<Buff> List { get; } = new List<Buff>();

        public Buff? Prefix => Amalur.Buffs.GetValueOrDefault(BitConverter.ToUInt32(Bytes, _endOfSection - 8));

        public Buff? Suffix => Amalur.Buffs.GetValueOrDefault(BitConverter.ToUInt32(Bytes, _endOfSection - 4));
    }
}
