namespace System.Diagnostics.CodeAnalysis
{
    // not including in .net framework for whatever reason.
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool _)
        { 
        }
    }
}