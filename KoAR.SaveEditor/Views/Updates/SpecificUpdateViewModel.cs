using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class SpecificUpdateViewModel : UpdateViewModelBase
    {
        public SpecificUpdateViewModel(IReleaseInfo target, string title, string? preamble)
            : base(new[] { target })
        {
            (this.Title, this.Preamble) = (title, preamble);
        }

        public override string? Preamble { get; }

        public override string Title { get; }
    }
}
