using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KoAR.Core;

namespace ItemTesting
{

    //    static class Program
    //    {
    //        static byte[] GetBytesFromText(string text)
    //        {
    //            List<byte> list = new List<byte>();
    //            foreach (string word in text.Trim().Split(' '))
    //            {
    //                string txt = word.Trim();
    //                list.Add(byte.Parse(txt, NumberStyles.HexNumber));
    //            }
    //            return list.ToArray();
    //        }

    //        static string GetBytesString(byte[] b) => string.Join(' ', b.Select(x => x.ToString("X2")));
    //        static void PrintByteString(byte[] b) => Console.WriteLine(GetBytesString(b));
    //        static void WriteByteArray(byte[] b, string path) => File.WriteAllText(path, string.Join(", ", b.Select(x => $"0x{x:X2}")));
    //        static void PrintRuler()
    //        {
    //            Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
    //            Console.WriteLine(new string('-', 120));
    //        }

    //        static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
    //        {
    //            var results = new List<int>();
    //            int ix = data.IndexOf(sequence);
    //            int start = 0;

    //            while (ix != -1)
    //            {
    //                results.Add(start + ix);
    //                start += ix + sequence.Length;
    //                ix = data.Slice(start).IndexOf(sequence);
    //            }
    //            return results;
    //        }


    //        private static char[] AllCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    //        private static DisposableLocalDb LocalDb;

    //        public static void BulkInsertTable(string name, IEnumerable<object[]> table,
    //            string initializer = "id = 0,hex=space(8), name = space(8000)")
    //        {
    //            using var conn = new SqlConnection(LocalDb.ConnectionString);
    //            conn.Open();
    //            using var cmd = new SqlCommand($"select {initializer} into {name} where 1=0;", conn);
    //            cmd.ExecuteNonQuery();
    //            using var bulk = new SqlBulkCopy(conn)
    //            {
    //                DestinationTableName = name,
    //                BatchSize = 10_000,
    //                EnableStreaming = true
    //            };
    //            using var adapter = new DbDataReaderAdapter(table);
    //            bulk.WriteToServer(adapter);
    //        }
    static class Program
    {

        static void ConvertSymbolsToLua(string inPath, string outPath)
        {
            foreach (var file in Directory.EnumerateFiles(inPath, "symbol_table_*.bin", SearchOption.TopDirectoryOnly))
            {
                var fileInfo = new FileInfo(file);
                var data = File.ReadAllBytes(file);
                var elementCount = BitConverter.ToInt32(data, 0);
                var firstString = 8 + elementCount * 12;

                File.WriteAllText(Path.Combine(outPath, fileInfo.Name["symbol_table_".Length..^4] + ".lua"),
                    "{\n" + string.Join(",\n", Enumerable.Range(0, elementCount)
                    .Select(x => (id: BitConverter.ToInt32(data, 4 + x * 12), s: BitConverter.ToInt32(data, 4 + x * 12 + 4), e: BitConverter.ToInt32(data, 4 + x * 12 + 8)))
                    .Select(y => $"{{'{y.id:X6}','{Encoding.Default.GetString(data[(firstString + y.s)..(firstString + y.e - 1)])}'}}")) + "\n}");
            }
        }

        static void ConvertSymbolsToCsv(string inPath, string outPath)
        {
            foreach (var file in Directory.EnumerateFiles(inPath, "symbol_table_*.bin", SearchOption.TopDirectoryOnly))
            {
                var fileInfo = new FileInfo(file);
                var data = File.ReadAllBytes(file);
                var elementCount = BitConverter.ToInt32(data, 0);
                var firstString = 8 + elementCount * 12;

                File.WriteAllLines(Path.Combine(outPath, fileInfo.Name["symbol_table_".Length..^4] + ".csv"),
                    Enumerable.Range(0, elementCount)
                    .Select(x => (id: BitConverter.ToInt32(data, 4 + x * 12), s: BitConverter.ToInt32(data, 4 + x * 12 + 4), e: BitConverter.ToInt32(data, 4 + x * 12 + 8)))
                    .Select(y => $"{y.id},{Encoding.Default.GetString(data[(firstString + y.s)..(firstString + y.e - 1)])}"));
            }
        }

        //static void Main()
        //{
        //    const string path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save77.sav";
        //    //const string path = @"..\..\..\..\9190114save90.sav";
        //    Amalur.Initialize(@"..\..\..\..\Koar.SaveEditor\");
        //    GameSave gameSave = new GameSave(path);

