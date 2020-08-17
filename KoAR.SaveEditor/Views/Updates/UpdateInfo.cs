using System.Collections.Generic;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateInfo
    {
        public UpdateInfo(string version, string uri, int size, IReadOnlyList<IReleaseInfo> releases)
        {
            (this.Version, this.Uri, this.Size, this.Releases) = (version, uri, size, releases);
        }

        public IReadOnlyList<IReleaseInfo> Releases { get; }

        public int Size { get; }

        public string Uri { get; }

        public string Version { get; }
    }
}
