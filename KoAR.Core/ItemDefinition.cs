using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KoAR.Core;

public sealed partial class ItemDefinition : IDefinition
{
#if DEBUG
    internal ItemDefinition(uint typeId)
    {
        Category = EquipmentCategory.Unknown;
        TypeId = typeId;
        Name = InternalName = Amalur.SimTypes[typeId];
        SocketTypes = "";
        ItemBuffs = ItemDefinitionBuffMemory.Empty;
        PlayerBuffs = Array.Empty<Buff>();
    }
#endif

    private ItemDefinition(EquipmentCategory category, uint typeId, byte level, string name, string internalName, float maxDurability, Rarity rarity,
        string socketTypes, Element element, ArmorType armorType, Buff? prefix, Buff? suffix,
        Buff[] itemBuffs, Buff[] playerBuffs, bool isMerchant, bool affixableName, bool hasVariants)
    {
        Category = category;
        TypeId = typeId;
        Level = level;
        Name = name;
        InternalName = internalName;
        MaxDurability = maxDurability;
        Rarity = rarity;
        SocketTypes = socketTypes;
        ArmorType = armorType;
        Element = element;
        PlayerBuffs = playerBuffs;
        ItemBuffs = itemBuffs.Length == 0 && prefix is null && suffix is null
            ? ItemDefinitionBuffMemory.Empty
            : new(itemBuffs, prefix, suffix);
        IsMerchant = isMerchant;
        AffixableName = affixableName;
        HasVariants = hasVariants;
        if(internalName.StartsWith("mit") && internalName.Contains("chaos"))
        {
            HasVariants = true;
            ChaosTier = char.ToUpperInvariant(internalName[^1]);
        }
    }

    public EquipmentCategory Category { get; }
    public uint TypeId { get; }
    public byte Level { get; }
    public string Name { get; }
    public string InternalName { get; }
    public float MaxDurability { get; }
    public Rarity Rarity { get; }
    public string SocketTypes { get; }
    public Element Element { get; }
    public ArmorType ArmorType { get; }
    public Buff[] PlayerBuffs { get; }
    public bool AffixableName { get; }
    public bool HasVariants { get; }
    public bool IsMerchant { get; }
    public IItemBuffMemory ItemBuffs { get; }
    public bool RequiresFatesworn => InternalName.StartsWith("mit_");
    public char? ChaosTier { get; }

    public IEnumerable<Socket> GetSockets() => SocketTypes.Select(socket => new Socket(socket));
    

    public string CategoryDisplayName => this switch
    {
        { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Hat } => "Hood",
        { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Feet } => "Boots",
        { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Hands } => "Gloves",
        { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Legs } => "Leggings",
        { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Torso } => "Armor",
        { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Shield } => "Buckler",
        { ArmorType: ArmorType.Might, Category: EquipmentCategory.Hat } => "Helm",
        { ArmorType: ArmorType.Might, Category: EquipmentCategory.Feet } => "Greaves",
        { ArmorType: ArmorType.Might, Category: EquipmentCategory.Hands } => "Gauntlets",
        { ArmorType: ArmorType.Might, Category: EquipmentCategory.Legs } => "Chausses",
        { ArmorType: ArmorType.Might, Category: EquipmentCategory.Torso } => "Cuirass",
        { ArmorType: ArmorType.Might, Category: EquipmentCategory.Shield } => "Kite Shield",
        { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Hat } => "Cowl",
        { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Feet } => "Shoes",
        { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Hands } => "Handwraps",
        { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Robes } => "Robes",
        { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Shield } => "Talisman",
        _ => Category.ToString()
    };
}
