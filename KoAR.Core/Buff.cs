using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace KoAR.Core;

public record Buff(uint Id, string? Modifier = null, string Name = null!, string? Flavor = null, Rarity Rarity = default, 
    BuffTypes BuffType = default, ApplyType ApplyType = default) : IDefinition
{
    [JsonPropertyName("desc")]
    public BuffDescription[] Descriptions { get; set; } = Array.Empty<BuffDescription>();

    public string TitleText => (((BuffTypes.TransientOrAffix & BuffType) == BuffType ? Modifier : Flavor) ?? Name).Replace('\n', '.');

    public string ShortDisplayText => $"{TitleText} [{(Descriptions.Any() ? string.Join(";", Descriptions.Select(x => x.Text)) : "None")}]";

    public bool RequiresFatesworn => Name.StartsWith("mit_");
}

public record BuffDescription([property: JsonPropertyName("param_icon")] string? Icon, string? Text)
{
    public static readonly BuffDescription Empty = new(Icon: "Default", Text: "None");
}

[Flags]
public enum BuffTypes
{
    None = 0,
    Normal = 1 << 0,
    Curse = 1 << 1,
    Destiny = 1 << 2,
    Disease = 1 << 3,
    Prefix = 1 << 4,
    SpecialCurse = 1 << 5,
    SpecialDisease = 1 << 6,
    Suffix = 1 << 7,
    TemporaryPositive = 1 << 8,
    Trait = 1 << 9,
    Affix = Prefix | Suffix,
    TransientBuff = Curse | Disease | SpecialCurse | TemporaryPositive,
    TransientOrAffix = Affix | TransientBuff,
}

public enum ApplyType
{
    OnOwner,
    OnObject
}
