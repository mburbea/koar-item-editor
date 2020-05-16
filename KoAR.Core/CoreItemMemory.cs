using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KoAR.Core
{
    class CoreItemMemory
    {
        private static ReadOnlySpan<uint> EffectPrefixes => new[] { 0x57_8E_73u, 0x58_6E_AAu, 0x4B_03_f9u, 0x4b_43_f4u };

        private readonly struct Offset
        {
            public const int MysteryInteger = 13;
            public const int EffectCount = MysteryInteger + 4;
            public const int FirstEffect = EffectCount + 17;

            private readonly int _count;
            public Offset(int count) => _count = count;


            public int PostEffect => FirstEffect + _count * 16;
            public int DisplayEffectCount => PostEffect + 4;
            public int FirstDisplayEffect => DisplayEffectCount + 4;
        }

        public int ItemIndex { get; }
        public int DataLength { get; }
        public byte[] ItemBytes { get; set; }

        public int ItemId => MemoryUtilities.Read<int>(ItemBytes, 0);

        private Offset Offsets => new Offset(EffectCount);

        public int EffectCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offset.EffectCount);
            set => MemoryUtilities.Write(ItemBytes, Offset.EffectCount, value);
        }

        public int MysteryInteger
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offset.MysteryInteger);
            set => MemoryUtilities.Write(ItemBytes, Offset.MysteryInteger, value);
        }

        public int DisplayEffectCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.DisplayEffectCount);
            set => MemoryUtilities.Write(ItemBytes, Offsets.DisplayEffectCount, value);
        }

        public List<EffectInfo> ReadEffects()
        {
            var effects = new List<EffectInfo>();
            // Currently assuming these have to be the same.
            for (int i = 0; i < EffectCount; i++)
            {
                effects.Add(new EffectInfo
                {
                    Code = MemoryUtilities.Read<uint>(ItemBytes, Offsets.FirstDisplayEffect + i * 8).ToString("X6")
                });
            }

            return effects;
        }

        public void WriteEffects(List<EffectInfo> effects)
        {
            var currentLength = EffectCount * 24 + 8;
            Span<ulong> effectData = stackalloc ulong[effects.Count * 3 + 1];
            for(int i = 0; i < effects.Count; i++)
            {
                ulong effect = uint.Parse(effects[i].Code, NumberStyles.HexNumber);
                effectData[i * 2] = EffectPrefixes[i] | effect << 32;
                effectData[(i * 2) + 1] = ulong.MaxValue;
                effectData[(effects.Count * 2) + 1 + i] = effect | (ulong)uint.MaxValue << 32;
            }
            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offset.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
            EffectCount = effects.Count;
            DisplayEffectCount = effects.Count;
        }
    }
}
