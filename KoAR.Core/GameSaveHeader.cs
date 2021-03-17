using System;

namespace KoAR.Core
{
    public class GameSaveHeader
    {
        private const int RemasterHeaderLength = 6 * 1024 - 8;
        private readonly int _dataLengthOffset;
        private readonly GameSave _gameSave;

        public GameSaveHeader(GameSave gameSave)
        {
            ReadOnlySpan<byte> endOfStructMarker = new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 };
            _gameSave = gameSave;
            var span = gameSave.Bytes.AsSpan();
            var ix = span.IndexOf(endOfStructMarker);
            
            while(gameSave.IsRemaster && MemoryUtilities.Read<int>(_gameSave.Bytes, ix + 8) != 0)
            {
                span = gameSave.Bytes.AsSpan(ix + 8);
                ix += 8 + span.IndexOf(new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 });
            }
            _dataLengthOffset = ix - 4;
        }

        public int BodyDataLength
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, _dataLengthOffset);
            set => MemoryUtilities.Write(_gameSave.Bytes, _dataLengthOffset, value);
        }

        public int Length => _gameSave.IsRemaster ? RemasterHeaderLength : _dataLengthOffset + 12;
    }
}
