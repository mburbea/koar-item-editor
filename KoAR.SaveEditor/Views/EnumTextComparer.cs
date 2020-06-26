using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class EnumTextComparer : IComparer
    {
        public static readonly EnumTextComparer Instance = new EnumTextComparer();

        private EnumTextComparer()
        {
        }

        public int Compare([AllowNull] object x, [AllowNull] object y)
        {
            if (x == y)
            {
                return 0;
            }
            if (y is null)
            {
                return 1;
            }
            return x switch
            {
                null => -1,
                DataContainer left when y is DataContainer right => this.Compare(left.Data, right.Data),
                Enum left when y is Enum right => string.Compare(left.ToString(), right.ToString()),
                _ => throw new ArgumentException(!(x is Enum) ? nameof(x) : nameof(y))
            };
        }
    }
}
