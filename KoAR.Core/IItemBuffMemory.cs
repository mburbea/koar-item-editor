using System.Collections.Generic;

namespace KoAR.Core
{
    public interface IItemBuffMemory
    {
        List<Buff> List { get; }
        Buff? Prefix { get; }
        Buff? Suffix { get; }
    }
}