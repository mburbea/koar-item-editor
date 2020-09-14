using System.Runtime.CompilerServices;

namespace KoAR.Core
{
    public class RemasterStashItem : StashItem
    {
        public RemasterStashItem(GameSave gameSave, int offset, int dataLength) : base(gameSave, offset, dataLength)
        {
        }

        private ref InventoryState State => ref Unsafe.As<byte, InventoryState>(ref Bytes[Offsets.IsStolen]);

        public override bool HasCustomName => (Bytes[Offsets.HasCustomName] & 16) != 0;

        public override bool IsStolen => (State & InventoryState.Stolen) == InventoryState.Stolen;
    }
}
