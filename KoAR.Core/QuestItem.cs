using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class QuestItem(GameSave gameSave, QuestItemDefinition definition, int offset)
{
    internal int Offset { get; set; } = offset;

    private ref InventoryFlags Flags => ref Unsafe.As<byte, InventoryFlags>(ref gameSave.Body[Offset]);

    public string Name => definition.Name;

    public bool IsUnsellable
    {
        get => Flags.HasFlag(InventoryFlags.Unsellable);
        set => Flags = Flags.SetFlag(InventoryFlags.Unsellable, value);
    }
}
