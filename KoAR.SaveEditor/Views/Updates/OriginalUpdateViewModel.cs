using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates;

public sealed class OriginalUpdateViewModel(IReleaseInfo target) : UpdateViewModelBase(target)
{
    public override string? Preamble => $"You are currently running v{App.Version} of the editor. 3.x releases are only tested to work with the remaster. " +
        "While it is possible that this application will work without issues in the original edition, it might not. It is recommended for " +
        $"users of the original game to instead use the latest 2.x release (v{this.Target.Version}) which you can download below.";

    public override string Title => "Remaster vs Original Edition";
}
