namespace System.Diagnostics.CodeAnalysis
{
    public sealed class AllowNullAttribute : Attribute
    {
    }

    public sealed class MaybeNullAttribute : Attribute
    {
    }

    public sealed class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute(string _)
        {
        }
    }

    public sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool _)
        {
        }
    }
}
