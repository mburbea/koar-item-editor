using System;

namespace KoAR.SaveEditor.Updates
{
    public readonly struct UpdateInfo : IEquatable<UpdateInfo>
    {
        public UpdateInfo(string version, string body, string zipFileUrl, long fileSize)
        {
            this.Version = version;
            this.Body = body;
            this.ZipFileUrl = zipFileUrl;
            this.FileSize = fileSize;
        }

        public string Body { get; }

        public long FileSize { get; }

        public bool IsEmpty => this.FileSize == default;

        public string Version { get; }

        public string ZipFileUrl { get; }

        public static bool operator !=(UpdateInfo left, UpdateInfo right) => !left.Equals(right);

        public static bool operator ==(UpdateInfo left, UpdateInfo right) => left.Equals(right);

        public override bool Equals(object obj) => obj is UpdateInfo other && this.Equals(other);

        public bool Equals(UpdateInfo other)
        {
            return this.Version == other.Version && this.Body == other.Body &&
                this.ZipFileUrl == other.ZipFileUrl && this.FileSize == other.FileSize;
        }

        public override int GetHashCode() => (this.Version, this.Body, this.ZipFileUrl, this.FileSize).GetHashCode();
    }
}
