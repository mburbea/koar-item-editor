using System.Collections.Generic;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateInfo
    {
        public UpdateInfo(IReadOnlyList<IReleaseInfo> releases)
        {
            IReleaseInfo latest = releases[0];
            (this.Version, this.Uri, this.Size, this.Releases) = (latest.Version, latest.ZipFileUri, latest.ZipFileSize, releases);
        }

        public IReadOnlyList<IReleaseInfo> Releases { get; }

        public int Size { get; }

        public string Uri { get; }

        public string Version { get; }
    }
}
