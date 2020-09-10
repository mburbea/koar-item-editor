using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KoAR.Core
{
    public partial class StashItem : IItem
    {
        private byte[] Bytes { get; }
        public List<Buff> PlayerBuffs { get; } = new List<Buff>();

        public StashItem(GameSave gameSave, int offset, int datalength)
        {
            ItemOffset = offset;
            Bytes = gameSave.Body.AsSpan(offset, datalength).ToArray();
            PlayerBuffs.Capacity = BuffCount;
            var firstBuff = Offsets.FirstBuff;
            for (int i = 0; i < PlayerBuffs.Capacity; i++)
            {
                PlayerBuffs.Add(Amalur.GetBuff(MemoryUtilities.Read<uint>(Bytes, firstBuff + (i * 8))));
            }
            if (HasCustomName)
            {
                ItemName = Encoding.Default.GetString(Bytes, Offsets.Name, NameLength);
            }
            ItemBuffs = Bytes[Offsets.HasItemBuffs] == 0x14 ? new ItemBuffMemory(this) : Definition.ItemBuffs;
        }

        internal int DataLength => Bytes.Length;

        internal int ItemOffset { get; set; }

        private Offset Offsets => new Offset(this);

        public ItemDefinition Definition => Amalur.ItemDefinitions[MemoryUtilities.Read<uint>(Bytes)];

        public float CurrentDurability => MemoryUtilities.Read<float>(Bytes, Offsets.Durability);

        private int BuffCount => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);

        public bool IsStolen => Bytes[Offsets.IsStolen] == 1;

        public bool HasCustomName => Bytes[Offsets.HasCustomName] == 1;

        private int NameLength => MemoryUtilities.Read<int>(Bytes, Offsets.NameLength);

        public string ItemName { get; } = string.Empty;

        public IItemBuffMemory ItemBuffs { get; }

        public byte Level => Definition.Level;

        public float MaxDurability => Definition.MaxDurability;

        public Rarity Rarity => Definition.Rarity == Rarity.Set
            ? Rarity.Set
            : PlayerBuffs.Select(x => x.Rarity)
                .Concat(ItemBuffs.List.Select(x => x.Rarity))
                .Concat(new[] { ItemBuffs.Prefix?.Rarity ?? default, ItemBuffs.Suffix?.Rarity ?? default, Definition.SocketTypes.Any() ? Rarity.Infrequent : Rarity.Common })
                .Max();

        public IEnumerable<Socket> GetSockets() => Definition.GetSockets();
    }
}
