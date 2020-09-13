namespace KoAR.Core
{
    public class Gem
    {
        private readonly GameSave _gameSave;
        public Gem(GameSave gameSave, int itemOffset) => (_gameSave, ItemOffset) = (gameSave, itemOffset);

        public int ItemOffset { get; set; }

        public GemDefinition Definition
        {
            get => Amalur.GemDefinitions[MemoryUtilities.Read<uint>(_gameSave.Body, ItemOffset)];
            set => MemoryUtilities.Write(_gameSave.Body, ItemOffset, value.TypeId);
        }
    }
}