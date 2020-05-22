using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <summary>
        /// The head of the equipment, property and indicate the number of attributes of the data relative to equipment data head offset
        /// </summary>
        public static List<EffectInfo> Effects { get; } = new List<EffectInfo>();
        public static Dictionary<string, CoreEffectInfo> CoreEffects { get; } = new Dictionary<string, CoreEffectInfo>(StringComparer.OrdinalIgnoreCase);
        private static ReadOnlySpan<byte> EquipmentSequence => new byte[]     { 0x0B, 0x00, 0x00, 0x00, 0x68, 0xD5, 0x24, 0x00, 0x03 };
        private static byte[] _bytes;

        public static byte[] Bytes
        {
            get => _bytes ?? throw new Exception("Save file not open");
            set => _bytes = value;
        }

        /// <summary>
        /// Read save-file
        /// </summary>
        /// <param name="path">archive path</param>
        public static void ReadFile(string path) => _bytes = File.ReadAllBytes(path);

        /// <summary>
        /// Save save-file
        /// </summary>
        /// <param name="path">save path</param>
        public static void SaveFile(string path) => File.WriteAllBytes(path, Bytes);

        public static bool IsFileOpen => _bytes != null;
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
                var code = parts[0];
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
                    Code = element.Attribute("id").Value.ToUpperInvariant(),
                    DisplayText = element.Value.Trim()
                }));
        }

        private static int GetBagOffset()
        {
            ReadOnlySpan<byte> InventoryLimit = new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
            ReadOnlySpan<byte> IncreaseAmount = new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
            ReadOnlySpan<byte> CurrentInventoryCount = new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
            ReadOnlySpan<byte> span = Bytes;
            var curInvCountOffset = span.IndexOf(CurrentInventoryCount) + CurrentInventoryCount.Length;
            var inventoryLimitOffset = span.IndexOf(InventoryLimit) + InventoryLimit.Length;
            var increaseAmountOffset = span.IndexOf(IncreaseAmount) + IncreaseAmount.Length;
            var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
            var inventoryLimitOrder = inventoryLimitOffset == finalOffset
                ? 3
                : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;

            return finalOffset + (inventoryLimitOrder * 12);
        }

        public static int GetMaxBagCount() => MemoryUtilities.Read<int>(Bytes, GetBagOffset());

        public static void EditMaxBagCount(int count) => MemoryUtilities.Write(Bytes, GetBagOffset(), count);

        public static List<EffectInfo> GetEffectList(ItemMemoryInfo item)
        {
            var itemEffects = item.ReadEffects();
            foreach (EffectInfo attInfo in itemEffects)
            {
                attInfo.DisplayText = Effects.FirstOrDefault(x => x.Code == attInfo.Code)?.DisplayText ?? "Unknown";
            }

            return itemEffects;
        }

        public static List<ItemMemoryInfo> GetAllEquipment()
        {
            static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
            {
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
            var bytes = Bytes;
            var indexList = GetAllIndices(bytes, EquipmentSequence);
            indexList.Add(bytes.Length);
            for (int i = 0; i < indexList.Count - 1; i++)
            {
                if (ItemMemoryInfo.Create(bytes, indexList[i], indexList[i + 1]) is ItemMemoryInfo item)
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