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
            ReadOnlySpan<byte> stashIndicator = new byte[] { 0x00, 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 };
            Offset = Amalur.Bytes.AsSpan().IndexOf(stashIndicator) + 7;
        }

        private int Size
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, Offset);
            set => MemoryUtilities.Write(Amalur.Bytes, Offset, value);
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(Amalur.Bytes, Offset + 4);
            set => MemoryUtilities.Write(Amalur.Bytes, Offset + 4, value);
        }

        private uint NextItemType
        {
            get => MemoryUtilities.Read<uint>(Amalur.Bytes, Offset + 8);
        }

        public void AddItem(uint typeId)
        {
            Span<uint> buffer = stackalloc uint[7];
            buffer[0] = typeId;
            buffer[1] = 0x00_03_0A;
            buffer[3] = 0x01_41_E0;
            buffer[6] = NextItemType << 8 | 0xFF;
            Amalur.Bytes = MemoryUtilities.ReplaceBytes(Amalur.Bytes, Offset + 8, 3, MemoryMarshal.Cast<uint, byte>(buffer));
            Size += 25;
            Count++;
        }

    }
}
