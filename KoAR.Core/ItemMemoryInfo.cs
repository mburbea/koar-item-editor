using System;
using System.Buffers.Binary;
using System.Collections.Generic;
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
            static EquipmentCategory DetermineEquipmentType(ReadOnlySpan<byte> bytes, Span<byte> buffer, byte byte13)
            {
                ReadOnlySpan<byte> weaponTypeSequence = new byte[] { 0xD4, 0x08, 0x46, 0x00, 0x01 };
                ReadOnlySpan<byte> additionalInfoSequence = new byte[] { 0x8D, 0xE3, 0x47, 0x00, 0x02 };
                weaponTypeSequence.CopyTo(buffer.Slice(8));
                var offset = bytes.IndexOf(buffer);
                if (offset == -1)
                {
                    return EquipmentCategory.Armor; // Armor doesn't have this section.
                }
                var equipTypeByte = bytes[offset + 13];
                additionalInfoSequence.CopyTo(buffer.Slice(8));
                var aisOffset = bytes.IndexOf(buffer);
                var d = bytes[aisOffset + 17];
                return equipTypeByte switch
                {
                    0x10 => EquipmentCategory.Shield,
                    0x18 => EquipmentCategory.LongBow,
                    0x20 when d == 0x00 || d == 0xBC || d == 0x55 || d == 0x56 || d == 0x18 => EquipmentCategory.LongSword,
                    0x20 => EquipmentCategory.GreatSword,
                    0x24 when d == 0x00 || d == 0x40 || d == 0x41 || d == 0x2C || d == 0xE8 => EquipmentCategory.Daggers,
                    0x24 => EquipmentCategory.FaeBlades,
                    0x1C when d == 0x00 || d == 0x18 || d == 0x53 || d == 0x54 => EquipmentCategory.Staff,
                    0x1C when d == 0x3E || d == 0x3F || d == 0xEA || d == 0xEB => EquipmentCategory.Chakrams,
                    0x1C when d == 0xEC || d == 0x43 || d == 0x7E => EquipmentCategory.Hammer,
                    0x14 when d == 0x1D || d == 0x18 || d == 0xC9 || d == 0xAF => EquipmentCategory.Talisman,
                    0x14 when d == 0x4A || d == 0x47 || d == 0x48 => EquipmentCategory.Sceptre,
                    0x14 when d == 0x1B || d == 0xCA => EquipmentCategory.Buckler,
                    0x14 when d == 0x00 && byte13 == 0x3B => EquipmentCategory.Talisman,
                    0x14 when d == 0x00 => EquipmentCategory.Buckler, /* 0x33 0x23 0x2B 0x00 */
                    _ => EquipmentCategory.Unknown,
                };
            }

            Span<byte> buffer = stackalloc byte[13];
            ItemIndex = itemIndex;
            DataLength = dataLength;
            ItemBytes = bytes.Slice(itemIndex, dataLength).ToArray();
            bytes.Slice(itemIndex, 8).CopyTo(buffer);
            CoreEffects = new CoreEffectMemory(buffer);
            Category = DetermineEquipmentType(bytes, buffer, ItemBytes[13]);
            _typeIdOffset = bytes.IndexOf(buffer.Slice(0, 4)) + 4;
            Effects = new List<uint>(ItemBytes[Offset.EffectCount]);
            for (int i = 0; i < Effects.Capacity; i++)
            {
                Effects.Add(MemoryUtilities.Read<uint>(ItemBytes, Offset.FirstEffect + i * 8));
            }

        }

        public CoreEffectMemory CoreEffects { get; internal set; }
        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.CurrentDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.CurrentDurability, value);
        }

        public int DataLength { get; internal set; }

        public List<uint> Effects { get; }

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

        public EquipmentCategory Category { get; }

        // this holds a pointer to the current instance of the item template.
        // any material change will blow away the backing array.
        private readonly int _typeIdOffset;

        public uint TypeId
        {
            get => MemoryUtilities.Read<uint>(Amalur.Bytes, _typeIdOffset);
            set => MemoryUtilities.Write(Amalur.Bytes, _typeIdOffset, value);
        }

        public byte[] ItemBytes { get; set; }

        public uint ItemId => MemoryUtilities.Read<uint>(ItemBytes);

        public int ItemIndex { get; internal set; }

        public string ItemName
        {
            get
            {
                if (!HasCustomName)
                {
                    return $"Unknown ({BinaryPrimitives.ReadUInt32BigEndian(ItemBytes):X8})";
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

        private Offset Offsets => new Offset(Effects.Count);

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

        public void Serialize()
        {
            int currentCount = ItemBytes[Offset.EffectCount];
            if (currentCount == Effects.Count)
            {
                return;
            }
            var offsets = new Offset(currentCount);
            var currentLength = offsets.PostEffect - Offset.FirstEffect;
            ItemBytes[Offset.EffectCount] = (byte)Effects.Count;
            Span<ulong> effectData = stackalloc ulong[Effects.Count];

            for (int i = 0; i < effectData.Length; i++)
            {
                effectData[i] = Effects[i] | (ulong)uint.MaxValue << 32;
            }

            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offset.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
        }
    }
}