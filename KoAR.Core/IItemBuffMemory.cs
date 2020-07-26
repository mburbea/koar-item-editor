using System.Collections.Generic;

namespace KoAR.Core
{
    public interface IItemBuffMemory
    {
        IList<Buff> List { get; }
        Buff? Prefix { get; }
        Buff? Suffix { get; }
    }
}