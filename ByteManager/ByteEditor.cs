using System;
using System.Collections.Generic;
using System.IO;

namespace ByteManager
{
    /// <summary>
    /// File Operationing in binary mode
    /// </summary>
    public class ByteEditor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ByteEditor(byte[] bytes = null)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Binary array file
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="path">Absolute file-path</param>
        public void ReadFile(string path)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.Open);
                Bytes = new byte[fs.Length];
                fs.Read(Bytes, 0, (int)fs.Length);
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
            if (Bytes == null)
            {
                throw new Exception("File not open");
            }
            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                fs.Write(Bytes, 0, Bytes.Length);
            }
            catch
            {
                throw new Exception("Archive saving failed");
            }
        }

        /// <summary>
        /// Get all occurences of a byte sequence.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public List<int> GetAllIndices(ReadOnlySpan<byte> bytes)
        {
            ReadOnlySpan<byte> data = Bytes;
            var results = new List<int>();
            int ix = data.IndexOf(bytes);
            int start = 0;
            while(ix != -1)
            {
                results.Add(start + ix);
                start += ix + bytes.Length;
                ix = data.Slice(start).IndexOf(bytes);
            }
            return results;
        }

        /// <summary>
        /// Get a length byte array starting from index
        /// </summary>
        /// <param name="index">The beginning of the index</param>
        /// <param name="length">Length</param>
        /// <returns>byte Array</returns>
        public byte[] GetBytesByIndexAndLength(int index, int length)
        {
            return Bytes.AsSpan(index, length).ToArray();
        }

        /// <summary>
        /// Get a uint at this location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint GetUInt32ByIndex(int index)
        {
            return BitConverter.ToUInt32(Bytes, index);
        }

        /// <summary>
        /// 删除指定索引开始指定长度的数组
        /// </summary>
        /// <param name="index">要开始删除的索引</param>
        /// <param name="length">要删除的长度</param>
        public void DeleteIntsByIndexAndLength(int index, int length)
        {
            if (Bytes == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((length + index) > Bytes.Length)
            {
                throw new Exception("Length out of bounds");
            }
            List<byte> temp = new List<byte>();
            for (int i = 0; i < Bytes.Length; i++)
            {
                if (i < index || i >= index + length)
                {
                    temp.Add(Bytes[i]);
                }
            }
            Bytes = temp.ToArray();
        }

        /// <summary>
        /// 通过起始索引和终止索引删除数组中的某段数据(包括终止索引所在数据)
        /// </summary>
        /// <param name="start">起始索引</param>
        /// <param name="end">终止索引</param>
        public void DeleteIntsByStartAndEnd(int start, int end)
        {
            if (Bytes == null)
            {
                throw new Exception("File is not open");
            }
            else if (start < 0)
            {
                throw new Exception("Starting Index is invalid");
            }
            else if (end < start)
            {
                throw new Exception("Termination Index is invalid");
            }
            else if (end > Bytes.Length - 1)
            {
                throw new Exception("Termination Index is invalid");
            }


            List<byte> temp = new List<byte>();
            for (int i = 0; i < Bytes.Length; i++)
            {
                if (i >= start && i <= end)
                {
                    continue;
                }
                temp.Add(Bytes[i]);
            }
            Bytes = temp.ToArray();
        }

        /// <summary>
        /// 修改指定索引开始的指定数组为新byte数组
        /// </summary>
        /// <param name="index">要修改的起始索引</param>
        /// <param name="newBytes">要修改的数值</param>
        public void EditByIndex(int index, byte[] newBytes)
        {
            if (Bytes == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            else if ((newBytes.Length + index) > Bytes.Length)
            {
                throw new Exception("Length of the new array out of bounds");
            }

            newBytes.CopyTo(Bytes.AsSpan(index, newBytes.Length));
        }

        /// <summary>
        /// 添加指定byte数组到指定索引处
        /// </summary>
        /// <param name="index">要添加到的索引位置</param>
        /// <param name="newBytes">byte数组</param>
        public void AddByIndex(int index, byte[] newBytes)
        {
            if (Bytes == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            var buffer = new byte[Bytes.Length + newBytes.Length];
            Bytes.AsSpan(0, index).CopyTo(buffer);
            newBytes.CopyTo(buffer, index);
            Bytes.AsSpan(index, Bytes.Length - index).CopyTo(buffer.AsSpan(index + newBytes.Length));
            List<byte> temp = new List<byte>();
            for (int i = 0; i < Bytes.Length; i++)
            {
                if (i == index)
                {
                    for (int j = 0; j < newBytes.Length; j++)
                    {
                        temp.Add(newBytes[j]);
                    }
                }
                temp.Add(Bytes[i]);
            }
            for(int i=0; i < temp.Count; i++)
            {
                if(temp[i] != buffer[i])
                {
                    throw null;
                }
            }
            Bytes = buffer;
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
            if (Bytes == null)
            {
                throw new Exception("File is not open");
            }
            if (index < 0)
            {
                throw new Exception("Index is invalid");
            }
            if (index + length > Bytes.Length)
            {
                throw new Exception("Length out of bounds");
            }
            if (length < bytes.Length - 1)
            {
                throw new Exception("The length of the new array is too long");
            }

            return Bytes.AsSpan(index, length).IndexOf(bytes) != -1;
        }

        /// <summary>
        /// 向byte数组末尾添加数组
        /// </summary>
        /// <param name="bt"></param>
        public void AddToEnd(byte[] bt)
        {
            if (Bytes == null)
            {
                throw new Exception("File is not open");
            }

            byte[] array = new byte[Bytes.Length + bt.Length];
            Bytes.CopyTo(array, 0);
            bt.CopyTo(array, Bytes.Length);
            Bytes = array;
        }

        /// <summary>
        /// 从某索引开始删除到末尾
        /// </summary>
        /// <param name="index">开始索引</param>
        public void DeleteToEnd(int index)
        {
            Bytes = Bytes.AsSpan(0, index).ToArray();
        }
    }
}
