using Force.Crc32;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KoAR.Core
{
    public sealed class GameSave
    {
        private const int MaxRemasterBodySize = 4 * 1024 * 1024;
        private const int CompressedFlag = 0x62_69_6C_7A; // zlib

        private readonly int _bagOffset;
        private readonly int _gameStateStartOffset;
        private readonly int[] _dataLengthOffsets;
        private readonly Container _itemBuffsContainer;
        private readonly Container _itemContainer;
        private readonly Container _itemSocketsContainer;
        private readonly GameSaveHeader _header;
        private int _originalBodyLength;

        public GameSave(string fileName)
        {
            Bytes = File.ReadAllBytes(FileName = fileName);
            IsRemaster = BitConverter.ToInt32(Bytes, 8) == 0;
            if (IsRemaster && !Path.GetFileNameWithoutExtension(fileName).StartsWith("svd_fmt_5_"))
            {
                throw new NotSupportedException("Save file is not a user save and changing them can lead to the game infinite looping. The editor only supports saves that start with svd_fmt_5.");
            }
            _header = new GameSaveHeader(this);
            if (BitConverter.ToInt32(Bytes, BodyStart) == CompressedFlag)
            {
                Body = new byte[_header.BodyDataLength];
                var bundleInfoStart = BodyStart + 12;
                var bundleInfoSize = BitConverter.ToInt32(Bytes, bundleInfoStart - 4);
                using var bundleInfoData = new ZlibStream(new MemoryStream(Bytes, bundleInfoStart, bundleInfoSize), CompressionMode.Decompress);
                var endOfBundle = bundleInfoData.Read(Body, 0, Body.Length);
                var gameStateStart = bundleInfoStart + bundleInfoSize + 4;
                var gameStateSize = BitConverter.ToInt32(Bytes, gameStateStart - 4);
                using var gameStateData = new ZlibStream(new MemoryStream(Bytes, gameStateStart, gameStateSize), CompressionMode.Decompress);
                gameStateData.Read(Body, endOfBundle, Body.Length - endOfBundle);
            }
            else
            {
                Body = Bytes.AsSpan(BodyStart, BodyDataLength).ToArray();
            }
            _originalBodyLength = Body.Length;
            Stash = Stash.TryCreateStash(this);
            ReadOnlySpan<byte> data = Body;
            _bagOffset = GetBagOffset(data);
            _gameStateStartOffset = data.IndexOf(new byte[5] { 0xF7, 0x5D, 0x3C, 0x00, 0x0A });
            _dataLengthOffsets = new[]{
                _gameStateStartOffset + 5, // gameStateSize
                data.IndexOf(new byte[5] { 0x0C, 0xAE, 0x32, 0x00, 0x00 }) + 5, // unknown length 1
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

            for (int ixOfActor = _dataLengthOffsets[^1] + 4; BitConverter.ToInt32(Body, ixOfActor) == 0x00_75_2D_06; ixOfActor += dataLength)
            {
                dataLength = 9 + BitConverter.ToInt32(Body, ixOfActor + 5);
                var id = BitConverter.ToInt32(Body, ixOfActor + 9);
                var typeIdOffset = ixOfActor + 13;
                var typeId = BitConverter.ToUInt32(Body, typeIdOffset);
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
                if (BitConverter.ToInt32(Body, itemOffset + 17) == playerActor)
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
            FindEquippedItems(playerActor);

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

        private void FindEquippedItems(int playerActor)
        {
            var data = Body.AsSpan();
            ReadOnlySpan<byte> signature = new byte[9] { 0x0B, 0x00, 0x00, 0x00, 0x41, 0xF5, 0x7E, 0x00, 0x04 };
            Span<byte> temp = stackalloc byte[13];
            MemoryUtilities.Write(temp, 0, playerActor);
            signature.CopyTo(temp[4..]);
            int offset = data.IndexOf(MemoryMarshal.AsBytes(temp));
            int dataLength = MemoryUtilities.Read<int>(data, offset + 13);
            // 17 is the loot table
            // 21 is the count of items in the inventory.
            var partInventory = MemoryMarshal.Cast<byte, int>(data.Slice(offset + 17, dataLength));
            var inventoryCount = partInventory[1];
            var equippedItemsCount = partInventory[inventoryCount + 2];
            var equippedData = partInventory.Slice(inventoryCount + 3, equippedItemsCount);

            foreach (var itemId in equippedData)
            {
                if (itemId != 0 && Items.FirstOrDefault(x => x.ItemId == itemId) is Item item)
                {
                    EquippedItems.Add(item);
                }
            }
        }

        public Encoding Encoding => IsRemaster ? Encoding.UTF8 : Encoding.Default;
        public bool IsRemaster { get; }
        private int BodyStart => 8 + _header.Length + 4;
        public byte[] Bytes { get; internal set; }
        public byte[] Body { get; internal set; }
        public string FileName { get; }

        public int InventorySize
        {
            get => MemoryUtilities.Read<int>(Body, _bagOffset);
            set => MemoryUtilities.Write(Body, _bagOffset, value);
        }

        private int BodyDataLength
        {
            get => IsRemaster ? MemoryUtilities.Read<int>(Bytes, 8 + _header.Length) : Bytes.Length - BodyStart;
            set
            {
                if (IsRemaster)
                {
                    MemoryUtilities.Write(Bytes, 8 + _header.Length, value);
                }
            }
        }

        public List<Item> Items { get; } = new List<Item>();

        public HashSet<Item> EquippedItems { get; } = new HashSet<Item>();

        public Dictionary<int, Gem> Gems { get; } = new Dictionary<int, Gem>();

        public Stash? Stash { get; private set; }

        public List<QuestItem> QuestItems { get; } = new List<QuestItem>();

        internal void UpdateDataLengths(int itemOffset, int delta)
        {
            _header.BodyDataLength += delta;
            foreach (var offset in _dataLengthOffsets)
            {
                if (offset < itemOffset)
                {
                    var oldVal = MemoryUtilities.Read<int>(Body, offset);
                    MemoryUtilities.Write(Body, offset, delta + oldVal);
                }
            }
        }

        public void SaveFile()
        {
            File.Copy(FileName, $"{FileName}.bak", true);
            if (IsRemaster)
            {
                // possibly unneccessary but it can't hurt.
                Bytes.AsSpan(BodyStart, MaxRemasterBodySize).Clear();
                if (Body.Length > MaxRemasterBodySize)
                {
                    var (bundleStream, bundleLength) = CompressStream(Body, 0, _gameStateStartOffset);
                    var (gameStateStream, gameStateLength) = CompressStream(Body, _gameStateStartOffset, Body.Length - _gameStateStartOffset);
                    BodyDataLength = bundleLength + gameStateLength + 16; //The bundles + 4 int32s (flag, uncompressed length, bundleInfoLength, gameStateLength).
                    MemoryUtilities.Write(Bytes, BodyStart, CompressedFlag); // zlib
                    MemoryUtilities.Write(Bytes, BodyStart + 4, _header.BodyDataLength);
                    MemoryUtilities.Write(Bytes, BodyStart + 8, bundleLength);
                    bundleStream.Read(Bytes, BodyStart + 12, bundleLength);
                    MemoryUtilities.Write(Bytes, BodyStart + 12 + bundleLength, gameStateLength);
                    gameStateStream.Read(Bytes, BodyStart + 16 + bundleLength, gameStateLength);
                }
                else
                {
                    BodyDataLength = Body.Length;
                    Body.CopyTo(Bytes, BodyStart);
                }
                MemoryUtilities.Write(Bytes, 0, Crc32Algorithm.Compute(Bytes, 8, Bytes.Length - 8)); // fileCrc
                MemoryUtilities.Write(Bytes, 4, Crc32Algorithm.Compute(Bytes, 8, _header.Length)); // headerCrc
            }
            else
            {
                BodyDataLength = Body.Length;
                Bytes = MemoryUtilities.ReplaceBytes(Bytes, BodyStart, _originalBodyLength, Body);
                _originalBodyLength = Body.Length;
            }
            File.WriteAllBytes(FileName, Bytes);

            static (Stream, int) CompressStream(byte[] body, int start, int count)
            {
                var stream = new MemoryStream();
                using (var zip = new ZlibStream(stream, CompressionMode.Compress, leaveOpen: true))
                {
                    zip.Write(body, start, count);
                }
                stream.Seek(0L, SeekOrigin.Begin);
                return (stream, (int)stream.Length);
            }
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
                foreach (var gem in item.Gems)
                {
                    if (gem.ItemOffset > itemOffset)
                    {
                        gem.ItemOffset += delta;
                    }
                }
            }
            foreach (var item in Items)
            {
                if (item.ItemSockets.ItemOffset > itemOffset)
                {
                    item.ItemSockets.ItemOffset += delta;
                    foreach (var gem in item.ItemSockets.Gems)
                    {
                        if (gem.ItemOffset > itemOffset)
                        {
                            gem.ItemOffset += delta;
                        }
                    }
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
                var prevLength = Body.Length;
                Body = MemoryUtilities.ReplaceBytes(Body, itemOffset, dataLength, newBytes);
                return Body.Length - prevLength;
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
