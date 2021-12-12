﻿using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class RemasterStashItem : StashItem
{
    public RemasterStashItem(GameSave gameSave, int offset, int dataLength, Gem[] gems) : base(gameSave, offset, dataLength, gems)
    {
    }

    private ref InventoryFlags Flags => ref Unsafe.As<byte, InventoryFlags>(ref Bytes[Offsets.InventoryFlags]);
    private ref ExtendedInventoryFlags ExtendedFlags => ref Unsafe.As<byte, ExtendedInventoryFlags>(ref Bytes[Offsets.ExtendedInventoryFlags]);

    public override bool HasCustomName => ExtendedFlags.HasFlag(ExtendedInventoryFlags.HasCustomName);

    public override bool IsStolen => Flags.HasFlag(InventoryFlags.IsFromStolenSource);
}
