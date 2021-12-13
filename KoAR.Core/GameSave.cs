using Ionic.Zlib;
using StringLiteral;
using System;
using System.Collections.Generic;
using System.IO;
//using System.IO.Compression;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace KoAR.Core;

public sealed partial class GameSave
{
    private const int MaxRemasterBodySize = 4 * 1024 * 1024;
    private const int MaxSwitchBodySize = 256 * 1024 * 10;
    private const int CompressedFlag = 0x62_69_6C_7A; // zlib
    private const string RemasterFilePattern = "svd_fmt_5_";
    private const string SwitchFilePattern = "svd_04_";

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
        SaveType = IsRemaster switch
        {
            true when Path.GetExtension(fileName) is "" => SaveType.Switch,
            true => SaveType.Remaster,
            false => SaveType.Original,
        };
        var pattern = SaveType == SaveType.Switch ? SwitchFilePattern : RemasterFilePattern;
        if (IsRemaster && !Path.GetFileNameWithoutExtension(fileName).StartsWith(pattern))
        {
            throw new NotSupportedException($"Save file is not a user save and changing them can lead to the game infinite looping. The editor only supports saves that start with {pattern}.");
        }
        _header = new(this);
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
        var typeSectionOffset = data.IndexOf(new byte[5] { 0x23, 0xCC, 0x58, 0x00, 0x04 }) is int ix and not -1 ? ix : data.IndexOf(new byte[5] { 0x23, 0xCC, 0x58, 0x00, 0x03 });
        _dataLengthOffsets = new[]{
                _gameStateStartOffset + 5, // gameStateSize
                data.IndexOf(new byte[5] { 0x0C, 0xAE, 0x32, 0x00, 0x00 }) + 5, // unknown length 1
                typeSectionOffset + 5, // type section length
            };
        _itemContainer = new(this, data.IndexOf(new byte[5] { 0xD3, 0x34, 0x43, 0x00, 0x00 }), 0x00_24_D5_68_00_00_00_0Bul);
        _itemBuffsContainer = new(this, data.IndexOf(new byte[5] { 0xBB, 0xD5, 0x43, 0x00, 0x00 }), 0x00_28_60_84_00_00_00_0Bul);
        _itemSocketsContainer = new(this, data.IndexOf(new byte[5] { 0x93, 0xCC, 0x80, 0x00, 0x00 }), 0x00_59_36_38_00_00_00_0Bul);
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
                Gems.Add(id, new(this, typeIdOffset));
            }
            else if (Amalur.PlayerTypeIds.IndexOf(typeId) != -1)
            {
                playerActor = id;
            }
#if DEBUG
            else
            {
                candidates.Add((id, typeIdOffset, null));
            }
