﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public class ItemBuffMemory : IItemBuffMemory
    {
        internal static List<(Item item, uint instanceId)> SetOfInstances = new List<(Item, uint)>();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int BuffCount = DataLength + 4;
            public const int FirstActiveBuff = BuffCount + 4;
        }

        internal static uint GetSelfBuffInstanceId(int index) => (uint)Hasher.GetHash($"selfbuff_{index}");
        internal static uint GetAffixInstanceId(Buff? buff) => buff is null ? 0 : (uint)Hasher.GetHash($"selfbuff{buff.BuffType}_{buff.Id}");
        internal static uint GetSocketInstanceId(int slot) => (uint)Hasher.GetHash($"socketable_self_{slot}_0");

        private readonly Item _item;

        internal ItemBuffMemory(GameSave gameSave, Item item, int itemOffset, int dataLength)
        {
            (_item, ItemOffset) = (item, itemOffset);
            Bytes = gameSave.Body.AsSpan(itemOffset, dataLength).ToArray();
            var buffData = Bytes.AsSpan(Offsets.BuffCount); // everything else except for the affixes.
            var activeBuffs = BuffInstance.ReadList(ref buffData);
            var inactiveBuffs = BuffInstance.ReadList(ref buffData);
            var selfBuffs = BuffDuration.ReadList(ref buffData);
            foreach (var (buffId, _) in selfBuffs)
            {
                List.Add(Amalur.GetBuff(buffId));
            }
            var socketInstances = _item.ItemSockets.Gems
                .Select((gem, slot) => (gem, slot))
                .Where(t => t.gem.Definition.Buff.ApplyType == ApplyType.OnObject)
                .Select(t => GetSocketInstanceId(t.slot))
                .ToArray();

            foreach (var (instanceId, buffId, _) in activeBuffs)
            {
                var buff = Amalur.GetBuff(buffId);
                if (GetSelfBuffInstanceId(List.IndexOf(buff)) != instanceId
                    && instanceId != GetAffixInstanceId(Prefix)
                    && instanceId != GetAffixInstanceId(Suffix)
                    && !socketInstances.Contains(instanceId))
                {
                    SetOfInstances.Add((item, instanceId));
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
                buffData[i] = List[i].Id | ((ulong)uint.MaxValue) << 32;
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