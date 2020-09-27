using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public readonly ref struct BuffInstanceList
    {

        public readonly Span<byte> Data;
        public BuffInstanceList(Span<byte> data)
        {
            Data = data;
            
        }

        public readonly int Count => MemoryMarshal.Read<int>(Data);
        public readonly int EndOfStruct => 4 + (Count * Unsafe.SizeOf<BuffInstance>());
        public readonly ReadOnlySpan<BuffInstance> List => MemoryMarshal.Cast<byte, BuffInstance>(Data.Slice(4, Count * Unsafe.SizeOf<BuffInstance>()));
    }
    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 16)]
    public readonly struct BuffInstance
    {
        public readonly uint InstanceId;
        public readonly uint BuffId;
        public readonly ulong Duration;

        public BuffInstance(uint instanceId, uint buffId, ulong duration) => (InstanceId, BuffId, Duration) = (instanceId, buffId, duration);

        public void Deconstruct(out uint instanceId, out uint buffId, out ulong duration) => (instanceId, buffId, duration) = (InstanceId, BuffId, Duration);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 8)]
    public readonly struct BuffDuration
    {
        public readonly uint BuffId;
        public readonly uint Duration;
        public BuffDuration(uint buffId, uint duration) => (BuffId, Duration) = (buffId, duration);

        public void Deconstruct(out uint buffId, out uint duration) => (buffId, duration) = (BuffId, Duration);
    }
}
