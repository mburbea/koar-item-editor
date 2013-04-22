using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ByteManager
{
    /// <summary>
    /// File Operationing in binary mode
    /// </summary>
    public class ByteEditer
    {
        byte[] btList;
        /// <summary>
        /// Constructor parameters (need to call Open to open the file)
        /// </summary>
        public ByteEditer()
        {
        }
        /// <summary>
        /// Create a known instance of a byte array
        /// </summary>
        /// <param name="btList"></param>
        public ByteEditer(byte[] btList)
        {
            this.btList = btList;
        }

        /// <summary>
        /// Binary array file
        /// </summary>
        public byte[] BtList
        {
            get { return btList; }
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="path">Absolute file-path</param>
        public void ReadFile(String path)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open);
                btList = new byte[fs.Length];
                fs.Read(btList, 0, (int)fs.Length);
            }
            catch
            {
                throw new Exception("File reading fails");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Save file
        /// </summary>
        /// <param name="path">Save the file's absolute path</param>
        public void SaveFile(String path)
        {
            if (btList == null)
            {
                throw new Exception("File not open");
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Create);
                fs.Write(btList, 0, btList.Length);
            }
            catch
            {
                throw new Exception("Archive saving failed");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Find the address where the first character in the file index (does not support Chinese, and special symbols may be more than one record)
        /// </summary>
        /// <param name="name">String name</param>
        /// <returns>Strings where binary array start index</returns>
        public List<int> FindIndexByString(String name)
        {
            if (btList == null)
            {
                throw new Exception("File not open");
            }
            if (name=="")
            {
                throw new Exception("Empty string is not allowed");
            }
            if (btList.Length < name.Length)
            {
                throw new Exception("File is too short");
            }
            
            List<int> indexList = new List<int>();
            int index = -1;
            for (int i = 0; i <= btList.Length-name.Length; i++)
            {
                for (int j = 0; j < name.Length; j++)
                {
                    if (btList[i + j] == name[j])
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
                if (index !=-1)
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
            if (btList == null)
            {
                throw new Exception("The file is not open");
            }
            if (bt ==null || bt.Length==0)
            {
                throw new Exception("Find an empty array is not allowed");
            }
            if (btList.Length < bt.Length)
            {
                throw new Exception("File is too short");
            }

            List<int> indexList = new List<int>();
            int index = -1;
            for (int i = 0; i <= btList.Length-bt.Length; i++)
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    if (btList[i + j] == bt[j])
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
        /// 查找int数组在文件中所在的地址索引(可能不止一条记录)
        /// </summary>
        /// <param name="it">要查找的int数组</param>
        /// <returns>int数组所在位置索引列表</returns>
        public List<int> FindIndexList(int[] it)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (it == null || it.Length == 0)
            {
                throw new Exception("不允许查找空数组");
            }
            if (btList.Length < it.Length)
            {
                throw new Exception("文件过短");
            }

            List<int> indexList = new List<int>();
            int index = -1;
            for (int i = 0; i <= btList.Length - it.Length; i++)
            {
                for (int j = 0; j < it.Length; j++)
                {
                    if (btList[i + j] == it[j])
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
        /// 从某地址开始查找int数组在文件中所在的第一个地址索引
        /// </summary>
        /// <param name="startIndex">开始地址</param>
        /// <param name="it">要查找的int数组</param>
        /// <returns>第一个索引(没有则返回-1)</returns>
        public int FindFirstIndex(int startIndex, int[] it)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (it == null || it.Length == 0)
            {
                throw new Exception("不允许查找空数组");
            }
            if (btList.Length < it.Length)
            {
                throw new Exception("文件过短");
            }

            int index = -1;
            for (int i = startIndex; i <= btList.Length - it.Length; i++)
            {
                for (int j = 0; j < it.Length; j++)
                {
                    if (btList[i + j] == it[j])
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
        /// 从某地址开始查找byte数组在文件中所在的第一个地址索引(查不到则返回-1)
        /// </summary>
        /// <param name="startIndex">开始地址</param>
        /// <param name="bt">要查找的byte数组</param>
        /// <returns>第一个索引(没有则返回-1)</returns>
        public int FindFirstIndex(int startIndex, byte[] bt)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (bt == null || bt.Length == 0)
            {
                throw new Exception("不允许查找空数组");
            }
            if (btList.Length < bt.Length)
            {
                throw new Exception("文件过短");
            }

            int index = -1;
            for (int i = startIndex; i <= btList.Length - bt.Length; i++)
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    if (btList[i + j] == bt[j])
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
        /// 获取从某索引开始某长度的字符串(只支持英文)
        /// </summary>
        /// <param name="index">索引开头</param>
        /// <param name="length">长度</param>
        /// <returns>字符串</returns>
        public String GetStringByIndexAndLength(int index, int length)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((length + index) > btList.Length)
            {
                throw new Exception("长度超出界限");
            }
            return new String(System.Text.Encoding.Default.GetChars(btList, index, length));
        }

        /// <summary>
        /// 获取从某索引开始某长度的int数组
        /// </summary>
        /// <param name="index">索引开头</param>
        /// <param name="length">长度</param>
        /// <returns>int数组</returns>
        public int[] GetIntsByIndexAndLength(int index, int length)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((length + index) > btList.Length)
            {
                throw new Exception("长度超出界限");
            }
            int[] it = new int[length];
            for (int i = 0; i < length; i++)
            {
                it[i] = btList[index + i];
            }
            return it;
        }

        /// <summary>
        /// 获取从某索引开始某长度的byte数组
        /// </summary>
        /// <param name="index">索引开头</param>
        /// <param name="length">长度</param>
        /// <returns>byte数组</returns>
        public byte[] GetBytsByIndexAndLength(int index, int length)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((length + index) > btList.Length)
            {
                throw new Exception("长度超出界限");
            }
            byte[] it = new byte[length];
            for (int i = 0; i < length; i++)
            {
                it[i] = btList[index + i];
            }
            return it;
        }

        /// <summary>
        /// 获取指定索引开始至少某长度的符合规范的字符串(如果字符串不止这些长度,则自动向后延长,规范:字符串不包含中文和特殊字符)
        /// </summary>
        /// <param name="index">指定索引开头</param>
        /// <param name="length">最小长度</param>
        /// <returns>字符串</returns>
        public String GetStringByIndexAndMinLength(int index, int length)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((length + index) > btList.Length)
            {
                throw new Exception("长度超出界限");
            }

            int addLength = 0;
            while (BtList[index + length + addLength] >= '0' && BtList[index + length + addLength] <= '9' || BtList[index + length + addLength] >= 'a' && BtList[index + length + addLength] <= 'z' || BtList[index + length + addLength] >= 'A' && BtList[index + length + addLength] <= 'Z' || BtList[index + length + addLength] == ' ' || BtList[index + length + addLength] == '_')
            {
                addLength++;
            }
            return new String(System.Text.Encoding.Default.GetChars(BtList, index, length + addLength));
        }

        /// <summary>
        /// 删除指定索引开始指定长度的数组
        /// </summary>
        /// <param name="index">要开始删除的索引</param>
        /// <param name="length">要删除的长度</param>
        public void DeleteIntsByIndexAndLength(int index, int length)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((length + index) > btList.Length)
            {
                throw new Exception("长度超出界限");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < btList.Length; i++)
            {
                if (i < index || i >= index + length)
                {
                    temp.Add(btList[i]);
                } 
            }
            btList = temp.ToArray();
        }

        /// <summary>
        /// 通过起始索引和终止索引删除数组中的某段数据(包括终止索引所在数据)
        /// </summary>
        /// <param name="start">起始索引</param>
        /// <param name="end">终止索引</param>
        public void DeleteIntsByStartAndEnd(int start,int end)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }else if (start < 0)
            {
                throw new Exception("起始索引无效");
            }
            else if (end<start)
            {
                throw new Exception("终止索引无效");
            }
            else if (end > btList.Length - 1)
            {
                throw new Exception("终止索引无效");
            }

            List<byte> temp = new List<byte>();
            for (int i = 0; i < btList.Length; i++)
            {
                if (i >= start && i<=end )
                {
                    continue;
                }
                temp.Add(btList[i]);
            }
            btList = temp.ToArray();
        }

        /// <summary>
        /// 修改指定索引开始的数据为新int数组
        /// </summary>
        /// <param name="index">要修改的起始索引</param>
        /// <param name="newInts">要修改的数值</param>
        public void EditByIndex(int index, int[] newInts)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((newInts.Length + index) > btList.Length)
            {
                throw new Exception("新数组长度超出界限");
            }
            for (int i = 0; i < newInts.Length; i++)
            {
                btList[index + i] = (byte)newInts[i];
            }
        }

        /// <summary>
        /// 修改指定索引开始的指定数组为新byte数组
        /// </summary>
        /// <param name="index">要修改的起始索引</param>
        /// <param name="newBytes">要修改的数值</param>
        public void EditByIndex(int index, byte[] newBytes)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            else if ((newBytes.Length + index) > btList.Length)
            {
                throw new Exception("新数组长度超出界限");
            }
            for (int i = 0; i < newBytes.Length; i++)
            {
                btList[index + i] = newBytes[i];
            }
        }

        /// <summary>
        /// 添加指定int数组到指定索引处
        /// </summary>
        /// <param name="index">要添加到的索引位置</param>
        /// <param name="newInts">int数组</param>
        public void AddByIndex(int index, int[] newInts)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < btList.Length; i++)
            {
                if (i == index)
                {
                    for (int j = 0; j < newInts.Length; j++)
                    {
                        temp.Add((byte)newInts[j]);
                    }
                }
                temp.Add(btList[i]);
            }
            btList = temp.ToArray();
        }

        /// <summary>
        /// 添加指定byte数组到指定索引处
        /// </summary>
        /// <param name="index">要添加到的索引位置</param>
        /// <param name="newBytes">byte数组</param>
        public void AddByIndex(int index, byte[] newBytes)
        {
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < btList.Length; i++)
            {
                if (i == index)
                {
                    for (int j = 0; j < newBytes.Length; j++)
                    {
                        temp.Add(newBytes[j]);
                    }
                }
                temp.Add(btList[i]);
            }
            btList = temp.ToArray();
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
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            if (index + length > btList.Length)
            {
                throw new Exception("长度超出界限");
            }
            if (length < ints.Length -1)
            {
                throw new Exception("新数组长度过长");
            }
            bool isInts=false;
            for (int i = index;i<= index + length- ints.Length; i++)
            {
                for (int j = 0; j < ints.Length; j++)
                {
                    if (btList[i + j] == ints[j])
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
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            if (index < 0)
            {
                throw new Exception("索引无效");
            }
            if (index + length > btList.Length)
            {
                throw new Exception("长度超出界限");
            }
            if (length < bytes.Length-1)
            {
                throw new Exception("新数组长度过长");
            }
            bool isInts = false;
            for (int i = index; i <= index + length-bytes.Length; i++)
            {
                for (int j = 0; j < bytes.Length; j++)
                {
                    if (btList[i + j] == bytes[j])
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
            if (btList == null)
            {
                throw new Exception("文件未打开");
            }
            List<byte> temp = new List<byte>(btList);
            temp.AddRange(bt);
            btList = temp.ToArray();
        }

        /// <summary>
        /// 从某索引开始删除到末尾
        /// </summary>
        /// <param name="index">开始索引</param>
        public void DeleteToEnd(int index)
        {
            List<byte> temp = new List<byte>(btList);
            temp.RemoveRange(index, temp.Count - index);
            btList = temp.ToArray();
        }
    }
}
