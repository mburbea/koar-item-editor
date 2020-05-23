using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using KoAR.Core;

namespace ItemTesting
{
    class Program
    {
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


        static void Main(string[] args)
        {
            var path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save91.sav";

            Amalur.ReadFile(path);
            var bytes = Amalur.Bytes;
            var interest = new[] { "D", "Primal Daggers" };
            var mems = Amalur.GetAllEquipment()
                //.Where(x => (x.CurrentDurability == 58 && x.MaxDurability == 84) || interest.Contains(x.ItemName) //&& x.ItemId == 0x19_02_1C)
                .Where(x=> x.Category == EquipmentCategory.Chakrams || x.Category == EquipmentCategory.Daggers)
            .ToArray();
            //ReadOnlySpan<byte> itemDesc = new byte[5] { 0x06, 0x2D, 0x75, 0x00, 0x05 };
            //var indices = GetAllIndices(bytes, itemDesc).GroupBy(x => bytes[x + 27]).ToDictionary(x => x.Key, x => x.Count());

            for (int i = 0; i < mems.Length; i++)
            {
                byte[] crap = new byte[8]; //{ 0x1C, 0x02, 0x19, 0x_00, 0x92, 0xE1, 0x1E, 0x00 };
                BitConverter.TryWriteBytes(crap, mems[i].ItemId);
                BitConverter.TryWriteBytes(crap.AsSpan(4), mems[i].TypeId);
                var loc = bytes.AsSpan().IndexOf(crap);
                var header = bytes.AsSpan(loc + 8, 5);
                //Debug.Assert(bytes[loc - 4] == 0x3B);
                var coo = bytes.AsSpan(loc - 9, 68).ToArray();
                //bytes[loc + 34] = 0x92;
                //bytes[loc + 35] = 0xE1;
                //bytes[loc + 36] = 0x1E;
                //bytes[loc + 64] = 0x3B;
                Console.WriteLine($"{i} Item ID: {(mems[i].HasCustomName? mems[i].ItemName : mems[i].ItemId.ToString("X8"))} {mems[i].Category}");
                PrintRuler();
                PrintByteString(coo);
            }
            //Amalur.SaveFile(path);


            //            var items = mems
            //                .Select(x => (x.ItemName, ItemId:x.ItemBytes.AsSpan(0, 4).ToArray())).OrderBy(x=>x.ItemName).ToArray();

            //            //var items = new[] { "92 05 FC 00 0B 00 00 00", 
            //            //    "A2 05 FB 00 0B 00 00 00", 
            //            //    "91 05 EF 00 0B 00 00 00" }.Select(GetBytesFromText).ToArray();
            //            var offsets = items.Select(x => GetAllIndices(bytes, x.ItemId)).ToArray();
            //            var poop = bytes.AsSpan(offsets[0][0] + 8, 5);
            //            for (int i = 0; i < offsets.Length; i++)
            //            {
            //                List<int> offset = (List<int>)offsets[i];
            //                Debug.Assert(bytes.AsSpan(offset[0] + 8, 5).SequenceEqual(poop),items[i].ItemName);
            //            }
            //            bytes[offsets[0][0] + 4] = 0xF1;
            //            bytes[offsets[0][0] + 5] = 0x1D;
            //            bytes[offsets[0][0] + 6] = 0x20;
            //            bytes[offsets[0][0] + 7] = 0x00;
            //            //offsets[0] = offsets[0].Except( new[]{
            //            //    144844
            //            //    ,156940
            //            //    ,214472
            //            //    ,233348
            //            //    ,243325
            //            //    ,253623
            //            //    ,266855
            //            //    ,270662 }).ToList();
            //            bytes[offsets[0][0] + 18] = 0x2a;
            //            //bytes[offsets[0][0] + 34] = 0xf1;
            //            //bytes[offsets[0][0] + 35] = 0x1d;
            //            //bytes[offsets[0][0] + 36] = 0x20;
            //            //bytes[offsets[0][0] + 37] = 0x00;
            //            Amalur.SaveFile(path);
            //            return;
            //            var core = GetBytesFromText("84 60 28 00 00");
            //            var equipSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };

            //            const int magicNumber = 0x00FF00FF;
            //            static string Converter(int x)=> ((x = x >> 16 | x << 16) >> 8 & magicNumber | (x & magicNumber) << 8).ToString("X8");

            //            for (int i= 0; i <1; i++)
            //            {
            //                Console.WriteLine(new string('-', 120));
            //                var output = new byte[items.Length][];
            //                Console.WriteLine(new string('*', 120));
            //                Console.WriteLine($"Section {i}");
            //                Console.WriteLine(new string('*', 120));
            //                Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
            //                Console.WriteLine(new string('-', 120));

            //                var buffer = new byte[9];
            //                for (int j = 0; j < offsets.Length; j++)
            //                {
            //                    if (offsets[j].Count <= i) continue;
            //                    int ix = offsets[j][i];
            //                    bytes[(ix + 4)..(ix + 13)].CopyTo(buffer, 0);
            //                    ReadOnlySpan<byte> supportSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };
            //                    ReadOnlySpan<byte> coreSeq = new byte[] { 0x84, 0x60, 0x28, 0x00, 0x00 };

            //                    if (bytes.AsSpan(ix + 8, 5).SequenceEqual(supportSeq))
            //                    {
            //                        var itemMemory = mems[j];
            //                        var offset = new ItemMemoryInfo.Offset(itemMemory.EffectCount);

            //                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Support component sequence");
            //                        Console.WriteLine($"byte13:{bytes[ix + 13]:X2}");
            //                        Console.WriteLine($"Post Effect bytes:{GetBytesString(bytes[(ix + offset.PostEffect)..(ix + offset.CurrentDurability)])}");
            //                        Console.WriteLine($"After Max Durability:{GetBytesString(bytes[(ix + offset.MaxDurability + 4)..(ix + offset.MaxDurability + 8)])}");
            //                        Console.WriteLine($"byte before customName:{bytes[ix+offset.HasCustomName - 1]:X2}");

            //                        continue;
            //                    }
            //                    else if (bytes.AsSpan(ix + 8, 5).SequenceEqual(coreSeq))
            //                    {
            //                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Core component sequence");
            //                        continue;
            //                    }
            //                    var nextPos = bytes.AsSpan(ix + 13).IndexOf(buffer);


            //                    output[j] = bytes[ix..(ix + 113 + nextPos - 4)];
            //                    Console.WriteLine($"--- Item {j}: {items[j].ItemName} offset {ix} ---");
            //                    Console.WriteLine(string.Join(' ', output[j].Select(x => x.ToString("X2"))));
            //                }
            //                if (output.Any(x => x is null || x.Length == 0)) continue;
            //                var smallest = output.FirstOrDefault(x => x.Length == output.Min(y => y.Length));
            //                if (output.All(x => x.AsSpan(8, smallest.Length - 8).SequenceEqual(smallest.AsSpan(8, smallest.Length - 8))))
            //                {
            //                    Console.WriteLine($"****Section {i} is identical****");
            //                }


            //            }
            //            return;


            //            static List<int> GetAllIndices(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
            //            {
            //                var results = new List<int>();
            //                int ix = data.IndexOf(sequence);
            //                int start = 0;

            //                while (ix != -1)
            //                {
            //                    results.Add(start + ix);
            //                    start += ix + sequence.Length;
            //                    ix = data.Slice(start).IndexOf(sequence);
            //                }
            //                return results;
            //            }

            //}
        }
    }
}
