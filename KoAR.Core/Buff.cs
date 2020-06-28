using System.Text.Json.Serialization;

namespace KoAR.Core
{
    public class Buff
    {
        public uint Id { get; set; }
        public string? Modifier { get; set; }
        public string Name { get; set; } = "";
        public string? Flavor { get; set; }
        public Rarity Rarity { get; set; }
        public BuffType? Ap { get; set; }
        public BuffDescription[]? Desc { get; set; }
    }

    public class BuffDescription
    {
        [JsonPropertyName("param_icon")]
        public string? ParamIcon { get; set; }
        public string? Text { get; set; }
        [JsonPropertyName("buff_id")]
        public string? BuffId { get; set; }
    }

    public enum BuffType
    {
        Normal,
        Curse,
        Destiny,
        Disease,
        Prefix,
        Self,
        SpecialCurse,
        SpecialDisease,
        Suffix,
        TemporaryPositive,
        Trait
    }
}
