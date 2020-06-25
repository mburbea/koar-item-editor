using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KoAR.Core
{
    public readonly struct Container
    {
        private readonly int _offset;

        public Container(int offset) => _offset = offset;


        public void UpdateDataLength(int delta)
        {
            MemoryUtilities.Write(Amalur.Bytes, _offset + 5, MemoryUtilities.Read<int>(Amalur.Bytes, _offset + 5) + delta);
            MemoryUtilities.Write(Amalur.Bytes, _offset + 13, MemoryUtilities.Read<int>(Amalur.Bytes, _offset + 13) + delta);
        }

        public int Count => BitConverter.ToInt32(Amalur.Bytes, _offset + 18);
        public int FirstItemOffset => _offset + 26;
    }
}
