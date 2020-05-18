using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    /// <summary>
    /// Archive Operation for Kingdosm of Amalur(supports 1.0.0.2)
    /// </summary>
    public class AmalurSaveEditor
    {
        /// <summary>
        /// The head of the equipment, property and indicate the number of attributes of the data relative to equipment data head offset
        /// </summary>
        private static ReadOnlySpan<byte> InventoryLimit => new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
        private static ReadOnlySpan<byte> IncreaseAmount => new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
        private static ReadOnlySpan<byte> CurrentInventoryCount => new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
        private static ReadOnlySpan<byte> EquipmentSequence => new byte[]     { 0x0B, 0x00, 0x00, 0x00, 0x68, 0xD5, 0x24, 0x00, 0x03 };
        private static ReadOnlySpan<byte> CoreAttributeSequence => new byte[] { 0x0B, 0x00, 0x00, 0x00, 0x84, 0x60, 0x28, 0x00, 0x00 };
        private static ReadOnlySpan<byte> EquipTypeSequence => new byte[]     { 0x0B, 0x00, 0x00, 0x00, 0xD4, 0x08, 0x46, 0x00, 0x01 };
        private static ReadOnlySpan<byte> DifferentiatingSeq => new byte[]    { 0x0B, 0x00, 0x00, 0x00, 0x8D, 0xE3, 0x47, 0x00, 0x02 };
        private byte[] _bytes;

        public byte[] Bytes
        {
            get => _bytes ?? throw new Exception("Save file not open");
            set => _bytes = value;
        }

        /// <summary>
        /// Read save-file
        /// </summary>
        /// <param name="path">archive path</param>
        public void ReadFile(string path)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.Open);
                _bytes = new byte[fs.Length];
                fs.Read(_bytes, 0, (int)fs.Length);
            }
            catch
            {
                throw new Exception("File cannot open!");
            }
        }

        /// <summary>
        /// Save save-file
        /// </summary>
        /// <param name="path">save path</param>
        public void SaveFile(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                fs.Write(Bytes, 0, Bytes.Length);
            }
            catch
            {
                throw new Exception("Saving failed!");
            }
        }

        private int GetBagOffset()
        {
            ReadOnlySpan<byte> span = Bytes;
            var curInvCountOffset = span.IndexOf(CurrentInventoryCount) + CurrentInventoryCount.Length;
            var inventoryLimitOffset = span.IndexOf(InventoryLimit) + InventoryLimit.Length;
            var increaseAmountOffset = span.IndexOf(IncreaseAmount) + IncreaseAmount.Length;
            var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
            var inventoryLimitOrder = inventoryLimitOffset == finalOffset
                ? 3
                : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;

            return finalOffset + (inventoryLimitOrder * 12);
        }

        public int GetMaxBagCount() => MemoryUtilities.Read<int>(Bytes, GetBagOffset());

        public void EditMaxBagCount(int count) => MemoryUtilities.Write(Bytes, GetBagOffset(), count);

        public List<CoreEffectInfo> GetCoreEffectInfos(CoreItemMemory coreItem, IReadOnlyDictionary<string, CoreEffectInfo> effects)
        {
            var itemEffects = coreItem.ReadEffects();
            for(int i = 0; i < itemEffects.Count; i++)
            {
                if(effects.TryGetValue(itemEffects[i].Code, out var definition)){
                    itemEffects[i] = definition.Clone();
                }
            }

            return itemEffects;
        }

        public List<EffectInfo> GetEffectList(ItemMemoryInfo weaponInfo, IEnumerable<EffectInfo> effects)
        {
            var itemEffects = weaponInfo.ReadEffects();
            foreach (EffectInfo attInfo in itemEffects)
            {
                attInfo.DisplayText = effects.FirstOrDefault(x => x.Code == attInfo.Code)?.DisplayText ?? "Unknown";
            }

            return itemEffects;
        }


        public List<ItemMemoryInfo> GetAllEquipment()
        {
            static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
            {
                var results = new List<int>();
                int ix = data.IndexOf(sequence);
                int start = 0;

                while (ix != -1)
                {
                    results.Add(start + ix - 4);
                    start += ix + sequence.Length;
                    ix = data.Slice(start).IndexOf(sequence);
                }
                return results;
            }

            static EquipmentType DetermineEquipmentType(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> itemId)
            {
                Span<byte> buffer = stackalloc byte[13];
                itemId.CopyTo(buffer);
                EquipTypeSequence.CopyTo(buffer.Slice(4));
                var offset = bytes.IndexOf(buffer);
                var differingByte = bytes[offset + 13];
                if(differingByte != 0x24 && differingByte != 0x1C)
                {
                    return differingByte switch
                    {
                        0x18 => EquipmentType.LongBow,
                        0x14 => EquipmentType.Sceptre,
                        0x20 => bytes[offset + 21] == 0 ? EquipmentType.GreatSword : EquipmentType.LongSword,
                        _ => EquipmentType.Unknown,
                    };
                }
                DifferentiatingSeq.CopyTo(buffer.Slice(4));
                offset = bytes.IndexOf(buffer);
                var magicStrengthOrFinesse = MemoryUtilities.Read<int>(bytes,offset + 17);
                return magicStrengthOrFinesse switch
                {
                    0x1D_5A_EA => EquipmentType.Chakrams,
                    0x1D_5A_EB => EquipmentType.FaeBlades,
                    0x1D_5A_EC => EquipmentType.Hammer,
                    0 => differingByte == 0x24 ? EquipmentType.Daggers : EquipmentType.Staff,
                    _=> EquipmentType.Unknown,
                };

            }

            List<ItemMemoryInfo> equipmentList = new List<ItemMemoryInfo>();
            var bytes = Bytes;
            var indexList = GetAllIndices(bytes, EquipmentSequence);
            var bins = new Dictionary<string, int>();
            var coreHeader = new byte[13];
            MemoryUtilities.ReplaceBytes(coreHeader, 4, 9, CoreAttributeSequence);
            for (int i = 0; i < indexList.Count; i++)
            {
                if (ItemMemoryInfo.Create(indexList[i], i == indexList.Count - 1
                    ? bytes.AsSpan(indexList[i])
                    : bytes.AsSpan(indexList[i], indexList[i + 1] - indexList[i])) is ItemMemoryInfo item)
                {
                    MemoryUtilities.Write(coreHeader, 0, item.ItemId);
                    var span = bytes.AsSpan(indexList[i]);

                    int coreOffset = span.IndexOf(coreHeader) + indexList[i];
                    item.CoreItemMemory = CoreItemMemory.Create(coreOffset, bytes.AsSpan(coreOffset));
                    item.EquipmentType = DetermineEquipmentType(bytes, bytes.AsSpan(indexList[i], 4));
                    equipmentList.Add(item);

                    var b13 = item.CoreItemMemory.MysteryInteger;
                    var itemName = item.ItemName;
                    var binStr = b13.ToString("X2");
                    bins.TryGetValue(binStr, out var c);
                    bins[binStr] = c + 1;

                }
            }
            var max = equipmentList.Max(x => x.CoreItemMemory.EffectCount);
            
            Console.WriteLine(max);
            Console.WriteLine(bins);

            return equipmentList;
        }

        /// <summary>
        /// Delete Equipment
        /// </summary>
        /// <param name="weapon"></param>
        public void DeleteEquipment(ItemMemoryInfo equipment)
            => Bytes = MemoryUtilities.ReplaceBytes(Bytes, equipment.ItemIndex, equipment.DataLength, equipment.ItemBytes);

        public void WriteEquipmentBytes(ItemMemoryInfo equipment, out bool lengthChanged)
        {
            var bytes = Bytes;
            var oldLength = bytes.Length;
            var coreMemory = equipment.CoreItemMemory;
            bytes = MemoryUtilities.ReplaceBytes(bytes, coreMemory.ItemIndex, coreMemory.DataLength, coreMemory.ItemBytes);
            bytes = MemoryUtilities.ReplaceBytes(bytes, equipment.ItemIndex, equipment.DataLength, equipment.ItemBytes);
            Bytes = bytes;
            lengthChanged = Bytes.Length != oldLength;
        }
    }
}