using System;

namespace System.Diagnostics.CodeAnalysis
{
    // not including in .net framework for whatever reason.
    internal class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool _)
        { 
        }
    }
}