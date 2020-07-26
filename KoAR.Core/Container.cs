using System;
using System.Collections;
using System.Collections.Generic;

namespace KoAR.Core
{
    public readonly struct Container : IEnumerable<(int id, int offset, int dataLength)>
    {
        private static class Offsets
        {
            public const int DataLength = 5;
            public const int DataLength2 = 14;
            public const int FirstItem = 26;
            public const int Count = 18;
        }
        private readonly GameSave _gameSave;
        public int Offset { get; }
        private readonly ulong _pattern;

        public Container(GameSave gameSave, int offset, ulong pattern) => (_gameSave, Offset, _pattern) = (gameSave, offset, pattern);

        private int DataLength
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, Offset + Offsets.DataLength);
            set => MemoryUtilities.Write(_gameSave.Bytes, Offset + Offsets.DataLength, value);
        }

        private int DataLength2
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, Offset + Offsets.DataLength2);
            set => MemoryUtilities.Write(_gameSave.Bytes, Offset + Offsets.DataLength2, value);
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, Offset + Offsets.Count);
        }

        public void UpdateDataLength(int delta)
        {
            DataLength += delta;
            DataLength2 += delta;
        }

        public IEnumerator<(int id, int offset, int dataLength)> GetEnumerator()
        {
            var offset = Offset + Offsets.FirstItem;
            int id;
            int dataLength;
            while (true)
            {
                while ((id = BitConverter.ToInt32(_gameSave.Bytes, offset)) == 0)
                {
                    offset += 4;
                }
                if (BitConverter.ToUInt64(_gameSave.Bytes, offset + 4) != _pattern)
                {
                    break;
                }
                dataLength = 17 + BitConverter.ToInt32(_gameSave.Bytes, offset + 13);
                yield return (id, offset, dataLength);
                offset += dataLength;
            }
        }

        public Container UpdateOffset(int delta) => new Container(_gameSave, Offset, _pattern);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
