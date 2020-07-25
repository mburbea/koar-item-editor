using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        public static Dictionary<uint, Buff> BuffMap { get; } = new Dictionary<uint, Buff>();
        public static List<Buff> Buffs { get; } = new List<Buff>();
        public static Dictionary<uint, GemDefinition> GemDefinitions { get; } = new Dictionary<uint, GemDefinition>();
        public static Dictionary<uint, ItemDefinition> ItemDefinitions { get; } = new Dictionary<uint, ItemDefinition>();

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
            AddData(BuffMap, Buffs, buff => buff.Id);
            AddData(GemDefinitions, GemDefinition.ParseFile(GetPath("gemDefinitions.csv")), gemDef => gemDef.TypeId);
            AddData(ItemDefinitions, ItemDefinition.ParseFile(GetPath("definitions.csv")), itemDef => itemDef.TypeId);
            Debug.WriteLine(sw.Elapsed);

            string GetPath(string fileName)
            {
                var filePath = Path.Combine(path, fileName);
                return File.Exists(filePath)
                    ? filePath
                    : throw new InvalidOperationException($"Cannot find {fileName}");
            }

            static void AddData<TKey, TData>(Dictionary<TKey, TData> dictionary, IEnumerable<TData> data, Func<TData, TKey> getKey)
            {
                foreach (var datum in data)
                {
                    dictionary.Add(getKey(datum), datum);
                }
            }
        }

        internal static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class => dictionary.TryGetValue(key, out TValue? res) ? res : default;
    }
}
