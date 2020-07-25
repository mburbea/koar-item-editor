using System;

namespace KoAR.Core
{
    public class ItemGems
    {
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int GemCount = 17;
            public const int FirstGem = 21;
        }

        private readonly GameSave _gameSave;
        internal int ItemOffset { get; set; }

        public ItemGems(GameSave gameSave, int itemOffset, int dataLength)
        {
            (_gameSave, ItemOffset) = (gameSave, itemOffset);
            Gems =  GemCount > 0  ? new Gem[GemCount] : Array.Empty<Gem>();
            for (int i = 0; i < Gems.Length; i++)
            {
                Gems[i] = gameSave.Gems.GetOrDefault(this[Offsets.FirstGem + i * 4])!;
            }
        }

        private int this[int index]
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, ItemOffset + index);
            set => MemoryUtilities.Write(_gameSave.Bytes, ItemOffset + index, value);
        }

        private int DataLength => this[Offsets.DataLength];

        private int GemCount => this[Offsets.GemCount];

        public Gem[] Gems { get; }
    }
}
