using System.Runtime.CompilerServices;

namespace KoAR.Core
{
    public class QuestItem
    {
        private readonly GameSave _gameSave;
        private readonly QuestItemDefinition _definition;

        public QuestItem(GameSave gameSave, QuestItemDefinition definition, int offset) => 
            (_gameSave, _definition, Offset) = (gameSave, definition, offset);

        internal int Offset { get; set; }

        private ref InventoryState State => ref Unsafe.As<byte, InventoryState>(ref _gameSave.Bytes[Offset]);

        public string Name => _definition.Name;

        public string InternalName => _definition.InternalName;

        public bool IsUnsellable
        {
            get => (State & InventoryState.Unsellable) == InventoryState.Unsellable;
            set => State = value ? State | InventoryState.Unsellable : State & ~InventoryState.Unsellable;
        }
    }
}
