using System;
using System.Collections.Generic;

namespace KoAR.SaveEditor.Views.Updates
{
    public interface IRelease
    {
        IReadOnlyList<IReleaseAsset> Assets { get; }

        string Body { get; }

        public DateTime PublishedAt { get; }

        string Version { get; }
    }

    public interface IReleaseAsset
    {
        string BrowserDownloadUrl { get; }

        int Size { get; }
    }
}
