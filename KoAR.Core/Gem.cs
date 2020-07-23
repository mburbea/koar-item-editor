namespace KoAR.Core
{
    public class Gem
    {
        private readonly GameSave _gameSave;
        public Gem(GameSave gameSave, int itemOffset)
        {
            _gameSave = gameSave;
            ItemOffset = itemOffset;
        }

        public int ItemOffset { get; }

        public GemDefinition Definition
        {
            get => Amalur.GemDefinitions[MemoryUtilities.Read<uint>(_gameSave.Bytes, ItemOffset)];
            set => MemoryUtilities.Write(_gameSave.Bytes, ItemOffset, value.TypeId);
        }
    }
}