using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ByteManager
{
    /// <summary>
    /// File Operationing in binary mode
    /// </summary>
    public class ByteEditor
    {
        /// <summary>
        /// Constructor parameters (need to call Open to open the file)
        /// </summary>
        public ByteEditor()
        {
        }
        /// <summary>
        /// Create a known instance of a byte array
        /// </summary>
        /// <param name="btList"></param>
        public ByteEditor(byte[] btList)
        {
            BtList = btList;
        }

        /// <summary>
        /// Binary array file
        /// </summary>
        public byte[] BtList { get; private set; }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="path">Absolute file-path</param>
        public void ReadFile(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    BtList = new byte[fs.Length];
                    fs.Read(BtList, 0, (int)fs.Length);
                }
            }
            catch
            {
                throw new Exception("File cannot open!");
            }
            
        }

        /// <summary>
        /// Save file
        /// </summary>
        /// <param name="path">Save the file's absolute path</param>
        public void SaveFile(string path)
        {
            if (BtList == null)
            {
                throw new Exception("File not open");
            }
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write(BtList, 0, BtList.Length);
                }
            }
            catch
            {
                throw new Exception("Archive saving failed");
            }
        }

        /// <summary>
        /// Find the address where the first character in the file index (does not support Chinese, and special symbols may be more than one record)
        /// </summary>
        /// <param name="name">String name</param>
        /// <returns>Strings where binary array start index</returns>
        public List<int> FindIndexByString(string name)
        {
            if (BtList == null)
            {
                throw new Exception("File not open");
            }
            if (name == "")
            {
                throw new Exception("Empty string is not allowed");
            }
            if (BtList.Length < name.Length)
            {
                throw new Exception("File is too short");
            }

            List<int> indexList = new List<int>();
            int index = -1;
            for (int i = 0; i <= BtList.Length - name.Length; i++)
            {
                for (int j = 0; j < name.Length; j++)
                {
                    if (BtList[i + j] == name[j])
                    {
                        index = i;
                        continue;
                    }
                    else
                    {
                        index = -1;
                        break;
                    }
                }
                if (index != -1)
                {
                    indexList.Add(index);
                }
                continue;
            }
            return indexList;
        }

        /// <summary>
        /// Find the address where the index by an array of byte in the file (possibly more than one record)
        /// </summary>
        /// <param name="bt">To find an array of byte</param>
        /// <returns>byte array where the position index list</returns>
        public List<int> FindIndexList(byte[] bt)
        {
            if (BtList == null)
            {
                throw new Exception("The file is not open");
            }
            if (bt ==null || bt.Length==0)
            {
                throw new Exception("Find an empty array is not allowed");
            }
            if (BtList.Length < bt.Length)
            {
                throw new Exception("File is too short");
            }

            List<int> indexList = new List<int>();
            int index = -1;
            for (int i = 0; i <= BtList.Length-bt.Length; i++)
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    if (BtList[i + j] == bt[j])
                    {
                        index = i;
                    }
                    else
                    {
                        index = -1;
                        break;
                    }
                }
                if (index != -1)
                {
                    indexList.Add(index);
                }
            }
            return indexList;
        }

        /// <summary>
        /// Find the the int array address in the file where the index (may be more than one record)
        /// </summary>
        /// <param name="it">To find an array of int</param>
        /// <returns>int Location of the array index list</returns>
        public List<int> FindIndexList(int[] it)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (it == null || it.Length == 0)
            {
                throw new Exception("Find an empty array is not allowed");
            }
            if (BtList.Length < it.Length)
            {
                throw new Exception("The file is too short");
            }

            List<int> indexList = new List<int>();
            int index = -1;
            for (int i = 0; i <= BtList.Length - it.Length; i++)
            {
                for (int j = 0; j < it.Length; j++)
                {
                    if (BtList[i + j] == it[j])
                    {
                        index = i;
                    }
                    else
                    {
                        index = -1;
                        break;
                    }
                }
                if (index != -1)
                {
                    indexList.Add(index);
                }
            }
            return indexList;
        }

        /// <summary>
        /// Find int array in the file where the first address index from an address
        /// </summary>
        /// <param name="startIndex">Start address</param>
        /// <param name="it">To find an array of int</param>
        /// <returns>First index -1 is returned (no)</returns>
        public int FindFirstIndex(int startIndex, int[] it)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (it == null || it.Length == 0)
            {
                throw new Exception("Find an empty array is not allowed");
            }
            if (BtList.Length < it.Length)
            {
                throw new Exception("The file is too short");
            }

            int index = -1;
            for (int i = startIndex; i <= BtList.Length - it.Length; i++)
            {
                for (int j = 0; j < it.Length; j++)
                {
                    if (BtList[i + j] == it[j])
                    {
                        index = i;
                    }
                    else
                    {
                        index = -1;
                        break;
                    }
                }
                if (index != -1)
                {
                    return index;
                }
            }
            return index;
        }

        /// <summary>
        /// Find an address byte array in the file where the first address index (finding out -1 is returned)
        /// </summary>
        /// <param name="startIndex">Start address</param>
        /// <param name="bt">To find the byte array</param>
        /// <returns>First index -1 is returned (no)</returns>
        public int FindFirstIndex(int startIndex, byte[] bt)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (bt == null || bt.Length == 0)
            {
                throw new Exception("Find an empty array is not allowed");
            }
            if (BtList.Length < bt.Length)
            {
                throw new Exception("The file is too short");
            }

            int index = -1;
            for (int i = startIndex; i <= BtList.Length - bt.Length; i++)
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    if (BtList[i + j] == bt[j])
                    {
                        index = i;
                    }
                    else
                    {
                        index = -1;
                        break;
                    }
                }
                if (index != -1)
                {
                    return index;
                }
            }
            return index;
        }

        /// <summary>
        /// Get a length of string from index (only supports English)
        /// </summary>
        /// <param name="index">The beginning of the index</param>
        /// <param name="length">长度</param>
        /// <returns>字符串</returns>
        public string GetStringByIndexAndLength(int index, int length)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((length + index) > BtList.Length)
            {
                throw new Exception("Length out of bounds");
            }
            return new string(System.Text.Encoding.Default.GetChars(BtList, index, length));
        }

        /// <summary>
        /// Get a length byte array starting from index
        /// </summary>
        /// <param name="index">The beginning of the index</param>
        /// <param name="length">Length</param>
        /// <returns>byte Array</returns>
        public byte[] GetBytesByIndexAndLength(int index, int length)
        {
            return BtList.AsSpan(index, length).ToArray();
        }

        /// <summary>
        /// Get a uint at this location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint GetUInt32ByIndexAndLength(int index)
        {
            return MemoryMarshal.Read<uint>(BtList.AsSpan(index, 4));
        }

        /// <summary>
        /// Get the specified index began at least a length compliant string (if the string is more than the length,
        /// automatic backward extension, specification: string does not contain Chinese and special characters)
        /// </summary>
        /// <param name="index">Starting with a specified index</param>
        /// <param name="length">Minimum length</param>
        /// <returns>String</returns>
        public string GetStringByIndexAndMinLength(int index, int length)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((length + index) > BtList.Length)
            {
                throw new Exception("Length out of bounds");
            }

            int addLength = 0;
            while (BtList[index + length + addLength] >= '0' && BtList[index + length + addLength] <= '9' || BtList[index + length + addLength] >= 'a' && BtList[index + length + addLength] <= 'z' || BtList[index + length + addLength] >= 'A' && BtList[index + length + addLength] <= 'Z' || BtList[index + length + addLength] == ' ' || BtList[index + length + addLength] == '_')
            {
                addLength++;
            }
            return new string(System.Text.Encoding.Default.GetChars(BtList, index, length + addLength));
        }

        /// <summary>
        /// 删除指定索引开始指定长度的数组
        /// </summary>
        /// <param name="index">要开始删除的索引</param>
        /// <param name="length">要删除的长度</param>
        public void DeleteIntsByIndexAndLength(int index, int length)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((length + index) > BtList.Length)
            {
                throw new Exception("Length out of bounds");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < BtList.Length; i++)
            {
                if (i < index || i >= index + length)
                {
                    temp.Add(BtList[i]);
                } 
            }
            BtList = temp.ToArray();
        }

        /// <summary>
        /// 通过起始索引和终止索引删除数组中的某段数据(包括终止索引所在数据)
        /// </summary>
        /// <param name="start">起始索引</param>
        /// <param name="end">终止索引</param>
        public void DeleteIntsByStartAndEnd(int start,int end)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }else if (start < 0)
            {
                throw new Exception("Starting Index is invalid");
            }
            else if (end<start)
            {
                throw new Exception("Termination Index is invalid");
            }
            else if (end > BtList.Length - 1)
            {
                throw new Exception("Termination Index is invalid");
            }

            List<byte> temp = new List<byte>();
            for (int i = 0; i < BtList.Length; i++)
            {
                if (i >= start && i<=end )
                {
                    continue;
                }
                temp.Add(BtList[i]);
            }
            BtList = temp.ToArray();
        }

        /// <summary>
        /// 修改指定索引开始的数据为新int数组
        /// </summary>
        /// <param name="index">要修改的起始索引</param>
        /// <param name="newInts">要修改的数值</param>
        public void EditByIndex(int index, int[] newInts)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((newInts.Length + index) > BtList.Length)
            {
                throw new Exception("Length of the new array out of bounds");
            }
            for (int i = 0; i < newInts.Length; i++)
            {
                BtList[index + i] = (byte)newInts[i];
            }
        }

        /// <summary>
        /// 修改指定索引开始的指定数组为新byte数组
        /// </summary>
        /// <param name="index">要修改的起始索引</param>
        /// <param name="newBytes">要修改的数值</param>
        public void EditByIndex(int index, byte[] newBytes)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((newBytes.Length + index) > BtList.Length)
            {
                throw new Exception("Length of the new array out of bounds");
            }
            for (int i = 0; i < newBytes.Length; i++)
            {
                BtList[index + i] = newBytes[i];
            }
        }

        /// <summary>
        /// 添加指定int数组到指定索引处
        /// </summary>
        /// <param name="index">要添加到的索引位置</param>
        /// <param name="newInts">int数组</param>
        public void AddByIndex(int index, int[] newInts)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < BtList.Length; i++)
            {
                if (i == index)
                {
                    for (int j = 0; j < newInts.Length; j++)
                    {
                        temp.Add((byte)newInts[j]);
                    }
                }
                temp.Add(BtList[i]);
            }
            BtList = temp.ToArray();
        }

        /// <summary>
        /// 添加指定byte数组到指定索引处
        /// </summary>
        /// <param name="index">要添加到的索引位置</param>
        /// <param name="newBytes">byte数组</param>
        public void AddByIndex(int index, byte[] newBytes)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < BtList.Length; i++)
            {
                if (i == index)
                {
                    for (int j = 0; j < newBytes.Length; j++)
                    {
                        temp.Add(newBytes[j]);
                    }
                }
                temp.Add(BtList[i]);
            }
            BtList = temp.ToArray();
        }

        /// <summary>
        /// 检查从某索引开始指定长度内是否存在某int数组
        /// </summary>
        /// <param name="ints">要查找的数组</param>
        /// <param name="index">要开始查找的索引</param>
        /// <param name="length">指定最大查找长度</param>
        /// <returns>是否存在某数组</returns>
        public bool HasIntsByIndexAndLength(int[] ints, int index, int length)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            if (index + length > BtList.Length)
            {
                throw new Exception("Length out of bounds");
            }
            if (length < ints.Length -1)
            {
                throw new Exception("The length of the new array is too long");
            }
            bool isInts=false;
            for (int i = index;i<= index + length- ints.Length; i++)
            {
                for (int j = 0; j < ints.Length; j++)
                {
                    if (BtList[i + j] == ints[j])
                    {
                        isInts = true;
                        continue;
                    }
                    else
                    {
                        isInts = false;
                        break;
                    }
                }
                if (isInts)
                {
                    isInts = true;
                    break;
                }
            }

            return isInts;
        }

        /// <summary>
        /// 检查从某索引开始指定长度内是否存在某byte数组
        /// </summary>
        /// <param name="bytes">要查找的数组</param>
        /// <param name="index">要开始查找的索引</param>
        /// <param name="length">指定最大查找长度</param>
        /// <returns>是否存在某数组</returns>
        public bool HasBytesByIndexAndLength(byte[] bytes, int index, int length)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            if (index + length > BtList.Length)
            {
                throw new Exception("Length out of bounds");
            }
            if (length < bytes.Length-1)
            {
                throw new Exception("The length of the new array is too long");
            }
            bool isInts = false;
            for (int i = index; i <= index + length-bytes.Length; i++)
            {
                for (int j = 0; j < bytes.Length; j++)
                {
                    if (BtList[i + j] == bytes[j])
                    {
                        isInts = true;
                        continue;
                    }
                    else
                    {
                        isInts = false;
                        break;
                    }
                }
                if (isInts)
                {
                    isInts = true;
                    break;
                }
            }

            return isInts;
        }

        /// <summary>
        /// 向byte数组末尾添加数组
        /// </summary>
        /// <param name="bt"></param>
        public void AddToEnd(byte[] bt)
        {
            if (BtList == null)
            {
                throw new Exception("File is not open");
            }
            List<byte> temp = new List<byte>(BtList);
            temp.AddRange(bt);
            BtList = temp.ToArray();
        }

        /// <summary>
        /// 从某索引开始删除到末尾
        /// </summary>
        /// <param name="index">开始索引</param>
        public void DeleteToEnd(int index)
        {
            List<byte> temp = new List<byte>(BtList);
            temp.RemoveRange(index, temp.Count - index);
            BtList = temp.ToArray();
        }
    }
}
