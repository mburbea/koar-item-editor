using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            using (var buffsStream = GetStream("buffs.json"))
            {
                using var reader = new StreamReader(buffsStream);
                Buffs = JsonSerializer.Deserialize<Buff[]>(reader.ReadToEnd(), jsonOptions).ToDictionary(buff => buff.Id);
            }
            using (var gemsStream = GetStream("gemDefinitions.csv"))
            {
                GemDefinitions = GemDefinition.ParseFile(gemsStream).ToDictionary(def => def.TypeId);
            }
            using (var itemsStream = GetStream("definitions.csv"))
            {
                ItemDefinitions = ItemDefinition.ParseFile(itemsStream).ToDictionary(def => def.TypeId);
            }
            using (var questItemsStream = GetStream("questItemDefinitions.json"))
            {
                using var reader = new StreamReader(questItemsStream);
                QuestItemDefinitions = JsonSerializer.Deserialize<QuestItemDefinition[]>(reader.ReadToEnd(), jsonOptions).ToDictionary(def => def.Id);
            }
            Debug.WriteLine(sw.Elapsed);

            static Stream GetStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(Amalur).Namespace}.Data.{name}");
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
    }
}
