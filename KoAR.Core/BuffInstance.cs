using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core;

[StructLayout(LayoutKind.Sequential)]
public readonly struct BuffInstance
{
    public readonly uint InstanceId;
    public readonly uint BuffId;
    public readonly ulong Duration;

    public BuffInstance(uint instanceId, uint buffId) => (InstanceId, BuffId, Duration) = (instanceId, buffId, ulong.MaxValue);

    public void Deconstruct(out uint instanceId, out uint buffId, out ulong duration) => (instanceId, buffId, duration) = (InstanceId, BuffId, Duration);

    public static Span<BuffInstance> ReadList(ref Span<byte> data) => BuffMethods.ReadList<BuffInstance>(ref data);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct BuffDuration
{
    public readonly uint BuffId;
    public readonly uint Duration;

    public BuffDuration(uint buffId) => (BuffId, Duration) = (buffId, uint.MaxValue);

    public void Deconstruct(out uint buffId, out uint duration) => (buffId, duration) = (BuffId, Duration);

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
