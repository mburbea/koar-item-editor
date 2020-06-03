using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
            static EquipmentCategory DetermineEquipmentType(ReadOnlySpan<byte> bytes, Span<byte> buffer)
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
                    0x14 => EquipmentCategory.Sceptre,
                    0x18 => EquipmentCategory.Longbow,
                    0x20 when d == 0x00 || d == 0xBC || d == 0x55 || d == 0x56 || d == 0x18 => EquipmentCategory.Longsword,
                    0x20 => EquipmentCategory.Greatsword,
                    0x24 when d == 0x00 || d == 0x40 || d == 0x41 || d == 0x2C || d == 0xE8 => EquipmentCategory.Daggers,
                    0x24 => EquipmentCategory.Faeblades,
                    0x1C when d == 0x00 || d == 0x18 || d == 0x53 || d == 0x54 => EquipmentCategory.Staff,
                    0x1C when d == 0x3E || d == 0x3F || d == 0xEA || d == 0xEB => EquipmentCategory.Chakrams,
                    0x1C when d == 0xEC || d == 0x43 || d == 0x7E => EquipmentCategory.Hammer,
                    _ => EquipmentCategory.Unknown,
                };
            }

            Span<byte> buffer = stackalloc byte[13];
            ItemIndex = itemIndex;
            DataLength = dataLength;
            ItemBytes = bytes.Slice(itemIndex, dataLength).ToArray();
            bytes.Slice(itemIndex, 8).CopyTo(buffer);
            CoreEffects = new CoreEffectMemory(buffer);
            _typeIdOffset = bytes.IndexOf(buffer.Slice(0, 4)) + 4;
            if (bytes[_typeIdOffset + 10] == 1)
            {
                Category = bytes[_typeIdOffset + 14] switch
                {
                    0xEC => EquipmentCategory.Buckler,
                    0xED => EquipmentCategory.Shield,
                    0xEE => EquipmentCategory.Talisman,
                    0xF3 => EquipmentCategory.Chakrams,
                    _ => EquipmentCategory.Unknown,
                };
                _levelShiftOffset = 8;
            }
            if (Category == EquipmentCategory.Unknown)
            {
                Category = DetermineEquipmentType(bytes, buffer);
            }

            Effects = new List<uint>(ItemBytes[Offset.EffectCount]);
            for (int i = 0; i < Effects.Capacity; i++)
            {
                Effects.Add(MemoryUtilities.Read<uint>(ItemBytes, Offset.FirstEffect + i * 8));
            }
        }

        public CoreEffectMemory CoreEffects { get; }

        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.CurrentDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.CurrentDurability, value);
        }

        internal int DataLength { get; set; }

        public List<uint> Effects { get; } = new List<uint>();

        public bool HasCustomName
        {
            get => ItemBytes[Offsets.HasCustomName] == 1;
            private set => ItemBytes[Offsets.HasCustomName] = (byte)(value ? 1 : 0);
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

        public EquipmentCategory Category { get; }

        private readonly int _typeIdOffset;
        private readonly byte _levelShiftOffset;
        private int LevelOffset => _typeIdOffset + 14 + _levelShiftOffset;

        public uint TypeId
        {
            get => MemoryUtilities.Read<uint>(Amalur.Bytes, _typeIdOffset);
            set
            {
                MemoryUtilities.Write(Amalur.Bytes, _typeIdOffset, value);
                MemoryUtilities.Write(Amalur.Bytes, _typeIdOffset + 30 + _levelShiftOffset, value);
            }
        }

        public byte Level
        {
            get => Amalur.Bytes[LevelOffset];
            set => Amalur.Bytes[LevelOffset] = value;
        }

        public byte[] ItemBytes { get; private set; }

        public uint ItemId => MemoryUtilities.Read<uint>(ItemBytes);

        public int ItemIndex { get; internal set; }

        private int NameLength
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.CustomNameLength);
            set => MemoryUtilities.Write(ItemBytes, Offsets.CustomNameLength, value);
        }

        public string ItemName
        {
            get => HasCustomName
                ? Encoding.Default.GetString(ItemBytes, Offsets.CustomNameText, NameLength)
                : string.Empty;
            set
            {
                if (value.Length > 0)
                {
                    var newBytes = Encoding.Default.GetBytes(value);
                    if (Offsets.CustomNameText + newBytes.Length != ItemBytes.Length)
                    {
                        var buffer = new byte[Offsets.CustomNameText + newBytes.Length];
                        ItemBytes.AsSpan(0, Offsets.CustomNameLength).CopyTo(buffer);
                        ItemBytes = buffer;
                    }
                    HasCustomName = true;
                    NameLength = newBytes.Length;
                    newBytes.CopyTo(ItemBytes, Offsets.CustomNameText);
                }
                else if (HasCustomName)
                {
                    ItemBytes = ItemBytes.AsSpan(0, Offsets.CustomNameLength).ToArray();
                    HasCustomName = false;
                }
            }
        }

        public float MaxDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.MaxDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.MaxDurability, value);
        }

        private Offset Offsets => new Offset(Effects.Count);

        public void Rematerialize(byte[] bytes)
        {
            ItemBytes = bytes;
            Effects.Clear();
            Effects.Capacity = ItemBytes[Offset.EffectCount];
            for (int i = 0; i < Effects.Capacity; i++)
            {
                Effects.Add(MemoryUtilities.Read<uint>(ItemBytes, Offset.FirstEffect + i * 8));
            }
        }

        public static bool TryCreate(int itemIndex, int nextOffset, [NotNullWhen(true)] out ItemMemoryInfo? item)
        {
            item = null;
            var bytes = Amalur.Bytes;
            if (nextOffset - itemIndex < MinEquipmentLength)
            {
                return false;
            }
            var offsets = new Offset(MemoryUtilities.Read<int>(bytes, itemIndex + Offset.EffectCount));
            if (!IsValidDurability(MemoryUtilities.Read<float>(bytes, itemIndex + offsets.CurrentDurability))
                || !IsValidDurability(MemoryUtilities.Read<float>(bytes, itemIndex + offsets.MaxDurability)))
            {
                return false;
            }
            int dataLength = bytes[itemIndex + offsets.HasCustomName] != 1
                ? offsets.CustomNameLength
                : offsets.CustomNameText + MemoryUtilities.Read<int>(bytes, itemIndex + offsets.CustomNameLength);
            item = new ItemMemoryInfo(bytes, itemIndex, dataLength);
            return true;
        }

        public static bool IsValidDurability(float durability) => durability > DurabilityLowerBound && durability < DurabilityUpperBound;

        internal byte[] Serialize(bool forced = false)
        {
            int currentCount = ItemBytes[Offset.EffectCount];
            if (!forced && currentCount == Effects.Count)
            {
                return ItemBytes;
            }
            var currentLength = new Offset(currentCount).PostEffect - Offset.FirstEffect;
            ItemBytes[Offset.EffectCount] = (byte)Effects.Count;
            Span<ulong> effectData = stackalloc ulong[Effects.Count];
            for (int i = 0; i < effectData.Length; i++)
            {
                effectData[i] = Effects[i] | (ulong)uint.MaxValue << 32;
            }
            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offset.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
            return ItemBytes;
        }

        internal void WriteToCsv()
        {
            IEnumerable<string> rows = new[] {
                $"{Category},{TypeId:X6},{Level},\"{ItemName}\",{MaxDurability},{string.Join("", CoreEffects.List.Select(x => x.ToString("X6")))},{string.Join("", Effects.Select(x => x.ToString("X6")))}"
            };
            if (!File.Exists("items.user.csv"))
            {
                rows = rows.Prepend("Category,TypeId,Level,ItemName,Durability,CoreEffects,Effects");
            }
            File.AppendAllLines("items.user.csv", rows);
        }

        internal void LoadFromDefinition(TypeDefinition definition, bool assignName)
        {
            TypeId = definition.TypeId;
            CurrentDurability = definition.MaxDurability;
            MaxDurability = definition.MaxDurability;
            CoreEffects.List.Clear();
            CoreEffects.List.AddRange(definition.CoreEffects);
            Effects.Clear();
            Effects.AddRange(definition.Effects);
            ItemName = assignName ? definition.Name : "";
        }
    }
}