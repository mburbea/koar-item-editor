using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace KoAR.Core
{
    public class ItemBuffMemory : IItemBuffMemory
    {
        internal static List<(uint itemId, int offset, uint instanceId, bool hasPrefix, bool hasSuffix)> SetOfInstances = new List<(uint, int, uint, bool, bool)>();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int BuffCount = DataLength + 4;
            public const int FirstBuff = BuffCount + 4;
        }

        static uint GetDefaultInstanceId(int index) => (uint)Hasher.GetHash($"selfbuff_{index}");

        internal ItemBuffMemory(byte[] gameBytes, int coreOffset, int coreLength)
        {
            ItemOffset = coreOffset;
            Bytes = gameBytes.AsSpan(coreOffset, coreLength).ToArray();
            var itemId = MemoryUtilities.Read<uint>(Bytes);
            List.Capacity = MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
            var firstBuff = Offsets.FirstBuff;
            for (int i = 0; i < List.Capacity; i++)
            {
                var instanceId = MemoryUtilities.Read<uint>(Bytes, firstBuff + (i * 16));
                var buffId = MemoryUtilities.Read<uint>(Bytes, firstBuff + (i * 16) + 4);
                if (instanceId != GetDefaultInstanceId(i))
                {
                    UnsupportedFormat = true;
                    SetOfInstances.Add((itemId, i, instanceId, Prefix != null, Suffix != null));
                }
                List.Add(Amalur.GetBuff(buffId));
            }
            var displayCount = MemoryUtilities.Read<int>(Bytes, Offsets.FirstBuff + 4 + (List.Count * 16));
            if (displayCount != List.Count)
            {
                UnsupportedFormat = true;
            }
        }

        internal byte[] Bytes { get; private set; }
        public int ItemOffset { get; internal set; }
        public bool UnsupportedFormat { get; }
        public List<Buff> List { get; } = new List<Buff>();

        internal int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength) + 17;
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value - 17);
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
            set => MemoryUtilities.Write(Bytes, Offsets.BuffCount, value);
        }

        public Buff? Prefix
        {
            get => Amalur.BuffMap.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 8));
            set => MemoryUtilities.Write(Bytes, Bytes.Length - 8, value?.Id ?? 0);
        }

        public Buff? Suffix
        {
            get => Amalur.BuffMap.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 4));
            set => MemoryUtilities.Write(Bytes, Bytes.Length - 4, value?.Id ?? 0);
        }

        internal byte[] Serialize(bool forced = false)
        {
            if (!forced && List.Count == Count)
            {
                return Bytes;
            }
            var currentLength = Bytes.Length - 8 - Offsets.FirstBuff;
            Span<ulong> buffData = stackalloc ulong[List.Count * 3 + 1];
            for (int i = 0; i < List.Count; i++)
            {
                ulong buffId = List[i].Id;
                buffData[i * 2] = GetDefaultInstanceId(i) | buffId << 32;
                buffData[i * 2 + 1] = ulong.MaxValue; // duration, max duration apparently.
                buffData[List.Count * 2 + 1 + i] = buffId | ((ulong)uint.MaxValue) << 32;
            }
            buffData[List.Count * 2] = ((ulong)List.Count) << 32;
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstBuff, currentLength, MemoryMarshal.AsBytes(buffData));
            this.Count = List.Count;
            DataLength = Bytes.Length;
            return Bytes;
        }
    }
}