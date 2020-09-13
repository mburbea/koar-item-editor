using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace KoAR.Core
{
    public class Stash
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
            ReadOnlySpan<byte> itemMarker = new byte[] { 0x0A, (byte)(isRemaster ? 0x04 : 0x03), 0x00, 0x00, 0x00, 0x00 };
            var results = new List<int>();
            var start = 0;
            int ix = data.IndexOf(itemMarker);
            while (ix != -1)
            {
                results.Add(start + ix - 4);
                start += ix + itemMarker.Length;
                var segment = data.Slice(start);
                ix = segment.IndexOf(itemMarker);
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
                    if (Amalur.ItemDefinitions.ContainsKey(MemoryUtilities.Read<uint>(_gameSave.Body, _offset + indices[i])))
                    {
                        var item = Factory(gameSave, _offset + indices[i], indices[i + 1] - indices[i]);
                        Items.Add(item);
                        if(item.GemCount > 0u)
                        {
                            for(var j = 1; j <= item.GemCount; j++)
                            {
                                if (Amalur.GemDefinitions.ContainsKey(MemoryUtilities.Read<uint>(_gameSave.Body, _offset + indices[i + j])))
                                {
                                    item.Gems.Add(new Gem(_gameSave, _offset + indices[i + j]));
                                }
                            }
                        }
                    }
                    
                }
                // ok we might read this twice, who cares.
                if (Amalur.ItemDefinitions.ContainsKey(MemoryUtilities.Read<uint>(_gameSave.Body, _offset + indices[^1])))
                {
                    Items.Add(Factory(gameSave, _offset + indices[^1], DataLength - indices[^1]));
                }
            }

            static StashItem Factory(GameSave gameSave, int offset, int datalength) => gameSave.IsRemaster
                ? new RemasterStashItem(gameSave, offset, datalength)
                : new StashItem(gameSave, offset, datalength);
        }

        public int DataLength
        {
            get => MemoryUtilities.Read<int>(_gameSave.Body, _offset) - 17;
            private set
            {
                MemoryUtilities.Write(_gameSave.Body, _offset, value + 17);
                MemoryUtilities.Write(_gameSave.Body, _offset + Offsets.DataLength2, value - Offsets.DataLength2 + 17);
            }
        }

        private int Count
        {
            get => MemoryUtilities.Read<int>(_gameSave.Body, _offset + Offsets.Count);
            set => MemoryUtilities.Write(_gameSave.Body, _offset + Offsets.Count, value);
        }

        public List<StashItem> Items { get; } = new List<StashItem>();

        public StashItem AddItem(ItemDefinition type)
        {
            // Why don't we use the StashItem class? 
            // 1. Because we don't support mutating them yet
            // 2. We blow away everything when we do this operation anyway.
            // 3. We rely on the fact that the game will regenerate the ItemBuff section when the stash spawns the item. (Primarily to avoid thinking about instanceIds...)
            Span<byte> temp = stackalloc byte[25 + type.PlayerBuffs.Length * 8];
            var sectionHeader = _gameSave.IsRemaster ? 0x04_0Aul : 0x03_0Aul; 
            MemoryUtilities.Write(temp, 0, type.TypeId | sectionHeader << 32);
            MemoryUtilities.Write(temp, 10, type.MaxDurability);
            temp[14] = 1;
            MemoryUtilities.Write(temp, 18, type.PlayerBuffs.Length);
            for (int i = 0; i < type.PlayerBuffs.Length; i++)
            {
                MemoryUtilities.Write(temp, i * 8 + 22, type.PlayerBuffs[i].Id | ((ulong)uint.MaxValue) << 32);
            }
            temp[^1] = 0xFF;
            var offset = _offset + Offsets.FirstItem;
            _gameSave.Body = MemoryUtilities.ReplaceBytes(_gameSave.Body, offset, 0, temp);
            DataLength += temp.Length;
            Count++;
            Items.Add(new StashItem(_gameSave, offset, temp.Length));
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
        {
            ReadOnlySpan<byte> stashIndicator = new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 };
            var offset = gameSave.Body.AsSpan().IndexOf(stashIndicator);
            return offset != -1 ? new Stash(gameSave, offset - 3) : null;
        }
    }
}
