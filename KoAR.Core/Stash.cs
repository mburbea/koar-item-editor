using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public sealed class Stash
    {
        private static class Offsets
        {
            public const int DataLength = 0;
            public const int DataLength2 = 9;
            public const int Count = 13;
            public const int FirstItem = 17;
        }

        private readonly GameSave _gameSave;

        private readonly int _offset;

        private static List<int> GetAllIndices(ReadOnlySpan<byte> data, bool isRemaster = false)
        {
            ReadOnlySpan<byte> itemMarker = isRemaster
                ? new byte[6] { 0x0A, 0x04, 0, 0, 0, 0 }
                : new byte[6] { 0x0A, 0x03, 0, 0, 0, 0 };
            var results = new List<int>();
            var start = 0;
            var segment = data;
            while (segment.IndexOf(itemMarker) is int ix and not -1)
            {
                results.Add(start + ix - 4);
                start += ix + itemMarker.Length;
                segment = segment[(ix + itemMarker.Length)..];
            }
            return results;
        }

        public Stash(GameSave gameSave, int offset)
        {
            (_gameSave, _offset) = (gameSave, offset);
            Items.Capacity = Count;
            Span<byte> data = _gameSave.Body.AsSpan(_offset, DataLength);

            if (Items.Capacity > 0)
            {
                var indices = GetAllIndices(data, gameSave.IsRemaster);
                for (int i = 0; i < indices.Count - 1; i++)
                {
                    if (Amalur.ItemDefinitions.ContainsKey(BitConverter.ToUInt32(_gameSave.Body, _offset + indices[i])))
                    {
                        var itemStart = indices[i];
                        var gems = Array.Empty<Gem>();
                        if (_gameSave.Body[_offset + indices[i + 1] - 1] != 0xFF)
                        {
                            var gemList = new List<Gem>();
                            var ix = _offset + indices[i + 1] - 4;
                            uint handle;
                            while ((handle = BitConverter.ToUInt32(_gameSave.Body, ix)) > 4u)
                            {
                                ix -= 4;
                            }
                            for (uint j = 0; j < handle; j++)
                            {
                                i++;
                                if (Amalur.GemDefinitions.ContainsKey(BitConverter.ToUInt32(_gameSave.Body, _offset + indices[i])))
                                {
                                    gemList.Add(new(_gameSave, _offset + indices[i]));
                                }
                            }
                            gems = gemList.ToArray();
                        }
                        var item = CreateStashItem(gameSave, _offset + itemStart, (i + 1 == indices.Count ? DataLength : indices[i + 1]) - itemStart, gems);
                        Items.Add(item);
                    }

                }
                // ok we might read this twice, who cares.
                if (Amalur.ItemDefinitions.ContainsKey(BitConverter.ToUInt32(_gameSave.Body, _offset + indices[^1])))
                {
                    Items.Add(CreateStashItem(gameSave, _offset + indices[^1], DataLength - indices[^1], Array.Empty<Gem>()));
                }
            }
        }

        static StashItem CreateStashItem(GameSave gameSave, int offset, int datalength, Gem[] gems) => gameSave.IsRemaster
            ? new RemasterStashItem(gameSave, offset, datalength, gems)
            : new(gameSave, offset, datalength, gems);

        public int DataLength
        {
            get => BitConverter.ToInt32(_gameSave.Body, _offset) - 17;
            private set
            {
                Unsafe.WriteUnaligned(ref _gameSave.Body[_offset], value + 17);
                Unsafe.WriteUnaligned(ref _gameSave.Body[_offset + Offsets.DataLength2], value - Offsets.DataLength2 + 17);
            }
        }

        private int Count
        {
            get => BitConverter.ToInt32(_gameSave.Body, _offset + Offsets.Count);
            set => Unsafe.WriteUnaligned(ref _gameSave.Body[_offset + Offsets.Count], value);
        }

        public List<StashItem> Items { get; } = new();

        public StashItem AddItem(ItemDefinition type)
        {
            // I don't write the item buff section as the game will regenerate it from the simtype blueprint when it spawns the item. (Primarily to avoid thinking about instanceIds...)
            Span<byte> temp = stackalloc byte[25 + type.PlayerBuffs.Length * 8];
            ulong sectionHeader = _gameSave.IsRemaster ? 0x04_0Aul : 0x03_0Aul;
            Unsafe.WriteUnaligned(ref temp[0], type.TypeId | sectionHeader << 32);
            Unsafe.WriteUnaligned(ref temp[10], _gameSave.IsRemaster && type.Category.IsJewelry() ? 100f : type.MaxDurability);
            temp[14] = 1; // quantity
            Unsafe.WriteUnaligned(ref temp[18], type.PlayerBuffs.Length);
            MemoryMarshal.AsBytes(Array.ConvertAll(type.PlayerBuffs, buff => new BuffDuration(buff.Id)).AsSpan()).CopyTo(temp[22..]);
            temp[^3] = (byte)(_gameSave.IsRemaster ? InventoryFlags.CanBeConvertedToGold | InventoryFlags.IsEquipment : default);
            temp[^2] = (byte)(_gameSave.IsRemaster switch
            {
                true when type.Category == EquipmentCategory.Shield => ExtendedInventoryFlags.IsShield,
                true when type.Category.IsWeapon() => ExtendedInventoryFlags.IsWeapon,
                _ => default
            });
            temp[^1] = 0xFF;
            var offset = _offset + Offsets.FirstItem;
            _gameSave.Body = MemoryUtilities.ReplaceBytes(_gameSave.Body, offset, 0, temp);
            DataLength += temp.Length;
            Count++;
            Items.Add(CreateStashItem(_gameSave, offset, temp.Length, Array.Empty<Gem>()));
            _gameSave.UpdateOffsets(offset, temp.Length);
            _gameSave.UpdateDataLengths(offset, temp.Length);
            return Items[^1];
        }

        public void DeleteItem(StashItem item)
        {
            var itemLength = item.DataLength;
            Items.Remove(item);
            _gameSave.Body = MemoryUtilities.ReplaceBytes(_gameSave.Body, item.ItemOffset, itemLength, Array.Empty<byte>());
            Count--;
            DataLength -= itemLength;
            _gameSave.UpdateOffsets(item.ItemOffset, -itemLength);
            _gameSave.UpdateDataLengths(item.ItemOffset, -itemLength);
        }

        public static Stash? TryCreateStash(GameSave gameSave)
            => gameSave.Body.AsSpan().IndexOf(new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 }) is int offset and not -1
                ? new(gameSave, offset - 3)
                : null;
    }
}
