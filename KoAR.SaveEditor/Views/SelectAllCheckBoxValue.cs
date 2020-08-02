using System;
using System.Collections.Generic;

namespace KoAR.SaveEditor.Views
{
    public static class SelectAllCheckBoxValue
    {
        public static bool? GetSelectAllCheckBoxValue<T>(this IReadOnlyList<T> items, Func<T, bool> projection)
        {
            if (items.Count == 0)
            {
                return true;
            }
            bool first = projection(items[0]);
            for (int index = 1; index < items.Count; index++)
            {
                if (projection(items[index]) != first)
                {
                    return null;
                }
            }
            return first;
        }
    }
}
