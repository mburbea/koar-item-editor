using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public class ItemBuffMemory : IItemBuffMemory
    {
        internal static List<(Item item, int offset, uint instanceId)> SetOfInstances = new List<(Item, int, uint)>();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int BuffCount = DataLength + 4;
            public const int FirstActiveBuff = BuffCount + 4;
        }

        internal static uint GetSelfBuffInstanceId(int index) => (uint)Hasher.GetHash($"selfbuff_{index}");
        internal static uint GetAffixInstanceId(Buff? buff) => buff is null ? 0 : (uint)Hasher.GetHash($"selfbuff{buff.BuffType}_{buff.Id}");
        internal static uint GetSocketInstanceId(int socket) => (uint)Hasher.GetHash($"socketable_self_{socket}_0");

        private readonly Item _item;

        internal ItemBuffMemory(GameSave gameSave, Item item, int itemOffset, int dataLength)
        {
            (_item, ItemOffset) = (item, itemOffset);
            Bytes = gameSave.Body.AsSpan(itemOffset, dataLength).ToArray();
            var count = Count;
            var selfBuffCount = MemoryUtilities.Read<int>(Bytes, Offsets.FirstActiveBuff + 4 + (count * 16));
            var selfBuffs = MemoryMarshal.Cast<byte, uint>(Bytes.AsSpan(Offsets.FirstActiveBuff + 8 + (count * 16), selfBuffCount * 8));
            for (int i = 0; i < selfBuffs.Length; i += 2)
            {
                List.Add(Amalur.GetBuff(selfBuffs[i]));
            }
            var activeBuffs = MemoryMarshal.Cast<byte, uint>(Bytes.AsSpan(Offsets.FirstActiveBuff, count * 16));
            var socketInstances = _item.ItemSockets.Gems
                .Select((g, i) => (g, i))
                .Where(t => t.g.Definition.Buff.ApplyType == ApplyType.OnObject)
                .Select(t => GetSocketInstanceId(t.i))
                .ToArray();

            for (int i = 0; i < activeBuffs.Length; i += 4)
            {
                var instanceId = activeBuffs[i];
                var buff = Amalur.GetBuff(activeBuffs[i + 1]);
                if (GetSelfBuffInstanceId(List.IndexOf(buff)) != instanceId
                    && instanceId != GetAffixInstanceId(Prefix)
                    && instanceId != GetAffixInstanceId(Suffix)
                    && !socketInstances.Contains(instanceId))
                {
                    SetOfInstances.Add((item, i / 4, instanceId));
                    UnsupportedFormat = true;
                    continue;
                }
            }
        }

        internal byte[] Bytes { get; private set; }
        public int ItemOffset { get; internal set; }
        public bool UnsupportedFormat { get; internal set; }
        public IList<Buff> List { get; } = new List<Buff>();

        internal int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength) + 17;
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value - 17);
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
            set => MemoryUtilities.Write(Bytes, Offsets.BuffCount, value);
        }

        private int ActiveBuffCount => List.Count
            + (Prefix?.ApplyType == ApplyType.OnObject ? 1 : 0)
            + (Suffix?.ApplyType == ApplyType.OnObject ? 1 : 0)
            + _item.ItemSockets.Gems.Count(x => x.Definition.Buff.ApplyType == ApplyType.OnObject);

        public Buff? Prefix
        {
            get => Amalur.Buffs.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 8));
            set => MemoryUtilities.Write(Bytes, Bytes.Length - 8, value?.Id ?? 0);
        }

        public Buff? Suffix
        {
            get => Amalur.Buffs.GetOrDefault(MemoryUtilities.Read<uint>(Bytes, Bytes.Length - 4));
            set => MemoryUtilities.Write(Bytes, Bytes.Length - 4, value?.Id ?? 0);
        }

        internal byte[] Serialize(bool forced = false)
        {
            if (!forced && ActiveBuffCount == Count)
            {
                return Bytes;
            }
            var currentLength = Bytes.Length - 8 - Offsets.FirstActiveBuff;
            Span<ulong> buffer = stackalloc ulong[1 + 2 * ActiveBuffCount + List.Count];
            var buffData = WriteAffixBuffInstance(buffer, Prefix);
            buffData = WriteAffixBuffInstance(buffData, Suffix);
            for (int i = 0; i < List.Count; i++)
            {
                buffData = WriteSelfBuffInstance(buffData, List[i], i);
            }
            for (int i = 0; i < _item.ItemSockets.Gems.Length; i++)
            {
                buffData = WriteSocketBuffInstance(buffData, _item.ItemSockets.Gems[i].Definition.Buff, i);
            }
            buffData[0] = ((ulong)List.Count) << 32;
            buffData = buffData[1..];
            for (int i = 0; i < List.Count; i++)
            {
                ulong buffId = List[i].Id;
                buffData[i] = buffId | ((ulong)uint.MaxValue) << 32;
            }
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstActiveBuff, currentLength, MemoryMarshal.AsBytes(buffer));
            Count = ActiveBuffCount;
            DataLength = Bytes.Length;
            return Bytes;

            static Span<ulong> WriteBuffInstance(Span<ulong> buffData, Buff buff, uint instanceId)
            {
                ulong buffId = buff.Id;
                buffData[0] = instanceId | buffId << 32;
                buffData[1] = ulong.MaxValue;
                return buffData[2..];
            }

            static Span<ulong> WriteSelfBuffInstance(Span<ulong> buffData, Buff buff, int index) => WriteBuffInstance(buffData, buff, GetSelfBuffInstanceId(index));

            static Span<ulong> WriteAffixBuffInstance(Span<ulong> buffData, Buff? buff) => buff?.ApplyType == ApplyType.OnObject ?
                WriteBuffInstance(buffData, buff, GetAffixInstanceId(buff)) : buffData;

            static Span<ulong> WriteSocketBuffInstance(Span<ulong> buffData, Buff? buff, int socket) => buff?.ApplyType == ApplyType.OnObject ?
                WriteBuffInstance(buffData, buff, GetSocketInstanceId(socket)) : buffData;
        }
    }
}