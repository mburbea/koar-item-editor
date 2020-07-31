using System;

namespace KoAR.SaveEditor.Views.Updates
{
    public readonly struct UpdateInfo : IEquatable<UpdateInfo>
    {
        public UpdateInfo(string version, DateTime publishedAt, string body, string zipFileUrl, int fileSize)
        {
            this.Version = version;
            this.PublishedAt = publishedAt;
            this.Body = body;
            this.ZipFileUrl = zipFileUrl;
            this.FileSize = fileSize;
        }

        public string Body { get; }

        public int FileSize { get; }

        public bool IsEmpty => this.PublishedAt == default;

        public DateTime PublishedAt { get; }

        public string Version { get; }

        public string ZipFileUrl { get; }

        public static bool operator !=(UpdateInfo left, UpdateInfo right) => !left.Equals(right);

        public static bool operator ==(UpdateInfo left, UpdateInfo right) => left.Equals(right);

        public override bool Equals(object obj) => obj is UpdateInfo other && this.Equals(other);

        public bool Equals(UpdateInfo other) => this.GetValues().Equals(other.GetValues());

        public override int GetHashCode() => this.GetValues().GetHashCode();

        private (string, DateTime, string, string, int) GetValues() => (this.Version, this.PublishedAt, this.Body, this.ZipFileUrl, this.FileSize);
    }
}
