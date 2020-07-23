using System;
using System.Collections.Generic;
using System.Text;

namespace KoAR.Core
{
    public class ItemDefinitionBuffMemory : IItemBuffMemory
    {
        internal static readonly ItemDefinitionBuffMemory Empty = new ItemDefinitionBuffMemory(Array.Empty<Buff>(), null, null);

        internal ItemDefinitionBuffMemory(Buff[] list, Buff? prefix, Buff? suffix) => (List, Prefix, Suffix) = (list, prefix, suffix);

        public IList<Buff> List { get; }

        public Buff? Prefix { get; }

        public Buff? Suffix { get; }
    }

}
