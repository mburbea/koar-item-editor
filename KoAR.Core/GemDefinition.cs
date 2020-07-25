using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public class GemDefinition
    {
        private GemDefinition(uint typeId, string name, string internalName, uint buffId, char socketType)
        {
            TypeId = typeId;
            Name = name;
            InternalName = internalName;
            Buff = Amalur.GetBuff(buffId);
            SocketType = socketType;
        }

        public uint TypeId { get; }
        public string Name { get; set; }
        public string InternalName { get; set; }
        public Buff Buff { get; set; }
        public char SocketType { get; set; }

        internal static IEnumerable<GemDefinition> ParseFile(string path)
        {
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                var parts = line.Split(Amalur.Seperator);
                yield return new GemDefinition(
                    typeId: uint.Parse(parts[0]),
                    name: parts[1],
                    internalName: parts[2],
                    buffId: uint.Parse(parts[3]),
                    socketType: parts[4][0]
                );
            }
        }
    }
}
