using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public sealed class GameSave
    {
        private int? _bagOffset;
        private int[] _dataLengthOffsets = Array.Empty<int>();

        private Container _itemBuffsContainer;
        private Container _itemContainer;
        private Container _itemGemsContainer;

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

        private void GetAllEquipment()
        {
            ReadOnlySpan<byte> unknownLengthSeq = new byte[] { 0x0C, 0xAE, 0x32, 0x00, 0x00 };
            ReadOnlySpan<byte> unknownLengthSeq2 = new byte[] { 0xF7, 0x5D, 0x3C, 0x00, 0x0A };
            ReadOnlySpan<byte> typeIdSeq = new byte[] { 0x23, 0xCC, 0x58, 0x00, 0x03 };
            ReadOnlySpan<byte> fileLengthSeq = new byte[8] { 0, 0, 0, 0, 0xA, 0, 0, 0 };
            ReadOnlySpan<byte> itemsMarker = new byte[5] { 0xD3, 0x34, 0x43, 0x00, 0x00 };
            ReadOnlySpan<byte> itemBuffsMarker = new byte[5] { 0xBB, 0xD5, 0x43, 0x00, 0x00 };
            ReadOnlySpan<byte> itemGemsMarker = new byte[5] { 0x93, 0xCC, 0x80, 0x00, 0x00 };
            const int playerHumanMale = 0x0A386D;
            const int playerHumanFemale = 0x0A386E;
            const int playerElfMale = 0x0A386F;
            const int playerElfFemale = 0x0A3870;

            ReadOnlySpan<byte> data = Bytes;
            _dataLengthOffsets = new[]{
                data.IndexOf(fileLengthSeq) - 4,
                data.IndexOf(unknownLengthSeq) + 5,
                data.IndexOf(unknownLengthSeq2) + 5,
                data.IndexOf(typeIdSeq) + 5,
            };

            _itemContainer = new Container(this, data.IndexOf(itemsMarker), 0x00_24_D5_68_00_00_00_0Bul);
            _itemBuffsContainer = new Container(this, data.IndexOf(itemBuffsMarker), 0x00_28_60_84_00_00_00_0Bul);
            _itemGemsContainer = new Container(this, data.IndexOf(itemGemsMarker), 0x00_59_36_38_00_00_00_0Bul);
            var itemMemoryLocs = _itemContainer.ToDictionary(x => x.id, x => (x.offset, x.dataLength));
            var itemBuffLocs = _itemBuffsContainer.ToDictionary(x => x.id, x => (x.offset, x.dataLength));
            var itemGemLocs = _itemGemsContainer.ToDictionary(x => x.id, x => (x.offset, x.dataLength));
            QuestItems.Clear();
            Items.Clear();
            Gems.Clear();
            Stash = Stash.TryCreateStash(this);
            int ixOfActor = _dataLengthOffsets[^1] + 4;
            int playerActor = 0;
            var candidates = new List<(int id, int typeIdOffset)>();
            var questItemCandidates = new List<(int id, QuestItemDefinition questItemDefinition)>();

            if (BitConverter.ToInt32(Bytes, ixOfActor) == 0)
            {
                ixOfActor += 4;
            }
            while (BitConverter.ToInt32(Bytes, ixOfActor) == 0x00_75_2D_06)
            {
                var dataLength = 9 + BitConverter.ToInt32(Bytes, ixOfActor + 5);
                var id = BitConverter.ToInt32(Bytes, ixOfActor + 9);
                var typeIdOffset = ixOfActor + 13;
                var typeId = BitConverter.ToUInt32(Bytes, typeIdOffset);
                if (Amalur.ItemDefinitions.ContainsKey(typeId))
                {
                    candidates.Add((id, typeIdOffset));
                }
                else if (Amalur.GemDefinitions.ContainsKey(typeId))
                {
                    Gems.Add(id, new Gem(this, typeIdOffset));
                }
                else if (typeId == playerHumanMale || typeId == playerHumanFemale || typeId == playerElfMale || typeId == playerElfFemale)
                {
                    playerActor = id;
                }
                else if (Amalur.QuestItemDefinitions.TryGetValue(typeId, out var questItemDefinition))
                {
                    questItemCandidates.Add((id, questItemDefinition));
                }
                ixOfActor += dataLength;
            }
            foreach (var (id, typeIdOffset) in candidates)
            {
                var (itemOffset, itemLength) = itemMemoryLocs[id];
                var (itemBuffsOffset, itemBuffsLength) = itemBuffLocs[id];
                var (itemGemsOffset, itemGemsLength) = itemGemLocs[id];
                if (BitConverter.ToInt32(Bytes, itemOffset + 17) == playerActor)
                {
                    Items.Add(new Item(this, typeIdOffset, itemOffset, itemLength, itemBuffsOffset, itemBuffsLength, itemGemsOffset, itemGemsLength));
                }
            }
            foreach (var (id, questItemDef) in questItemCandidates)
            {
                var (itemOffset, itemLength) = itemMemoryLocs[id];
                var inventoryState = (InventoryState)Bytes[itemOffset + itemLength - 3];
                if (BitConverter.ToInt32(Bytes, itemOffset + 17) == playerActor)
                {
                    QuestItems.Add(new QuestItem(this, questItemDef, itemOffset + itemLength - 3));
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
            if (delta != 0)
            {
                _itemGemsContainer = _itemGemsContainer.UpdateOffset(itemOffset, delta);
                _itemBuffsContainer = _itemBuffsContainer.UpdateOffset(itemOffset, delta);
                _itemContainer = _itemContainer.UpdateOffset(itemOffset, delta);
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
                    if (item.ItemGems.ItemOffset > itemOffset)
                    {
                        item.ItemGems.ItemOffset += delta;
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
        }

        public void WriteEquipmentBytes(Item item, bool forced = false)
        {
            int WriteItem(int itemOffset, int dataLength, byte[] bytes)
            {
                var prevLength = Bytes.Length;
                Bytes = MemoryUtilities.ReplaceBytes(Bytes, itemOffset, dataLength, bytes);
                int delta = Bytes.Length - prevLength;
                UpdateOffsets(itemOffset, delta);
                return delta;
            }

            var delta = WriteItem(item.ItemBuffs.ItemOffset, item.ItemBuffs.DataLength, item.ItemBuffs.Serialize(forced));
            if (delta != 0)
            {
                _itemBuffsContainer.UpdateDataLength(delta);
            }
            var delta2 = WriteItem(item.ItemOffset, item.DataLength, item.Serialize(forced));
            if (delta2 != 0)
            {
                _itemContainer.UpdateDataLength(delta2);
            }

            UpdateDataLengths(item.ItemOffset, delta + delta2);
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
