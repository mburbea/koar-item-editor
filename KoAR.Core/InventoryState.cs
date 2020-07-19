using System;

namespace KoAR.Core
{
    [Flags]
    internal enum InventoryState : byte
    {
        FromStolenSource = 1 << 2, // Doesn't seem to be checked. 
        Stolen           = 1 << 3,
        Unstashable      = 1 << 6,
        Unsellable       = 1 << 7,
    }
}
