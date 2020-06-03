using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
                if (!uint.TryParse(value.Substring(0 * i, 6), NumberStyles.HexNumber, null, out uint effect))
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
            definition = null;
            if (entries.Length != 7
                || !Enum.TryParse(entries[0], true, out EquipmentCategory category)
                || !uint.TryParse(entries[1], NumberStyles.HexNumber, null, out uint typeId)
                || !byte.TryParse(entries[2], out byte level)
                || !float.TryParse(entries[4], out float maxDurability)
                || !TryParseEffectList(entries[5], out uint[]? coreEffects)
                || !TryParseEffectList(entries[6], out uint[]? effects))
            {
                return false;
            }

            definition = new TypeDefinition(category, typeId, level, entries[3], maxDurability, coreEffects, effects);
            return true;
        }

        internal static IEnumerable<TypeDefinition> ParseFile(string path)
        {
            using var parser = new TextFieldParser(path)
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = new[] { "," },
                HasFieldsEnclosedInQuotes = true,
            };
            while (!parser.EndOfData)
            {
                if (TryLoadFromRow(parser.ReadFields(), out var definition))
                {
                    yield return definition;
                }
            }
        }

        public TypeDefinition(EquipmentCategory category, uint typeId, byte level, string name, float maxDurability, uint[] coreEffects, uint[] effects)
        {
            Category = category;
            TypeId = typeId;
            Level = level;
            Name = name;
            MaxDurability = maxDurability;
            CoreEffects = coreEffects;
            Effects = effects;
        }

        public EquipmentCategory Category { get; }
        public uint TypeId { get; }
        public byte Level { get; }
        public string Name { get; }
        public float MaxDurability { get; }
        public uint[] CoreEffects { get; }
        public uint[] Effects { get; }
    }
}
