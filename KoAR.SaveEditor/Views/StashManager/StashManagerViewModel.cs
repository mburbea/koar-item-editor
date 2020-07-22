using System;
using System.Collections.Generic;
using System.Linq;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.StashManager
{
    public sealed class StashManagerViewModel : ItemsViewModelBase<StashItemModel>
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
