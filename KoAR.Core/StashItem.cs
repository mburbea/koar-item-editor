using System;
using System.Collections.Generic;
using System.Text;

namespace KoAR.Core
{
    public class StashItem
    {
        public class StashItemBuffMemory
        {
            private int Offset { get; }
            public List<Buff> List { get; } = new List<Buff>();
            
            public Buff? Prefix
            {
                get => null;
                set { }
            }

            public Buff? Suffix
            {
                get => null;
                set { }
            }

        }

        public readonly struct Offsets
        {
            readonly int _effectCount;
            public Offsets(int effectCount) => _effectCount = effectCount;
            public const int TypeId = 0;
            public const int Durability = 10;
            public const int Quantity = 14;
            public const int BuffCount = 18;
        }

        public byte[] Bytes { get; } = Array.Empty<byte>();
        public List<Buff> PlayerBuffs { get; } = new List<Buff>();

        public StashItem(int offset, int datalength)
        {
            Bytes = Amalur.Bytes.AsSpan(offset, datalength).ToArray();
            PlayerBuffs.Capacity = BuffCount;
            for(int i = 0; i < PlayerBuffs.Capacity; i++)
            {
                PlayerBuffs.Add(Amalur.GetBuff(MemoryUtilities.Read<uint>(Bytes, Offsets.BuffCount + 4 + i * 8)));
            }
            if(HasCustomNameFlag == 1)
            {
                ItemName = Encoding.Default.GetString(Bytes, FirstChar, NameLength);
            }
        }

        public TypeDefinition TypeDefinition
        {
            get => Amalur.TypeDefinitions[MemoryUtilities.Read<uint>(Bytes)];
        }

        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(Bytes, Offsets.Durability);
        }

        private int BuffCount
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
        }

        public byte HasCustomNameFlag
        {
            get => Bytes[Offsets.BuffCount + BuffCount * 8 + 2];
        }

        private int NameLength
        {
            get => HasCustomNameFlag != 1 ? -1 : MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount + 4 + BuffCount * 8 + 3);
        }

        private int FirstChar
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount + 4 + BuffCount * 8 + 7);
        }

        private string Name { get; } = "";

        public bool HasCustomName => ItemName != string.Empty;

        public string ItemName { get; } = string.Empty;

        public StashItemBuffMemory? ItemBuffs { get; }

    }
}
