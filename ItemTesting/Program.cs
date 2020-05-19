using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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


        static void Main(string[] args)
        {

            var path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save96.sav";
            //using var fs = new FileStream(path, FileMode.Open);
            //var bytes = new byte[fs.Length];
            //var memory = fs.Read(bytes, 0, bytes.Length);
            //Console.WriteLine(string.Join(' ', bytes[284851..284951].Select(x => x.ToString("X2"))));
            //ulong effectPattern = 0xFF_FF_FF_FF_00_1C_3D_7E;
            //var patternLocation = GetAllIndices(bytes, BitConverter.GetBytes(effectPattern));
            //Debug.Assert(patternLocation.Count == 12);

            //LikelyCandidates("D1", "62 01 7E 00 0B 00 00 00");
            //LikelyCandidates("D2", "17 01 77 00 0B 00 00 00");
            //LikelyCandidates("D3", "83 01 95 00 0B 00 00 00");
            //LikelyCandidates("T2", "70 00 5A 00 0B 00 00 00");
            //LikelyCandidates("T1", "5b 00 53 00 0B 00 00 00");
            //LikelyCandidates("Primal Dagggers", "04 05 DD 02 0B 00 00 00");
            //LikelyCandidates("Primal Bow", "2B 09 9E 0B 0B 00 00 00");
            //LikelyCandidates("PGW", "2F 09 32 01 0B 00 00 00");
            //LikelyCandidates("TEST", "70 00 1B 01 0B 00 00 00");
            var editor = new AmalurSaveEditor();
            editor.ReadFile(path);
            var bytes = editor.Bytes;
            var interest = new[] { "PLS","PGW","ABC","Primal Chakrams", "Garbage"};
            var mems = editor.GetAllEquipment()
                .Where(x => interest.Contains(x.ItemName)
                || x.ItemBytes.AsSpan(0,4).SequenceEqual(new byte[] { 0x42, 0x09, 0xEB, 0x07  })
                || x.ItemBytes.AsSpan(0,4).SequenceEqual(new byte[] { 0x0D, 0x09, 0x01, 0x03, })
                ).ToArray();
            var items = mems
                .Select(x => (x.ItemName, ItemId:x.ItemBytes.AsSpan(0, 8).ToArray())).OrderBy(x=>x.ItemName).ToArray();
            
            //var items = new[] { "92 05 FC 00 0B 00 00 00", 
            //    "A2 05 FB 00 0B 00 00 00", 
            //    "91 05 EF 00 0B 00 00 00" }.Select(GetBytesFromText).ToArray();
            var offsets = items.Select(x => GetAllIndices(bytes, x.ItemId)).ToArray();

            var core = GetBytesFromText("84 60 28 00 00");
            var equipSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };

            const int magicNumber = 0x00FF00FF;
            static string GetBytesString(byte[] b) => string.Join(' ', b.Select(x => x.ToString("X2")));
            static string Converter(int x)=> ((x = x >> 16 | x << 16) >> 8 & magicNumber | (x & magicNumber) << 8).ToString("X8");

            for (int i= 0; i < offsets[0].Count; i++)
            {
                Console.WriteLine(new string('-', 120));
                var output = new byte[items.Length][];
                Console.WriteLine(new string('*', 120));
                Console.WriteLine($"Section {i}");
                Console.WriteLine(new string('*', 120));
                Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
                Console.WriteLine(new string('-', 120));

                var buffer = new byte[9];
                for (int j = 0; j < offsets.Length; j++)
                {
                    if (offsets[j].Count <= i) continue;
                    int ix = offsets[j][i];
                    bytes[(ix + 4)..(ix + 13)].CopyTo(buffer, 0);
                    ReadOnlySpan<byte> supportSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };
                    ReadOnlySpan<byte> coreSeq = new byte[] { 0x84, 0x60, 0x28, 0x00, 0x00 };

                    if (bytes.AsSpan(ix + 8, 5).SequenceEqual(supportSeq))
                    {
                        var itemMemory = mems[j];
                        var offset = new ItemMemoryInfo.Offset(itemMemory.EffectCount);

                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Support component sequence");
                        Console.WriteLine($"byte13:{bytes[ix + 13]:X2}");
                        Console.WriteLine($"Post Effect bytes:{GetBytesString(bytes[(ix + offset.PostEffect)..(ix + offset.CurrentDurability)])}");
                        Console.WriteLine($"After Max Durability:{GetBytesString(bytes[(ix + offset.MaxDurability + 4)..(ix + offset.MaxDurability + 8)])}");
                        Console.WriteLine($"byte before customName:{bytes[ix+offset.HasCustomName - 1]:X2}");

                        continue;
                    }
                    else if (bytes.AsSpan(ix + 8, 5).SequenceEqual(coreSeq))
                    {
                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Core component sequence");
                        continue;
                    }
                    var nextPos = bytes.AsSpan(ix + 13).IndexOf(buffer);
                    

                    output[j] = bytes[ix..(ix + 13 + nextPos - 4)];
                    Console.WriteLine($"--- Item {j}: {items[j].ItemName} offset {ix} ---");
                    Console.WriteLine(string.Join(' ', output[j].Select(x => x.ToString("X2"))));
                }
                if (output.Any(x => x is null || x.Length == 0)) continue;
                var smallest = output.FirstOrDefault(x => x.Length == output.Min(y => y.Length));
                if (output.All(x => x.AsSpan(8, smallest.Length - 8).SequenceEqual(smallest.AsSpan(8, smallest.Length - 8))))
                {
                    Console.WriteLine($"****Section {i} is identical****");
                }

                 
            }
            return;


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
        
}
    }
}
