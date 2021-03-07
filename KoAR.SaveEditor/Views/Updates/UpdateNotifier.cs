using System;
using System.Threading;
using System.Threading.Tasks;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateNotifier
    {
        private UpdateInfo? _update;

        public event EventHandler? UpdateChanged;

        public UpdateInfo? Update
        {
            get => this._update;
            private set
            {
                this._update = value;
                this.UpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            IReleaseInfo[]? releases = await UpdateMethods.FetchUpdateReleasesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            this.Update = releases != null ? new(releases) : null;
        }
    }
}
