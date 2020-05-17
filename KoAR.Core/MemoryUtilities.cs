using System;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    internal static class MemoryUtilities
    {
        public static byte[] ReplaceBytes(byte[] bytes, int offset, int length, ReadOnlySpan<byte> newData)
        {
            if (newData.Length == length)
            {
                newData.CopyTo(bytes.AsSpan(offset));
                return bytes;
            }

            var buffer = new byte[bytes.Length + (newData.Length - length)];
            bytes.AsSpan(0, offset).CopyTo(buffer);
            newData.CopyTo(buffer.AsSpan(offset));
            bytes.AsSpan(offset + length).CopyTo(buffer.AsSpan(offset + newData.Length));
            return buffer;
        }

        public static T Read<T>(ReadOnlySpan<byte> span, int offset = 0)
            where T : struct
            => MemoryMarshal.Read<T>(span.Slice(offset));

        public static void Write<T>(Span<byte> span, int offset, T value)
            where T : struct
            => MemoryMarshal.Write(span.Slice(offset), ref value);
    }
}
