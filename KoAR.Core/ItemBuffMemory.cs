using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public sealed class ItemBuffMemory : IItemBuffMemory
    {
        internal static List<(Item item, uint instanceId)> SetOfInstances = new();
        private static class Offsets
        {
            public const int DataLength = 13;
            public const int BuffCount = DataLength + 4;
            public const int FirstActiveBuff = BuffCount + 4;
        }

        internal static uint GetSelfBuffInstanceId(int index) => (uint)Hasher.GetHash($"selfbuff_{index}");
        internal static uint GetAffixInstanceId(Buff? buff) => buff is { BuffType: var type, Id: var id } ? (uint)Hasher.GetHash($"selfbuff{type}_{id}") : 0;
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
            get => BitConverter.ToInt32(Bytes, Offsets.DataLength) + 17;
            set => Unsafe.WriteUnaligned(ref Bytes[Offsets.DataLength], value - 17);
        }

        private int Count
        {
            get => BitConverter.ToInt32(Bytes, Offsets.BuffCount);
            set => Unsafe.WriteUnaligned(ref Bytes[Offsets.BuffCount], value);
        }

        private int ActiveBuffCount => List.Count
            + (Prefix?.ApplyType == ApplyType.OnObject ? 1 : 0)
            + (Suffix?.ApplyType == ApplyType.OnObject ? 1 : 0)
            + _item.ItemSockets.Gems.Count(x => x.Definition.Buff.ApplyType == ApplyType.OnObject);

        public Buff? Prefix
        {
            get => Amalur.Buffs.GetOrDefault(BitConverter.ToUInt32(Bytes, Bytes.Length - 8));
            set => Unsafe.WriteUnaligned(ref Bytes[Bytes.Length - 8], value?.Id ?? 0);
        }

        public Buff? Suffix
        {
            get => Amalur.Buffs.GetOrDefault(BitConverter.ToUInt32(Bytes, Bytes.Length - 4));
            set => Unsafe.WriteUnaligned(ref Bytes[Bytes.Length - 4], value?.Id ?? 0);
        }

        internal byte[] Serialize(bool forced = false)
        {
            if (!forced && ActiveBuffCount == Count)
            {
                return Bytes;
            }
            var currentLength = Bytes.Length - 8 - Offsets.FirstActiveBuff;
            var activeBuffBytes = MemoryMarshal.AsBytes(
                (Prefix?.ApplyType == ApplyType.OnObject ? new[] { new BuffInstance(GetAffixInstanceId(Prefix), Prefix.Id) } : Array.Empty<BuffInstance>())
                .Concat(Suffix?.ApplyType == ApplyType.OnObject ? new[] { new BuffInstance(GetAffixInstanceId(Suffix), Suffix.Id) } : Array.Empty<BuffInstance>())
                .Concat(List.Select((buff, i) => new BuffInstance(GetSelfBuffInstanceId(i), buff.Id)))
                .Concat(_item.ItemSockets.Gems
                        .Select((gem, slot) => (gem.Definition.Buff, slot))
                        .Where(x => x.Buff.ApplyType == ApplyType.OnObject)
                        .Select(x => new BuffInstance(GetSocketInstanceId(x.slot), x.Buff.Id)))
                .ToArray()
                .AsSpan());
            var selfBuffBytes = MemoryMarshal.AsBytes(List.Select(buff => new BuffDuration(buff.Id)).ToArray().AsSpan());
            Span<byte> buffer = stackalloc byte[8 + activeBuffBytes.Length + selfBuffBytes.Length];
            activeBuffBytes.CopyTo(buffer);
            Unsafe.WriteUnaligned(ref buffer[activeBuffBytes.Length + 4], List.Count);
            selfBuffBytes.CopyTo(buffer[(activeBuffBytes.Length + 8)..]);
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstActiveBuff, currentLength, MemoryMarshal.AsBytes(buffer));
            Count = ActiveBuffCount;
            DataLength = Bytes.Length;
            return Bytes;
        }
    }
}