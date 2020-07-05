using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Linq;

namespace KoAR.Core
{
    /// <summary>
    /// Archive Operation for Kingdoms of Amalur(supports 1.0.0.2)
    /// </summary>
    public static class Amalur
    {
        private static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<(TKey, TValue)> data)
        {
            foreach (var (k, v) in data)
            {
                dictionary.Add(k, v);
            }
        }
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default) =>
            dictionary.TryGetValue(key, out var res) ? res : defaultValue;


        //public static Dictionary<uint, string> Buffs { get; } = new Dictionary<uint, string>();
        public static List<EffectInfo> Effects { get; } = new List<EffectInfo>();
        public static Dictionary<uint, CoreEffectInfo> CoreEffects { get; } = new Dictionary<uint, CoreEffectInfo>();
        public static Dictionary<uint, EffectInfo> DedupedEffects { get; } = new Dictionary<uint, EffectInfo>();
        public static Dictionary<uint, TypeDefinition> TypeDefinitions { get; } = new Dictionary<uint, TypeDefinition>();
        public static List<ItemMemoryInfo> Items { get; } = new List<ItemMemoryInfo>();
        public static Dictionary<uint, Buff> Buffs { get; } = new Dictionary<uint, Buff>();

        private static int? _bagOffset;

        internal static byte[] Bytes { get; set; } = Array.Empty<byte>();

        private static int _fileLengthOffset;
        private static int _simTypeOffset;
        private static Container ItemMemoryContainer;
        private static Container CoreEffectContainer;

        public static void ReadFile(string path)
        {
            _bagOffset = null;
            Bytes = File.ReadAllBytes(path);
            GetAllEquipment();
        }

        public static void SaveFile(string path)
        {
            if (!IsFileOpen)
            {
                return;
            }
            File.Copy(path, path + ".bak", true);
            File.WriteAllBytes(path, Bytes);
        }

        public static Stash? Stash { get; private set; }

        public static bool IsFileOpen => Bytes.Length != 0;

        public static void Initialize(string? path = null)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            var sw = Stopwatch.StartNew();
            path ??= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            TypeDefinitions.AddRange(TypeDefinition.ParseFile(Path.Combine(path, "items.csv")).Select(x => (x.TypeId, x)));

            var effectCsv = Path.Combine(path, "CoreEffects.csv");
            if (!File.Exists(effectCsv))
            {
                throw new InvalidOperationException("Cannot find CoreEffects.csv");
            }

            CoreEffects.AddRange(File.ReadLines(effectCsv).Skip(1).Select(row =>
            {
                var parts = row.Split(',');
                var code = uint.Parse(parts[0], NumberStyles.HexNumber);
                return (code, new CoreEffectInfo
                (
                    code,
                    Enum.TryParse(parts[1], true, out DamageType damageType) ? damageType : default,
                    float.Parse(parts[2])
                ));
            }));
            var propertiesXml = Path.Combine(path, "properties.xml");
            if (!File.Exists(propertiesXml))
            {
                throw new InvalidOperationException("Cannot find properties.xml");
            }
            using var stream = File.OpenRead(propertiesXml);
            Effects.AddRange(XDocument.Load(stream).Root
                .Elements()
                .Select(element => new EffectInfo
                (
                    uint.TryParse(element.Attribute("id").Value, NumberStyles.HexNumber, null, out var parsed) ? parsed : 0u,
                    element.Value.Trim()
                )));
            DedupedEffects.AddRange(Effects
                .Where(x => x.Code != 0)
                .GroupBy(x => x.Code)
                .Select(x => (x.Key, x.First())));

            var buffJson = Path.Combine(path, "buff.json");
            if (!File.Exists(buffJson))
            {
                throw new InvalidOperationException("Cannot find buff.json");
            }
            Buffs.AddRange(
            JsonSerializer.Deserialize<Buff[]>(
                File.ReadAllBytes(buffJson), new JsonSerializerOptions { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = {new JsonStringEnumConverter()} 
                })
              .Select(x => (x.Id, x)));

            Debug.WriteLine(sw.Elapsed);
            return;
        }

        private static int GetBagOffset()
        {
            ReadOnlySpan<byte> inventoryLimit = new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
            ReadOnlySpan<byte> increaseAmount = new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
            ReadOnlySpan<byte> currentInventoryCount = new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
            ReadOnlySpan<byte> span = Bytes;
            var curInvCountOffset = span.IndexOf(currentInventoryCount) + currentInventoryCount.Length;
            var inventoryLimitOffset = span.IndexOf(inventoryLimit) + inventoryLimit.Length;
            var increaseAmountOffset = span.IndexOf(increaseAmount) + increaseAmount.Length;
            var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
            var inventoryLimitOrder = inventoryLimitOffset == finalOffset
                ? 3
                : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;

            return finalOffset + (inventoryLimitOrder * 12);
        }

        public static int InventorySize
        {
            get => MemoryUtilities.Read<int>(Bytes, _bagOffset ??= GetBagOffset());
            set => MemoryUtilities.Write(Bytes, _bagOffset ??= GetBagOffset(), value);
        }

        public static void GetAllEquipment()
        {
            ReadOnlySpan<byte> typeIdSeq = new byte[] { 0x23, 0xCC, 0x58, 0x00, 0x03 };
            ReadOnlySpan<byte> fileLengthSeq = new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 };
            ReadOnlySpan<byte> ItemEffectMarker = new byte[5] { 0xD3, 0x34, 0x43, 0x00, 0x00 }; // 26 to first item. 5 to first DL, 13 to second DL. 18 for count
            ReadOnlySpan<byte> coreEffectMarker = new byte[5] { 0xBB, 0xD5, 0x43, 0x00, 0x00 }; // 26 to first item. 5 to first DL, 13 to second DL. 18 for count
            ReadOnlySpan<byte> data = Bytes;

            _fileLengthOffset = data.IndexOf(fileLengthSeq) - 4;
            ItemMemoryContainer = new Container(data.IndexOf(ItemEffectMarker), 0x00_24_D5_68_00_00_00_0Bul);
            CoreEffectContainer = new Container(data.IndexOf(coreEffectMarker), 0x00_28_60_84_00_00_00_0Bul);
            var itemMemoryLocs = ItemMemoryContainer.ToDictionary(x => x.id, x => (x.offset, x.datalength));
            var coreLocs = CoreEffectContainer.ToDictionary(x => x.id, x => (x.offset, x.datalength));
            Items.Clear();
            
            Stash = Stash.TryCreateStash();

            _simTypeOffset = data.IndexOf(typeIdSeq);
            int ixOfActor = _simTypeOffset + 9;
            if (BitConverter.ToInt32(Bytes, ixOfActor) == 0)
            {
                ixOfActor += 4;
            }

            while (BitConverter.ToInt32(Bytes, ixOfActor) == 0x00_75_2D_06)
            {
                var datalength = 9 + BitConverter.ToInt32(Bytes, ixOfActor + 5);
                var id = BitConverter.ToInt32(Bytes, ixOfActor + 9);
                var typeId = BitConverter.ToUInt32(Bytes, ixOfActor + 13);

                if (TypeDefinitions.TryGetValue(typeId, out var definition))
                {
                    var (itemOffset,itemLength) = itemMemoryLocs[id];
                    var (coreOffset, coreLength) = coreLocs[id];
                    Items.Add(new ItemMemoryInfo(ixOfActor + 13, itemOffset, itemLength, coreOffset, coreLength));
                }
                ixOfActor += datalength;
            }
        }

        public static void WriteEquipmentBytes(ItemMemoryInfo item, bool forced = false)
        {
            static int WriteItem(int itemIndex, int oldLength, byte[] bytes)
            {
                Bytes = MemoryUtilities.ReplaceBytes(Bytes, itemIndex, oldLength, bytes);
                int delta = bytes.Length - oldLength;
                if (delta != 0)
                {
                    foreach (var item in Items)
                    {
                        if (item.CoreEffects.ItemIndex > itemIndex)
                        {
                            item.CoreEffects.ItemIndex += delta;
                        }
                        if (item.ItemIndex > itemIndex)
                        {
                            item.ItemIndex += delta;
                        }
                    }
                }
                return delta;
            }

            var delta = WriteItem(item.CoreEffects.ItemIndex, item.CoreEffects.DataLength, item.CoreEffects.Serialize(forced));
            if(delta != 0)
            {
                CoreEffectContainer.UpdateDataLength(delta);
            }
            delta = WriteItem(item.ItemIndex, item.DataLength, item.Serialize(forced));
            if (delta != 0)
            {
                ItemMemoryContainer.UpdateDataLength(delta);
            }

        }
    }
}