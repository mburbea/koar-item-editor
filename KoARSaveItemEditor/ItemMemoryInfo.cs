using ByteManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace KoARSaveItemEditor
{
    /// <summary>
    /// Equipment Memory Information
    /// </summary>
    public class ItemMemoryInfo
    {
        public ItemMemoryInfo()
        {
            Offset = new Offsets(this);
        }
        /// <summary>
        /// Equipment head Index(XX XX XX XX 0B 00 00 00 68 D5 24 00 03)
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        /// Next Equipment head Index
        /// </summary>
        public int NextItemIndex { get; set; }

        /// <summary>
        /// Equipment data
        /// </summary>
        public byte[] ItemBytes { get; set; }

        private Offsets Offset {get;}
        private readonly struct Offsets
        {
            private readonly ItemMemoryInfo _this;
            public Offsets(ItemMemoryInfo @this) => _this  = @this;

            public int EffectCount => AmalurSaveEditor.EffectOffset;
            public int PostEffect => EffectCount + _this.EffectCount * 8;
            public int CurrentDurability => PostEffect + 8;
            public int MaxDurability => CurrentDurability + 4;

            public int SellableFlag => MaxDurability + 8;
            public int HasCustomName => SellableFlag + 2;
            public int CustomNameLength => HasCustomName + 1;
            public int CustomNameText => CustomNameLength + 4;

        }

        /// <summary>
        /// Equipment Name
        /// </summary>
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
                    ItemBytes.AsSpan(0, Offset.CustomNameLength);
                    MemoryMarshal.Write(buffer.AsSpan(Offset.CustomNameLength), ref newLength);
                    newBytes.CopyTo(buffer, Offset.CustomNameText);
                    ItemBytes = buffer;
                }
            }
        }

        public bool HasCustomName => ItemBytes[Offset.HasCustomName] != 0;

        /// <summary>
        /// Number of Effects
        /// </summary>
        public int EffectCount => BitConverter.ToInt32(ItemBytes, Offset.EffectCount);

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

            for (int i = 0, offset = Offset.EffectCount + 4; i < EffectCount; i++,offset+=8)
            {
                effects.Add(new EffectInfo
                {
                    Code = BitConverter.ToUInt32(ItemBytes, offset).ToString("X6")
                });
            }

            return effects;
        }

        public void WriteEffects(List<EffectInfo> value)
        {
            int newCount = value.Count;
            var buffer = new byte[ItemBytes.Length + (newCount - EffectCount) * 8];
            ItemBytes.AsSpan(0, Offset.EffectCount).CopyTo(buffer);
            MemoryMarshal.Write(buffer.AsSpan(Offset.EffectCount), ref newCount);
            int offset = Offset.EffectCount + 4;
            for (int i = 0; i < value.Count; i++)
            {
                ulong data = uint.Parse(value[i].Code, NumberStyles.HexNumber) | (ulong)uint.MaxValue << 32;
                MemoryMarshal.Write(buffer.AsSpan(offset), ref data);
                offset += 8;
            }

            var destination = buffer.AsSpan(offset);
            var src = ItemBytes.AsSpan(Offset.PostEffect);
            src.CopyTo(destination);

            ItemBytes = buffer;
        }
 
    }
}
