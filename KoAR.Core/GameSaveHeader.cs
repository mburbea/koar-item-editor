using System;

namespace KoAR.Core
{
    public class GameSaveHeader
    {
        private const int RemasterHeaderSize = 6 * 1024 - 8;
        public byte[] Bytes { get; }
        private int _dataLengthOffset { get; }

        public int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, _dataLengthOffset);
            set => MemoryUtilities.Write(Bytes, _dataLengthOffset, value);
        }

        public GameSaveHeader(GameSave save)
        {
            var data = save.Bytes.AsSpan(8);
            _dataLengthOffset = data.IndexOf(new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 }) - 4;
            Bytes = save.IsRemaster ?
                data.Slice(0, RemasterHeaderSize).ToArray()
                : data.Slice(0, _dataLengthOffset + 12).ToArray();
        }
    }
}
