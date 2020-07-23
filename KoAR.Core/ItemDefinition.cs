using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public class ItemDefinition
    {
        private static bool TryParseBuffList(string value, [NotNullWhen(true)] out Buff[]? buffs)
        {
            buffs = null;
            if (value.Length % 6 != 0)
            {
                return false;
            }
            var results = value.Length == 0 ? Array.Empty<Buff>() : new Buff[value.Length / 6];
            for (int i = 0; i < results.Length; i++)
            {
                if (!uint.TryParse(value.Substring(i * 6, 6), NumberStyles.HexNumber, null, out uint buffId))
                {
                    return false;
                }
                results[i] = Amalur.GetBuff(buffId);
            }
            buffs = results;
            return true;
        }

        private static bool TryLoadFromRow(string[] entries, [NotNullWhen(true)] out ItemDefinition? definition)
        {
            definition = null;
            if (entries.Length != 15
                || !Enum.TryParse(entries[0], true, out EquipmentCategory category)
                || !uint.TryParse(entries[1], NumberStyles.HexNumber, null, out uint typeId)
                || !byte.TryParse(entries[2], out byte level)
                || !float.TryParse(entries[5], out float maxDurability)
                || !Enum.TryParse(entries[6], out Rarity rarity)
                || !Enum.TryParse(entries[8], out Element element)
                || !Enum.TryParse(entries[9], out ArmorType armorType)
                || !uint.TryParse(entries[10], NumberStyles.HexNumber, null, out uint prefix)
                || !uint.TryParse(entries[11], NumberStyles.HexNumber, null, out uint suffix)
                || !TryParseBuffList(entries[12], out var itemBuffs)
                || !TryParseBuffList(entries[13], out var playerBuffs)
                || !bool.TryParse(entries[14], out bool hasVariants))
            {
                return false;
            }
            definition = new ItemDefinition(category, typeId, level, entries[3], entries[4], maxDurability, rarity, entries[7], 
                element, armorType, Amalur.BuffMap.GetOrDefault(prefix), Amalur.BuffMap.GetOrDefault(suffix), itemBuffs, playerBuffs, hasVariants);
            return true;
        }

        internal static IEnumerable<ItemDefinition> ParseFile(string path)
        {
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                if (TryLoadFromRow(line.Split(Amalur.Seperator), out var definition))
                {
                    yield return definition;
                }
            }
        }

        internal ItemDefinition(EquipmentCategory category, uint typeId, byte level, string name, string internalName, float maxDurability, Rarity rarity,
            string sockets, Element element, ArmorType armorType, Buff? prefix, Buff? suffix, Buff[] itemBuffs, Buff[] playerBuffs, bool hasVariants)
        {
            Category = category;
            TypeId = typeId;
            Level = level;
            Name = name;
            InternalName = internalName;
            MaxDurability = maxDurability;
            Rarity = rarity;
            Sockets = sockets;
            ArmorType = armorType;
            Element = element;
            PlayerBuffs = playerBuffs;
            HasVariants = hasVariants;
            ItemBuffs = itemBuffs.Length == 0 && prefix is null && suffix is null
                ? ItemDefinitionBuffMemory.Empty
                : new ItemDefinitionBuffMemory(itemBuffs, prefix, suffix);
            

            // merchant search is case sensitive to avoid affixing the Merchant's hat.
            IsMerchant = InternalName.Contains("merchant");
            AffixableName = IsMerchant || internalName.IndexOf("common", StringComparison.OrdinalIgnoreCase) != -1;
        }

        public EquipmentCategory Category { get; }
        public uint TypeId { get; }
        public byte Level { get; }
        public string Name { get; }
        public string InternalName { get; }
        public float MaxDurability { get; }
        public Rarity Rarity { get; }
        public string Sockets { get; }
        public Element Element { get; }
        public ArmorType ArmorType { get; }
        public Buff[] PlayerBuffs { get; }
        public bool AffixableName { get; }
        public bool HasVariants { get; }
        public bool IsMerchant { get; }
        public IItemBuffMemory ItemBuffs { get; }

        public string CategoryDisplayName => this switch
        {
            ItemDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Hat } => "Hood",
            ItemDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Feet } => "Boots",
            ItemDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Hands } => "Gloves",
            ItemDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Legs } => "Leggings",
            ItemDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Torso } => "Armor",
            ItemDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Shield } => "Buckler",
            ItemDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Hat } => "Helm",
            ItemDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Feet } => "Greaves",
            ItemDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Hands } => "Gauntlets",
            ItemDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Legs } => "Chausses",
            ItemDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Torso } => "Cuirass",
            ItemDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Shield } => "Kite Shield",
            ItemDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Hat } => "Cowl",
            ItemDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Feet } => "Shoes",
            ItemDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Hands } => "Handwraps",
            ItemDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Robes } => "Robes",
            ItemDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Shield } => "Talisman",
            _ => Category.ToString()
        };
    }
}
