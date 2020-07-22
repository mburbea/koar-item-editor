using System;

namespace KoAR.Core
{
    // since this is an internal enum, I've switched to constants as it made the code less annoying.
    internal static class InventoryState
    {
        public const byte FromStolenSource = 1 << 2; // Doesn't seem to be checked. 
        public const byte Stolen = 1 << 3;
        public const byte Unstashable = 1 << 6;
        public const byte Unsellable = 1 << 7;
    }
}
