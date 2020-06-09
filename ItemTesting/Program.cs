using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using KoAR.Core;
using System.IO;
using System.Buffers.Binary;
using System.Xml;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ItemTesting
{
    class Program
    {
        static readonly Regex _topRegex = new Regex(@"^Number of entries\: (?<lines>\d+)$", RegexOptions.ExplicitCapture);
        static readonly Regex _lineRegex = new Regex(@"\s*\d+\s\|\s*(?<id>\d+)\:\s(?<text>.*)$", RegexOptions.ExplicitCapture);

        static Dictionary<uint, string> ParseStream(Stream stream)
        {
            using var reader = new StreamReader(stream,leaveOpen:true);
            int lines = int.Parse(_topRegex.Match(reader.ReadLine()).Groups["lines"].Value);
            var dictionary = new Dictionary<uint, string>();
            for (int index = 0; index < lines; index++)
            {
                Match match = _lineRegex.Match(reader.ReadLine());
                dictionary.Add(uint.Parse(match.Groups["id"].Value), match.Groups["text"].Value);
            }
            return dictionary;
        }






        static byte[] GetBytesFromText(string text)
        {
            List<byte> list = new List<byte>();
            foreach (string word in text.Trim().Split(' '))
            {
                string txt = word.Trim();
                list.Add(byte.Parse(txt, NumberStyles.HexNumber));
            }
            return list.ToArray();
        }
        static string GetBytesString(byte[] b) => string.Join(' ', b.Select(x => x.ToString("X2")));
        static void PrintByteString(byte[] b) => Console.WriteLine(GetBytesString(b));
        static void WriteByteArray(byte[] b, string path) => File.WriteAllText(path, string.Join(", ", b.Select(x => $"0x{x:X2}")));
        static void PrintRuler()
        {
            Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
            Console.WriteLine(new string('-', 120));
        }

        static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
        {
            var results = new List<int>();
            int ix = data.IndexOf(sequence);
            int start = 0;

            while (ix != -1)
            {
                results.Add(start + ix);
                start += ix + sequence.Length;
                ix = data.Slice(start).IndexOf(sequence);
            }
            return results;
        }
        private static char[] AllCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        static void Main()
        {
            var archive = ZipFile.OpenRead(@"..\..\..\..\symbol_tables.zip");

            var parsed = new Dictionary<string, Dictionary<uint, string>>();
            foreach (var entry in archive.Entries)
            {
                using Stream stream = entry.Open();
                parsed.Add(entry.Name, ParseStream(stream));
            }









            var path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save97.sav";
            var sw = Stopwatch.StartNew();
            Amalur.ReadFile(path);
            var torso = GetAllIndices(Amalur.Bytes, BitConverter.GetBytes(590481));
            var legs = GetAllIndices(Amalur.Bytes, BitConverter.GetBytes(590480));
            MemoryUtilities.Write(Amalur.Bytes, torso[0], 1476282);
            MemoryUtilities.Write(Amalur.Bytes, legs[0], 1476266);

            Amalur.SaveFile(path);
            //var ruler = GetAllIndices(Amalur.Bytes, new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 });
            //var stash = new Stash();
            //stash.FirstItemTypeId = 511225;
            //PrintRuler();
            //PrintByteString(Amalur.Bytes.AsSpan(stash.Offset, stash.DataLength).ToArray());
            //Amalur.SaveFile(path);
            //        return;

            //        foreach (var mem in mems)
            //        {
            //            var poo = Amalur.Bytes.AsSpan(mem.CoreEffects.ItemIndex, 17 + mem.CoreEffects.MysteryInteger).ToArray();
            //            PrintRuler();
            //            PrintByteString(poo);
            //        }
            //        Amalur.SaveFile(path);
            //        Console.Read();
            //        return;

            //        //Console.Read();
            //        //ReadOnlySpan<byte> itemDesc = new byte[5] { 0x06, 0x2D, 0x75, 0x00, 0x05 };
            //        //var indices = GetAllIndices(bytes, itemDesc).GroupBy(x => bytes[x + 27]).ToDictionary(x => x.Key, x => x.Count());
            //        ReadOnlySpan<byte> donor = new byte[]
            //        {
            //            0x06, 0x2D, 0x75, 0x00, 0x05, 0x3B, 0x00, 0x00, 0x00, 0x04, 0x05, 0xDD, 0x02, 0xF1, 0x1D, 0x20,
            //            0x00, 0x83, 0x06, 0x0B, 0x00, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x99,
            //            0xFB, 0x37, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xF1, 0x1D, 0x20, 0x00, 0x00,
            //            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //            0x00, 0x00, 0x00, 0x00
            //        };
            //        ReadOnlySpan<byte> donor2 = new byte[]
            //        {
            //            0x06, 0x2D, 0x75, 0x00, 0x05, 0x3B, 0x00, 0x00, 0x00, 0x59, 0x00, 0x2E, 0x01, 0xFA, 0x78, 0x07,
            //            0x00, 0x24, 0xA0, 0x15, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x99, 
            //            0xFB, 0x37, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xFA, 0x78, 0x07, 0x00, 0x00,
            //            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //            0x00, 0x00, 0x00, 0x00
            //        };

            //        var ore = new[] { "Iron", "Steel", "Azurite", "Sylvanite", "Prismere" };
            //        var wood = new[] { "Birch", "Elm", "Oak", "Ash", "Ebony" };
            //        var metal = new[] { "Bronze", "Copper", "Silver", "Gold", "Platinum" };
            //        var leather = new[] { "Leather", "Boiled Leather", "Studded Leather", "Trollhide","Dreadscale" };
            //        var cloth = new[] { "Cotton", "Linen", "Silk", "Hexweave", "Spiritweave" };

            //        foreach (var item in mems)
            //        {
            //            var material = item.Category switch
            //            {
            //                EquipmentCategory.Talisman => metal,
            //                EquipmentCategory.Buckler => wood,
            //                EquipmentCategory.Longbow => wood,
            //                EquipmentCategory.Staff => wood,
            //                EquipmentCategory.Sceptre => wood,

            //                _ => ore
            //            };
            //            item.ItemName = $"{item.ItemName}";
            //            item.GetTypeDefinition().WriteToCsv();
            //        }
            //        return;
            //            var arrays = new List<byte[]>[mems.Length];
            //        var j = 0;
            //        foreach(var chakrams  in mems)
            //        {
            //            arrays[j] = new List<byte[]>();
            //            var foo = chakrams.ItemBytes.AsSpan(0, 4);

            //            ReadOnlySpan<byte> marker = new byte[] { 0x06, 0x2D, 0x75, 0x00, 0x05 };
            //            var last = 0;
            //            var start = Amalur.Bytes.AsSpan().IndexOf(foo);
            //            Console.WriteLine($"{chakrams.Category} {chakrams.ItemName}");
            //            while (start != -1)
            //            {
            //                int next = -1;
            //                start += last;
            //                if (Amalur.Bytes[start+4] == 0x0B)
            //                {
            //                    next = Amalur.Bytes.AsSpan(start + 13).IndexOf(Amalur.Bytes.AsSpan(start + 4, 9));
            //                }
            //                var end = arrays[j].Count switch
            //                {
            //                    0 => 59,
            //                    _ when next !=-1 => next + 9,
            //                    _ => 150
            //                };

            //                arrays[j].Add(Amalur.Bytes.AsSpan(start + 4, end - 4).ToArray());

            //                last = start + end;
            //                start = Amalur.Bytes.AsSpan(last).IndexOf(foo);
            //            }

            //            j++;
            //        }

            //        for (int i = 0; i < arrays[0].Count; i++)
            //        {
            //            var sample = arrays[0][i];
            //            var isEqual = true;
            //            foreach (var list in arrays.Skip(1))
            //            {
            //                if (!list[i].SequenceEqual(sample))
            //                {
            //                    isEqual = false;
            //                    break;
            //                }
            //            }
            //            if (!isEqual)
            //            {
            //                Console.WriteLine($"Section {i}");
            //                PrintRuler();
            //                foreach (var (list,mem) in arrays.Zip(mems))
            //                {
            //                    Console.WriteLine($"{mem.Category} {BinaryPrimitives.ReadInt32BigEndian(mem.ItemBytes):X8} {mem.TypeId:X6} {mem.Level}");
            //                    PrintByteString(list[i]);
            //                    Console.WriteLine();
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine($"Section {i} is identical");
            //            }

            //        }
            //        Console.WriteLine(sw.Elapsed);
            //        //Amalur.SaveFile(path);


            //        //            var items = mems
            //        //                .Select(x => (x.ItemName, ItemId:x.ItemBytes.AsSpan(0, 4).ToArray())).OrderBy(x=>x.ItemName).ToArray();

            //        //            //var items = new[] { "92 05 FC 00 0B 00 00 00", 
            //        //            //    "A2 05 FB 00 0B 00 00 00", 
            //        //            //    "91 05 EF 00 0B 00 00 00" }.Select(GetBytesFromText).ToArray();
            //        //            var offsets = items.Select(x => GetAllIndices(bytes, x.ItemId)).ToArray();
            //        //            var poop = bytes.AsSpan(offsets[0][0] + 8, 5);
            //        //            for (int i = 0; i < offsets.Length; i++)
            //        //            {
            //        //                List<int> offset = (List<int>)offsets[i];
            //        //                Debug.Assert(bytes.AsSpan(offset[0] + 8, 5).SequenceEqual(poop),items[i].ItemName);
            //        //            }
            //        //            bytes[offsets[0][0] + 4] = 0xF1;
            //        //            bytes[offsets[0][0] + 5] = 0x1D;
            //        //            bytes[offsets[0][0] + 6] = 0x20;
            //        //            bytes[offsets[0][0] + 7] = 0x00;
            //        //            //offsets[0] = offsets[0].Except( new[]{
            //        //            //    144844
            //        //            //    ,156940
            //        //            //    ,214472
            //        //            //    ,233348
            //        //            //    ,243325
            //        //            //    ,253623
            //        //            //    ,266855
            //        //            //    ,270662 }).ToList();
            //        //            bytes[offsets[0][0] + 18] = 0x2a;
            //        //            //bytes[offsets[0][0] + 34] = 0xf1;
            //        //            //bytes[offsets[0][0] + 35] = 0x1d;
            //        //            //bytes[offsets[0][0] + 36] = 0x20;
            //        //            //bytes[offsets[0][0] + 37] = 0x00;
            //        //            Amalur.SaveFile(path);
            //        //            return;
            //        //            var core = GetBytesFromText("84 60 28 00 00");
            //        //            var equipSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };

            //        //            const int magicNumber = 0x00FF00FF;
            //        //            static string Converter(int x)=> ((x = x >> 16 | x << 16) >> 8 & magicNumber | (x & magicNumber) << 8).ToString("X8");

            //        //            for (int i= 0; i <1; i++)
            //        //            {
            //        //                Console.WriteLine(new string('-', 120));
            //        //                var output = new byte[items.Length][];
            //        //                Console.WriteLine(new string('*', 120));
            //        //                Console.WriteLine($"Section {i}");
            //        //                Console.WriteLine(new string('*', 120));
            //        //                Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
            //        //                Console.WriteLine(new string('-', 120));

            //        //                var buffer = new byte[9];
            //        //                for (int j = 0; j < offsets.Length; j++)
            //        //                {
            //        //                    if (offsets[j].Count <= i) continue;
            //        //                    int ix = offsets[j][i];
            //        //                    bytes[(ix + 4)..(ix + 13)].CopyTo(buffer, 0);
            //        //                    ReadOnlySpan<byte> supportSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };
            //        //                    ReadOnlySpan<byte> coreSeq = new byte[] { 0x84, 0x60, 0x28, 0x00, 0x00 };

            //        //                    if (bytes.AsSpan(ix + 8, 5).SequenceEqual(supportSeq))
            //        //                    {
            //        //                        var itemMemory = mems[j];
            //        //                        var offset = new ItemMemoryInfo.Offset(itemMemory.EffectCount);

            //        //                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Support component sequence");
            //        //                        Console.WriteLine($"byte13:{bytes[ix + 13]:X2}");
            //        //                        Console.WriteLine($"Post Effect bytes:{GetBytesString(bytes[(ix + offset.PostEffect)..(ix + offset.CurrentDurability)])}");
            //        //                        Console.WriteLine($"After Max Durability:{GetBytesString(bytes[(ix + offset.MaxDurability + 4)..(ix + offset.MaxDurability + 8)])}");
            //        //                        Console.WriteLine($"byte before customName:{bytes[ix+offset.HasCustomName - 1]:X2}");

            //        //                        continue;
            //        //                    }
            //        //                    else if (bytes.AsSpan(ix + 8, 5).SequenceEqual(coreSeq))
            //        //                    {
            //        //                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Core component sequence");
            //        //                        continue;
            //        //                    }
            //        //                    var nextPos = bytes.AsSpan(ix + 13).IndexOf(buffer);


            //        //                    output[j] = bytes[ix..(ix + 113 + nextPos - 4)];
            //        //                    Console.WriteLine($"--- Item {j}: {items[j].ItemName} offset {ix} ---");
            //        //                    Console.WriteLine(string.Join(' ', output[j].Select(x => x.ToString("X2"))));
            //        //                }
            //        //                if (output.Any(x => x is null || x.Length == 0)) continue;
            //        //                var smallest = output.FirstOrDefault(x => x.Length == output.Min(y => y.Length));
            //        //                if (output.All(x => x.AsSpan(8, smallest.Length - 8).SequenceEqual(smallest.AsSpan(8, smallest.Length - 8))))
            //        //                {
            //        //                    Console.WriteLine($"****Section {i} is identical****");
            //        //                }


            //        //            }
            //        //            return;


            //        //            static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
            //        //            {
            //        //                var results = new List<int>();
            //        //                int ix = data.IndexOf(sequence);
            //        //                int start = 0;

            //        //                while (ix != -1)
            //        //                {
            //        //                    results.Add(start + ix);
            //        //                    start += ix + sequence.Length;
            //        //                    ix = data.Slice(start).IndexOf(sequence);
            //        //                }
            //        //                return results;
            //        //            }

            //        //}

        }
    }
}
