using ByteManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace KoARSaveItemEditor
{
    /// <summary>
    /// Equipment Memory Information
    /// </summary>
    public class ItemMemoryInfo
    {
        /// <summary>
        /// Equipment head Index(XX XX XX XX 0B 00 00 00 68 D5 24 00 03)
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        /// Next Equipment head Index
        /// </summary>
        public int NextItemIndex { get; set; }

        /// <summary>
        /// Equipment list of attributes of the head in the archive index (accounting for 4-byte, indicates that the equipment has a number of attributes)
        /// </summary>
        public int AttHeadIndex => ItemIndex + AmalurSaveEditor.ItemAttHeadOffSet;

        /// <summary>
        /// Equipment data
        /// </summary>
        public byte[] ItemBytes { get; set; }

        /// <summary>
        /// Equipment Name
        /// </summary>
        public string ItemName
        {
            get
            {
                if (ItemBytes[AmalurSaveEditor.ItemAttHeadOffSet + 22 + AttCount * 8] != 1)
                {
                    return "Unknown";
                }
                else
                {
                    int count = BitConverter.ToInt32(ItemBytes,AmalurSaveEditor.ItemAttHeadOffSet+22+AttCount*8+1);
                    return System.Text.Encoding.Default.GetString(ItemBytes, AmalurSaveEditor.ItemAttHeadOffSet + 27 + 8 * AttCount, count);
                }
            }
            set
            {
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                byteEditor.DeleteToEnd(AmalurSaveEditor.ItemAttHeadOffSet + 22 + AttCount * 8+1);
                if (value.Length != 0)
                {
                    byteEditor.EditByIndex(AmalurSaveEditor.ItemAttHeadOffSet + 22 + AttCount * 8, new byte[] { 1 });
                    byteEditor.AddToEnd(BitConverter.GetBytes(value.Length));
                    byte[] nameList = System.Text.Encoding.Default.GetBytes(value);
                    byteEditor.AddToEnd(nameList);
                }
                else
                {
                    byteEditor.EditByIndex(AmalurSaveEditor.ItemAttHeadOffSet + 22 + AttCount * 8, new byte[] { 0 });
                }
                ItemBytes = byteEditor.BtList;
            }
        }

        /// <summary>
        /// Number of Attributes
        /// </summary>
        public int AttCount => BitConverter.ToInt32(ItemBytes, AmalurSaveEditor.ItemAttHeadOffSet);

        /// <summary>
        /// Current Durability
        /// </summary>
        public float CurrentDurability
        {
            get => BitConverter.ToSingle(ItemBytes, AmalurSaveEditor.ItemAttHeadOffSet + 8 + 8 * AttCount);
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                byteEditor.EditByIndex(AmalurSaveEditor.ItemAttHeadOffSet + 8 + 8 * AttCount, bt);
                ItemBytes = byteEditor.BtList;
            }
        }

        /// <summary>
        /// Maximum durability
        /// </summary>
        public float MaxDurability
        {
            get => BitConverter.ToSingle(ItemBytes, AmalurSaveEditor.ItemAttHeadOffSet + 12 + 8 * AttCount);
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                byteEditor.EditByIndex(AmalurSaveEditor.ItemAttHeadOffSet + 12 + 8 * AttCount, bt);
                ItemBytes = byteEditor.BtList;
            }
        }

        /// <summary>
        /// list of attributes on equipment
        /// </summary>
        public List<AttributeMemoryInfo> ItemAttList
        {
            get
            {
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                List<AttributeMemoryInfo> attList = new List<AttributeMemoryInfo>();

                int attIndex = AmalurSaveEditor.ItemAttHeadOffSet + 4;
                for (int i = 0; i < AttCount; i++)
                {
                    AttributeMemoryInfo att = new AttributeMemoryInfo
                    {
                        Value = byteEditor.GetIntsByIndexAndLength(attIndex, 4)
                    };
                    string val1 = Convert.ToString(att.Value[2], 16).ToUpper();
                    string val2 = Convert.ToString(att.Value[1], 16).ToUpper();
                    string val3 = Convert.ToString(att.Value[0], 16).ToUpper();

                    att.Code = (val1.Length == 2 ? val1 : ("0" + val1)) + (val2.Length == 2 ? val2 : ("0" + val2)) + (val3.Length == 2 ? val3 : ("0" + val3));
                    attList.Add(att);

                    attIndex += 8;
                }
                return attList;
            }
            set
            {
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                byteEditor.DeleteIntsByIndexAndLength(AmalurSaveEditor.ItemAttHeadOffSet + 4, 8 * AttCount);
                byteEditor.EditByIndex(AmalurSaveEditor.ItemAttHeadOffSet, BitConverter.GetBytes(value.Count));
                
                foreach (AttributeMemoryInfo att in value)
                {
                    Span<uint> uints = stackalloc uint[2];
                    uints[0] = uint.Parse(att.Code, NumberStyles.HexNumber);
                    uints[1] = uint.MaxValue;
                    byte[] news = new byte[8];
                    byteEditor.AddByIndex(AmalurSaveEditor.ItemAttHeadOffSet + 4, MemoryMarshal.AsBytes(uints).ToArray());
                }

                ItemBytes = byteEditor.BtList;
            }
        }
    }
}
