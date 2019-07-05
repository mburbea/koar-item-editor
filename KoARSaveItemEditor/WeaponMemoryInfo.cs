using System;
using System.Collections.Generic;

namespace KoARSaveItemEditor
{
    /// <summary>
    /// Equipment Memory Information
    /// </summary>
    public class WeaponMemoryInfo
    {
        private int weaponIndex;
        /// <summary>
        /// Equipment head Index(XX XX XX XX 0B 00 00 00 68 D5 24 00 03)
        /// </summary>
        public int WeaponIndex
        {
            get { return weaponIndex; }
            set { weaponIndex = value; }
        }

        private int nextWeaponIndex;
        /// <summary>
        /// Next Equipment head Index
        /// </summary>
        public int NextWeaponIndex
        {
            get { return nextWeaponIndex; }
            set { nextWeaponIndex = value; }
        }

        /// <summary>
        /// Equipment list of attributes of the head in the archive index (accounting for 4-byte, indicates that the equipment has a number of attributes)
        /// </summary>
        public int AttHeadIndex
        {
            get { return weaponIndex + AmalurSaveEditer.WeaponAttHeadOffSet; }
        }

        private byte[] weaponBytes;
        /// <summary>
        /// Equipment data
        /// </summary>
        public byte[] WeaponBytes
        {
            get { return weaponBytes; }
            set { weaponBytes = value; }
        }

        /// <summary>
        /// Equipment Name
        /// </summary>
        public String WeaponName
        {
            get
            {
                if (weaponBytes[AmalurSaveEditer.WeaponAttHeadOffSet + 22 + AttCount * 8] != 1)
                {
                    return "unkown";
                }
                else
                {
                    int count = BitConverter.ToInt32(weaponBytes,AmalurSaveEditer.WeaponAttHeadOffSet+22+AttCount*8+1);
                    return System.Text.Encoding.Default.GetString(weaponBytes, AmalurSaveEditer.WeaponAttHeadOffSet + 27 + 8 * AttCount, count);
                }
            }
            set
            {
                ByteManager.ByteEditor byteEditor = new ByteManager.ByteEditor(weaponBytes);
                byteEditor.DeleteToEnd(AmalurSaveEditer.WeaponAttHeadOffSet + 22 + AttCount * 8+1);
                if (value.Length != 0)
                {
                    byteEditor.EditByIndex(AmalurSaveEditer.WeaponAttHeadOffSet + 22 + AttCount * 8, new byte[] { 1 });
                    byteEditor.AddToEnd(BitConverter.GetBytes(value.Length));
                    byte[] nameList = System.Text.Encoding.Default.GetBytes(value);
                    byteEditor.AddToEnd(nameList);
                }
                else
                {
                    byteEditor.EditByIndex(AmalurSaveEditer.WeaponAttHeadOffSet + 22 + AttCount * 8, new byte[] { 0 });
                }
                weaponBytes = byteEditor.BtList;
            }
        }

        /// <summary>
        /// Number of Attributes
        /// </summary>
        public int AttCount
        {
            get { return BitConverter.ToInt32(weaponBytes, AmalurSaveEditer.WeaponAttHeadOffSet); }
        }

        /// <summary>
        /// Current Durability
        /// </summary>
        public float CurrentDurability
        {
            get { return BitConverter.ToSingle(weaponBytes, AmalurSaveEditer.WeaponAttHeadOffSet + 8 + 8 * AttCount); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                ByteManager.ByteEditor byteEditor = new ByteManager.ByteEditor(weaponBytes);
                byteEditor.EditByIndex(AmalurSaveEditer.WeaponAttHeadOffSet + 8 + 8 * AttCount, bt);
                weaponBytes = byteEditor.BtList;
            }
        }

        /// <summary>
        /// Maximum durability
        /// </summary>
        public float MaxDurability
        {
            get { return BitConverter.ToSingle(weaponBytes, AmalurSaveEditer.WeaponAttHeadOffSet + 12 + 8 * AttCount); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                ByteManager.ByteEditor byteEditor = new ByteManager.ByteEditor(weaponBytes);
                byteEditor.EditByIndex(AmalurSaveEditer.WeaponAttHeadOffSet + 12 + 8 * AttCount, bt);
                weaponBytes = byteEditor.BtList;
            }
        }

        /// <summary>
        /// list of attributes on equipment
        /// </summary>
        public List<AttributeMemoryInfo> WeaponAttList
        {
            get
            {
                ByteManager.ByteEditor byteEditor = new ByteManager.ByteEditor(weaponBytes);
                List<AttributeMemoryInfo> attList = new List<AttributeMemoryInfo>();

                int attIndex = AmalurSaveEditer.WeaponAttHeadOffSet + 4;
                for (int i = 0; i < AttCount; i++)
                {
                    AttributeMemoryInfo att = new AttributeMemoryInfo();

                    att.Value = byteEditor.GetIntsByIndexAndLength(attIndex, 4);
                    String val1 = Convert.ToString(att.Value[2], 16).ToUpper();
                    String val2 = Convert.ToString(att.Value[1], 16).ToUpper();
                    String val3 = Convert.ToString(att.Value[0], 16).ToUpper();

                    att.Code = (val1.Length == 2 ? val1 : ("0" + val1)) + (val2.Length == 2 ? val2 : ("0" + val2)) + (val3.Length == 2 ? val3 : ("0" + val3));
                    attList.Add(att);

                    attIndex += 8;
                }
                return attList;
            }
            set
            {
                ByteManager.ByteEditor byteEditor = new ByteManager.ByteEditor(weaponBytes);
                byteEditor.DeleteIntsByIndexAndLength(AmalurSaveEditer.WeaponAttHeadOffSet + 4, 8 * AttCount);
                byteEditor.EditByIndex(AmalurSaveEditer.WeaponAttHeadOffSet, BitConverter.GetBytes(value.Count));
                foreach (AttributeMemoryInfo att in value)
                {
                    byte[] news = new byte[8];
                    news[0] = byte.Parse(att.Code.ToUpper().Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    news[1] = byte.Parse(att.Code.ToUpper().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    news[2] = byte.Parse(att.Code.ToUpper().Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    news[3] = 0;
                    news[4] = 255;
                    news[5] = 255;
                    news[6] = 255;
                    news[7] = 255;
                    byteEditor.AddByIndex(AmalurSaveEditer.WeaponAttHeadOffSet + 4, news);
                }
                weaponBytes = byteEditor.BtList;
            }
        }
    }
}
