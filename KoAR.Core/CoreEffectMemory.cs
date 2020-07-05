using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace KoAR.Core
{
    public class CoreEffectMemory
    {
        internal static List<(uint itemId, int offset, uint prefix)> SetOfInstances = new List<(uint, int, uint)>();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int EffectCount = DataLength + 4;
            public const int FirstEffect = EffectCount + 4;
        }

        /// <summary>
        /// Remember to change <see cref="MaxCoreEffects"/> if max supported core effecs changes.
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
                
        public const int MaxCoreEffects = 7;

        internal CoreEffectMemory(int coreOffset, int coreLength)
        {
            ItemIndex = coreOffset;
            Bytes = Amalur.Bytes.AsSpan(coreOffset, coreLength).ToArray();
            var itemId = MemoryUtilities.Read<uint>(Bytes);
            int count = MemoryUtilities.Read<int>(Bytes, Offsets.EffectCount);
            for (int i = 0; i < count; i++)
            {
                var instanceId = MemoryUtilities.Read<uint>(Bytes, Offsets.FirstEffect + (i * 16));
                var effect = MemoryUtilities.Read<uint>(Bytes, Offsets.FirstEffect + (i * 16) + 4);
                var expectedId = i < 7 ? MemoryUtilities.Read<uint>(InstanceIds, i * 4) : 0;
                if (instanceId != expectedId)
                {
                    UnsupportedFormat = true;
                    SetOfInstances.Add((itemId, i, instanceId));
                }
                List.Add(effect);
            }
            var displayCount = MemoryUtilities.Read<int>(Bytes, Offsets.FirstEffect + 4 + (count * 16));
            if (displayCount != count)
            {
                UnsupportedFormat = true;
            }
        }

        internal byte[] Bytes { get; private set; }
        public int ItemIndex { get; internal set; }
        public int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength) + 17;
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value - 17);
        }

        public int Count => List.Count;

        public uint Prefix
        {
            get => MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 8);
            set => MemoryUtilities.Write(Bytes, Bytes.Length - 8, value);
        }

        public uint Suffix
        {
            get => MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 4);
            set => MemoryUtilities.Write(Bytes, Bytes.Length - 4, value);
        }

        public bool UnsupportedFormat { get; }
        public List<uint> List { get; } = new List<uint>();

        internal byte[] Serialize(bool forced = false)
        {
            int currentCount = MemoryUtilities.Read<int>(Bytes, Offsets.EffectCount);
            if (!forced && currentCount == Count)
            {
                return Bytes;
            }
            var currentLength = currentCount * 24 + 8;
            var newCount = List.Count;
            var prefixes = MemoryMarshal.Cast<byte, uint>(InstanceIds);
            Span<ulong> effectData = stackalloc ulong[newCount * 3 + 1];
            for (int i = 0; i < newCount; i++)
            {
                ulong effect = List[i];
                effectData[i * 2] = prefixes[i] | effect << 32;
                effectData[i * 2 + 1] = ulong.MaxValue;
                effectData[newCount * 2 + 1 + i] = effect | ((ulong)uint.MaxValue) << 32;
            }
            effectData[newCount * 2] = ((ulong)newCount) << 32;
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
            MemoryUtilities.Write(Bytes, Offsets.EffectCount, newCount);
            DataLength = Bytes.Length;
            return Bytes;
        }
    }
}