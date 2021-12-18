using System.Runtime.CompilerServices;

namespace KoAR.Core;

public sealed class QuestItem
{
    private readonly GameSave _gameSave;
    private readonly QuestItemDefinition _definition;

    public QuestItem(GameSave gameSave, QuestItemDefinition definition, int offset) =>
        (_gameSave, _definition, Offset) = (gameSave, definition, offset);

    internal int Offset { get; set; }

    private ref InventoryFlags Flags => ref Unsafe.As<byte, InventoryFlags>(ref _gameSave.Body[Offset]);

    public string Name => _definition.Name;

    public bool IsUnsellable
    {
        get => Flags.HasFlag(InventoryFlags.Unsellable);
        set => Flags = Flags.SetFlag(InventoryFlags.Unsellable, value);
    }
}
