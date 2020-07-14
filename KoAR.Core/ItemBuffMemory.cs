using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace KoAR.Core
{
    public class ItemBuffMemory
    {
        internal static List<(uint itemId, int offset, uint instanceId, bool hasPrefix, bool hasSuffix)> SetOfInstances = new List<(uint, int, uint, bool, bool)>();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int BuffCount = DataLength + 4;
            public const int FirstBuff = BuffCount + 4;
        }

        /// <summary>
        /// Remember to change <see cref="MaxItemBuffs"/> if max supported item buffs changes.
        /// </summary>
        private static ReadOnlySpan<byte> InstanceIds => new byte[] {
            0x73, 0x8E, 0x57, 0x00,
            0xAA, 0x6E, 0x58, 0x00,
            0xF9, 0x03, 0x4B, 0x00,
            0xF4, 0x43, 0x4B, 0x00,
            0xEB, 0x87, 0x4B, 0x00,
            0x0A, 0xC2, 0x4B, 0x00,
            0x71, 0xFF, 0x4B, 0x00
        };
                
        public const int MaxItemBuffs = 7;

        internal ItemBuffMemory(GameSave gameSave, int coreOffset, int coreLength)
        {
            (GameSave, ItemIndex) = (gameSave, coreOffset);
            Bytes = gameSave.Bytes.AsSpan(coreOffset, coreLength).ToArray();
            var itemId = MemoryUtilities.Read<uint>(Bytes);
            int count = MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
            for (int i = 0; i < count; i++)
            {
                var instanceId = MemoryUtilities.Read<uint>(Bytes, Offsets.FirstBuff + (i * 16));
                var buffId = MemoryUtilities.Read<uint>(Bytes, Offsets.FirstBuff + (i * 16) + 4);
                var expectedId = i < 7 ? MemoryUtilities.Read<uint>(InstanceIds, i * 4) : 0;
                if (instanceId != expectedId)
                {
                    UnsupportedFormat = true;
                    SetOfInstances.Add((itemId, i, instanceId, Prefix != null, Suffix != null));
                }
                List.Add(Amalur.GetBuff(buffId));
            }
            var displayCount = MemoryUtilities.Read<int>(Bytes, Offsets.FirstBuff + 4 + (count * 16));
            if (displayCount != count)
            {
                UnsupportedFormat = true;
            }
        }

        public GameSave GameSave { get; }

        internal byte[] Bytes { get; private set; }
        public int ItemIndex { get; internal set; }
        public int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength) + 17;
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value - 17);
        }

        public int Count => List.Count;

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

        public bool UnsupportedFormat { get; }
        public List<Buff> List { get; } = new List<Buff>();

        internal byte[] Serialize(bool forced = false)
        {
            int currentCount = MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
            if (!forced && currentCount == Count)
            {
                return Bytes;
            }
            var currentLength = Bytes.Length - 8 - Offsets.FirstBuff;
            var newCount = List.Count;
            var prefixes = MemoryMarshal.Cast<byte, uint>(InstanceIds);
            Span<ulong> buffData = stackalloc ulong[newCount * 3 + 1];
            for (int i = 0; i < newCount; i++)
            {
                ulong buffId = List[i].Id;
                buffData[i * 2] = prefixes[i] | buffId << 32;
                buffData[i * 2 + 1] = ulong.MaxValue;
                buffData[newCount * 2 + 1 + i] = buffId | ((ulong)uint.MaxValue) << 32;
            }
            buffData[newCount * 2] = ((ulong)newCount) << 32;
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstBuff, currentLength, MemoryMarshal.AsBytes(buffData));
            MemoryUtilities.Write(Bytes, Offsets.BuffCount, newCount);
            DataLength = Bytes.Length;
            return Bytes;
        }
    }
}