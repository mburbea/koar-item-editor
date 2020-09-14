using System;

namespace KoAR.SaveEditor.Updates
{
    public readonly struct DownloadProgress : IEquatable<DownloadProgress>
    {
        public DownloadProgress(int bytesTransferred, double speed)
        {
            (this.BytesTransferred, this.Speed, this.Error) = (bytesTransferred, speed, default);
        }

        public DownloadProgress(Exception error)
        {
            (this.BytesTransferred, this.Speed, this.Error) = (default, default, error);
        }

        public int BytesTransferred { get; }

        public Exception? Error { get; }

        public double Speed { get; }

        public static implicit operator DownloadProgress(Exception error) => new DownloadProgress(error);

        public bool Equals(DownloadProgress other) => this.GetValues().Equals(other.GetValues());

        public override bool Equals(object obj) => obj is DownloadProgress other && this.Equals(other);

        public override int GetHashCode() => this.GetValues().GetHashCode();

        private (int, double, Exception?) GetValues() => (this.BytesTransferred, this.Speed, this.Error);
    }
}
