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
        public static bool IsValidDurability(float durability) => durability > 0f && durability < 100f;

        public static ItemMemoryInfo Create(int itemIndex, ReadOnlySpan<byte> span)
        {
            static T Read<T>(ReadOnlySpan<byte> span, int offset)
                where T: struct
            => MemoryMarshal.Read<T>(span.Slice(offset));

            if(span.Length < 44)
            {
                return null;
            }
            var offsets = new Offsets(Read<int>(span,Offsets.EffectCount));
            int itemLength;
            if (span[offsets.HasCustomName] != 1)
            {
                itemLength = offsets.CustomNameText;
            }
            else
            {
                var nameLength = Read<int>(span, offsets.CustomNameLength);
                itemLength = offsets.CustomNameText + nameLength;
            }

            if(!IsValidDurability(Read<float>(span, offsets.CurrentDurability))
                && !IsValidDurability(Read<float>(span, offsets.MaxDurability)))
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

        private Offsets? _offset;
        private Offsets Offset => _offset ??= new Offsets(EffectCount);

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
                    int nameLength = BitConverter.ToInt32(ItemBytes, Offset.CustomNameLength);
                    return Encoding.Default.GetString(ItemBytes, Offset.CustomNameText, nameLength);
                }
            }
            set
            {
                if (!HasCustomName)
                {
                    throw new Exception("Item's name is unmodifiable");
                }
                var currentLength = BitConverter.ToUInt32(ItemBytes, Offset.CustomNameLength);
                var newBytes = Encoding.Default.GetBytes(value);
                if(currentLength == value.Length)
                {
                    newBytes.CopyTo(ItemBytes, Offset.CustomNameText);
                }
                else
                {
                    var newLength = newBytes.Length;
                    var buffer = new byte[Offset.CustomNameText + value.Length];
                    ItemBytes.AsSpan(0, Offset.CustomNameLength).CopyTo(buffer);
                    MemoryMarshal.Write(buffer.AsSpan(Offset.CustomNameLength), ref newLength);
                    newBytes.CopyTo(buffer, Offset.CustomNameText);
                    ItemBytes = buffer;
                }
            }
        }

        public bool HasCustomName => ItemBytes[Offset.HasCustomName] == 1;

        /// <summary>
        /// Number of Effects
        /// </summary>
        public int EffectCount => BitConverter.ToInt32(ItemBytes, Offsets.EffectCount);

        /// <summary>
        /// Current Durability
        /// </summary>
        public float CurrentDurability
        {
            get => BitConverter.ToSingle(ItemBytes, Offset.CurrentDurability);
            set => MemoryMarshal.Write(ItemBytes.AsSpan(Offset.CurrentDurability), ref value);
        }

        /// <summary>
        /// Maximum durability
        /// </summary>
        public float MaxDurability
        {
            get => BitConverter.ToSingle(ItemBytes, Offset.MaxDurability);
            set => MemoryMarshal.Write(ItemBytes.AsSpan(Offset.MaxDurability), ref value);
        }

        public bool IsUnsellable
        {
            get => (ItemBytes[Offset.SellableFlag] & 0x80) == 0x80;
            set
            {
                if (value)
                {
                    ItemBytes[Offset.SellableFlag] |= 0x80;
                }
                else
                {
                    ItemBytes[Offset.SellableFlag] &= 0x7F;
                }
            }

        }


        public List<EffectInfo> ReadEffects()
        {
            List<EffectInfo> effects = new List<EffectInfo>();

            for (int i = 0, offset = Offsets.EffectCount + 4; i < EffectCount; i++,offset+=8)
            {
                effects.Add(new EffectInfo
                {
                    Code = BitConverter.ToUInt32(ItemBytes, offset).ToString("X6")
                });
            }

            return effects;
        }

        public void WriteEffects(List<EffectInfo> newEffects)
        {
            int newCount = newEffects.Count;
            var buffer = new byte[ItemBytes.Length + (newCount - EffectCount) * 8];
            ItemBytes.AsSpan(0, Offsets.EffectCount).CopyTo(buffer);
            MemoryMarshal.Write(buffer.AsSpan(Offsets.EffectCount), ref newCount);
            int offset = Offsets.EffectCount + 4;
            foreach (EffectInfo effect in newEffects)
            {
                ulong data = uint.Parse(effect.Code, NumberStyles.HexNumber) | (ulong)uint.MaxValue << 32;
                MemoryMarshal.Write(buffer.AsSpan(offset), ref data);
                offset += 8;
            }

            ItemBytes.AsSpan(Offset.PostEffect).CopyTo(buffer.AsSpan(offset));

            ItemBytes = buffer;
        }
 
    }
}
