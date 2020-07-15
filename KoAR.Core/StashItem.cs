using System;
using System.Collections.Generic;
using System.Text;

namespace KoAR.Core
{
    public partial class StashItem : IItem
    {
        private readonly struct Offset
        {
            private readonly StashItem _item;
            public Offset(StashItem item) => _item = item;
            public const int TypeId = 0;
            public const int Durability = 10;
            public const int Quantity = 14;
            public const int BuffCount = 18;
            public const int FirstBuff = BuffCount + 4;
            public readonly int PostBuffs => FirstBuff + _item.BuffCount * 8;
            public readonly int IsStolen => PostBuffs;
            public readonly int HasCustomName => IsStolen + 1;
            public readonly int NameLength => HasCustomName + 1;
            public readonly int Name => NameLength + 4;
            public readonly int HasItemBuffs => _item.HasCustomName ? Name + _item.NameLength : HasCustomName + 1;
            public readonly int ItemBuffCount => HasItemBuffs + 3;
            public readonly int FirstItemBuff => ItemBuffCount + 4;
        }

        private byte[] Bytes { get; }
        public List<Buff> PlayerBuffs { get; } = new List<Buff>();

        public StashItem(GameSave gameSave, int offset, int datalength)
        {
            Bytes = gameSave.Bytes.AsSpan(offset, datalength).ToArray();
            PlayerBuffs.Capacity = BuffCount;
            var firstBuff = Offset.FirstBuff;
            for (int i = 0; i < PlayerBuffs.Capacity; i++)
            {
                PlayerBuffs.Add(Amalur.GetBuff(MemoryUtilities.Read<uint>(Bytes, firstBuff + (i * 8))));
            }
            if (HasCustomName)
            {
                ItemName = Encoding.Default.GetString(Bytes, Offsets.Name, NameLength);
            }
            ItemBuffs = Bytes[Offsets.HasItemBuffs] == 0x14 ? new ItemBuffMemory(this) : MissingItemBuffMemory.Instance;
        }

        public int ItemOffset { get; }

        private Offset Offsets => new Offset(this);

        public TypeDefinition TypeDefinition => Amalur.TypeDefinitions[MemoryUtilities.Read<uint>(Bytes)];

        public float CurrentDurability => MemoryUtilities.Read<float>(Bytes, Offset.Durability);

        private int BuffCount => MemoryUtilities.Read<int>(Bytes, Offset.BuffCount);

        public bool IsStolen => Bytes[Offsets.IsStolen] == 1;

        public bool HasCustomName => Bytes[Offsets.HasCustomName] == 1;

        private int NameLength => MemoryUtilities.Read<int>(Bytes, Offsets.NameLength);

        public string ItemName { get; } = string.Empty;

        public IItemBuffMemory ItemBuffs { get; }

        public byte Level => throw new NotImplementedException();

        public float MaxDurability => throw new NotImplementedException();

        public Rarity Rarity => throw new NotImplementedException();
    }
}
