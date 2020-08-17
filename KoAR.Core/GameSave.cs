﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public sealed class GameSave
    {
        private readonly int _bagOffset;
        private readonly int[] _dataLengthOffsets;
        private Container _itemBuffsContainer;
        private Container _itemContainer;
        private Container _itemSocketsContainer;

        public GameSave(string fileName)
        {
            Bytes = File.ReadAllBytes(FileName = fileName);
            ReadOnlySpan<byte> data = Bytes;
            Stash = Stash.TryCreateStash(this);
            _bagOffset = GetBagOffset(data);
            _dataLengthOffsets = new[]{
                data.IndexOf(new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 }) - 4, // file length
                data.IndexOf(new byte[5] { 0x0C, 0xAE, 0x32, 0x00, 0x00 }) + 5, // unknown length 1
                data.IndexOf(new byte[5] { 0xF7, 0x5D, 0x3C, 0x00, 0x0A }) + 5, // unknown length 2
                data.IndexOf(new byte[5] { 0x23, 0xCC, 0x58, 0x00, 0x03 }) + 5, // type section length
            };
            _itemContainer = new Container(this, data.IndexOf(new byte[5] { 0xD3, 0x34, 0x43, 0x00, 0x00 }), 0x00_24_D5_68_00_00_00_0Bul);
            _itemBuffsContainer = new Container(this, data.IndexOf(new byte[5] { 0xBB, 0xD5, 0x43, 0x00, 0x00 }), 0x00_28_60_84_00_00_00_0Bul);
            _itemSocketsContainer = new Container(this, data.IndexOf(new byte[5] { 0x93, 0xCC, 0x80, 0x00, 0x00 }), 0x00_59_36_38_00_00_00_0Bul);
            var itemLocs = _itemContainer.ToDictionary(x => x.id, x => (x.offset, x.dataLength));
            var itemBuffsLocs = _itemBuffsContainer.ToDictionary(x => x.id, x => (x.offset, x.dataLength));
            var itemSocketsLocs = _itemSocketsContainer.ToDictionary(x => x.id, x => (x.offset, x.dataLength));
            int dataLength, playerActor = 0;
            var candidates = new List<(int id, int typeIdOffset, QuestItemDefinition? questItemDef)>();

            for (int ixOfActor = _dataLengthOffsets[^1] + 4; BitConverter.ToInt32(Bytes, ixOfActor) == 0x00_75_2D_06; ixOfActor += dataLength)
            {
                dataLength = 9 + BitConverter.ToInt32(Bytes, ixOfActor + 5);
                var id = BitConverter.ToInt32(Bytes, ixOfActor + 9);
                var typeIdOffset = ixOfActor + 13;
                var typeId = BitConverter.ToUInt32(Bytes, typeIdOffset);
                if (Amalur.ItemDefinitions.ContainsKey(typeId))
                {
                    candidates.Add((id, typeIdOffset, null));
                }
                else if (Amalur.QuestItemDefinitions.TryGetValue(typeId, out var questItemDefinition))
                {
                    candidates.Add((id, typeIdOffset, questItemDefinition));
                }
                else if (Amalur.GemDefinitions.ContainsKey(typeId))
                {
                    Gems.Add(id, new Gem(this, typeIdOffset));
                }
                else if (Amalur.PlayerTypeIds.IndexOf(typeId) != -1)
                {
                    playerActor = id;
                }
            }
            foreach (var (id, typeIdOffset, questItemDef) in candidates)
            {
                var (itemOffset, itemLength) = itemLocs[id];
                if (BitConverter.ToInt32(Bytes, itemOffset + 17) == playerActor)
                {
                    if (questItemDef != null)
                    {
                        QuestItems.Add(new QuestItem(this, questItemDef, itemOffset + itemLength - 3));
                    }
                    else
                    {
                        var (itemBuffsOffset, itemBuffsLength) = itemBuffsLocs[id];
                        var (itemGemsOffset, itemGemsLength) = itemSocketsLocs[id];
                        Items.Add(new Item(this, typeIdOffset, itemOffset, itemLength, itemBuffsOffset, itemBuffsLength, itemGemsOffset, itemGemsLength));
                    }
                }
            }

            static int GetBagOffset(ReadOnlySpan<byte> data)
            {
                ReadOnlySpan<byte> inventoryLimit = new[] { (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'l', (byte)'i', (byte)'m', (byte)'i', (byte)'t' };
                ReadOnlySpan<byte> increaseAmount = new[] { (byte)'i', (byte)'n', (byte)'c', (byte)'r', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'_', (byte)'a', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
                ReadOnlySpan<byte> currentInventoryCount = new[] { (byte)'c', (byte)'u', (byte)'r', (byte)'r', (byte)'e', (byte)'n', (byte)'t', (byte)'_', (byte)'i', (byte)'n', (byte)'v', (byte)'e', (byte)'n', (byte)'t', (byte)'o', (byte)'r', (byte)'y', (byte)'_', (byte)'c', (byte)'o', (byte)'u', (byte)'n', (byte)'t' };
                var curInvCountOffset = data.IndexOf(currentInventoryCount) + currentInventoryCount.Length;
                var inventoryLimitOffset = data.IndexOf(inventoryLimit) + inventoryLimit.Length;
                var increaseAmountOffset = data.IndexOf(increaseAmount) + increaseAmount.Length;
                var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
                var inventoryLimitOrder = inventoryLimitOffset == finalOffset
                    ? 3
                    : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;

                return finalOffset + (inventoryLimitOrder * 12);
            }
        }

        public byte[] Bytes { get; internal set; }

        public string FileName { get; }

        public int InventorySize
        {
            get => MemoryUtilities.Read<int>(Bytes, _bagOffset);
            set => MemoryUtilities.Write(Bytes, _bagOffset, value);
        }

        public List<Item> Items { get; } = new List<Item>();

        public Dictionary<int, Gem> Gems { get; } = new Dictionary<int, Gem>();

        public Stash? Stash { get; private set; }

        public List<QuestItem> QuestItems { get; } = new List<QuestItem>();

        internal void UpdateDataLengths(int itemOffset, int delta)
        {
            foreach (var offset in _dataLengthOffsets)
            {
                if (offset < itemOffset)
                {
                    var oldVal = MemoryUtilities.Read<int>(Bytes, offset);
                    MemoryUtilities.Write(Bytes, offset, delta + oldVal);
                }
            }
        }

        public void SaveFile()
        {
            File.Copy(FileName, $"{FileName}.bak", true);
            File.WriteAllBytes(FileName, Bytes);
        }

        public void UpdateOffsets(int itemOffset, int delta)
        {
            _itemSocketsContainer.Offset += _itemSocketsContainer.Offset > itemOffset ? delta : 0;
            _itemBuffsContainer.Offset += _itemBuffsContainer.Offset > itemOffset ? delta : 0;
            _itemContainer.Offset += _itemContainer.Offset > itemOffset ? delta : 0;
            for (int i = 0; i < _dataLengthOffsets.Length; i++)
            {
                _dataLengthOffsets[i] += _dataLengthOffsets[i] > itemOffset ? delta : 0;
            }
            foreach (var item in Stash?.Items ?? Enumerable.Empty<StashItem>())
            {
                if (item.ItemOffset > itemOffset)
                {
                    item.ItemOffset += delta;
                }
            }
            foreach (var item in Items)
            {
                if (item.ItemSockets.ItemOffset > itemOffset)
                {
                    item.ItemSockets.ItemOffset += delta;
                }
                if (item.ItemBuffs.ItemOffset > itemOffset)
                {
                    item.ItemBuffs.ItemOffset += delta;
                }
                if (item.ItemOffset > itemOffset)
                {
                    item.ItemOffset += delta;
                }
                if (item.TypeIdOffset > itemOffset)
                {
                    item.TypeIdOffset += delta;
                }
            }
            foreach (var questItem in QuestItems)
            {
                if (questItem.Offset > itemOffset)
                {
                    questItem.Offset += delta;
                }
            }
        }

        public void WriteEquipmentBytes(Item item, bool forced = false)
        {
            int WriteSection(int itemOffset, int dataLength, ReadOnlySpan<byte> newBytes)
            {
                var prevLength = Bytes.Length;
                Bytes = MemoryUtilities.ReplaceBytes(Bytes, itemOffset, dataLength, newBytes);
                return Bytes.Length - prevLength;
            }

            var delta = WriteSection(item.ItemBuffs.ItemOffset, item.ItemBuffs.DataLength, item.ItemBuffs.Serialize(forced));
            if (delta != 0)
            {
                UpdateOffsets(item.ItemBuffs.ItemOffset, delta);
                _itemBuffsContainer.UpdateDataLength(delta);
            }
            var delta2 = WriteSection(item.ItemOffset, item.DataLength, item.Serialize(forced));
            if (delta2 != 0)
            {
                UpdateOffsets(item.ItemOffset, delta2);
                _itemContainer.UpdateDataLength(delta2);
            }

            UpdateDataLengths(item.ItemOffset, delta + delta2);
        }
    }
}