#endif
        }
        foreach (var (id, typeIdOffset, questItemDef) in candidates)
        {
            if (itemLocs.TryGetValue(id, out var loc))
            {
                var (itemOffset, itemLength) = loc;
                if (BitConverter.ToInt32(Body, itemOffset + 17) == playerActor)
                {
                    if (questItemDef != null)
                    {
                        QuestItems.Add(new(this, questItemDef, itemOffset + itemLength - 3));
                    }
                    else
                    {
                        var (itemBuffsOffset, itemBuffsLength) = itemBuffsLocs[id];
                        var (itemGemsOffset, itemGemsLength) = itemSocketsLocs.GetValueOrDefault(id);
                        if (itemGemsOffset > 0)
                        {
                            Items.Add(new(this, typeIdOffset, itemOffset, itemLength, itemBuffsOffset, itemBuffsLength, itemGemsOffset, itemGemsLength));
                        }
                    }
                }
            }
        }
        FindEquippedItems(playerActor);

        static int GetBagOffset(ReadOnlySpan<byte> data)
        {
            var inventoryLimit = Utf8InventoryLimit();
            var increaseAmount = Utf8IncreaseAmount();
            var currentInventoryCount = Utf8CurrentInventoryCount();
            var curInvCountOffset = data.IndexOf(currentInventoryCount) + currentInventoryCount.Length;
            var inventoryLimitOffset = data.IndexOf(inventoryLimit) + inventoryLimit.Length;
            var increaseAmountOffset = data.IndexOf(increaseAmount) + increaseAmount.Length;
            var finalOffset = Math.Max(Math.Max(curInvCountOffset, inventoryLimitOffset), increaseAmountOffset);
            var inventoryLimitOrder = inventoryLimitOffset == finalOffset
                ? 3
                : inventoryLimitOffset < Math.Min(curInvCountOffset, increaseAmountOffset) ? 1 : 2;

            return finalOffset + (inventoryLimitOrder * 12);
        }

        void FindEquippedItems(int playerActor)
        {
            var data = Body.AsSpan();
            Span<byte> temp = stackalloc byte[12];
            Unsafe.WriteUnaligned(ref temp[0], playerActor);
            Unsafe.WriteUnaligned(ref temp[4], 0x00_7E_F5_41_00_00_00_0Bul);
            int offset = data.IndexOf(temp);
            int dataLength = BitConverter.ToInt32(data[(offset + 13)..]);
            // 17 is the loot table
            // 21 is the count of items in the inventory.
            var partInventory = MemoryMarshal.Cast<byte, int>(data.Slice(offset + 17, dataLength));
            var inventoryCount = partInventory[1];
            var equippedItemsCount = partInventory[inventoryCount + 2];
            var equippedData = partInventory.Slice(inventoryCount + 3, equippedItemsCount);
            var inventoryDict = Items.ToDictionary(x => x.ItemId);

            foreach (var itemId in equippedData)
            {
                if (itemId != 0 && inventoryDict.TryGetValue(itemId, out var item))
                {
                    EquippedItems.Add(item);
                }
            }
        }
    }

    [Utf8("inventory_limit")]
    private static partial ReadOnlySpan<byte> Utf8InventoryLimit();

    [Utf8("increase_amount")]
    private static partial ReadOnlySpan<byte> Utf8IncreaseAmount();

    [Utf8("current_inventory_count")]
    private static partial ReadOnlySpan<byte> Utf8CurrentInventoryCount();

    public Encoding Encoding => IsRemaster ? Encoding.UTF8 : Encoding.Default;
    public bool IsRemaster { get; }
    public SaveType SaveType { get; }
    private int BodyStart => 8 + _header.Length + 4;
    public byte[] Bytes { get; internal set; }
    public byte[] Body { get; internal set; }
    public string FileName { get; }

    public int InventorySize
    {
        get => BitConverter.ToInt32(Body, _bagOffset);
        set => Unsafe.WriteUnaligned(ref Body[_bagOffset], value);
    }

    private int BodyDataLength
    {
        get => IsRemaster ? BitConverter.ToInt32(Bytes, 8 + _header.Length) : Bytes.Length - BodyStart;
        set
        {
            if (IsRemaster)
            {
                Unsafe.WriteUnaligned(ref Bytes[8 + _header.Length], value);
            }
        }
    }

    public List<Item> Items { get; } = new();

    public HashSet<Item> EquippedItems { get; } = new();

    public Dictionary<int, Gem> Gems { get; } = new();

    public Stash? Stash { get; private set; }

    public List<QuestItem> QuestItems { get; } = new();

    internal void UpdateDataLengths(int itemOffset, int delta)
    {
        _header.BodyDataLength += delta;
        foreach (var offset in _dataLengthOffsets)
        {
            if (offset < itemOffset)
            {
                var oldVal = BitConverter.ToInt32(Body, offset);
                Unsafe.WriteUnaligned(ref Body[offset], delta + oldVal);
            }
        }
    }

    public string SaveFile()
    {
        var backupPath = Path.Combine(Path.GetDirectoryName(FileName)!, "backup");
        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }
        File.Copy(FileName, backupPath = Path.Combine(backupPath, Path.GetFileName(FileName)), true);
        if (IsRemaster)
        {
            int maxBodySize = SaveType == SaveType.Switch ? MaxSwitchBodySize : MaxRemasterBodySize;
            // possibly unneccessary but it can't hurt.
            Bytes.AsSpan(BodyStart, maxBodySize).Clear();
            if (Body.Length > maxBodySize)
            {
                var (bundleStream, bundleLength) = CompressStream(Body, 0, _gameStateStartOffset);
                var (gameStateStream, gameStateLength) = CompressStream(Body, _gameStateStartOffset, Body.Length - _gameStateStartOffset);
                BodyDataLength = bundleLength + gameStateLength + 16; //The bundles + 4 int32s (flag, uncompressed length, bundleInfoLength, gameStateLength).
                Unsafe.WriteUnaligned(ref Bytes[BodyStart], CompressedFlag); // zlib
                Unsafe.WriteUnaligned(ref Bytes[BodyStart + 4], _header.BodyDataLength);
                Unsafe.WriteUnaligned(ref Bytes[BodyStart + 8], bundleLength);
                bundleStream.Read(Bytes, BodyStart + 12, bundleLength);
                Unsafe.WriteUnaligned(ref Bytes[BodyStart + 12 + bundleLength], gameStateLength);
                gameStateStream.Read(Bytes, BodyStart + 16 + bundleLength, gameStateLength);
            }
            else
            {
                BodyDataLength = Body.Length;
                Body.CopyTo(Bytes, BodyStart);
            }
            Crc32.Hash(Bytes.AsSpan(8), Bytes);
            Crc32.Hash(Bytes.AsSpan(8, _header.Length), Bytes.AsSpan(4, 4));
        }
        else
        {
            BodyDataLength = Body.Length;
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, BodyStart, _originalBodyLength, Body);
            _originalBodyLength = Body.Length;
        }
        File.WriteAllBytes(FileName, Bytes);
        return backupPath;

        static (Stream, int) CompressStream(byte[] body, int start, int count)
        {
            var stream = new MemoryStream();
            using (var zip = new ZlibStream(stream, CompressionMode.Compress, leaveOpen: true))
            {
                Unsafe.WriteUnaligned(ref body[start], count);
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
