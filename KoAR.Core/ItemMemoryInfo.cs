using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KoAR.Core
{
    /// <summary>
    /// Equipment Memory Information
    /// </summary>
    public class ItemMemoryInfo
    {
        public static bool IsValidDurability(float durability) => durability > 0f && durability < 100f;

        public static ItemMemoryInfo Create(int itemIndex, ReadOnlySpan<byte> span)
        {
            if(span.Length < 44)
            {
                return null;
            }
            var offsets = new Offsets(MemoryUtilities.Read<int>(span,Offsets.EffectCount));
            int itemLength;
            if (span[offsets.HasCustomName] != 1)
            {
                itemLength = offsets.CustomNameLength;
            }
            else
            {
                var nameLength = MemoryUtilities.Read<int>(span, offsets.CustomNameLength);
                itemLength = offsets.CustomNameText + nameLength;
            }

            if(!IsValidDurability(MemoryUtilities.Read<float>(span, offsets.CurrentDurability))
                && !IsValidDurability(MemoryUtilities.Read<float>(span, offsets.MaxDurability)))
            {
                return null;
            }

            return new ItemMemoryInfo
            {
                ItemIndex = itemIndex,
                ItemLength = itemLength,
                ItemBytes = span.Slice(0, itemLength).ToArray()
            };
        }

        public int ItemIndex { get; set; }
        public int ItemLength { get; set; }
        public byte[] ItemBytes { get; set; }

        private Offsets Offsets => new Offsets(EffectCount);

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
            }
        }

        public bool HasCustomName => ItemBytes[Offsets.HasCustomName] == 1;

        /// <summary>
        /// Number of Effects
        /// </summary>
        public int EffectCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.EffectCount);
            set => MemoryUtilities.Write(ItemBytes, Offsets.EffectCount, value);
        }

        /// <summary>
        /// Current Durability
        /// </summary>
        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.CurrentDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.CurrentDurability, value);
        }

        /// <summary>
        /// Maximum durability
        /// </summary>
        public float MaxDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.MaxDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.MaxDurability, value);
        }

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


        public List<EffectInfo> ReadEffects()
        {
            List<EffectInfo> effects = new List<EffectInfo>();

            for (int i = 0; i < EffectCount; i++)
            {
                effects.Add(new EffectInfo
                {
                    Code = MemoryUtilities.Read<uint>(ItemBytes, Offsets.FirstEffect + i*8).ToString("X6")
                });
            }
            return effects;
        }

        public void WriteEffects(List<EffectInfo> newEffects)
        {
            var currentCount = EffectCount;
            EffectCount = newEffects.Count;
            Span<ulong> effectData = stackalloc ulong[newEffects.Count];

            for(int i = 0; i < effectData.Length; i++)
            {
                effectData[i] = uint.Parse(newEffects[i].Code, NumberStyles.HexNumber) | (ulong)uint.MaxValue << 32;
            }

            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offsets.FirstEffect, 8 * currentCount, MemoryMarshal.AsBytes(effectData));
        }
 
    }
}
