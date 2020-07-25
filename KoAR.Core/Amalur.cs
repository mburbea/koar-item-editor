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
        private static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<(TKey, TValue)> data)
        {
            foreach (var (k, v) in data)
            {
                dictionary.Add(k, v);
            }
        }

        internal static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class =>
            dictionary.TryGetValue(key, out TValue? res) ? res : default;

        public static Dictionary<uint, ItemDefinition> ItemDefinitions { get; } = new Dictionary<uint, ItemDefinition>();
        public static List<Buff> Buffs { get; } = new List<Buff>();
        public static Dictionary<uint, Buff> BuffMap { get; } = new Dictionary<uint, Buff>();
        public static Dictionary<uint, GemDefinition> GemDefinitions { get; } = new Dictionary<uint, GemDefinition>();

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
            BuffMap.AddRange(Buffs.Select(x => (x.Id, x)));
            GemDefinitions.AddRange(GemDefinition.ParseFile(GetPath("gemDefinitions.csv")).Select(x => (x.TypeId, x)));
            ItemDefinitions.AddRange(ItemDefinition.ParseFile(GetPath("definitions.csv")).Select(x => (x.TypeId, x)));
            Debug.WriteLine(sw.Elapsed);

            string GetPath(string fileName)
            {
                var filePath = Path.Combine(path, fileName);
                return File.Exists(filePath)
                    ? filePath
                    : throw new InvalidOperationException($"Cannot find {fileName}");
            }
        }

        public static Buff GetBuff(uint buffId) => BuffMap.TryGetValue(buffId, out var buff)
                    ? buff : new Buff { Id = buffId, Name = "Unknown" };
    }
}