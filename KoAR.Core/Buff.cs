namespace KoAR.Core
{
    public class Buff
    {
        public uint Id { get; set; }
        public string? Modifier { get; set; }
        public string? InternalName { get; set; }
        public Rarity Rarity { get; set; }
        public string? Ap { get; set; }
        public BuffDescription[]? BuffDescription { get; set; }
    }

    public class BuffDescription
    {
        public string? ParamIcon { get; set; }
        public string? Text { get; set; }
        public string? BuffId { get; set; }
        public object[]? ParamDescArgs { get; set; }
        public string? ParamDesc { get; set; }
    }
}
