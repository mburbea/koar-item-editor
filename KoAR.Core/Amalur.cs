using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace KoAR.Core
{
    public static class Amalur
    {
        internal static char[] Separator { get; } = { ',' };
        public static Dictionary<uint, Buff> BuffMap { get; } = new Dictionary<uint, Buff>();
        public static List<Buff> Buffs { get; } = new List<Buff>();
        public static Dictionary<uint, GemDefinition> GemDefinitions { get; } = new Dictionary<uint, GemDefinition>();
        public static Dictionary<uint, ItemDefinition> ItemDefinitions { get; } = new Dictionary<uint, ItemDefinition>();
        public static Dictionary<uint, QuestItemDefinition> QuestItemDefinitions { get; } = new Dictionary<uint, QuestItemDefinition>();

        public static Buff GetBuff(uint buffId) => BuffMap.GetOrDefault(buffId, new Buff { Id = buffId, Name = "Unknown" });

        public static void Initialize(string? path = null)
        {
            var sw = Stopwatch.StartNew();
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            path ??= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var serializationOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            Buffs.AddRange(JsonSerializer.Deserialize<Buff[]>(File.ReadAllBytes(GetPath("buffs.json")), serializationOptions));
            BuffMap.AddRange(Buffs, buff => buff.Id);
            GemDefinitions.AddRange(GemDefinition.ParseFile(GetPath("gemDefinitions.csv")), gemDef => gemDef.TypeId);
            ItemDefinitions.AddRange(ItemDefinition.ParseFile(GetPath("definitions.csv")), itemDef => itemDef.TypeId);
            QuestItemDefinitions.AddRange(JsonSerializer.Deserialize<QuestItemDefinition[]>(File.ReadAllBytes(GetPath("questItemDefinitions.json")), serializationOptions), questItemDef => questItemDef.Id);
            Debug.WriteLine(sw.Elapsed);

            string GetPath(string fileName)
            {
                var filePath = Path.Combine(path, fileName);
                return File.Exists(filePath)
                    ? filePath
                    : throw new InvalidOperationException($"Cannot find {fileName}");
            }
        }

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        internal static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
            where TValue : class => dictionary.TryGetValue(key, out TValue res) ? res : defaultValue;

        private static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TValue> values, Func<TValue, TKey> getKey)
        {
            foreach (var value in values)
            {
                dictionary.Add(getKey(value), value);
            }
        }
    }
}
