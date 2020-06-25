using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace KoAR.Core
{
    public class TypeDefinition
    {
        private static bool TryParseEffectList(string value, [NotNullWhen(true)] out uint[]? effects)
        {
            effects = null;
            if (value.Length % 6 != 0)
            {
                return false;
            }
            var results = value.Length == 0 ? Array.Empty<uint>() : new uint[value.Length / 6];
            for (int i = 0; i < results.Length; i++)
            {
                if (!uint.TryParse(value.Substring(i * 6, 6), NumberStyles.HexNumber, null, out uint effect))
                {
                    return false;
                }
                results[i] = effect;
            }
            effects = results;
            return true;
        }

        private static bool TryLoadFromRow(string[] entries, [NotNullWhen(true)] out TypeDefinition? definition)
        {
            // category,typeId,level,name
            // internal_name,durability,sockets,rarity
            // element,armorType,coreEffects,effects
            definition = null;
            if (entries.Length != 15
                || !Enum.TryParse(entries[0], true, out EquipmentCategory category)
                || !uint.TryParse(entries[1], NumberStyles.HexNumber, null, out uint typeId)
                || !byte.TryParse(entries[2], out byte level)
                || !float.TryParse(entries[5], out float maxDurability)
                || !Enum.TryParse(entries[6], out Rarity rarity)
                || !int.TryParse(entries[7], out int weapon)
                || !int.TryParse(entries[8], out int armor)
                || !int.TryParse(entries[9], out int utility)
                || !int.TryParse(entries[10], out int epic)
                || !Enum.TryParse(entries[11], out Element element)
                || !Enum.TryParse(entries[12], out ArmorType armorType)
                || !TryParseEffectList(entries[13], out uint[]? coreEffects)
                || !TryParseEffectList(entries[14], out uint[]? effects))
            {
                return false;
            }

            definition = new TypeDefinition(category, typeId, level, entries[3], entries[4], maxDurability, rarity, weapon, armor, utility, epic, element, armorType, coreEffects, effects);
            return true;
        }

        internal static IEnumerable<TypeDefinition> ParseFile(string path)
        {
            using var parser = new TextFieldParser(path)
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = new[] { "," },
                HasFieldsEnclosedInQuotes = true
            };
            while (!parser.EndOfData)
            {
                if (TryLoadFromRow(parser.ReadFields(), out var definition))
                {
                    yield return definition;
                }
            }
        }

        internal void WriteToCsv()
        {
            IEnumerable<string> rows = new[] {
                $"{Category},{TypeId:X6},{Level},\"{Name}\",{MaxDurability},{string.Join("", CoreEffects.Select(x => x.ToString("X6")))},{string.Join("", Effects.Select(x => x.ToString("X6")))}"
            };
            if (!File.Exists("items.user.csv"))
            {
                rows = rows.Prepend("Category,TypeId,Level,ItemName,Durability,CoreEffects,Effects");
            }
            File.AppendAllLines("items.user.csv", rows);
        }

        internal TypeDefinition(EquipmentCategory category, uint typeId, byte level, string name, string internalName, float maxDurability, Rarity rarity,
            int weapon, int armor, int utility, int epic, Element element, ArmorType armorType, uint[] coreEffects, uint[] effects)
        {
            Category = category;
            TypeId = typeId;
            Level = level;
            Name = name;
            InternalName = internalName;
            MaxDurability = maxDurability;
            Rarity = rarity;
            WeaponSockets = weapon;
            ArmorSockets = armor;
            UtilitySockets = utility;
            EpicSockets = epic;
            ArmorType = armorType;
            Element = element;
            CoreEffects = coreEffects;
            Effects = effects;
        }

        public EquipmentCategory Category { get; }
        public uint TypeId { get; }
        public byte Level { get; }
        public string Name { get; }
        public string InternalName { get; }
        public float MaxDurability { get; }
        public Rarity Rarity { get; }
        public int WeaponSockets { get; }
        public int ArmorSockets { get; }
        public int UtilitySockets { get; }
        public int EpicSockets { get; }
        public Element Element { get; }
        public ArmorType ArmorType { get; }
        public uint[] CoreEffects { get; }
        public uint[] Effects { get; }
    }
}
