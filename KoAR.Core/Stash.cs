using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KoAR.Core
{
    public class Stash
    {
        public int Offset { get; }
        
        public Stash()
        {
            ReadOnlySpan<byte> stashIndicator = new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 };
            Offset = Amalur.Bytes.AsSpan().IndexOf(stashIndicator) - 3;
        }

        public int DataLength
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, Offset);
            set
            {
                MemoryUtilities.Write(Amalur.Bytes, Offset, value);
                MemoryUtilities.Write(Amalur.Bytes, Offset + 9, value - 9);
            }
        }

        public int DataLength2
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, Offset + 9);
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, Offset + 13);
            set => MemoryUtilities.Write(Amalur.Bytes, Offset + 13, value);
        }

        public uint FirstItemTypeId
        {
            get => MemoryUtilities.Read<uint>(Amalur.Bytes, Offset + 17);
            set => MemoryUtilities.Write(Amalur.Bytes, Offset + 17, value);
        }

        public void AddItem(uint typeId)
        {
            ReadOnlySpan<byte> src = new byte[] { 0, 0, 0, 0, 0x0A, 0x03, 0, 0, 0, 0, 0, 0, 0x80, 0x3F, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xFF };
            Span<byte> temp = stackalloc byte[25];
            src.CopyTo(temp);
            MemoryUtilities.Write(temp, 0, typeId);
            Amalur.Bytes = MemoryUtilities.ReplaceBytes(Amalur.Bytes, Offset + 17, 0, temp);
            DataLength += 25;
            Count++;
        }

    }
}
