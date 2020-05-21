using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace KoAR.Core
{
    public enum Rarity
    {
        Common = 0x14,
        Unique = 0x2C,
        Uncommon = 0x44,
        Rare = 0x5C,
        Question = 0x24,
        WhoKnows = 0x74
    };

    public class CoreEffectList : IList<CoreEffectInfo>, IReadOnlyList<CoreEffectInfo>
    {
        private readonly struct Offset
        {
            public const int MysteryInteger = 13;
            public const int EffectCount = MysteryInteger + 4;
            public const int FirstEffect = EffectCount + 4;

            private readonly int _count;
            public Offset(int count) => _count = count;

            public int PostEffect => FirstEffect + _count * 16;
            public int DisplayEffectCount => PostEffect + 4;
            public int FirstDisplayEffect => DisplayEffectCount + 4;
        }

        private static ReadOnlySpan<uint> Prefixes => new[] { 0x57_8E_73u, 0x58_6E_AAu, 0x4B_03_f9u, 0x4b_43_f4u };
        private readonly List<CoreEffectInfo> _list;

        public CoreEffectInfo this[int index] {
            get => _list[index];
            set
            {
                _list[index] = value;// this'll throw an exception if out of bound so who cares.
                Serialize();
            }
        }

        public byte[] Bytes { get; private set; }
        public int ItemIndex { get; private set; }
        public int DataLength { get; private set; }
        public byte MysteryInteger
        {
            get => Bytes[Offset.MysteryInteger];
        }

        public int Count
        {
            get => _list.Count;
            private set
            {
                Bytes[Offset.EffectCount + 4 + (16 * value)] = Bytes[Offset.EffectCount] = (byte)value;
            }
        }

        public bool IsReadOnly => false;

        public void Add(CoreEffectInfo item)
        {
            if(_list.Count == 4)
            {
                throw new InvalidOperationException("List can only have four elements");
            }
            _list.Add(item);
            Serialize();
        }

        public void Clear()
        {
            _list.Clear();
            Serialize();
        }

        public void Insert(int index, CoreEffectInfo item)
        {
            _list.Remove(item);
            Serialize();
        }

        public bool Remove(CoreEffectInfo item)
        {
            if (_list.Remove(item))
            {
                Serialize();
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index); // this'll throw if you did something stupid.
           
            Serialize();
        }

        private void Serialize()
        {
            byte length = Bytes[Offset.EffectCount];
            var currentLength = length * 24 + 8;
            Span<ulong> effectData = stackalloc ulong[_list.Count * 3 + 1];
            for (int i = 0; i < _list.Count; i++)
            {
                ulong effect = uint.Parse(_list[i].Code, NumberStyles.HexNumber);
                effectData[i * 2] = Prefixes[i] | effect << 32;
                effectData[(i * 2) + 1] = ulong.MaxValue;
                effectData[(_list.Count * 2) + i] = effect | (ulong)uint.MaxValue << 32;
            }
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offset.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
            Count = _list.Count;
        }

        public bool Contains(CoreEffectInfo item) => _list.Contains(item);
        public int IndexOf(CoreEffectInfo item) => _list.IndexOf(item);
        public void CopyTo(CoreEffectInfo[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<CoreEffectInfo> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }

    public class CoreItemMemory
    {
        private static ReadOnlySpan<uint> EffectPrefixes => new[] { 0x57_8E_73u, 0x58_6E_AAu, 0x4B_03_f9u, 0x4b_43_f4u };

        public static CoreItemMemory Create(int itemIndex, ReadOnlySpan<byte> span)
        {
            if (span.Length < 29)
            {
                return null;
            }

            var effectCount = MemoryUtilities.Read<int>(span, Offset.EffectCount);
            var offsets = new Offset(effectCount);

            int dataLength = offsets.FirstDisplayEffect + effectCount * 8;

            return new CoreItemMemory(itemIndex, dataLength, span);
        }

        private readonly struct Offset
        {
            public const int MysteryInteger = 13;
            public const int EffectCount = MysteryInteger + 4;
            public const int FirstEffect = EffectCount + 4;

            private readonly int _count;
            public Offset(int count) => _count = count;


            public int PostEffect => FirstEffect + _count * 16;
            public int DisplayEffectCount => PostEffect + 4;
            public int FirstDisplayEffect => DisplayEffectCount + 4;
        }

        private CoreItemMemory(int itemIndex, int dataLength, ReadOnlySpan<byte> span)
            => (ItemIndex, DataLength, ItemBytes) = (itemIndex, dataLength, span.Slice(0, dataLength).ToArray());

        public int ItemIndex { get; }
        public int DataLength { get; }
        public byte[] ItemBytes { get; set; }

        public int ItemId => MemoryUtilities.Read<int>(ItemBytes);

        private Offset Offsets => new Offset(EffectCount);

        public int EffectCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offset.EffectCount);
            set => MemoryUtilities.Write(ItemBytes, Offset.EffectCount, value);
        }

        public int MysteryInteger
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offset.MysteryInteger);
            set => MemoryUtilities.Write(ItemBytes, Offset.MysteryInteger, value);
        }

        public int DisplayEffectCount
        {
            get => MemoryUtilities.Read<int>(ItemBytes, Offsets.DisplayEffectCount);
            set => MemoryUtilities.Write(ItemBytes, Offsets.DisplayEffectCount, value);
        }

        public List<CoreEffectInfo> ReadEffects()
        {
            var effects = new List<CoreEffectInfo>();
            // Currently assuming these have to be the same.
            for (int i = 0; i < EffectCount; i++)
            {
                effects.Add(new CoreEffectInfo
                {
                    Code = MemoryUtilities.Read<uint>(ItemBytes, Offsets.FirstDisplayEffect + i * 8).ToString("X6")
                });
            }

            return effects;
        }

        public void WriteEffects(List<CoreEffectInfo> effects)
        {
            var currentLength = EffectCount * 24 + 8;
            Span<ulong> effectData = stackalloc ulong[effects.Count * 3 + 1];
            for(int i = 0; i < effects.Count; i++)
            {
                ulong effect = uint.Parse(effects[i].Code, NumberStyles.HexNumber);
                effectData[i * 2] = EffectPrefixes[i] | effect << 32;
                effectData[(i * 2) + 1] = ulong.MaxValue;
                effectData[(effects.Count * 2) + 1 + i] = effect | (ulong)uint.MaxValue << 32;
            }
            ItemBytes = MemoryUtilities.ReplaceBytes(ItemBytes, Offset.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
            EffectCount = effects.Count;
            DisplayEffectCount = effects.Count;
        }
    }
}
