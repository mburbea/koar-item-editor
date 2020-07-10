using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public class TypeDefinition
    {
        private static bool TryParseEffectList(string value, [NotNullWhen(true)] out Buff[]? effects)
        {
            effects = null;
            if (value.Length % 6 != 0)
            {
                return false;
            }
            var results = value.Length == 0 ? Array.Empty<Buff>() : new Buff[value.Length / 6];
            for (int i = 0; i < results.Length; i++)
            {
                if (!uint.TryParse(value.Substring(i * 6, 6), NumberStyles.HexNumber, null, out uint effect))
                {
                    return false;
                }
                results[i] = Amalur.GetBuff(effect);
            }
            effects = results;
            return true;
        }

        private static bool TryLoadFromRow(string[] entries, [NotNullWhen(true)] out TypeDefinition? definition)
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
                || !TryParseEffectList(entries[12], out var coreEffects)
                || !TryParseEffectList(entries[13], out var effects)
                || !bool.TryParse(entries[14], out bool hasVariants))
            {
                return false;
            }
            definition = new TypeDefinition(category, typeId, level, entries[3], entries[4], maxDurability, rarity, entries[7], 
                element, armorType, Amalur.GetBuff(prefix), Amalur.GetBuff(suffix), coreEffects, effects, hasVariants);
            return true;
        }

        internal static IEnumerable<TypeDefinition> ParseFile(string path)
        {
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                if (TryLoadFromRow(line.Split(','), out var definition))
                {
                    yield return definition;
                }
            }
        }

        internal TypeDefinition(EquipmentCategory category, uint typeId, byte level, string name, string internalName, float maxDurability, Rarity rarity,
            string sockets, Element element, ArmorType armorType, Buff? prefix, Buff? suffix, Buff[] coreEffects, Buff[] effects, bool hasVariants)
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
            CoreEffects = coreEffects;
            Effects = effects;
            Prefix = prefix;
            Suffix = suffix;
            HasVariants = hasVariants;
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
        public Buff[] CoreEffects { get; }
        public Buff[] Effects { get; }
        public Buff? Prefix { get; }
        public Buff? Suffix { get; }
        public bool AffixableName { get; }
        public bool HasVariants { get; }
        public bool IsMerchant { get; }

        public string CategoryDisplayName => this switch
        {
            TypeDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Hat } => "Hood",
            TypeDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Feet } => "Boots",
            TypeDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Hands } => "Gloves",
            TypeDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Legs } => "Leggings",
            TypeDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Torso } => "Armor",
            TypeDefinition { ArmorType: ArmorType.Finesse, Category: EquipmentCategory.Shield } => "Buckler",
            TypeDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Hat } => "Helm",
            TypeDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Feet } => "Greaves",
            TypeDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Hands } => "Gauntlets",
            TypeDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Legs } => "Chausses",
            TypeDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Torso } => "Cuirass",
            TypeDefinition { ArmorType: ArmorType.Might, Category: EquipmentCategory.Shield } => "Kite Shield",
            TypeDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Hat } => "Cowl",
            TypeDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Feet } => "Shoes",
            TypeDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Hands } => "Handwraps",
            TypeDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Robes } => "Robes",
            TypeDefinition { ArmorType: ArmorType.Sorcery, Category: EquipmentCategory.Shield } => "Talisman",
            _ => Category.ToString()
        };
    }
}
