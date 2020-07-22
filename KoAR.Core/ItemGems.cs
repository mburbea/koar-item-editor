using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace KoAR.Core
{
    public class ItemGems
    {
        internal static ItemGems Empty { get; } =  new ItemGems();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int GemCount = 17;
            public const int FirstGem = 21;
        }

        internal byte[] Bytes { get; set; } = Array.Empty<byte>();

        internal int ItemOffset { get; set; }

        public ItemGems(GameSave gameSave, int itemOffset, int dataLength)
        {
            ItemOffset = itemOffset;
            Bytes = gameSave.Bytes.AsSpan(itemOffset, dataLength).ToArray();
            Span<int> gemIds = MemoryMarshal.Cast<byte, int>(Bytes.AsSpan(Offsets.FirstGem));
            Gems = gemIds.Length == 0 ? Gems : new Gem[gemIds.Length];
            for (int i = 0; i < gemIds.Length; i++)
            {
                Gems[i] = gameSave.Gems.GetOrDefault(gemIds[i]);
            }
        }

        private ItemGems()
        {
        }

        private int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength);
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value);
        }

        private int SocketCount => MemoryUtilities.Read<int>(Bytes, Offsets.GemCount);

        public Gem?[] Gems { get; } = Array.Empty<Gem>();
    }
}
