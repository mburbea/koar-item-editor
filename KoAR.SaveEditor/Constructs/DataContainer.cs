using System;

namespace KoAR.SaveEditor.Constructs
{
    public readonly struct DataContainer : IEquatable<DataContainer>
    {
        public static readonly DataContainer Empty = default;

        public DataContainer(object? data) => this.Data = data;

        public object? Data
        {
            get;
        }

        public static bool operator !=(DataContainer left, DataContainer right) => !left.Equals(right);

        public static bool operator ==(DataContainer left, DataContainer right) => left.Equals(right);

        public override bool Equals(object obj) => obj is DataContainer other && this.Equals(other);

        public bool Equals(DataContainer other) => Object.Equals(this.Data, other.Data);

        public override int GetHashCode() => this.Data == null ? 0 : this.Data.GetHashCode();
    }
}
