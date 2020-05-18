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

            var path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save84.sav";
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
            var interest = new[] { "IC1", "M", "M2", "M3","St1","St2","FC1","F1","F2","D1","Mastercrafted Sylvanite Greatswo" };
            var items = editor.GetAllEquipment()
                .Where(x=>  x.ItemId== 0x07EB0942)
                .Select(x => (x.ItemName, ItemId:x.ItemBytes.AsSpan(0, 8).ToArray())).OrderBy(x=>x.ItemName).ToArray();
            
            //var items = new[] { "92 05 FC 00 0B 00 00 00", 
            //    "A2 05 FB 00 0B 00 00 00", 
            //    "91 05 EF 00 0B 00 00 00" }.Select(GetBytesFromText).ToArray();
            var offsets = items.Select(x => GetAllIndices(bytes, x.ItemId)).ToArray();

            var poo = GetBytesFromText("0B 00 00 00");
            var core = GetBytesFromText("84 60 28 00 00");
            var equipSeq = new byte[] { 0x68, 0xD5, 0x24, 0x00, 0x03 };

            const int magicNumber = 0x00FF00FF;
            static string Converter(int x)=> ((x = x >> 16 | x << 16) >> 8 & magicNumber | (x & magicNumber) << 8).ToString("X8");

            for (int i= 0; i < offsets[0].Count; i++)
            {
                Console.WriteLine("-----------------------------------------------------------");
                var output = new byte[items.Length][];
                if(i == 3)
                {
                    Console.WriteLine($"***Section {i} Support item Sequence***");
                    Console.WriteLine("---");
                    continue;
                }
                if(i == 5)
                {
                    Console.WriteLine($"****Section {i} Core item Sequence***");
                    Console.WriteLine("---");
                    continue;
                }

                for (int j = 0; j < offsets.Length; j++)
                {
                    int ix = offsets[j][i];
                    PrintSection();
                    void PrintSection()
                    {
                        Console.WriteLine($"--- Item {j}: {items[j].ItemName} Section {i} offset {ix} ---");

                        var nextPos = bytes.AsSpan(ix + 8).IndexOf(poo) - 4;
                        output[j] = bytes[ix..(ix + nextPos)];
                        Console.WriteLine(string.Join(' ', Enumerable.Range(0, 40).Select(x => x.ToString("D2"))));
                        Console.WriteLine(new string('-', 120));
                        Console.WriteLine(string.Join(' ', output[j].Select(x => x.ToString("X2"))));
                        
                    }
                }
                if (output.Any(x => x is null)) continue;
                var smallest = output.FirstOrDefault(x => x.Length == output.Min(y => y.Length));
                if (output.All(x => x.AsSpan(8, smallest.Length - 8).SequenceEqual(smallest.AsSpan(8, smallest.Length - 8))))
                {
                    Console.WriteLine($"****Section {i} is identical****");
                }

                 
            }
            return;
            
            int LikelyCandidates(string name, string text)
            {
                text += " 84 60 28 00 00";
                var indices = GetAllIndices(bytes, GetBytesFromText(text));
                Debug.Assert(indices.Count == 1);
                var ix = indices[0];
                var count = bytes[ix + 17];
                int always_zero = bytes[ix + 17 + 4 +  count * 16];
                var count2 = bytes[ix + 17 + 4 +  count * 16 + 4];
                Debug.Assert(count == count2);
                var length = 17 +  12 + (count * 24);
                //int ohSnap=0;
                //for(int i = 0; i < patternLocation.Count -1; i++)
                //{
                //    var p = patternLocation[i];
                //    if(p-indices[0] > 0 && p-indices[0] < 100)
                //    {
                //        ohSnap = patternLocation[i + 1];
                //        break;
                //    }
                //}

                //bytes[ix + 25] = bytes[ix + 61] = 0x33; ;
                //bytes[ix + 26] = bytes[ix + 62] = 0xFE;
                //bytes[ix + 27] = bytes[ix + 63] = 0x1D;
                Console.WriteLine($"{name}: {length}");
                Console.WriteLine(string.Join(' ', bytes[ix..(ix + length + 100)].Select(x => x.ToString("X2"))));
                Console.WriteLine("---");
                //bytes[ix + 13] = 0x2C;
                return 0;
            }
            //fs.Seek(0, SeekOrigin.Begin);
            //fs.Write(bytes);

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
