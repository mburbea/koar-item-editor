using System;

namespace KoAR.SaveEditor.Constructs
{
    public readonly struct ObjectContainer : IEquatable<ObjectContainer>
    {
        public static readonly ObjectContainer Empty = new ObjectContainer();

        public ObjectContainer(object? item) => this.Item = item;

        public object? Item
        {
            get;
        }

        public override bool Equals(object obj) => obj is ObjectContainer other && this.Equals(other);

        public bool Equals(ObjectContainer other) => Object.Equals(this.Item, other.Item);

        public override int GetHashCode() => this.Item == null ? 0 : this.Item.GetHashCode();
    }
}
