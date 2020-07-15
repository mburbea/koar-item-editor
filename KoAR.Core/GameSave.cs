using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public sealed class GameSave
    {
        private int? _bagOffset;
        private int _fileLengthOffset;
        private int _simTypeOffset;
        private Container CoreEffectContainer;
        private Container ItemMemoryContainer;

        public GameSave(string fileName)
        {
            Bytes = File.ReadAllBytes(FileName = fileName);
            GetAllEquipment();
        }

        public byte[] Bytes { get; internal set; }

        public string FileName { get; }

        public int InventorySize
        {
            get => MemoryUtilities.Read<int>(Bytes, _bagOffset ??= GetBagOffset());
            set => MemoryUtilities.Write(Bytes, _bagOffset ??= GetBagOffset(), value);
        }

        public List<Item> Items { get; } = new List<Item>();

        public Stash? Stash { get; private set; }

        private int FileLength
        {
            get => MemoryUtilities.Read<int>(Bytes, _fileLengthOffset);
            set => MemoryUtilities.Write<int>(Bytes, _fileLengthOffset, value);
        }

        private int SimtypeSizes
        {
            get => MemoryUtilities.Read<int>(Bytes, _simTypeOffset + 5);
            set => MemoryUtilities.Write<int>(Bytes, _simTypeOffset + 5, value);
        }

        public void GetAllEquipment()
        {
            ReadOnlySpan<byte> typeIdSeq = new byte[] { 0x23, 0xCC, 0x58, 0x00, 0x03 };
            ReadOnlySpan<byte> fileLengthSeq = new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 };
            ReadOnlySpan<byte> ItemEffectMarker = new byte[5] { 0xD3, 0x34, 0x43, 0x00, 0x00 }; // 26 to first item. 5 to first DL, 13 to second DL. 18 for count
            ReadOnlySpan<byte> coreEffectMarker = new byte[5] { 0xBB, 0xD5, 0x43, 0x00, 0x00 }; // 26 to first item. 5 to first DL, 13 to second DL. 18 for count
            ReadOnlySpan<byte> data = Bytes;

            _fileLengthOffset = data.IndexOf(fileLengthSeq) - 4;
            ItemMemoryContainer = new Container(this, data.IndexOf(ItemEffectMarker), 0x00_24_D5_68_00_00_00_0Bul);
            CoreEffectContainer = new Container(this, data.IndexOf(coreEffectMarker), 0x00_28_60_84_00_00_00_0Bul);
            var itemMemoryLocs = ItemMemoryContainer.ToDictionary(x => x.id, x => (x.offset, x.datalength));
            var coreLocs = CoreEffectContainer.ToDictionary(x => x.id, x => (x.offset, x.datalength));
            Items.Clear();

            Stash = Stash.TryCreateStash(this);

            _simTypeOffset = data.IndexOf(typeIdSeq);
            int ixOfActor = _simTypeOffset + 9;
            if (BitConverter.ToInt32(Bytes, ixOfActor) == 0)
            {
                ixOfActor += 4;
            }

            while (BitConverter.ToInt32(Bytes, ixOfActor) == 0x00_75_2D_06)
            {
                var datalength = 9 + BitConverter.ToInt32(Bytes, ixOfActor + 5);
                var id = BitConverter.ToInt32(Bytes, ixOfActor + 9);
                var typeId = BitConverter.ToUInt32(Bytes, ixOfActor + 13);

                if (Amalur.TypeDefinitions.TryGetValue(typeId, out var definition))
                {
                    var (itemOffset, itemLength) = itemMemoryLocs[id];
                    var (coreOffset, coreLength) = coreLocs[id];
                    Items.Add(new Item(this, ixOfActor + 13, itemOffset, itemLength, coreOffset, coreLength));
                }
                ixOfActor += datalength;
            }
        }

        public void SaveFile()
        {
            File.Copy(FileName, $"{FileName}.bak", true);
            File.WriteAllBytes(FileName, Bytes);
        }

        public void WriteEquipmentBytes(Item item, bool forced = false)
        {
            int WriteItem(int itemIndex, int dataLength, byte[] bytes)
            {
                var prevLength = Bytes.Length;
                Bytes = MemoryUtilities.ReplaceBytes(Bytes, itemIndex, dataLength, bytes);
                int delta = Bytes.Length - prevLength;
                if (delta != 0)
                {
                    foreach (var item in Items)
                    {
                        if (item.ItemBuffs.ItemOffset > itemIndex)
                        {
                            item.ItemBuffs.ItemOffset += delta;
                        }
                        if (item.ItemOffset > itemIndex)
                        {
                            item.ItemOffset += delta;
                        }
                    }
                }
                return delta;
            }

            var delta = WriteItem(item.ItemBuffs.ItemOffset, item.ItemBuffs.DataLength, item.ItemBuffs.Serialize(forced));
            if (delta != 0)
            {
                CoreEffectContainer.UpdateDataLength(delta);
            }
            var delta2 = WriteItem(item.ItemOffset, item.DataLength, item.Serialize(forced));
            if (delta2 != 0)
            {
                ItemMemoryContainer.UpdateDataLength(delta2);
            }

            FileLength += (delta + delta2);
            SimtypeSizes += (delta + delta2);
        }

        private int GetBagOffset()
        {
            ReadOnlySpan<byte> inventoryLimit = new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
            ReadOnlySpan<byte> increaseAmount = new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
            ReadOnlySpan<byte> currentInventoryCount = new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
            ReadOnlySpan<byte> span = Bytes;
            var curInvCountOffset = span.IndexOf(currentInventoryCount) + currentInventoryCount.Length;
            var inventoryLimitOffset = span.IndexOf(inventoryLimit) + inventoryLimit.Length;
            var increaseAmountOffset = span.IndexOf(increaseAmount) + increaseAmount.Length;
            var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
            var inventoryLimitOrder = inventoryLimitOffset == finalOffset
                ? 3
                : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;

            return finalOffset + (inventoryLimitOrder * 12);
        }
    }
}