        //    Console.WriteLine(gameSave.Stash.Items.Count);
        //    foreach (var item in gameSave.Stash.Items)
        //    {
        //        Console.WriteLine(item.Definition.Name);
        //    }
        //}

        static Dictionary<uint, string> Dict = BuildSimtypeDict();

        private static Dictionary<uint, string> BuildSimtypeDict()
        {
            var data = File.ReadAllBytes(@"C:\e\symbol_table_simtype.bin");
            var elementCount = BitConverter.ToInt32(data, 0);
            var firstString = 8 + elementCount * 12;
            return Enumerable.Range(0, elementCount).Select(x => (id: BitConverter.ToUInt32(data, 4 + x * 12), s: BitConverter.ToInt32(data, 4 + x * 12 + 4), e: BitConverter.ToInt32(data, 4 + x * 12 + 8)))
                .ToDictionary(x => x.id, y => Encoding.Default.GetString(data[(firstString + y.s)..(firstString + y.e - 1)]));
        }

        static void Main()
        {
            ConvertSymbolsToCsv(@"C:\e\", @"C:\e\o");
            const string path = @"C:\Program Files (x86)\Steam\userdata\107335713\1041720\remote\autocloud\save\svd_fmt_5_19.sav";
            GameSave gs = new(path);
            foreach (var item in gs.Items.Where(x=> x.Definition.Category == EquipmentCategory.Unknown))
            {
                var id = item.Definition.TypeId;
                if (item.ItemSockets is null) continue;
                var n = Dict[id];
                if (n.StartsWith("bag_") || n.StartsWith("recipe_") || n.StartsWith("alchemypotion_")) continue;
                Console.WriteLine($"{Dict[id]},{item.ItemBuffs.Prefix?.Id},{item.ItemBuffs.Suffix?.Id}");

            }
            Console.Read();
        }
    }
}
//        {
//            static string FormatAsStr(IEnumerable<uint> effects) => string.Join("", effects.Select(x => $"{x:X6}"));

//            //const string path = @"..\..\..\..\9190114save90.sav";
//            const string path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save3.sav";
//            Amalur.Initialize(@"..\..\..\..\Koar.SaveEditor\");
//            Amalur.ReadFile(path);
//            foreach(var g in CoreEffectMemory.SetOfInstances.GroupBy(x=> x.instanceId))
//            {
//                Console.WriteLine($"{g.Key:X8}\t{string.Join(',',g.Select(x => x.offset).Distinct())}\t{g.Count()}\t{g.Count(x=> x.hasSuffix && x.hasPrefix)}\t{g.Count(x=>x.hasPrefix && !x.hasSuffix)}\t{g.Count(x=>x.hasSuffix && !x.hasPrefix)}");
//            }

//            //Amalur.Stash.AddItem(Amalur.TypeDefinitions[0x1FDF23]);
//            //Amalur.SaveFile(path);
//            //return;



//            //Amalur.WriteEquipmentBytes(chakrams, false);
//            //Amalur.SaveFile(path);
//            return;

//            int c = 0;
//            int nm = 0;
//            var dl = Amalur.Items.GroupBy(x => x.CoreEffects.DataLength).ToDictionary(x => x.Key, x => x.Count());
//            using var zarchive = ZipFile.OpenRead(@"..\..\..\..\ItemTesting\simtypes_unpacked.zip");
//            var buffer = (stackalloc byte[4]);
//            var lines = new List<(uint typeId, string name, uint parentId, string parentName)>();
//            var supers = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Armor_Default", "DEV_DummyFeet", "DEV_DummyHands", "DEV_Palmstones", "PARENT_amulet", "PARENT_bow", "PARENT_Chakram", 
//                "PARENT_clothing_feet", "PARENT_clothing_head", "PARENT_clothing_legs", "PARENT_clothing_torso", "PARENT_Daggers", "PARENT_Greathammer", "PARENT_mage_feet", "PARENT_mage_hands", 
//                "PARENT_mage_head", "PARENT_Mirrorblades", "PARENT_rogue_feet", "PARENT_rogue_hands", "PARENT_rogue_head", "PARENT_rogue_legs", "PARENT_rogue_torso", "PARENT_Sceptre", 
//                "PARENT_Shield", "PARENT_Staff", "PARENT_Sword_1H", "PARENT_Sword_2H", "PARENT_warrior_feet", "PARENT_warrior_hands", "PARENT_warrior_head", "PARENT_warrior_legs", "PARENT_warrior_torso",
//                "QuestItem_SA04_GemCuttersWraps", "QuestItem_SA04_PendantOfWarmth", "QuestItem_SQ07_Ravensfeet", "QuestItem_SQ103_MurghanCharm", 
//                "QuestItem_TR06_Amulet", "QuestItem_TR06_BurusBoots",
//                 "test_full_head","PARENT_chakram_fire","PARENT_chakram_ice","PARENT_chakram_lightning","PARENT_sceptre_fire","PARENT_sceptre_ice","PARENT_sceptre_lightning",
//                "PARENT_Shield_Mage","PARENT_Shield_Mage_Fire","PARENT_Shield_Mage_Ice","PARENT_Shield_Mage_Lightning","PARENT_Shield_Rogue","PARENT_Shield_Warrior","PARENT_staff_fire","PARENT_staff_ice",
//                "PARENT_staff_lightning","PARENT_chakram_primal","PARENT_sceptre_primal","PARENT_staff_primal","PARENT_mage_torso"
//                ,"PARENT_ring","QuestItem_FFQ04_MaidensRing", "QuestItem_HM01_SignetRing", "SA03_DocentsRing","QuestItem_SQ143_FyragnosRing", "QuestItem_SQ15_Ring","SA04_AdeptsRing", "DLC2_QuestItem_SQ09_Ring","QuestItem_TRSQ02_WeddingBand", "QuestItem_TRSQ02_WGSignet"};

//            var simtypes = File.ReadLines(@"..\..\..\..\ItemTesting\simtype.csv")
//                .Skip(1)
//                .ToDictionary(x => uint.Parse(x[..6], NumberStyles.HexNumber), x => x[7..]);
//            var byName = simtypes.ToDictionary(x => x.Value, x => x.Key, StringComparer.OrdinalIgnoreCase);
//            foreach (var entry in zarchive.Entries)
//            {
//                var sname = Path.GetFileNameWithoutExtension(entry.Name);
//                if(byName.TryGetValue(sname, out var typeId))
//                {
//                    using var stream = entry.Open();
//                    stream.Read(buffer);
//                    var parentId = BitConverter.ToUInt32(buffer);
//                    if(parentId == 0xCDCDCDCD 
//                        && (sname.Contains("SorceryPack") || sname.Contains("MightPack") || sname.Contains("FinessePack"))
//                        && char.IsLetter(sname[^1]))
//                    {
//                        var newName = sname[..^1] + 'a';
//                        byName.TryGetValue(newName, out parentId);
//                    }
//                    simtypes.TryGetValue(parentId, out var parentName);
//                    lines.Add((typeId, sname,parentId,parentName));
//                }
//            }
//            var itemsCsv = File.ReadAllLines(@"..\..\..\..\Koar.SaveEditor\items.csv").ToArray();
//            HashSet<uint> scalingItem = new HashSet<uint>();
//            foreach(var line in lines.Where(x=> Amalur.TypeDefinitions.TryGetValue(x.typeId, out var _) 
//            && !(supers.Contains(x.name)
//            || supers.Contains(x.parentName)
//            || x.name.Contains("dev_", StringComparison.OrdinalIgnoreCase)
//            || x.name.Contains("merchant") || x.name.Contains("socket")
//            )))//.Select(x => $"{x.typeId},{x.name},{x.parentId},{x.parentName}")
//            {
//                scalingItem.Add(line.typeId);
//                scalingItem.Add(line.parentId);
//            }
//            File.WriteAllLines("items2.csv",itemsCsv.Select((x, i) => x + "," + true switch {
//                _ when i == 0 => "IsScaling",
//                _ when x.IndexOf(',') is int ix
//                && scalingItem.Contains(uint.Parse(x[(ix+1)..(ix+7)], NumberStyles.HexNumber)) => bool.TrueString,
//                _ => bool.FalseString
//            }));
//            return;

//            var rec = lines.ToDictionary(x => (x.typeId, x.name), x => (x.parentId, x.parentName));
//            var output = new List<string> { "typeId,name,parentId,parentName" };

//            //foreach(var l in lines)
//            //{
//            //    if (supers.Contains(l.name))
//            //    {
//            //        var affix = affixedItems.GetOrDefault(l.typeId, "None,None");
//            //        output.Add($"{l.typeId:X6},{l.name},None,None,{affix}");
//            //        continue;
//            //    }
//            //    var candidate = (l.typeId, l.name);

//            //    while(rec.TryGetValue(candidate, out candidate))
//            //    {
//            //        if (supers.Contains(candidate.name))
//            //        {
//            //            var affix = affixedItems.GetOrDefault(l.typeId, "None,None");
//            //            output.Add($"{l.typeId:X6},{l.name},{candidate.typeId:X6},{candidate.name},{affix}");
//            //            break;
//            //        }
//            //    }
//            //}
//            File.WriteAllLines("parent_one.csv", output);

//            foreach (var (key,val) in dl.OrderBy(x=> x.Key))
//            {
//                Console.WriteLine($"{key - 17} : {val}");
//            }
//            foreach (var item in Amalur.Items)
//            {
//                var b = false;
//                var type = item.TypeDefinition;
//                var ce = FormatAsStr(item.CoreEffects.List);
//                var tce = FormatAsStr(type.CoreEffects);
//                var e = FormatAsStr(item.Effects);
//                var te = FormatAsStr(type.Effects);
//                if (tce != ce || te != e)
//                {
//                    b=true;
//                    Console.WriteLine($"Effects for '{type.InternalName}' are wrong ");
//                    Console.WriteLine($"Type_e:{te}\tType_ce:{tce}\tItem_e:{e}\tItem_ce:{ce} bad prefixes:{item.CoreEffects.UnsupportedFormat}");
//                    if(Amalur.Buffs.TryGetValue(item.CoreEffects.Prefix, out var p) | Amalur.Buffs.TryGetValue(item.CoreEffects.Suffix, out var s))
//                    {
//                        Console.WriteLine($"prefix {p?.Modifier} ({p?.Id:X6})\tsuffix {s?.Modifier} ({s?.Id:X6})");
//                    }
//                    c++;
//                    if(!type.InternalName.Contains("merchant", StringComparison.OrdinalIgnoreCase))
//                    {
//                        nm++;
//                    }
//                }
//            }
//            Console.WriteLine($"C:{c} NM:{nm}");
//            int i = 0;
//            foreach (var group in CoreEffectMemory.SetOfInstances.GroupBy(x=> x.instanceId).OrderByDescending(x=> x.Count()))
//            {
//                var what = (from item in Amalur.Items
//                           join id in @group.Select(x => x.itemId)
//                             on item.ItemId equals id
//                           select item.TypeDefinition);

//                var poo = what.Select(x => x.Category).Distinct();
//                var all_merchant = what.All(x => x.InternalName.Contains("merchant", StringComparison.OrdinalIgnoreCase));
//                Console.WriteLine($"{group.Key}\t{group.Key:X8}\t{string.Join(',', group.Select(x => x.offset).Distinct())}\t{group.Count()}\t{all_merchant}\t{string.Join(',', poo)}");
//            }
//            Console.WriteLine($"Total:{CoreEffectMemory.SetOfInstances.Count}");
//            //ConvertSymbolsToCsv(@"C:\temp\", @"C:\temp\output");

//            //var stash = new Stash();
//            ////var variants = new[] { 1710274u, 1716331u, 1716334u, 1716335u, 1716338u };
//            ////foreach (var variant in variants)
//            ////{
//            ////    stash.AddItem(variant);
//            ////}
//            //Amalur.SaveFile(path);
//            return;
//            LocalDb = new DisposableLocalDb("symbols", "alter database symbols collate latin1_general_bin2");
//            try
//            {
//                using var archive = ZipFile.OpenRead(@"..\..\..\..\symbol_tables.zip");
//                foreach (var entry in archive.Entries)
//                {
//                    using var reader = new StreamReader(entry.Open());
//                    BulkInsertTable(entry.Name[0..^4], Enumerable
//                        .Repeat(reader, int.MaxValue)
//                        .Select(rdr => rdr.ReadLine())
//                        .TakeWhile(s => s is string)
//                        .Skip(1)
//                        .Select(l => l.Split(','))
//                        .Select(a => new object[] { int.Parse(a[0], NumberStyles.HexNumber), a[0], a[1] }));
//                }
//                var qwat = string.Join(',',Amalur.TypeDefinitions[2071806].Effects);


//                BulkInsertTable("definitions", Amalur.TypeDefinitions.Values.Select(x => new object[]
//                 {
//                x.Category,
//                x.TypeId,
//                x.Level,
//                x.Name,
//                x.MaxDurability,
//                $"[{string.Join(',', x.CoreEffects)}]",
//                $"[{string.Join(',', x.Effects)}]",
//                 }),
//                "category=space(35),type_id=0,level=0,name=space(255),durability=0e,core_effects=space(8000),effects=space(8000)");
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("WTF");
//            }
//            finally
//            {
//                LocalDb.Dispose();
//            }
//            return;



//            var sw = Stopwatch.StartNew();
//            Amalur.ReadFile(path);
//            var torso = GetAllIndices(Amalur.Bytes, BitConverter.GetBytes(578143));
//            var face = GetAllIndices(Amalur.Bytes, BitConverter.GetBytes(1580320));
//            //MemoryUtilities.Write(Amalur.Bytes, 207204, 590481);
//            var q = MemoryUtilities.Read<int>(Amalur.Bytes, 207208);
//            // MemoryUtilities.Write(Amalur.Bytes, 206319, 1024178); 

//            Amalur.SaveFile(path);
//            //var ruler = GetAllIndices(Amalur.Bytes, new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 });
//            //var stash = new Stash();
//            //stash.FirstItemTypeId = 511225;
//            //PrintRuler();
//            //PrintByteString(Amalur.Bytes.AsSpan(stash.Offset, stash.DataLength).ToArray());
//            //Amalur.SaveFile(path);
//            //        return;

//            //        foreach (var mem in mems)
//            //        {
//            //            var poo = Amalur.Bytes.AsSpan(mem.CoreEffects.ItemIndex, 17 + mem.CoreEffects.MysteryInteger).ToArray();
//            //            PrintRuler();
//            //            PrintByteString(poo);
//            //        }
//            //        Amalur.SaveFile(path);
//            //        Console.Read();
//            //        return;

//            //        //Console.Read();
//            //        //ReadOnlySpan<byte> itemDesc = new byte[5] { 0x06, 0x2D, 0x75, 0x00, 0x05 };
//            //        //var indices = GetAllIndices(bytes, itemDesc).GroupBy(x => bytes[x + 27]).ToDictionary(x => x.Key, x => x.Count());
//            //        ReadOnlySpan<byte> donor = new byte[]
//            //        {
//            //            0x06, 0x2D, 0x75, 0x00, 0x05, 0x3B, 0x00, 0x00, 0x00, 0x04, 0x05, 0xDD, 0x02, 0xF1, 0x1D, 0x20,
//            //            0x00, 0x83, 0x06, 0x0B, 0x00, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x99,
//            //            0xFB, 0x37, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xF1, 0x1D, 0x20, 0x00, 0x00,
//            //            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//            //            0x00, 0x00, 0x00, 0x00
//            //        };
//            //        ReadOnlySpan<byte> donor2 = new byte[]
//            //        {
//            //            0x06, 0x2D, 0x75, 0x00, 0x05, 0x3B, 0x00, 0x00, 0x00, 0x59, 0x00, 0x2E, 0x01, 0xFA, 0x78, 0x07,
//            //            0x00, 0x24, 0xA0, 0x15, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x99, 
//            //            0xFB, 0x37, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xFA, 0x78, 0x07, 0x00, 0x00,
//            //            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
//            //            0x00, 0x00, 0x00, 0x00
//            //        };

//            //        var ore = new[] { "Iron", "Steel", "Azurite", "Sylvanite", "Prismere" };
//            //        var wood = new[] { "Birch", "Elm", "Oak", "Ash", "Ebony" };
//            //        var metal = new[] { "Bronze", "Copper", "Silver", "Gold", "Platinum" };
//            //        var leather = new[] { "Leather", "Boiled Leather", "Studded Leather", "Trollhide","Dreadscale" };
//            //        var cloth = new[] { "Cotton", "Linen", "Silk", "Hexweave", "Spiritweave" };

//            //        foreach (var item in mems)
//            //        {
//            //            var material = item.Category switch
//            //            {
//            //                EquipmentCategory.Talisman => metal,
//            //                EquipmentCategory.Buckler => wood,
//            //                EquipmentCategory.Longbow => wood,
//            //                EquipmentCategory.Staff => wood,
//            //                EquipmentCategory.Sceptre => wood,

//            //                _ => ore
//            //            };
//            //            item.ItemName = $"{item.ItemName}";
//            //            item.GetTypeDefinition().WriteToCsv();
//            //        }
//            //        return;
//            //            var arrays = new List<byte[]>[mems.Length];
//            //        var j = 0;
//            //        foreach(var chakrams  in mems)
//            //        {
//            //            arrays[j] = new List<byte[]>();
//            //            var foo = chakrams.ItemBytes.AsSpan(0, 4);

//            //            ReadOnlySpan<byte> marker = new byte[] { 0x06, 0x2D, 0x75, 0x00, 0x05 };
//            //            var last = 0;
//            //            var start = Amalur.Bytes.AsSpan().IndexOf(foo);
//            //            Console.WriteLine($"{chakrams.Category} {chakrams.ItemName}");
//            //            while (start != -1)
//            //            {
//            //                int next = -1;
//            //                start += last;
//            //                if (Amalur.Bytes[start+4] == 0x0B)
//            //                {
//            //                    next = Amalur.Bytes.AsSpan(start + 13).IndexOf(Amalur.Bytes.AsSpan(start + 4, 9));
//            //                }
//            //                var end = arrays[j].Count switch
//            //                {
//            //                    0 => 59,
//            //                    _ when next !=-1 => next + 9,
//            //                    _ => 150
//            //                };

//            //                arrays[j].Add(Amalur.Bytes.AsSpan(start + 4, end - 4).ToArray());

//            //                last = start + end;
//            //                start = Amalur.Bytes.AsSpan(last).IndexOf(foo);
//            //            }

//            //            j++;
//            //        }

//            //        for (int i = 0; i < arrays[0].Count; i++)
//            //        {
//            //            var sample = arrays[0][i];
//            //            var isEqual = true;
//            //            foreach (var list in arrays.Skip(1))
//            //            {
//            //                if (!list[i].SequenceEqual(sample))
//            //                {
//            //                    isEqual = false;
//            //                    break;
//            //                }
//            //            }
//            //            if (!isEqual)
//            //            {
//            //                Console.WriteLine($"Section {i}");
//            //                PrintRuler();
//            //                foreach (var (list,mem) in arrays.Zip(mems))
//            //                {
//            //                    Console.WriteLine($"{mem.Category} {BinaryPrimitives.ReadInt32BigEndian(mem.ItemBytes):X8} {mem.TypeId:X6} {mem.Level}");
//            //                    PrintByteString(list[i]);
//            //                    Console.WriteLine();
//            //                }
//            //            }
//            //            else
//            //            {
//            //                Console.WriteLine($"Section {i} is identical");
//            //            }

//            //        }
//            //        Console.WriteLine(sw.Elapsed);
//            //        //Amalur.SaveFile(path);


//            //        //            var items = mems
//            //        //                .Select(x => (x.ItemName, ItemId:x.ItemBytes.AsSpan(0, 4).ToArray())).OrderBy(x=>x.ItemName).ToArray();

//            //        //            //var items = new[] { "92 05 FC 00 0B 00 00 00", 
//            //        //            //    "A2 05 FB 00 0B 00 00 00", 
//            //        //            //    "91 05 EF 00 0B 00 00 00" }.Select(GetBytesFromText).ToArray();
//            //        //            var offsets = items.Select(x => GetAllIndices(bytes, x.ItemId)).ToArray();
//            //        //            var poop = bytes.AsSpan(offsets[0][0] + 8, 5);
//            //        //            for (int i = 0; i < offsets.Length; i++)
//            //        //            {
//            //        //                List<int> offset = (List<int>)offsets[i];
//            //        //                Debug.Assert(bytes.AsSpan(offset[0] + 8, 5).SequenceEqual(poop),items[i].ItemName);
//            //        //            }
//            //        //            bytes[offsets[0][0] + 4] = 0xF1;
//            //        //            bytes[offsets[0][0] + 5] = 0x1D;
//            //        //            bytes[offsets[0][0] + 6] = 0x20;
//            //        //            bytes[offsets[0][0] + 7] = 0x00;
//            //        //            //offsets[0] = offsets[0].Except( new[]{
//            //        //            //    144844
//            //        //            //    ,156940
//            //        //            //    ,214472
//            //        //            //    ,233348
//            //        //            //    ,243325
//            //        //            //    ,253623
//            //        //            //    ,266855
//            //        //            //    ,270662 }).ToList();
//            //        //            bytes[offsets[0][0] + 18] = 0x2a;
//            //        //            //bytes[offsets[0][0] + 34] = 0xf1;
//            //        //            //bytes[offsets[0][0] + 35] = 0x1d;
//            //        //            //bytes[offsets[0][0] + 36] = 0x20;
//            //        //            //bytes[offsets[0][0] + 37] = 0x00;
//            //        //            Amalur.SaveFile(path);
//            //        //            return;
//            //        //            var core = GetBytesFromText("84 60 28 00 00");
//            //        //            var equipSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };

//            //        //            const int magicNumber = 0x00FF00FF;
//            //        //            static string Converter(int x)=> ((x = x >> 16 | x << 16) >> 8 & magicNumber | (x & magicNumber) << 8).ToString("X8");

//            //        //            for (int i= 0; i <1; i++)
//            //        //            {
//            //        //                Console.WriteLine(new string('-', 120));
//            //        //                var output = new byte[items.Length][];
//            //        //                Console.WriteLine(new string('*', 120));
//            //        //                Console.WriteLine($"Section {i}");
//            //        //                Console.WriteLine(new string('*', 120));
//            //        //                Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
//            //        //                Console.WriteLine(new string('-', 120));

//            //        //                var buffer = new byte[9];
//            //        //                for (int j = 0; j < offsets.Length; j++)
//            //        //                {
//            //        //                    if (offsets[j].Count <= i) continue;
//            //        //                    int ix = offsets[j][i];
//            //        //                    bytes[(ix + 4)..(ix + 13)].CopyTo(buffer, 0);
//            //        //                    ReadOnlySpan<byte> supportSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };
//            //        //                    ReadOnlySpan<byte> coreSeq = new byte[] { 0x84, 0x60, 0x28, 0x00, 0x00 };

//            //        //                    if (bytes.AsSpan(ix + 8, 5).SequenceEqual(supportSeq))
//            //        //                    {
//            //        //                        var itemMemory = mems[j];
//            //        //                        var offset = new ItemMemoryInfo.Offset(itemMemory.EffectCount);

//            //        //                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Support component sequence");
//            //        //                        Console.WriteLine($"byte13:{bytes[ix + 13]:X2}");
//            //        //                        Console.WriteLine($"Post Effect bytes:{GetBytesString(bytes[(ix + offset.PostEffect)..(ix + offset.CurrentDurability)])}");
//            //        //                        Console.WriteLine($"After Max Durability:{GetBytesString(bytes[(ix + offset.MaxDurability + 4)..(ix + offset.MaxDurability + 8)])}");
//            //        //                        Console.WriteLine($"byte before customName:{bytes[ix+offset.HasCustomName - 1]:X2}");

//            //        //                        continue;
//            //        //                    }
//            //        //                    else if (bytes.AsSpan(ix + 8, 5).SequenceEqual(coreSeq))
//            //        //                    {
//            //        //                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Core component sequence");
//            //        //                        continue;
//            //        //                    }
//            //        //                    var nextPos = bytes.AsSpan(ix + 13).IndexOf(buffer);


//            //        //                    output[j] = bytes[ix..(ix + 113 + nextPos - 4)];
//            //        //                    Console.WriteLine($"--- Item {j}: {items[j].ItemName} offset {ix} ---");
//            //        //                    Console.WriteLine(string.Join(' ', output[j].Select(x => x.ToString("X2"))));
//            //        //                }
//            //        //                if (output.Any(x => x is null || x.Length == 0)) continue;
//            //        //                var smallest = output.FirstOrDefault(x => x.Length == output.Min(y => y.Length));
//            //        //                if (output.All(x => x.AsSpan(8, smallest.Length - 8).SequenceEqual(smallest.AsSpan(8, smallest.Length - 8))))
//            //        //                {
//            //        //                    Console.WriteLine($"****Section {i} is identical****");
//            //        //                }


//            //        //            }
//            //        //            return;


//            //        //            static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
//            //        //            {
//            //        //                var results = new List<int>();
//            //        //                int ix = data.IndexOf(sequence);
//            //        //                int start = 0;

//            //        //                while (ix != -1)
//            //        //                {
//            //        //                    results.Add(start + ix);
//            //        //                    start += ix + sequence.Length;
//            //        //                    ix = data.Slice(start).IndexOf(sequence);
//            //        //                }
//            //        //                return results;
//            //        //            }

//            //        //}

//        }
//    }
//}
