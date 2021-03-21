using System.Collections.Generic;
using System.IO;

namespace KoAR.Core
{
    public sealed class GemDefinition
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
        public string Name { get; }
        public string InternalName { get; }
        public Buff Buff { get; }
        public char SocketType { get; }

        internal static IEnumerable<GemDefinition> ParseFile(Stream stream)
        {
            using var reader = new StreamReader(stream);
            reader.ReadLine();
            while (reader.ReadLine() is { } line)
            {
                var parts = line.Split(Amalur.Separator);
                yield return new(
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
