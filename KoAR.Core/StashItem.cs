using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KoAR.Core
{
    public partial class StashItem : IItem
    {
        protected byte[] Bytes { get; }
        public List<Buff> PlayerBuffs { get; } = new List<Buff>();

        public Gem[] Gems { get; }

        public StashItem(GameSave gameSave, int offset, int dataLength, Gem[] gems)
        {
            ItemOffset = offset;
            Bytes = gameSave.Body.AsSpan(offset, dataLength).ToArray();
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
            Gems = gems;
            // socket section is either FF
            // or 20 02, followed by int32 count, and int32 handle per gem.
            int socketsStart = gems.Length == 0 
                ? Bytes.Length - 1 
                : gems[0].ItemOffset - offset - (4 * (1 + gems.Length)) - 2; 

            ItemBuffs = Bytes[Offsets.HasItemBuffs] == 0x14 ? new ItemBuffMemory(this, socketsStart) : Definition.ItemBuffs;
        }

        internal int DataLength => Bytes.Length;

        internal int ItemOffset { get; set; }

        protected Offset Offsets => new Offset(this);

        public ItemDefinition Definition => Amalur.ItemDefinitions[MemoryUtilities.Read<uint>(Bytes)];

        public float CurrentDurability => MemoryUtilities.Read<float>(Bytes, Offsets.Durability);

        private int BuffCount => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);

        public virtual bool IsStolen => Bytes[Offsets.InventoryFlags] == 1;

        public virtual bool HasCustomName => Bytes[Offsets.ExtendedInventoryFlags] == 1;

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

        public IEnumerable<Socket> GetSockets()
        {
            return Gems.Length switch
            {
                0 => Definition.GetSockets(),
                1 when Definition.SocketTypes.Length == 1 => new[] { new Socket(Definition.SocketTypes[0], Gems[0]) }, // trivial case.
                _ => Inner(Definition.SocketTypes, Gems.ToArray())
            };

            static IEnumerable<Socket> Inner(string sockets, Gem[] gems)
            {
                int start = 0;
                foreach (var socket in sockets)
                {
                    Gem? gem = null;
                    for (int i = start; i < gems.Length; i++)
                    {
                        if (gems[i].Definition.SocketType == socket)
                        {
                            gem = gems[i];
                            (gems[start], gems[i]) = (gems[i], gems[start]);
                            start++;
                            break;
                        }
                    }
                    yield return new Socket(socket, gem);
                }
            }
        }
    }
}
