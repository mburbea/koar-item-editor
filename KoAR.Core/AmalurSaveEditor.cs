using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
        private static ReadOnlySpan<byte> EquipmentSequence => new byte[] { 11, 0, 0, 0, 104, 213, 36, 0, 3 };
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
        /// <summary>
        /// Get maximum backpack capacity.
        /// </summary>
        /// <returns></returns>
        public int GetMaxBagCount()
        {
            return BitConverter.ToInt32(Bytes, GetBagOffset());

        }

        /// <summary>
        /// Modify maximum backpack capacity.
        /// </summary>
        /// <param name="c"></param>
        public void EditMaxBagCount(int c)
        {
            MemoryMarshal.Write(Bytes.AsSpan(GetBagOffset()), ref c);
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

        public bool IsValidDurability(float durability) => durability > 0f && durability < 100f;

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

            var indexList = GetAllIndices(Bytes, EquipmentSequence);

            for (int i = 0; i < indexList.Count; i++)
            {
                if (i != indexList.Count - 1)
                {
                    if (indexList[i + 1] - indexList[i] < 44)
                    {
                        continue;
                    }
                }

                ItemMemoryInfo equipment = new ItemMemoryInfo { ItemIndex = indexList[i] };
                if (i != indexList.Count - 1)
                {
                    equipment.NextItemIndex = indexList[i + 1];
                    equipment.ItemBytes = Bytes.AsSpan(indexList[i], indexList[i + 1] - indexList[i]).ToArray();

                    if (IsValidDurability(equipment.CurrentDurability) && IsValidDurability(equipment.MaxDurability))
                    {
                        equipmentList.Add(equipment);
                    }
                }
                else
                {
                    var span = Bytes.AsSpan(equipment.ItemIndex);
                    var offsets = new Offsets(MemoryMarshal.Read<int>(span.Slice(Offsets.EffectCount)));

                    int endIndex;
                    if (span[offsets.HasCustomName] != 1)
                    {
                        endIndex = offsets.CustomNameText;
                    }
                    else
                    {
                        var nameLength = MemoryMarshal.Read<int>(span.Slice(offsets.CustomNameLength));
                        endIndex = offsets.CustomNameText + nameLength;
                    }
                    equipment.ItemBytes = Bytes.AsSpan(equipment.ItemIndex, endIndex).ToArray();

                    if (IsValidDurability(equipment.CurrentDurability) && IsValidDurability(equipment.MaxDurability))
                    {
                        equipmentList.Add(equipment);
                    }
                }
            }

            return equipmentList;
        }

        /// <summary>
        /// Delete Equipment
        /// </summary>
        /// <param name="weapon"></param>
        public void DeleteEquipment(ItemMemoryInfo weapon)
        {
            weapon.ItemBytes = new byte[4];
            WriteEquipmentBytes(weapon);
        }

        public void WriteEquipmentBytes(ItemMemoryInfo weapon)
        {
            var bytes = Bytes;
            var delta = weapon.ItemBytes.Length + (weapon.NextItemIndex - weapon.ItemIndex);
            var buffer = new byte[bytes.Length + delta];
            bytes.AsSpan(0, weapon.ItemIndex).CopyTo(buffer);
            weapon.ItemBytes.CopyTo(buffer, weapon.ItemIndex);
            bytes.AsSpan(weapon.NextItemIndex).CopyTo(buffer.AsSpan(weapon.ItemIndex + weapon.ItemBytes.Length));
            Bytes = buffer;
        }
    }
}
