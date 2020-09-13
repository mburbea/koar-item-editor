namespace KoAR.Core
{
    public class RemasterStashItem : StashItem
    {
        public RemasterStashItem(GameSave gameSave, int offset, int datalength) : base(gameSave, offset, datalength)
        {
        }

        public override bool HasCustomName => (Bytes[Offsets.HasCustomName] & 16) != 0;
        public override bool IsStolen => ((InventoryState)Bytes[Offsets.IsStolen] & InventoryState.Stolen) != 0;
    }
}
