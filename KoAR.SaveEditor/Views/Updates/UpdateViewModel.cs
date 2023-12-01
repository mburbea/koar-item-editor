using System.Collections.Generic;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates;

public sealed class UpdateViewModel(IReadOnlyCollection<IReleaseInfo> updateReleases) : UpdateViewModelBase(updateReleases)
{
    public override string Title => "Update Available";
}
