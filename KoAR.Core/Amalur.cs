using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace KoAR.Core
{
    /// <summary>
    /// Archive Operation for Kingdoms of Amalur(supports 1.0.0.2)
    /// </summary>
    public static class Amalur
    {
        internal static readonly char[] Seperator = { ',' };

        internal static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class => dictionary.TryGetValue(key, out TValue? res) ? res : default;

        public static Dictionary<uint, ItemDefinition> ItemDefinitions { get; } = new Dictionary<uint, ItemDefinition>();
        public static List<Buff> Buffs { get; } = new List<Buff>();
        public static Dictionary<uint, Buff> BuffMap { get; } = new Dictionary<uint, Buff>();
        public static Dictionary<uint, GemDefinition> GemDefinitions { get; } = new Dictionary<uint, GemDefinition>();

        public static Buff GetBuff(uint buffId) =>
            BuffMap.TryGetValue(buffId, out var buff) ? buff : new Buff { Id = buffId, Name = "Unknown" };

        public static void Initialize(string? path = null)
        {
            var sw = Stopwatch.StartNew();
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            path ??= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            Buffs.AddRange(JsonSerializer.Deserialize<Buff[]>(File.ReadAllBytes(GetPath("buffs.json")), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            }));
            AddValues(BuffMap, Buffs, buff => buff.Id);
            AddValues(GemDefinitions, GemDefinition.ParseFile(GetPath("gemDefinitions.csv")), gem => gem.TypeId);
            AddValues(ItemDefinitions, ItemDefinition.ParseFile(GetPath("definitions.csv")), def => def.TypeId);
            Debug.WriteLine(sw.Elapsed);

            string GetPath(string fileName)
            {
                var filePath = Path.Combine(path, fileName);
                return File.Exists(filePath)
                    ? filePath
                    : throw new InvalidOperationException($"Cannot find {fileName}");
            }

            static void AddValues<TKey, TValue>(Dictionary<TKey, TValue> dictionary, IEnumerable<TValue> data, Func<TValue, TKey> getKey)
            {
                foreach (var value in data)
                {
                    dictionary.Add(getKey(value), value);
                }
            }
        }
    }
}