using System;
using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class ItemSockets
{
    private static class Offsets
    {
        public const int DataLength = 13;
        public const int GemCount = 17;
        public const int FirstGem = 21;
    }

    private readonly GameSave _gameSave;
    internal int ItemOffset { get; set; }

    public ItemSockets(GameSave gameSave, int itemOffset, int dataLength)
    {
        (_gameSave, ItemOffset) = (gameSave, itemOffset);
        Gems = GemCount > 0 ? new Gem[GemCount] : Array.Empty<Gem>();
        for (int i = 0; i < Gems.Length; i++)
        {
            Gems[i] = gameSave.Gems[this[Offsets.FirstGem + i * 4]];
        }
        if (DataLength != dataLength)
        {
            throw new InvalidOperationException();
        }
    }

    private int this[int index]
    {
        get => BitConverter.ToInt32(_gameSave.Body, ItemOffset + index);
        set => Unsafe.WriteUnaligned(ref _gameSave.Body[ItemOffset + index], value);
    }

    private int DataLength => this[Offsets.DataLength] + 17;

    private int GemCount => this[Offsets.GemCount];

    public Gem[] Gems { get; }
}
