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
        [JsonPropertyName("buff_type")]
        public BuffTypes BuffType { get; set; }
        [JsonPropertyName("apply_type")]
        public ApplyType ApplyType { get; set; }
        public BuffDescription[] Desc { get; set; } = Array.Empty<BuffDescription>();
    }

    public class BuffDescription
    {
        [JsonPropertyName("param_icon")]
        public string? Icon { get; set; }
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
        SpecialCurse        = 1 << 5,
        SpecialDisease      = 1 << 6,
        Suffix              = 1 << 7,
        TemporaryPositive   = 1 << 8,
        Trait               = 1 << 9
    }

    public enum ApplyType
    {
        OnOwner,
        OnObject
    }
}
