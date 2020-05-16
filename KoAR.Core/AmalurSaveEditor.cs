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
        private static ReadOnlySpan<byte> EquipmentSequence => new byte[]     { 0x0B, 00, 00, 00, 0x68, 0xD5, 0x24, 0x00, 0x03 };
        private static ReadOnlySpan<byte> CoreAttributeSequence => new byte[] { 0x0B, 00, 00, 00, 0x84, 0x60, 0x28, 0x00, 0x00};
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

            List<ItemMemoryInfo> equipmentList = new List<ItemMemoryInfo>();
            var bytes = Bytes;
            var indexList = GetAllIndices(bytes, EquipmentSequence);
            var coreIndices = GetAllIndices(bytes, CoreAttributeSequence);
            int numberOfWs=0, numberOfLs=0;
            var bins = new Dictionary<string, int>();

            for (int i = 0; i < indexList.Count; i++)
            {
                if(ItemMemoryInfo.Create(indexList[i], i == indexList.Count - 1 
                    ? bytes.AsSpan(indexList[i]) 
                    : bytes.AsSpan(indexList[i], indexList[i + 1] - indexList[i])) is ItemMemoryInfo item)
                {
                    equipmentList.Add(item);
                    var internalId = MemoryUtilities.Read<int>(bytes, item.ItemIndex);
                    var ix = coreIndices.FindIndex(x => MemoryUtilities.Read<int>(bytes, x) == internalId);
                    _ = ix == -1 ? numberOfLs++ : numberOfWs++;
                        if (ix != -1)
                        {
                            var b13 = bytes[coreIndices[ix] + 13];
                            var binStr = b13.ToString("X2");
                            bins.TryGetValue(binStr, out var c);
                            bins[binStr] = c + 1;
                        }
                }
            }
            Console.WriteLine(numberOfWs);
            Console.WriteLine(numberOfLs);
            Console.WriteLine(bins);

            return equipmentList;
        }

        /// <summary>
        /// Delete Equipment
        /// </summary>
        /// <param name="weapon"></param>
        public void DeleteEquipment(ItemMemoryInfo equipment) 
            => Bytes = MemoryUtilities.ReplaceBytes(Bytes, equipment.ItemIndex, equipment.DataLength, equipment.ItemBytes);

        public void WriteEquipmentBytes(ItemMemoryInfo equipment) => 
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, equipment.ItemIndex, equipment.DataLength, equipment.ItemBytes);
    }
}