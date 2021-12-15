namespace KoAR.Core;

public record GemDefinition(
    uint TypeId,
    string Name,
    string InternalName,
    // note that technically the game actually has an array and there is one gem that has 2. We don't care as even the game doesn't render this correctly.
    uint BuffId,
    char SocketType
) : IDefinition
{
    public Buff Buff => Amalur.GetBuff(BuffId);

    public bool RequiresFatesworn => InternalName.StartsWith("mit_");
}