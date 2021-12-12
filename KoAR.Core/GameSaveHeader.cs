using System;
using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class GameSaveHeader
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
        get => BitConverter.ToInt32(_gameSave.Bytes, _dataLengthOffset);
        set => Unsafe.WriteUnaligned(ref _gameSave.Bytes[_dataLengthOffset], value);
    }

    public int Length => _gameSave.IsRemaster ? RemasterHeaderLength : _dataLengthOffset + 12;
}
