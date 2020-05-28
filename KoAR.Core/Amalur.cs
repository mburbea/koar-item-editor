using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public static List<EffectInfo> Effects { get; } = new List<EffectInfo>();
        public static Dictionary<uint, CoreEffectInfo> CoreEffects { get; } = new Dictionary<uint, CoreEffectInfo>();
        public static Dictionary<uint, EffectInfo> DedupedEffects { get; } = new Dictionary<uint, EffectInfo>();
        public static List<ItemMemoryInfo> Items { get; } = new List<ItemMemoryInfo>();
        private static int? _bagOffset;

        internal static byte[] Bytes { get; set; }

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

        public static bool IsFileOpen => Bytes != null;

        public static void Initialize(string path = null)
        {
            path ??= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
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
                {
                    Code = code,
                    DamageType = Enum.TryParse(parts[1], true, out DamageType damageType) ? damageType : default,
                    Tier = float.Parse(parts[2])
                });
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
                {
                    Code = uint.TryParse(element.Attribute("id").Value, NumberStyles.HexNumber, null, out var parsed) ? parsed : 0u,
                    DisplayText = element.Value.Trim()
                }));
            DedupedEffects.AddRange(Effects
                .Where(x => x.Code != 0)
                .GroupBy(x => x.Code)
                .Select(x => (x.Key, x.First())));
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
            static List<int> FindAllCandidates()
            {
                ReadOnlySpan<byte> sequence = new byte[] { 0x0B, 0x00, 0x00, 0x00, 0x68, 0xD5, 0x24, 0x00, 0x03 };
                ReadOnlySpan<byte> data = Bytes;
                var results = new List<int>();
                int ix = data.IndexOf(sequence);
                int start = 0;

                while (ix != -1)
                {
                    results.Add(start + ix - 4);
                    start += ix + sequence.Length;
                    ix = data.Slice(start).IndexOf(sequence);
                }
                return results;
            }

            Items.Clear();
            var candidates = FindAllCandidates();
            candidates.Add(Bytes.Length);
            for (int i = 0; i < candidates.Count - 1; i++)
            {
                if (ItemMemoryInfo.Create(candidates[i], candidates[i + 1]) is ItemMemoryInfo item)
                {
                    Items.Add(item);
                }
            }
        }

        public static void WriteEquipmentBytes(ItemMemoryInfo item)
        {
            static int WriteItem(int itemIndex, int dataLength, byte[] bytes)
            {
                int oldLength = Bytes.Length;
                Bytes = MemoryUtilities.ReplaceBytes(Bytes, itemIndex, dataLength, bytes);
                int delta = Bytes.Length - oldLength;
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

            item.CoreEffects.DataLength += WriteItem(item.CoreEffects.ItemIndex, item.CoreEffects.DataLength, item.CoreEffects.Serialize());
            item.DataLength += WriteItem(item.ItemIndex, item.DataLength, item.Serialize());
        }
    }
}