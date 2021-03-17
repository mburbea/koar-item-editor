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
            _gameSave = gameSave;
            _dataLengthOffset = gameSave.Bytes.AsSpan().IndexOf(gameSave.IsRemaster
              ? new byte[16] { 0, 0, 0, 0, 0xA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
              : new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 }) - 4;
        }

        public int BodyDataLength
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, _dataLengthOffset);
            set => MemoryUtilities.Write(_gameSave.Bytes, _dataLengthOffset, value);
        }

        public int Length => _gameSave.IsRemaster ? RemasterHeaderLength : _dataLengthOffset + 12;
    }
}
