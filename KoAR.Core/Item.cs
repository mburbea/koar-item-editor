﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace KoAR.Core
{
    public partial class Item : IItem
    {
        public const float DurabilityLowerBound = 0f;
        public const float DurabilityUpperBound = 100f;
        public const int MinEquipmentLength = 44;

        public Item(GameSave gameSave, int typeIdOffset, int offset, int dataLength, int itemBuffsOffset, int itemBuffsLength, int itemGemsOffset, int itemGemsLength)
        {
            (_gameSave, TypeIdOffset, ItemOffset) = (gameSave, typeIdOffset, offset);
            _levelShiftOffset = (byte)(8 * gameSave.Body[TypeIdOffset + 10]);
            ItemBytes = _gameSave.Body.AsSpan(offset, dataLength).ToArray();
            ItemSockets = new ItemSockets(gameSave, itemGemsOffset, itemGemsLength);
            ItemBuffs = new ItemBuffMemory(gameSave, this, itemBuffsOffset, itemBuffsLength);
            PlayerBuffs = new List<Buff>(BuffCount);
            for (int i = 0; i < PlayerBuffs.Capacity; i++)
            {
                PlayerBuffs.Add(Amalur.GetBuff(MemoryUtilities.Read<uint>(ItemBytes, Offsets.FirstBuff + i * 8)));
            }
            if (HasCustomName)
            {
                ItemName = _gameSave.Encoding.GetString(ItemBytes, Offsets.Name, NameLength);
            }
        }

        public bool IsEquipped => _gameSave.EquippedItems.Contains(this);

        public ItemBuffMemory ItemBuffs { get; }

        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.CurrentDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.CurrentDurability, value);
        }

        internal int DataLength
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.DataLength) + 17;
            set => MemoryUtilities.Write(ItemBytes, Offsets.DataLength, value - 17);
        }

        public List<Buff> PlayerBuffs { get; } = new List<Buff>();

        public bool HasCustomName
        {
            get => ItemBytes[Offsets.HasCustomName] == 1;
            private set => ItemBytes[Offsets.HasCustomName] = (byte)(value ? 1 : 0);
        }

        private ref InventoryFlags Flags => ref Unsafe.As<byte, InventoryFlags>(ref ItemBytes[Offsets.InventoryFlags]);

        public int Owner => MemoryUtilities.Read<int>(ItemBytes, Offsets.Owner);

        public bool IsStolen
        {
            get => (Flags & InventoryFlags.IsFromStolenSource) == InventoryFlags.IsFromStolenSource;
            set => Flags = value ? Flags | InventoryFlags.IsFromStolenSource : Flags & ~InventoryFlags.IsFromStolenSource;
        }

        public bool IsUnsellable
        {
            get => (Flags & InventoryFlags.Unsellable) == InventoryFlags.Unsellable;
            set => Flags = value ? Flags | InventoryFlags.Unsellable : Flags & ~InventoryFlags.Unsellable;
        }

        public bool IsUnstashable
        {
            get => (Flags & InventoryFlags.Unstashable) == InventoryFlags.Unstashable;
            set => Flags = value ? Flags | InventoryFlags.Unstashable : Flags & ~InventoryFlags.Unstashable;
        }

        public ItemDefinition Definition
        {
            get => Amalur.ItemDefinitions[MemoryUtilities.Read<uint>(_gameSave.Body, TypeIdOffset)];
            private set
            {
                var oldType = Amalur.ItemDefinitions[MemoryUtilities.Read<uint>(_gameSave.Body, TypeIdOffset)];
                MemoryUtilities.Write(_gameSave.Body, TypeIdOffset, value.TypeId);
                MemoryUtilities.Write(_gameSave.Body, TypeIdOffset + 30 + _levelShiftOffset, value.TypeId);
                if (oldType.Category == EquipmentCategory.Shield && oldType.ArmorType != value.ArmorType)
                {
                    _gameSave.Body[TypeIdOffset + 14] = value.ArmorType switch
                    {
                        ArmorType.Finesse => 0xEC,
                        ArmorType.Might => 0xED,
                        ArmorType.Sorcery => 0xEE,
                        _ => _gameSave.Body[TypeIdOffset + 14],
                    };
                }
            }
        }


        private readonly byte _levelShiftOffset;
        private readonly GameSave _gameSave;
        private int LevelOffset => TypeIdOffset + 14 + _levelShiftOffset;

        public byte Level
        {
            get => _gameSave.Body[LevelOffset];
            set => _gameSave.Body[LevelOffset] = value;
        }

        internal byte[] ItemBytes { get; private set; }

        public int ItemId => MemoryUtilities.Read<int>(ItemBytes);

        public ItemSockets ItemSockets { get; }

        public int ItemOffset { get; internal set; }
        internal int TypeIdOffset { get; set; }

        private int NameLength
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.NameLength);
            set => MemoryUtilities.Write(ItemBytes, Offsets.NameLength, value);
        }

        public Rarity Rarity => Definition.Rarity == Rarity.Set
            ? Rarity.Set
            : PlayerBuffs.Select(x => x.Rarity)
                .Concat(ItemBuffs.List.Select(x => x.Rarity))
                .Concat(ItemSockets.Gems.Select(x => x.Definition.Buff.Rarity))
                .Concat(new[] { ItemBuffs.Prefix?.Rarity ?? default, ItemBuffs.Suffix?.Rarity ?? default, Definition.SocketTypes.Any() ? Rarity.Infrequent : Rarity.Common })
                .Max();

        public string ItemName { get; set; } = string.Empty;

        public float MaxDurability
        {
            get => MemoryUtilities.Read<float>(ItemBytes, Offsets.MaxDurability);
            set => MemoryUtilities.Write(ItemBytes, Offsets.MaxDurability, value);
        }

        private int BuffCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.BuffCount);
            set => MemoryUtilities.Write(ItemBytes, Offsets.BuffCount, value);
        }

        private Offset Offsets => new Offset(this);

        IItemBuffMemory IItem.ItemBuffs => ItemBuffs;

        public static bool IsValidDurability(float durability) => durability >= DurabilityLowerBound && durability <= DurabilityUpperBound;

        internal byte[] Serialize(bool forced = false)
        {
            if (HasCustomName != (ItemName.Length != 0)
                || HasCustomName && ItemName != _gameSave.Encoding.GetString(ItemBytes, Offsets.Name, NameLength))
            {
                if (ItemName.Length > 0)
                {
                    var newBytes = _gameSave.Encoding.GetBytes(ItemName);
                    if (Offsets.Name + newBytes.Length != ItemBytes.Length)
                    {
                        var buffer = new byte[Offsets.Name + newBytes.Length];
                        ItemBytes.AsSpan(0, Offsets.NameLength).CopyTo(buffer);
                        ItemBytes = buffer;
                    }
                    HasCustomName = true;
                    NameLength = newBytes.Length;
                    newBytes.CopyTo(ItemBytes, Offsets.Name);
                }
                else if (HasCustomName)
                {
                    ItemBytes = ItemBytes.AsSpan(0, Offsets.NameLength).ToArray();
                    HasCustomName = false;
                }
                DataLength = ItemBytes.Length;
            }

            if (!forced && PlayerBuffs.Count == BuffCount)
            {
                return ItemBytes;
            }
            var currentLength = Offsets.PostBuffs - Offsets.FirstBuff;
            MemoryUtilities.Write(ItemBytes, Offsets.BuffCount, PlayerBuffs.Count);
            Span<ulong> buffData = stackalloc ulong[PlayerBuffs.Count];
            for (int i = 0; i < buffData.Length; i++)
            {
                buffData[i] = PlayerBuffs[i].Id | (ulong)uint.MaxValue << 32;
            }
            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offsets.FirstBuff, currentLength, MemoryMarshal.AsBytes(buffData));
            DataLength = ItemBytes.Length;
            return ItemBytes;
        }

        public void ChangeDefinition(ItemDefinition definition, bool retainStats)
        {
            Definition = definition;
            if (retainStats)
            {
                return;
            }
            if (_gameSave.IsRemaster && definition.Category.IsJewelry())
            {
                CurrentDurability = 100;
                MaxDurability = -1;
            }
            else
            {
                CurrentDurability = MaxDurability = definition.MaxDurability;
            }
            ItemBuffs.List.Clear();
            foreach (var buff in definition.ItemBuffs.List)
            {
                ItemBuffs.List.Add(buff);
            }
            ItemBuffs.UnsupportedFormat = false; // If there was crap here it's gone now.
            ItemBuffs.Prefix = definition.ItemBuffs.Prefix;
            ItemBuffs.Suffix = definition.ItemBuffs.Suffix;
            PlayerBuffs.Clear();
            PlayerBuffs.AddRange(definition.PlayerBuffs);
            Level = definition.Level;
        }

        public IEnumerable<Socket> GetSockets()
        {
            return ItemSockets.Gems.Length switch
            {
                0 => Definition.GetSockets(),
                1 when Definition.SocketTypes.Length == 1 => new[] { new Socket(Definition.SocketTypes[0], ItemSockets.Gems[0]) }, // trivial case.
                _ => Inner(Definition.SocketTypes, ItemSockets.Gems.ToArray())
            };

            static IEnumerable<Socket> Inner(string sockets, Gem[] gems)
            {
                int start = 0;
                foreach (var socket in sockets)
                {
                    Gem? gem = null;
                    for (int i = start; i < gems.Length; i++)
                    {
                        if (gems[i].Definition.SocketType == socket)
                        {
                            gem = gems[i];
                            (gems[start], gems[i]) = (gems[i], gems[start]);
                            start++;
                            break;
                        }
                    }
                    yield return new Socket(socket, gem);
                }
            }
        }
    }
}