﻿using System;

namespace KoAR.Core
{
    public class Stash
    {
        public GameSave GameSave { get; }
        public int Offset { get; }

        public Stash(GameSave gameSave, int offset) => (GameSave, Offset) = (gameSave, offset);

        public int DataLength
        {
            get => MemoryUtilities.Read<int>(GameSave.Bytes, Offset);
            private set
            {
                MemoryUtilities.Write(GameSave.Bytes, Offset, value);
                MemoryUtilities.Write(GameSave.Bytes, Offset + 9, value - 9);
            }
        }

        public int Count
        {
            get => MemoryUtilities.Read<int>(GameSave.Bytes, Offset + 13);
            private set => MemoryUtilities.Write(GameSave.Bytes, Offset + 13, value);
        }

        public uint FirstItemTypeId
        {
            get => MemoryUtilities.Read<uint>(GameSave.Bytes, Offset + 17);
            private set => MemoryUtilities.Write(GameSave.Bytes, Offset + 17, value);
        }

        public void AddItem(TypeDefinition type)
        {
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
            GameSave.Bytes = MemoryUtilities.ReplaceBytes(GameSave.Bytes, Offset + 17, 0, temp);
            DataLength += temp.Length;
            Count++;
        }

        public static Stash? TryCreateStash(GameSave gameSave)
        {
            ReadOnlySpan<byte> stashIndicator = new byte[] { 0x00, 0xF5, 0x43, 0xEB, 0x00, 0x02 };
            var offset = gameSave.Bytes.AsSpan().IndexOf(stashIndicator);
            return offset == -1 ? null : new Stash(gameSave, offset - 3);
        }
    }
}
