using System;
using System.Linq;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class Release
    {
        public ReleaseAsset[] Assets { get; set; } = Array.Empty<ReleaseAsset>();

        public string Body { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }

        public string TagName { get; set; } = string.Empty;

        public string Version => this.TagName.Length == 0 ? string.Empty : this.TagName.Substring(1);

        public ReleaseAsset? GetZipFileAsset() => this.Assets.FirstOrDefault(asset => asset.IsZipFile);
    }

    public sealed class ReleaseAsset
    {
        public string BrowserDownloadUrl { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public bool IsZipFile => this.ContentType == "application/zip";

        public int Size { get; set; }
    }
}
