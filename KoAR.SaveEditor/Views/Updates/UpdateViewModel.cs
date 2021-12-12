namespace KoAR.SaveEditor.Views.Updates;

public sealed class UpdateViewModel : UpdateViewModelBase
{
    public UpdateViewModel(UpdateInfo update)
        : base(update.Releases)
    {
    }

    public override string Title => "Update Available";
}
