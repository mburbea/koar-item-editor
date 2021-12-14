using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct BuffInstance(uint InstanceId, uint BuffId, ulong Duration = ulong.MaxValue)
{
    public static Span<BuffInstance> ReadList(ref Span<byte> data) => BuffMethods.ReadList<BuffInstance>(ref data);
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct BuffDuration(uint BuffId, uint Duration = uint.MaxValue)
{ 
    public static Span<BuffDuration> ReadList(ref Span<byte> data) => BuffMethods.ReadList<BuffDuration>(ref data);
}

internal static class BuffMethods
{
    public static Span<T> ReadList<T>(ref Span<byte> data)
        where T : struct
    {
        var count = MemoryMarshal.Read<int>(data);
        var byteCount = count * Unsafe.SizeOf<T>();
        var list = MemoryMarshal.Cast<byte, T>(data.Slice(4, byteCount));
        data = data[(4 + byteCount)..];
        return list;
    }
}
