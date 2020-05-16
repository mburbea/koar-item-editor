using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Diagnostics;

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

            var path = @"C:\Program Files (x86)\Steam\userdata\107335713\102500\remote\9190114save92.sav";
            using var fs = new FileStream(path, FileMode.Open);
            var bytes = new byte[fs.Length];
            var memory = fs.Read(bytes, 0, bytes.Length);
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
            LikelyCandidates("PGW", "2F 09 32 01 0B 00 00 00");
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
                Console.WriteLine($"{name}: {length}");
                Console.WriteLine(string.Join(' ', bytes[ix..(ix + length)].Select(x => x.ToString("X2"))));
                Console.WriteLine("---");
                return 0;
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
        }
    }
}
