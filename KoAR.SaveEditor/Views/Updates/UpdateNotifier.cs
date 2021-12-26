using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates;

public sealed class UpdateNotifier
{
    private IReadOnlyCollection<IReleaseInfo>? _updateReleases;

    public event EventHandler? HasUpdateChanged;

    public bool HasUpdate => this.UpdateReleases is { Count: > 0 };

    public IReadOnlyCollection<IReleaseInfo>? UpdateReleases
    {
        get => this._updateReleases;
        private set
        {
            this._updateReleases = value;
            this.HasUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        this.UpdateReleases = await UpdateMethods.FetchUpdateReleasesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
