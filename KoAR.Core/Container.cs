﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KoAR.Core
{
    public readonly struct Container : IEnumerable<(int id, int offset, int datalength)>
    {
        private static class Offsets
        {
            public const int DataLength = 5;
            public const int DataLength2 = 14;
            public const int FirstItem = 26;
            public const int Count = 18;
        }
        private readonly int _offset;
        private readonly ulong _pattern;

        public Container(int offset, ulong pattern) => (_offset, _pattern) = (offset, pattern);

        private int DataLength
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, _offset + Offsets.DataLength);
            set => MemoryUtilities.Write(Amalur.Bytes, _offset + Offsets.DataLength, value);
        }

        private int DataLength2
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, _offset + Offsets.DataLength2);
            set => MemoryUtilities.Write(Amalur.Bytes, _offset + Offsets.DataLength2, value);
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, _offset + Offsets.Count);
        }


        public void UpdateDataLength(int delta)
        {
            DataLength += delta;
            DataLength2 += delta;
        }

        public IEnumerator<(int id, int offset, int datalength)> GetEnumerator()
        {
            var offset = _offset + Offsets.FirstItem;
            int id;
            int datalength;
            while (true)
            {
                while ((id = BitConverter.ToInt32(Amalur.Bytes, offset)) == 0)
                {
                    offset += 4;
                }
                if (BitConverter.ToUInt64(Amalur.Bytes, offset + 4) != _pattern)
                {
                    break;
                }
                datalength = 17 + BitConverter.ToInt32(Amalur.Bytes, offset + 13);
                yield return (id, offset, datalength);
                offset += datalength;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
