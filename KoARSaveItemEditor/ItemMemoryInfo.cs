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
                if (ItemBytes[AmalurSaveEditor.EffectOffset + 22 + EffectCount * 8] != 1)
                {
                    return "Unknown";
                }
                else
                {
                    int count = BitConverter.ToInt32(ItemBytes, AmalurSaveEditor.EffectOffset + 22 + EffectCount * 8 + 1);
                    return System.Text.Encoding.Default.GetString(ItemBytes, AmalurSaveEditor.EffectOffset + 27 + 8 * EffectCount, count);
                }
            }
            set
            {
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                byteEditor.DeleteToEnd(AmalurSaveEditor.EffectOffset + 22 + EffectCount * 8 + 1);
                if (value.Length != 0)
                {
                    byteEditor.EditByIndex(AmalurSaveEditor.EffectOffset + 22 + EffectCount * 8, new byte[] { 1 });
                    byteEditor.AddToEnd(BitConverter.GetBytes(value.Length));
                    byte[] nameList = System.Text.Encoding.Default.GetBytes(value);
                    byteEditor.AddToEnd(nameList);
                }
                else
                {
                    byteEditor.EditByIndex(AmalurSaveEditor.EffectOffset + 22 + EffectCount * 8, new byte[] { 0 });
                }
                ItemBytes = byteEditor.Bytes;
            }
        }

        /// <summary>
        /// Number of Effects
        /// </summary>
        public int EffectCount => BitConverter.ToInt32(ItemBytes, AmalurSaveEditor.EffectOffset);

        /// <summary>
        /// Current Durability
        /// </summary>
        public float CurrentDurability
        {
            get => BitConverter.ToSingle(ItemBytes, AmalurSaveEditor.EffectOffset + 8 + (8 * EffectCount));
            set => MemoryMarshal.Write(ItemBytes.AsSpan(AmalurSaveEditor.EffectOffset + 8 + (8 * EffectCount)), ref value);
        }

        /// <summary>
        /// Maximum durability
        /// </summary>
        public float MaxDurability
        {
            get => BitConverter.ToSingle(ItemBytes, AmalurSaveEditor.EffectOffset + 12 + 8 * EffectCount);
            set => MemoryMarshal.Write(ItemBytes.AsSpan(AmalurSaveEditor.EffectOffset + 12 + 8 * EffectCount), ref value);
        }

        public bool Unsellable
        {
            get => (ItemBytes[AmalurSaveEditor.EffectOffset + 20 + 8 * EffectCount] & 0x80) == 0x80;
            set
            {
                if (value)
                {
                    ItemBytes[AmalurSaveEditor.EffectOffset + 20 + 8 * EffectCount] |= 0x80;
                }
                else
                {
                    ItemBytes[AmalurSaveEditor.EffectOffset + 20 + 8 * EffectCount] &= 0x7F;
                }
            }

        }


        /// <summary>
        /// list of attributes on equipment
        /// </summary>
        public List<EffectInfo> ItemAttList
        {
            get
            {
                List<EffectInfo> attList = new List<EffectInfo>();

                int attIndex = AmalurSaveEditor.EffectOffset + 4;
                for (int i = 0; i < EffectCount; i++)
                {
                    EffectInfo att = new EffectInfo
                    {
                        Code = BitConverter.ToUInt32(ItemBytes, attIndex).ToString("X6")
                    };

                    attList.Add(att);

                    attIndex += 8;
                }
                return attList;
            }
            set
            {
                ByteEditor byteEditor = new ByteEditor(ItemBytes);
                byteEditor.DeleteIntsByIndexAndLength(AmalurSaveEditor.EffectOffset + 4, 8 * EffectCount);
                byteEditor.EditByIndex(AmalurSaveEditor.EffectOffset, BitConverter.GetBytes(value.Count));

                foreach (EffectInfo att in value)
                {
                    Span<uint> uints = stackalloc uint[2];
                    uints[0] = uint.Parse(att.Code, NumberStyles.HexNumber);
                    uints[1] = uint.MaxValue;
                    byteEditor.AddByIndex(AmalurSaveEditor.EffectOffset + 4, MemoryMarshal.AsBytes(uints).ToArray());
                }

                ItemBytes = byteEditor.Bytes;
            }
        }
    }
}
