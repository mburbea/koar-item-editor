using System;
using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class Gem
{
    private readonly GameSave _gameSave;
    public Gem(GameSave gameSave, int itemOffset) => (_gameSave, ItemOffset) = (gameSave, itemOffset);

    public int ItemOffset { get; set; }

    public GemDefinition Definition
    {
        get => Amalur.GemDefinitions[BitConverter.ToUInt32(_gameSave.Body, ItemOffset)];
        set => Unsafe.WriteUnaligned(ref _gameSave.Body[ItemOffset], value.TypeId);
    }
}
