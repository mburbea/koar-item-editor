using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace KoAR.Core;

internal sealed class Container(GameSave gameSave, int offset, ulong pattern) : IEnumerable<(int id, int offset, int dataLength)>
{
    private static class Offsets
    {
        public const int DataLength = 5;
        public const int DataLength2 = 14;
        public const int FirstItem = 26;
        public const int Count = 18;
    }

    public int Offset { get; set; } = offset;

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The suggested fix does not compile.")]
    private int DataLength
    {
        get => BitConverter.ToInt32(gameSave.Body, Offset + Offsets.DataLength);
        set => Unsafe.WriteUnaligned(ref gameSave.Body[Offset + Offsets.DataLength], value);
    }

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The suggested fix does not compile.")]
    private int DataLength2
    {
        get => BitConverter.ToInt32(gameSave.Body, Offset + Offsets.DataLength2);
        set => Unsafe.WriteUnaligned(ref gameSave.Body[Offset + Offsets.DataLength2], value);
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Useful for debugging")]
    private int Count => BitConverter.ToInt32(gameSave.Body, Offset + Offsets.Count);

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
            while ((id = BitConverter.ToInt32(gameSave.Body, offset)) == 0)
            {
                offset += 4;
            }
            if (BitConverter.ToUInt64(gameSave.Body, offset + 4) != pattern)
            {
                break;
            }
            dataLength = 17 + BitConverter.ToInt32(gameSave.Body, offset + 13);
            yield return (id, offset, dataLength);
            offset += dataLength;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
