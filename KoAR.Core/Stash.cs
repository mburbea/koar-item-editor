using System;
using System.Collections.Generic;

namespace KoAR.Core
{
    public class Stash
    {
        private readonly GameSave _gameSave;

        private readonly int _offset;

        public Stash(GameSave gameSave, int offset)
        {
            ReadOnlySpan<byte> itemMarker = new byte[] { 0x0A, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            (_gameSave, _offset) = (gameSave, offset);
            int lastOffset = _offset + 17;
            Items.Capacity = Count;
            Span<byte> data = _gameSave.Bytes.AsSpan(0, _offset + DataLength);
            for(int i = 1; i < Items.Capacity; i++)
            {
                var ix = data.Slice(lastOffset + 4 + itemMarker.Length).IndexOf(itemMarker) - 4;
                if(ix < 0)
                {
                    break; // shouldn't happen.
                }
                Items.Add(new StashItem(gameSave, lastOffset, ix));
                lastOffset += ix;
            }
            if(Items.Count != Items.Capacity)
            {
                Items.Add(new StashItem(gameSave, lastOffset, DataLength - 21 - lastOffset));
            }
        }

        public int DataLength
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, _offset);
            private set
            {
                MemoryUtilities.Write(_gameSave.Bytes, _offset, value);
                MemoryUtilities.Write(_gameSave.Bytes, _offset + 9, value - 9);
            }
        }

        public int Count
        {
            get => MemoryUtilities.Read<int>(_gameSave.Bytes, _offset + 13);
            private set => MemoryUtilities.Write(_gameSave.Bytes, _offset + 13, value);
        }

        public List<StashItem> Items { get; } = new List<StashItem>();

        public void AddItem(TypeDefinition type)
        {
            // Why don't we use the stashitem class? 
            // 1. Because we don't support mutating them yet
            // 2. We blow away everything when we do this operation anyway.
            // 3. We rely on the fact that the game will regenerate the ItemBuff section when the stash spawns the item.
            Span<byte> temp = stackalloc byte[25 + type.PlayerBuffs.Length * 8];
            MemoryUtilities.Write(temp, 0, type.TypeId | ((ulong)0x03_0A) << 32); // 8
            MemoryUtilities.Write(temp, 10, type.MaxDurability);
            temp[14] = 1;
            MemoryUtilities.Write(temp, 18, type.PlayerBuffs.Length);
            for (int i = 0; i < type.PlayerBuffs.Length; i++)
            {
                MemoryUtilities.Write(temp, i * 8 + 22, type.PlayerBuffs[i].Id | ((ulong)uint.MaxValue) << 32);
            }
            temp[^1] = 0xFF;
            _gameSave.Bytes = MemoryUtilities.ReplaceBytes(_gameSave.Bytes, _offset + 17, 0, temp);
            DataLength += temp.Length;
            Count++;
        }

        public static Stash? TryCreateStash(GameSave gameSave)
        {
            ReadOnlySpan<byte> stashIndicator = new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 };
            var offset = gameSave.Bytes.AsSpan().IndexOf(stashIndicator);
            return offset != -1 ? new Stash(gameSave, offset - 3) : null;
        }
    }
}
