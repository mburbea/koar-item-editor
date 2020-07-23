using KoAR.Core;

namespace KoAR.SaveEditor.Views.StashManager
{
    public sealed class StashManagerViewModel 
    {
        public StashManagerViewModel(GameSave gameSave, Stash stash)
        {
            this.GameSave = gameSave;
            this.Stash = stash;
        }

        public GameSave GameSave { get; }

        public Stash Stash { get; }
    }
}
