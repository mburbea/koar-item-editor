using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public partial class Item : IItem
    {
        public const float DurabilityLowerBound = 0f;
        public const float DurabilityUpperBound = 100f;

        public Item(GameSave gameSave, int typeIdOffset, int offset, int dataLength, int itemBuffsOffset, int itemBuffsLength, int itemGemsOffset, int itemGemsLength)
        {
            (_gameSave, TypeIdOffset, ItemOffset) = (gameSave, typeIdOffset, offset);
            _levelShiftOffset = (byte)(8 * gameSave.Body[TypeIdOffset + 10]);
            Bytes = _gameSave.Body.AsSpan(offset, dataLength).ToArray();
            ItemSockets = new(gameSave, itemGemsOffset, itemGemsLength);
            ItemBuffs = new(gameSave, this, itemBuffsOffset, itemBuffsLength);
            var span = Bytes.AsSpan(Offsets.BuffCount);
            foreach (var (buffId, _) in BuffDuration.ReadList(ref span))
            {
                PlayerBuffs.Add(Amalur.GetBuff(buffId));
            }
            if (HasCustomName)
            {
                ItemName = _gameSave.Encoding.GetString(Bytes, Offsets.Name, NameLength);
            }
        }

        public bool IsEquipped => _gameSave.EquippedItems.Contains(this);

        public ItemBuffMemory ItemBuffs { get; }

        public float CurrentDurability
        {
            get => MemoryUtilities.Read<float>(Bytes, Offsets.CurrentDurability);
            set => MemoryUtilities.Write(Bytes, Offsets.CurrentDurability, value);
        }

        internal int DataLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.DataLength) + 17;
            set => MemoryUtilities.Write(Bytes, Offsets.DataLength, value - 17);
        }

        public List<Buff> PlayerBuffs { get; } = new();

        public bool HasCustomName
        {
            get => Bytes[Offsets.HasCustomName] == 1;
            private set => Bytes[Offsets.HasCustomName] = (byte)(value ? 1 : 0);
        }

        private ref InventoryFlags Flags => ref Unsafe.As<byte, InventoryFlags>(ref Bytes[Offsets.InventoryFlags]);

        public int Owner => MemoryUtilities.Read<int>(Bytes, Offsets.Owner);

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

        internal byte[] Bytes { get; private set; }

        public int ItemId => MemoryUtilities.Read<int>(Bytes);

        public ItemSockets ItemSockets { get; }

        public int ItemOffset { get; internal set; }
        internal int TypeIdOffset { get; set; }

        private int NameLength
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.NameLength);
            set => MemoryUtilities.Write(Bytes, Offsets.NameLength, value);
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
            get => MemoryUtilities.Read<float>(Bytes, Offsets.MaxDurability);
            set => MemoryUtilities.Write(Bytes, Offsets.MaxDurability, value);
        }

        private int BuffCount
        {
            get => MemoryUtilities.Read<int>(Bytes, Offsets.BuffCount);
            set => MemoryUtilities.Write(Bytes, Offsets.BuffCount, value);
        }

        private Offset Offsets => new(this);

        IItemBuffMemory IItem.ItemBuffs => ItemBuffs;

        public static bool IsValidDurability(float durability) => durability >= DurabilityLowerBound && durability <= DurabilityUpperBound;

        internal byte[] Serialize(bool forced = false)
        {
            if (HasCustomName != (ItemName.Length != 0)
                || HasCustomName && ItemName != _gameSave.Encoding.GetString(Bytes, Offsets.Name, NameLength))
            {
                if (ItemName.Length > 0)
                {
                    var newBytes = _gameSave.Encoding.GetBytes(ItemName);
                    if (Offsets.Name + newBytes.Length != Bytes.Length)
                    {
                        var buffer = new byte[Offsets.Name + newBytes.Length];
                        Bytes.AsSpan(0, Offsets.NameLength).CopyTo(buffer);
                        Bytes = buffer;
                    }
                    HasCustomName = true;
                    NameLength = newBytes.Length;
                    newBytes.CopyTo(Bytes, Offsets.Name);
                }
                else if (HasCustomName)
                {
                    Bytes = Bytes.AsSpan(0, Offsets.NameLength).ToArray();
                    HasCustomName = false;
                }
                DataLength = Bytes.Length;
            }

            if (!forced && PlayerBuffs.Count == BuffCount)
            {
                return Bytes;
            }
            var currentLength = Offsets.PostBuffs - Offsets.FirstBuff;
            MemoryUtilities.Write(Bytes, Offsets.BuffCount, PlayerBuffs.Count);
            Span<BuffDuration> buffData = PlayerBuffs.Select(buff => new BuffDuration(buff.Id)).ToArray();
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstBuff, currentLength, MemoryMarshal.AsBytes(buffData));
            DataLength = Bytes.Length;
            return Bytes;
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
                    yield return new(socket, gem);
                }
            }
        }
    }
}