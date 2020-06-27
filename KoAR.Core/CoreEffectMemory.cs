using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public class CoreEffectMemory
    {
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int EffectCount = DataLength + 4;
            public const int FirstEffect = EffectCount + 4;
        }

        private static ReadOnlySpan<byte> Prefixes => new byte[] { 
            0x73, 0x8E, 0x57, 0x00,
            0xAA, 0x6E, 0x58, 0x00,
            0xF9, 0x03, 0x4B, 0x00,
            0xF4, 0x43, 0x4B, 0x00,
            0xEB, 0x87, 0x4B, 0x00,
            0x0A, 0xC2, 0x4B, 0x00,
            0x71, 0xFF, 0x4B, 0x00
        };

        internal CoreEffectMemory(Span<byte> buffer)
        {
            ReadOnlySpan<byte> bytes = Amalur.Bytes;
            ReadOnlySpan<byte> coreEffectSequence = new byte[] { 0x84, 0x60, 0x28, 0x00, 0x00 };
            coreEffectSequence.CopyTo(buffer.Slice(8));
            ItemIndex = bytes.IndexOf(buffer);
            ReadOnlySpan<byte> span = bytes.Slice(ItemIndex);
            int count = span[Offsets.EffectCount];
            Bytes = span.Slice(0, DataLength).ToArray();
            var firstDisplayEffect = Offsets.FirstEffect + (count * 16) + 8;
            for (int i = 0; i < count; i++)
            {
                List.Add(MemoryUtilities.Read<uint>(span, firstDisplayEffect + i * 8));
            }
        }

        internal byte[] Bytes { get; private set; }
        public int ItemIndex { get; internal set; }
        public int DataLength {

            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength);
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value);
        }

        public int Count
        {
            get => List.Count;
        }

        public List<uint> List { get; } = new List<uint>();

        internal byte[] Serialize(bool forced = false)
        {
            byte currentCount = Bytes[Offsets.EffectCount];
            if (!forced && currentCount == Count)
            {
                return Bytes;
            }
            var currentLength = currentCount * 24 + 8;
            var newCount = List.Count;
            var prefixes = MemoryMarshal.Cast<byte, uint>(Prefixes);
            Span<ulong> effectData = stackalloc ulong[newCount * 3 + 1];
            for (int i = 0; i < newCount; i++)
            {
                ulong effect = List[i];
                effectData[i * 2] = prefixes[i] | effect << 32;
                effectData[i * 2 + 1] = ulong.MaxValue;
                effectData[newCount * 2 + 1 + i] = effect | (ulong)uint.MaxValue << 32;
            }
            effectData[newCount * 2] = (ulong)(uint)newCount << 32;
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
           // Bytes[Offsets.DataLength] = Mystery[newCount];
            Bytes[Offsets.EffectCount] = (byte)newCount;
            return Bytes;
        }
    }
}