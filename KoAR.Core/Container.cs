using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace KoAR.Core;

internal sealed class Container : IEnumerable<(int id, int offset, int dataLength)>
{
    private static class Offsets
    {
        public const int DataLength = 5;
        public const int DataLength2 = 14;
        public const int FirstItem = 26;
        public const int Count = 18;
    }

    private readonly GameSave _gameSave;
    public int Offset { get; set; }
    private readonly ulong _pattern;

    public Container(GameSave gameSave, int offset, ulong pattern) => (_gameSave, Offset, _pattern) = (gameSave, offset, pattern);

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The suggested fix does not compile.")]
    private int DataLength
    {
        get => BitConverter.ToInt32(_gameSave.Body, Offset + Offsets.DataLength);
        set => Unsafe.WriteUnaligned(ref _gameSave.Body[Offset + Offsets.DataLength], value);
    }

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The suggested fix does not compile.")]
    private int DataLength2
    {
        get => BitConverter.ToInt32(_gameSave.Body, Offset + Offsets.DataLength2);
        set => Unsafe.WriteUnaligned(ref _gameSave.Body[Offset + Offsets.DataLength2], value);
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Useful for debugging")]
    private int Count => BitConverter.ToInt32(_gameSave.Body, Offset + Offsets.Count);

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
            while ((id = BitConverter.ToInt32(_gameSave.Body, offset)) == 0)
            {
                offset += 4;
            }
            if (BitConverter.ToUInt64(_gameSave.Body, offset + 4) != _pattern)
            {
                break;
            }
            dataLength = 17 + BitConverter.ToInt32(_gameSave.Body, offset + 13);
            yield return (id, offset, dataLength);
            offset += dataLength;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
