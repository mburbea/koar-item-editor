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
        Normal = 0x1,
        Curse = 0x2,
        Destiny = 0x4,
        Disease = 0x8,
        Prefix = 0x10,
        Self = 0x20,
        SpecialCurse = 0x40,
        SpecialDisease = 0x80,
        Suffix = 0x100,
        TemporaryPositive = 0x200,
        Trait = 0x400
    }
}
