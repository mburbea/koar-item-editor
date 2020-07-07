using System;
using System.Text.Json.Serialization;

namespace KoAR.Core
{
    public class Buff
    {
        public uint Id { get; set; }
        public string? Modifier { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Flavor { get; set; }
        public Rarity Rarity { get; set; }
        [JsonPropertyName("ap")]
        public BuffTypes BuffType { get; set; }
        public BuffDescription[] Desc { get; set; } = Array.Empty<BuffDescription>();
    }

    public class BuffDescription
    {
        [JsonPropertyName("param_icon")]
        public string? ParamIcon { get; set; }
        public string? Text { get; set; }
        [JsonPropertyName("buff_id")]
        public string? BuffId { get; set; }
    }

    [Flags]
    public enum BuffTypes
    {
        None                = 0,
        Normal              = 1 << 0,
        Curse               = 1 << 1,
        Destiny             = 1 << 2,
        Disease             = 1 << 3,
        Prefix              = 1 << 4,
        Self                = 1 << 5,
        SpecialCurse        = 1 << 6,
        SpecialDisease      = 1 << 7,
        Suffix              = 1 << 8,
        TemporaryPositive   = 1 << 9,
        Trait               = 1 << 10
    }
}
