using System.Collections.Generic;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates;

public sealed class UpdateInfo
{
    public UpdateInfo(IReadOnlyCollection<IReleaseInfo> releases) => this.Releases = releases;

    public IReadOnlyCollection<IReleaseInfo> Releases { get; }
}
