using System.Collections.Generic;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates;

public sealed class UpdateViewModel : UpdateViewModelBase
{
    public UpdateViewModel(IReadOnlyCollection<IReleaseInfo> updateReleases)
        : base(updateReleases)
    {
    }

    public override string Title => "Update Available";
}
