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
        public static List<EffectInfo> Effects { get; } = new List<EffectInfo>();
        public static Dictionary<uint, CoreEffectInfo> CoreEffects { get; } = new Dictionary<uint, CoreEffectInfo>();
        internal static Dictionary<uint, EffectInfo> DedupedEffects = new Dictionary<uint, EffectInfo>();

        internal static byte[] Bytes { get; set; }

        public static void ReadFile(string path) => Bytes = File.ReadAllBytes(path);

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

            foreach (var row in File.ReadLines(effectCsv).Skip(1))
            {
                var parts = row.Split(',');
                var code = uint.Parse(parts[0], NumberStyles.HexNumber);
                CoreEffects[code] = new CoreEffectInfo
                {
                    Code = code,
                    DamageType = Enum.TryParse(parts[1], true, out DamageType damageType) ? damageType : default,
                    Tier = float.Parse(parts[2])
                };
            }

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
            DedupedEffects = Effects.Where(x=> x.Code != 0).GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.First());
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

        public static int GetMaxBagCount() => MemoryUtilities.Read<int>(Bytes, GetBagOffset());

        public static void EditMaxBagCount(int count) => MemoryUtilities.Write(Bytes, GetBagOffset(), count);

        public static List<ItemMemoryInfo> GetAllEquipment()
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

            List<ItemMemoryInfo> equipmentList = new List<ItemMemoryInfo>();
            var candidates = FindAllCandidates();
            candidates.Add(Bytes.Length);
            for (int i = 0; i < candidates.Count - 1; i++)
            {
                if (ItemMemoryInfo.Create(candidates[i], candidates[i + 1]) is ItemMemoryInfo item)
                {
                    equipmentList.Add(item);
                }
            }
            return equipmentList;
        }

        public static void WriteEquipmentBytes(ItemMemoryInfo equipment, out bool lengthChanged)
        {
            var bytes = Bytes;
            var oldLength = bytes.Length;
            var coreMemory = equipment.CoreEffects;
            bytes = MemoryUtilities.ReplaceBytes(bytes, coreMemory.ItemIndex, coreMemory.DataLength, coreMemory.Bytes);
            bytes = MemoryUtilities.ReplaceBytes(bytes, equipment.ItemIndex, equipment.DataLength, equipment.ItemBytes);
            Bytes = bytes;
            lengthChanged = Bytes.Length != oldLength;
        }
    }
}