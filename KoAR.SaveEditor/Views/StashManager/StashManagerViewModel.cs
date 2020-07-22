using System;
using System.Collections.Generic;
using System.Linq;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.StashManager
{
    public sealed class StashManagerViewModel : ItemsViewModelBase<StashItemModel>
    {
        public StashManagerViewModel(Stash stash)
        {
            this.Stash = stash;
        }

        public Stash Stash { get; }
    }
}
