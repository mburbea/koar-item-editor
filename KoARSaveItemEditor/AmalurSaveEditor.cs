using System;
using System.Collections.Generic;
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
        public static int WeaponAttHeadOffSet = 21;
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
            if (br.BtList == null)
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

        /// <summary>
        /// 获取背包容量上限
        /// </summary>
        /// <returns></returns>
        public int GetMaxBagCount()
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }
            int index = br.FindIndexByString("current_inventory_count")[0] + 35;
            byte[] bt = br.GetBytesByIndexAndLength(index, 4);
            return BitConverter.ToInt32(bt,0);
        }

        /// <summary>
        /// Edit maximum Backpack capacity
        /// </summary>
        /// <param name="c"></param>
        public void EditMaxBagCount(int c)
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }
            int index = br.FindIndexByString("current_inventory_count")[0] + 35;
            byte[] bt = BitConverter.GetBytes(c);
            br.EditByIndex(index, bt);
        }

        /// <summary>
        /// List of Attributes on Equipment(including descriptions)
        /// </summary>
        /// <param name="weaponInfo">Equipment Object</param>
        /// <param name="attInfoList">Description of Properties</param>
        /// <returns>List of Attributes</returns>
        public List<AttributeMemoryInfo> GetAttList(WeaponMemoryInfo weaponInfo, List<AttributeInfo> attInfoList)
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }

            List<AttributeMemoryInfo> attList = weaponInfo.WeaponAttList;
            foreach(AttributeMemoryInfo attInfo in attList)
            {
                String text = "";
                foreach (AttributeInfo att in attInfoList)
                {
                    if (att.AttributeId == attInfo.Code)
                    {
                        text = att.AttributeText;
                    }
                }
                if (text == "")
                {
                    text = "Unknown";
                }
                attInfo.Detail = text;
            }
            return attList;
        }

        /// <summary>
        /// Check if equipment is Equipable
        /// </summary>
        /// <param name="weapon">Equipment Object</param>
        /// <returns></returns>
        public bool IsWeapon(WeaponMemoryInfo weapon)
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }

            byte[] bytes = new byte[9];
            bytes[0] = 11;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 0;
            bytes[4] = 104;
            bytes[5] = 213;
            bytes[6] = 36;
            bytes[7] = 0;
            bytes[8] = 3;
            try
            {
                return br.HasBytesByIndexAndLength(bytes, weapon.WeaponIndex+4, 17);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all Equipment
        /// </summary>
        /// <returns></returns>
        public List<WeaponMemoryInfo> GetAllWeapon()
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }

            List<WeaponMemoryInfo> weaponList = new List<WeaponMemoryInfo>();

            byte[] bytes = new byte[9];
            bytes[0] = 11;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 0;
            bytes[4] = 104;
            bytes[5] = 213;
            bytes[6] = 36;
            bytes[7] = 0;
            bytes[8] = 3;
            List<int> indexList = br.FindIndexList(bytes);
            
            for (int i = 0; i < indexList.Count; i++)
            {
                indexList[i] -=4;
            }

            for(int i=0;i<indexList.Count;i++)
            {
                if(i!=indexList.Count-1)
                {
                    if (indexList[i + 1] - indexList[i] < 44)
                    {
                        continue;
                    }
                }

                WeaponMemoryInfo weapon = new WeaponMemoryInfo();
                weapon.WeaponIndex = indexList[i];
                if (i != indexList.Count - 1)
                {
                    weapon.NextWeaponIndex = indexList[i + 1];
                    weapon.WeaponBytes = br.GetBytesByIndexAndLength(indexList[i], indexList[i + 1] - indexList[i]);

                    if (weapon.CurrentDurability != 100 && weapon.MaxDurability != -1 && weapon.MaxDurability != 100 && weapon.CurrentDurability != 0 && weapon.MaxDurability != 0)
                    {
                        weaponList.Add(weapon);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    int attHeadIndex = weapon.WeaponIndex + AmalurSaveEditor.WeaponAttHeadOffSet;
                    int attCount = BitConverter.ToInt32(br.BtList,attHeadIndex);
                    int endIndex = 0;
                    if (br.BtList[attHeadIndex + 22 + attCount * 8] != 1)
                    {
                        endIndex = attHeadIndex + 22 + attCount * 8;
                    }
                    else
                    {
                        int nameLength = 0;
                        nameLength = BitConverter.ToInt32(br.BtList, attHeadIndex + 22 + attCount * 8 + 1);
                        endIndex = attHeadIndex + 22 + attCount * 8 + nameLength + 4;
                    }
                    weapon.WeaponBytes = br.GetBytesByIndexAndLength(weapon.WeaponIndex, endIndex - weapon.WeaponIndex+1);
                    if (weapon.CurrentDurability != 100 && weapon.MaxDurability != -1 && weapon.MaxDurability != 100 && weapon.CurrentDurability != 0 && weapon.MaxDurability != 0)
                    {
                        weaponList.Add(weapon);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return weaponList;
        }

        /// <summary>
        /// Delete Equipment
        /// </summary>
        /// <param name="weapon"></param>
        public void DeleteWeapon(WeaponMemoryInfo weapon)
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }

            weapon.WeaponBytes = new byte[] {0,0,0,0 };
            WriteWeaponByte(weapon);
        }

        /// <summary>
        /// Saveing Equipment
        /// </summary>
        /// <param name="weapon">Written Equipment</param>
        public void WriteWeaponByte(WeaponMemoryInfo weapon)
        {
            if (br.BtList == null)
            {
                throw new Exception("Save file not open.");
            }

            br.DeleteIntsByStartAndEnd(weapon.WeaponIndex, weapon.NextWeaponIndex - 1);
            br.AddByIndex(weapon.WeaponIndex, weapon.WeaponBytes);
        }
    }
}
