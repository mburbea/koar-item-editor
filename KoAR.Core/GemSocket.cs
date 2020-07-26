using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace KoAR.Core
{
    public readonly struct GemSocket
    {
        public GemSocket(char socketType, Gem? gem) => (SocketType, Gem) = (socketType, gem);

        public char SocketType { get; }
        public Gem? Gem { get; }
    }
}
