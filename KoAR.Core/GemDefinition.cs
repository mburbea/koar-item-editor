using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace KoAR.Core;

public record GemDefinition(
    uint TypeId,
    string Name,
    string InternalName,
    // note that technically the game actually has an array and there is one gem that has 2. We don't care as even the game doesn't render this correctly.
    uint BuffId,
    char SocketType
    )
{
    private Buff[] _buffs = null!;

    public Buff Buff => Amalur.GetBuff(BuffId);
}

