using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ByteManager;

namespace KoARSaveItemEditor
{
    /// <summary>
    /// Archive Operation for Kingdosm of Amalur(supports 1.0.0.2)
    /// </summary>
    public class AmalurSaveEditor
    {
        /// <summary>
        /// The head of the equipment, property and indicate the number of attributes of the data relative to equipment data head offset
        /// </summary>
        public const int EffectOffset = 21;
        public const int InventoryCapacityOffset = 36;
        //public const string InventoryLimit = "inventory_limit";
        //public const string CurrentInventoryCount = "current_inventory_count";
        private ReadOnlySpan<byte> InventoryLimit => new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
        private ReadOnlySpan<byte> IncreaseAmount => new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
        private ReadOnlySpan<byte> CurrentInventoryCount => new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };

        private ReadOnlySpan<byte> EquipmentSequence => new byte[] { 11, 0, 0, 0, 104, 213, 36, 0, 3 };
        private ByteEditor br = null;

        /// <summary>
        /// Read save-file
        /// </summary>
        /// <param name="path">archive path</param>
        public void ReadFile(string path)
        {
            br = new ByteEditor();
            try
            {
                br.ReadFile(path);
            }
            catch
            {
                br = null;
                throw new Exception("Save file failed to open.");
            }
        }

        /// <summary>
        /// Save save-file
        /// </summary>
        /// <param name="path">save path</param>
        public void SaveFile(string path)
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }

            try
            {
                br.SaveFile(path);
            }
            catch
            {
                throw new Exception("Saving failed!");
            }
        }


        private int GetBagOffset()
        {
            ReadOnlySpan<byte> span = br.Bytes;
            var curInvCountOffset = span.IndexOf(CurrentInventoryCount) + CurrentInventoryCount.Length;
            var inventoryLimitOffset = span.IndexOf(InventoryLimit) + InventoryLimit.Length;
            var increaseAmountOffset = span.IndexOf(IncreaseAmount) + IncreaseAmount.Length;
            var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
            var inventoryLimitOrder = inventoryLimitOffset == finalOffset ? 3 : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;
            return finalOffset + (inventoryLimitOrder * 12);
        }
        /// <summary>
        /// Get maximum backpack capacity.
        /// </summary>
        /// <returns></returns>
        public int GetMaxBagCount()
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }


            return BitConverter.ToInt32(br.Bytes, GetBagOffset());

        }

        /// <summary>
        /// Modify maximum backpack capacity.
        /// </summary>
        /// <param name="c"></param>
        public void EditMaxBagCount(int c)
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }
            MemoryMarshal.Write(br.Bytes.AsSpan(GetBagOffset()), ref c);
        }

        public List<EffectInfo> GetEffectList(ItemMemoryInfo weaponInfo, IEnumerable<EffectInfo> effects)
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }
            List<EffectInfo> list = weaponInfo.ItemAttList;
            foreach (EffectInfo attInfo in list)
            {
                attInfo.DisplayText = effects.FirstOrDefault(x => x.Code == attInfo.Code)?.DisplayText ?? "Unknown";
            }
            return list;
        }

        public bool IsValidDurability(float durability) => durability > 0f && durability < 100f;

        public List<ItemMemoryInfo> GetAllEquipment()
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }

            List<ItemMemoryInfo> weaponList = new List<ItemMemoryInfo>();

            var indexList = br.GetAllIndices(EquipmentSequence);

            for (int i = 0; i < indexList.Count; i++)
            {
                indexList[i] -= 4;
            }

            for (int i = 0; i < indexList.Count; i++)
            {
                if (i != indexList.Count - 1)
                {
                    if (indexList[i + 1] - indexList[i] < 44)
                    {
                        continue;
                    }
                }

                ItemMemoryInfo weapon = new ItemMemoryInfo { ItemIndex = indexList[i] };
                if (i != indexList.Count - 1)
                {
                    weapon.NextItemIndex = indexList[i + 1];
                    weapon.ItemBytes = br.Bytes.AsSpan(indexList[i], indexList[i + 1] - indexList[i]).ToArray();

                    if (IsValidDurability(weapon.CurrentDurability) && IsValidDurability(weapon.MaxDurability))
                    {
                        weaponList.Add(weapon);
                    }
                }
                else
                {
                    int attHeadIndex = weapon.ItemIndex + EffectOffset;
                    int attCount = BitConverter.ToInt32(br.Bytes, attHeadIndex);
                    int endIndex;
                    if (br.Bytes[attHeadIndex + 22 + attCount * 8] != 1)
                    {
                        endIndex = attHeadIndex + 22 + attCount * 8;
                    }
                    else
                    {
                        int nameLength = BitConverter.ToInt32(br.Bytes, attHeadIndex + 22 + attCount * 8 + 1);
                        endIndex = attHeadIndex + 22 + attCount * 8 + nameLength + 4;
                    }
                    weapon.ItemBytes = br.Bytes.AsSpan(weapon.ItemIndex, endIndex - weapon.ItemIndex + 1).ToArray();

                    if (IsValidDurability(weapon.CurrentDurability) && IsValidDurability(weapon.MaxDurability))
                    {
                        weaponList.Add(weapon);
                    }
                }
            }

            return weaponList;
        }

        /// <summary>
        /// Delete Equipment
        /// </summary>
        /// <param name="weapon"></param>
        public void DeleteWeapon(ItemMemoryInfo weapon)
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }

            weapon.ItemBytes = new byte[4];
            WriteWeaponByte(weapon);
        }

        /// <summary>
        /// Saveing Equipment
        /// </summary>
        /// <param name="weapon">Written Equipment</param>
        public void WriteWeaponByte(ItemMemoryInfo weapon)
        {
            if (br.Bytes == null)
            {
                throw new Exception("Save file not open.");
            }

            br.DeleteIntsByIndexAndLength(weapon.ItemIndex, weapon.NextItemIndex - weapon.ItemIndex);
            br.AddByIndex(weapon.ItemIndex, weapon.ItemBytes);
        }
    }
}
