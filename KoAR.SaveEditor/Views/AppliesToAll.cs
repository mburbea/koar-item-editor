using System;
using System.Collections.Generic;
using System.Linq;

namespace KoAR.SaveEditor.Views
{
    public static class AppliesToAll
    {
        public static bool? GetAppliesToAll<T>(this IReadOnlyCollection<T> items, Func<T, bool> projection)
        {
            if (items.Count == 0)
            {
                return true;
            }
            bool first = projection(items.First());
            return items.Skip(1).Select(projection).Any(value => value != first) ? default(bool?) : first;
        }
    }
}
