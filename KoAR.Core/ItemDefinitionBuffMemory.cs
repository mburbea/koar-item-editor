using System;
using System.Collections.Generic;

namespace KoAR.Core;

public sealed class ItemDefinitionBuffMemory(Buff[] list, Buff? prefix, Buff? suffix) : IItemBuffMemory
{
    public static readonly ItemDefinitionBuffMemory Empty = new([], null, null);

    public IList<Buff> List => list;

    public Buff? Prefix => prefix;

    public Buff? Suffix => suffix;
}
