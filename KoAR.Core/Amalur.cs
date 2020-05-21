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
        private static ReadOnlySpan<byte> InventoryLimit => new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
        private static ReadOnlySpan<byte> IncreaseAmount => new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
        private static ReadOnlySpan<byte> CurrentInventoryCount => new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
        private static ReadOnlySpan<byte> EquipmentSequence => new byte[]     { 0x0B, 0x00, 0x00, 0x00, 0x68, 0xD5, 0x24, 0x00, 0x03 };
        private static ReadOnlySpan<byte> CoreAttributeSequence => new byte[] { 0x84, 0x60, 0x28, 0x00, 0x00 };
        private static ReadOnlySpan<byte> WeaponTypeSequence => new byte[]     { 0xD4, 0x08, 0x46, 0x00, 0x01 };
        private static ReadOnlySpan<byte> AdditionalInfoSequence => new byte[]    { 0x8D, 0xE3, 0x47, 0x00, 0x02 };
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
        public static void ReadFile(string path)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.Open);
                _bytes = new byte[fs.Length];
                fs.Read(_bytes, 0, (int)fs.Length);
            }
            catch
            {
                throw new Exception("File cannot open!");
            }
        }

        /// <summary>
        /// Save save-file
        /// </summary>
        /// <param name="path">save path</param>
        public static void SaveFile(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                fs.Write(Bytes, 0, Bytes.Length);
            }
            catch
            {
                throw new Exception("Saving failed!");
            }
        }

        public static bool IsInitialized => _bytes != null;
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
                CoreEffects[parts[0]] = new CoreEffectInfo
                {
                    Code = parts[0],
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

            static EquipmentType DetermineEquipmentType(ReadOnlySpan<byte> bytes, ItemMemoryInfo item)
            {
                Span<byte> buffer = stackalloc byte[13];
                item.ItemBytes.AsSpan(0, 8).CopyTo(buffer);
                WeaponTypeSequence.CopyTo(buffer.Slice(8));
                var offset = bytes.IndexOf(buffer);
                if (offset == -1)
                {
                    return EquipmentType.Armor; // Armor doesn't have this section.
                }
                var equipTypeByte = bytes[offset + 13];
                AdditionalInfoSequence.CopyTo(buffer.Slice(8));
                var aisOffset = bytes.IndexOf(buffer);
                var demystifyer = bytes[aisOffset + 17];
                var word = MemoryUtilities.Read<int>(bytes, aisOffset + 17);
                var word2 = MemoryUtilities.Read<int>(item.ItemBytes, 13);
                if (equipTypeByte == 0x14)
                {
                    if (demystifyer == 0)
                    {
                        Console.WriteLine("a");
                    }
                }

                return equipTypeByte switch
                {
                    0x10 => EquipmentType.Shield,
                    0x18 => EquipmentType.LongBow,
                    0x20 => demystifyer switch
                    {
                        0x00 => EquipmentType.LongSword,
                        0xBC => EquipmentType.LongSword,
                        0x55 => EquipmentType.LongSword,
                        0x56 => EquipmentType.LongSword,
                        0x18 => EquipmentType.LongSword,
                        _ => EquipmentType.GreatSword,
                    },
                    0x24 => demystifyer switch
                    {
                        0x00 => EquipmentType.Daggers,
                        0x40 => EquipmentType.Daggers,
                        0x41 => EquipmentType.Daggers,
                        0x2C => EquipmentType.Daggers,
                        0xE8 => EquipmentType.Daggers,
                        _ => EquipmentType.FaeBlades
                    },
                    0x1C => demystifyer switch
                    {
                        0x3E => EquipmentType.Chakrams,
                        0x3F => EquipmentType.Chakrams,
                        0xEA => EquipmentType.Chakrams,
                        0xEB => EquipmentType.Chakrams,
                        0xEC => EquipmentType.Hammer,
                        0x43 => EquipmentType.Hammer,
                        0x7E => EquipmentType.Hammer,
                        0x18 => EquipmentType.Staff,
                        0x53 => EquipmentType.Staff,
                        0x54 => EquipmentType.Staff,
                        0x00 => EquipmentType.Staff,
                        _ => EquipmentType.Unknown
                    },
                    0x14 => demystifyer switch
                    {
                        0x1D => EquipmentType.Talisman,
                        0x4A => EquipmentType.Sceptre,
                        0x47 => EquipmentType.Sceptre,
                        0x48 => EquipmentType.Sceptre,
                        0x1B => EquipmentType.Buckler,
                        0xCA => EquipmentType.Buckler,
                        0x18 => EquipmentType.Talisman,
                        0xC9 => EquipmentType.Talisman,
                        0xAF => EquipmentType.Talisman,
                        0x00 => item.ItemBytes[13] switch
                        {
                            0x33 => EquipmentType.Buckler,
                            0x23 => EquipmentType.Buckler,
                            0x2B => EquipmentType.Buckler,
                            0x3b => EquipmentType.Talisman,
                            0x00 => EquipmentType.Buckler,
                            _ => throw null,
                        },

                        //0x33 => EquipmentType.Buckler,//why are there two buckler codes?
                        // 0x3E => EquipmentType.Buckler,
                        // 0x2B => EquipmentType.FlameTalisman,// Shock talisman's are here too :(
                        // 0x23 => EquipmentType.FrostTalisman,
                        // 0x3F => EquipmentType.ShockTalisman,// might only be crafted shock talisman.*/
                        _ => EquipmentType.Unknown
                    },
                    _ => throw null,
                };
            }

            List<ItemMemoryInfo> equipmentList = new List<ItemMemoryInfo>();
            var bytes = Bytes;
            var indexList = GetAllIndices(bytes, EquipmentSequence);
            var bins = new Dictionary<string, int>();
            var coreHeader = new byte[13];
            var seqs = new Dictionary<ulong, int>();
            CoreAttributeSequence.CopyTo(coreHeader.AsSpan(8));
            for (int i = 0; i < indexList.Count; i++)
            {
                if (ItemMemoryInfo.Create(indexList[i], i == indexList.Count - 1
                    ? bytes.AsSpan(indexList[i])
                    : bytes.AsSpan(indexList[i], indexList[i + 1] - indexList[i])) is ItemMemoryInfo item)
                {
                    var wtf = BitConverter.ToUInt64(item.ItemBytes, 13);
                    seqs.TryGetValue(wtf, out var c2);
                    seqs[wtf] = c2 + 1;
                    item.ItemBytes.AsSpan(0, 8).CopyTo(coreHeader);
                    var span = bytes.AsSpan(indexList[i]);
                    int coreOffset = span.IndexOf(coreHeader) + indexList[i];
                    item.CoreEffects = new CoreEffectList(coreOffset, bytes.AsSpan(coreOffset));
                    item.EquipmentType = DetermineEquipmentType(bytes, item);
                    equipmentList.Add(item);

                    var b13 = item.CoreEffects.MysteryInteger;
                    var itemName = item.ItemName;
                    var binStr = b13.ToString("X2");
                    bins.TryGetValue(binStr, out var c);
                    bins[binStr] = c + 1;
                    if (item.CoreEffects.MysteryInteger == 0x40)
                    {
                        Console.WriteLine("WTF");
                    }
                }
            }
            var max = equipmentList.Max(x => x.CoreEffects.Count);
            
            Console.WriteLine(max);
            Console.WriteLine(bins);
            Console.WriteLine(seqs);
            var dict2 = seqs.ToDictionary(x => string.Join(" ", BitConverter.GetBytes(x.Key).Select(x => x.ToString("X2"))));
            return equipmentList;
        }

        /// <summary>
        /// Delete Equipment
        /// </summary>
        /// <param name="weapon"></param>
        public static void DeleteEquipment(ItemMemoryInfo equipment)
            => Bytes = MemoryUtilities.ReplaceBytes(Bytes, equipment.ItemIndex, equipment.DataLength, equipment.ItemBytes);

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