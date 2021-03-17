using System;
using System.Collections.Generic;

namespace KoAR.Core
{
    public class ItemDefinitionBuffMemory : IItemBuffMemory
    {
        public static readonly ItemDefinitionBuffMemory Empty = new(Array.Empty<Buff>(), null, null);

        internal ItemDefinitionBuffMemory(Buff[] list, Buff? prefix, Buff? suffix) => (List, Prefix, Suffix) = (list, prefix, suffix);

        public IList<Buff> List { get; }

        public Buff? Prefix { get; }

        public Buff? Suffix { get; }
    }
}
