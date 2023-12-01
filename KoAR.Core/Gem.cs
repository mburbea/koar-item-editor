using System;
using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class Gem(GameSave gameSave, int itemOffset)
{
    public int ItemOffset { get; set; } = itemOffset;

    public GemDefinition Definition
    {
        get => Amalur.GemDefinitions[BitConverter.ToUInt32(gameSave.Body, ItemOffset)];
        set => Unsafe.WriteUnaligned(ref gameSave.Body[ItemOffset], value.TypeId);
    }
}
