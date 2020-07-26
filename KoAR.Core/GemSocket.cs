﻿namespace KoAR.Core
{
    public readonly struct GemSocket
    {
        public GemSocket(char socketType, Gem? gem = null) => (SocketType, Gem) = (socketType, gem);

        public char SocketType { get; }
        public Gem? Gem { get; }
    }
}
