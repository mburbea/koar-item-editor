using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core;

public sealed class GameSaveHeader
{
    private const int RemasterHeaderLength = 6 * 1024 - 8;
    private const int FateswornPackageId = 12;
    private readonly int _dataLengthOffset;
    private readonly GameSave _gameSave;

    public GameSaveHeader(GameSave gameSave)
    {
        _gameSave = gameSave;
        _dataLengthOffset = gameSave.Bytes.AsSpan().IndexOf(gameSave.IsRemaster
          ? new byte[12] { 0, 0, 0, 0, 0xA, 0, 0, 0, 0, 0, 0, 0}
          : new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 }) - 4;
        if(gameSave.IsRemaster)
        {
            var packagesList = gameSave.Bytes.AsSpan(gameSave.Bytes.AsSpan().IndexOf(new byte[8] { 0, 0, 0, 1, 0, 0, 0, 2 }) -1);
            IsFateswornAware = MemoryMarshal.Cast<byte, int>(packagesList.Slice(4, 4 * BitConverter.ToInt32(packagesList)))
                .Contains(FateswornPackageId);
        }
    }

    public int BodyDataLength
    {
        get => BitConverter.ToInt32(_gameSave.Bytes, _dataLengthOffset);
        set => Unsafe.WriteUnaligned(ref _gameSave.Bytes[_dataLengthOffset], value);
    }

    public int Length => _gameSave.IsRemaster ? RemasterHeaderLength : _dataLengthOffset + 12;

    public bool IsFateswornAware { get; }
}
