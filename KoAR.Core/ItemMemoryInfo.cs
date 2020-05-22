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
    public partial class ItemMemoryInfo
    {
        public const float DurabilityLowerBound = 0f;
        public const float DurabilityUpperBound = 100f;
        public const int MinEquipmentLength = 44;

        private ItemMemoryInfo(ReadOnlySpan<byte> bytes, int itemIndex, int dataLength)
        {
            static EquipmentType DetermineEquipmentType(ReadOnlySpan<byte> bytes, Span<byte> buffer, byte byte13)
            {
                ReadOnlySpan<byte> weaponTypeSequence = new byte[] { 0xD4, 0x08, 0x46, 0x00, 0x01 };
                ReadOnlySpan<byte> additionalInfoSequence = new byte[] { 0x8D, 0xE3, 0x47, 0x00, 0x02 };
                weaponTypeSequence.CopyTo(buffer.Slice(8));
                var offset = bytes.IndexOf(buffer);
                if (offset == -1)
                {
                    return EquipmentType.Armor; // Armor doesn't have this section.
                }
                var equipTypeByte = bytes[offset + 13];
                additionalInfoSequence.CopyTo(buffer.Slice(8));
                var aisOffset = bytes.IndexOf(buffer);
                var d = bytes[aisOffset + 17];
                return equipTypeByte switch
                {
                    0x10 => EquipmentType.Shield,
                    0x18 => EquipmentType.LongBow,
                    0x20 when d == 0x00 || d == 0xBC || d == 0x55 || d == 0x56 || d == 0x18 => EquipmentType.LongSword,
                    0x20 => EquipmentType.GreatSword,
                    0x24 when d == 0x00 || d == 0x40 || d == 0x41 || d == 0x2C || d == 0xE8 => EquipmentType.Daggers,
                    0x24 => EquipmentType.FaeBlades,
                    0x1C when d == 0x00 || d == 0x18 || d == 0x53 || d == 0x54 => EquipmentType.Staff,
                    0x1C when d == 0x3E || d == 0x3F || d == 0xEA || d == 0xEB => EquipmentType.Chakrams,
                    0x1C when d == 0xEC || d == 0x43 || d == 0x7E => EquipmentType.Hammer,
                    0x14 when d == 0x1D || d == 0x18 || d == 0xC9 || d == 0xAF => EquipmentType.Talisman,
                    0x14 when d == 0x4A || d == 0x47 || d == 0x48 => EquipmentType.Sceptre,
                    0x14 when d == 0x1B || d == 0xCA => EquipmentType.Buckler,
                    0x14 when d == 0x00 && byte13 == 0x3B => EquipmentType.Talisman,
                    0x14 when d == 0x00 => EquipmentType.Buckler, /* 0x33 0x23 0x2B 0x00 */
                    _ => EquipmentType.Unknown,
                };
            }

            Span<byte> buffer = stackalloc byte[13];
            ItemIndex = itemIndex;
            DataLength = dataLength;
            ItemBytes = bytes.Slice(itemIndex, dataLength).ToArray();
            bytes.Slice(itemIndex, 8).CopyTo(buffer);
            CoreEffects = new CoreEffectList(buffer);
            EquipmentType = DetermineEquipmentType(bytes, buffer, ItemBytes[13]);
            _itemTemplateMemory = new Memory<byte>(Amalur.Bytes, bytes.IndexOf(buffer.Slice(0, 4)) + 4, 4);
        }

        public CoreEffectList CoreEffects { get; internal set; }
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

        public EquipmentType EquipmentType { get; }

        // this holds a pointer to the current instance of the item template.
        // any material change will blow away the backing array.
        private readonly Memory<byte> _itemTemplateMemory; 

        public int ItemTemplate
        {
            get => MemoryMarshal.Read<int>(_itemTemplateMemory.Span);
            set => MemoryMarshal.Write(_itemTemplateMemory.Span, ref value);
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
                int nameLength = MemoryUtilities.Read<int>(ItemBytes, Offsets.CustomNameLength);
                return Encoding.Default.GetString(ItemBytes, Offsets.CustomNameText, nameLength);
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

        public static ItemMemoryInfo Create(int itemIndex, int nextOffset)
        {
            var bytes = Amalur.Bytes;
            if (nextOffset - itemIndex < MinEquipmentLength)
            {
                return null;
            }
            var offsets = new Offset(MemoryUtilities.Read<int>(bytes, itemIndex + Offset.EffectCount));
            if (!IsValidDurability(MemoryUtilities.Read<float>(bytes, itemIndex + offsets.CurrentDurability))
                || !IsValidDurability(MemoryUtilities.Read<float>(bytes, itemIndex + offsets.MaxDurability)))
            {
                return null;
            }

            int dataLength = bytes[itemIndex + offsets.HasCustomName] != 1
                ? offsets.CustomNameLength
                : offsets.CustomNameText + MemoryUtilities.Read<int>(bytes, itemIndex + offsets.CustomNameLength);
            return new ItemMemoryInfo(bytes, itemIndex, dataLength);
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