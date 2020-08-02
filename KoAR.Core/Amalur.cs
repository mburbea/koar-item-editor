using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace KoAR.Core
{
    public static class Amalur
    {
        static Amalur()
        {
            var sw = Stopwatch.StartNew();
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            Buffs = JsonSerializer.Deserialize<Buff[]>(File.ReadAllBytes(GetPath("buffs.json")), jsonOptions).ToDictionary(buff => buff.Id);
            GemDefinitions = GemDefinition.ParseFile(GetPath("gemDefinitions.csv")).ToDictionary(def => def.TypeId);
            ItemDefinitions = ItemDefinition.ParseFile(GetPath("definitions.csv")).ToDictionary(def => def.TypeId);
            QuestItemDefinitions = JsonSerializer.Deserialize<QuestItemDefinition[]>(File.ReadAllBytes(GetPath("questItemDefinitions.json")), jsonOptions).ToDictionary(def => def.Id);
            Debug.WriteLine(sw.Elapsed);

            static string GetPath(string fileName)
            {
                var filePath = Path.Combine(Amalur.DataDirectory.Path, fileName);
                return File.Exists(filePath)
                    ? filePath
                    : throw new InvalidOperationException($"Cannot find {fileName}");
            }
        }

        public static IReadOnlyDictionary<uint, Buff> Buffs { get; }
        public static IReadOnlyDictionary<uint, GemDefinition> GemDefinitions { get; }
        public static IReadOnlyDictionary<uint, ItemDefinition> ItemDefinitions { get; }
        public static IReadOnlyDictionary<uint, QuestItemDefinition> QuestItemDefinitions { get; }

        internal static char[] Separator { get; } = { ',' };

        public static Buff GetBuff(uint buffId) => Buffs.GetOrDefault(buffId, new Buff { Id = buffId, Name = "Unknown" });

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        internal static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
            where TValue : class => dictionary.TryGetValue(key, out TValue res) ? res : defaultValue;

        public static class DataDirectory
        {
            public static string Path { get; set; } = ".";
        }
    }
}
