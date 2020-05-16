using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace KoAR.Core
{
    /// <summary>
    /// Equipment Memory Information
    /// </summary>
    public class ItemMemoryInfo
    {
        public readonly struct Offset
        {
            public const int EffectCount = 21;
            public const int FirstEffect = EffectCount + 4;

            private readonly int _count;
            public Offset(int count) => _count = count;
            public int PostEffect => FirstEffect + _count * 8;
            public int CurrentDurability => PostEffect + 4;
            public int MaxDurability => CurrentDurability + 4;

            public int SellableFlag => MaxDurability + 8;
            public int HasCustomName => SellableFlag + 2;
            public int CustomNameLength => HasCustomName + 1;
            public int CustomNameText => CustomNameLength + 4;
        }

        public const float DurabilityLowerBound = 0f;
        public const float DurabilityUpperBound = 100f;
        public const int MinEquipmentLength = 44;

        private ItemMemoryInfo(int itemIndex, int dataLength, ReadOnlySpan<byte> span)
        {
            (ItemIndex, DataLength, ItemBytes) = (itemIndex, dataLength, span.Slice(0, dataLength).ToArray());
        }

        public CoreItemMemory CoreItemMemory { get; set; }
        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.CurrentDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.CurrentDurability, value);
        }

        public int DataLength { get; }

        public int EffectCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offset.EffectCount);
            set => MemoryUtilities.Write(ItemBytes, Offset.EffectCount, value);
        }

        public bool HasCustomName => ItemBytes[Offsets.HasCustomName] == 1;

        public bool IsUnsellable
        {
            get => (ItemBytes[Offsets.SellableFlag] & 0x80) == 0x80;
            set
            {
                if (value)
                {
                    ItemBytes[Offsets.SellableFlag] |= 0x80;
                }
                else
                {
                    ItemBytes[Offsets.SellableFlag] &= 0x7F;
                }
            }
        }

        public byte[] ItemBytes { get; set; }

        public int ItemId => MemoryUtilities.Read<int>(ItemBytes);

        public int ItemIndex { get; }

        public string ItemName
        {
            get
            {
                if (!HasCustomName)
                {
                    return "Unknown";
                }
                else
                {
                    int nameLength = MemoryUtilities.Read<int>(ItemBytes, Offsets.CustomNameLength);
                    return Encoding.Default.GetString(ItemBytes, Offsets.CustomNameText, nameLength);
                }
            }
            set
            {
                if (!HasCustomName)
                {
                    throw new Exception("Item's name is unmodifiable");
                }
                var currentLength = MemoryUtilities.Read<int>(ItemBytes, Offsets.CustomNameLength);
                var newBytes = Encoding.Default.GetBytes(value);
                ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offsets.CustomNameText, currentLength, newBytes);
                MemoryUtilities.Write(ItemBytes, Offsets.CustomNameLength, newBytes.Length);
            }
        }

        public float MaxDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.MaxDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.MaxDurability, value);
        }

        private Offset Offsets => new Offset(EffectCount);

        public static ItemMemoryInfo Create(int itemIndex, ReadOnlySpan<byte> span)
        {
            var offsets = new Offset(MemoryUtilities.Read<int>(span, Offset.EffectCount));

            if (span.Length < MinEquipmentLength || !IsValidDurability(MemoryUtilities.Read<float>(span, offsets.CurrentDurability))
                || !IsValidDurability(MemoryUtilities.Read<float>(span, offsets.MaxDurability)))
            {
                return null;
            }

            int dataLength = span[offsets.HasCustomName] != 1
                ? offsets.CustomNameLength
                : offsets.CustomNameText + MemoryUtilities.Read<int>(span, offsets.CustomNameLength);

            return new ItemMemoryInfo(itemIndex, dataLength, span);
        }

        public static bool IsValidDurability(float durability) => durability > DurabilityLowerBound && durability < DurabilityUpperBound;

        public List<EffectInfo> ReadEffects()
        {
            List<EffectInfo> effects = new List<EffectInfo>();

            for (int i = 0; i < EffectCount; i++)
            {
                effects.Add(new EffectInfo
                {
                    Code = MemoryUtilities.Read<uint>(ItemBytes, Offset.FirstEffect + i * 8).ToString("X6")
                });
            }

            return effects;
        }

        public void WriteEffects(List<EffectInfo> newEffects)
        {
            var currentLength = Offsets.PostEffect - Offset.FirstEffect;
            EffectCount = newEffects.Count;
            Span<ulong> effectData = stackalloc ulong[newEffects.Count];

            for (int i = 0; i < effectData.Length; i++)
            {
                effectData[i] = uint.Parse(newEffects[i].Code, NumberStyles.HexNumber) | (ulong)uint.MaxValue << 32;
            }

            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offset.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
        }
    }
}