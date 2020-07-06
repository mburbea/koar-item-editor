namespace System.Diagnostics.CodeAnalysis
{
    public sealed class AllowNullAttribute : Attribute
    {
    }

    public sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool _)
        {
        }
    }
}
