using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace KoAR.Core
{
    public static class Amalur
    {
        static Amalur()
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance,
                Converters = { new JsonStringEnumConverter() }
            };
            using var zipStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(Amalur).Namespace}.Data.zip")!;
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            using var buffsStream = archive.GetEntry("buffs.json")!.Open();
            Buffs = JsonSerializer.DeserializeAsync<Buff[]>(buffsStream, jsonOptions).AsTask().Result!.ToDictionary(buff => buff.Id);
            using var questItemsStream = archive.GetEntry("questItemDefinitions.json")!.Open();
            QuestItemDefinitions = JsonSerializer.DeserializeAsync<QuestItemDefinition[]>(questItemsStream, jsonOptions).AsTask().Result!.ToDictionary(def => def.Id);
            using var gemsStream = archive.GetEntry("gemDefinitions.csv")!.Open();
            GemDefinitions = GemDefinition.ParseFile(gemsStream).ToDictionary(def => def.TypeId);
            using var itemsStream = archive.GetEntry("definitions.csv")!.Open();
            ItemDefinitions = ItemDefinition.ParseFile(itemsStream).ToDictionary(def => def.TypeId);
        }

        public static IReadOnlyDictionary<uint, Buff> Buffs { get; }
        public static IReadOnlyDictionary<uint, GemDefinition> GemDefinitions { get; }
        public static IReadOnlyDictionary<uint, ItemDefinition> ItemDefinitions { get; }
        public static IReadOnlyDictionary<uint, QuestItemDefinition> QuestItemDefinitions { get; }
        public static ReadOnlySpan<uint> PlayerTypeIds => MemoryMarshal.Cast<byte, uint>((ReadOnlySpan<byte>)new byte[16]{
            0x6D, 0x38, 0x0A, 0x00, // playerHumanMale
            0x6E, 0x38, 0x0A, 0x00, // playerHumanFemale
            0x6F, 0x38, 0x0A, 0x00, // playerElfMale
            0x70, 0x38, 0x0A, 0x00,  // playerElfFemale
        });

        internal static char[] Separator { get; } = { ',' };

        public static Buff GetBuff(uint buffId) => Buffs.GetOrDefault(buffId, new() { Id = buffId, Name = "Unknown" });

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        internal static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
            => dictionary.TryGetValue(key, out var res) ? res : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T SetFlag<T>(this T @enum, T flag, bool on)
            where T : struct, Enum
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                byte x = (byte)((Unsafe.As<T, byte>(ref @enum) & ~Unsafe.As<T, byte>(ref flag))
                    | (-Unsafe.As<bool, byte>(ref on) & Unsafe.As<T, byte>(ref flag)));
                return Unsafe.As<byte, T>(ref x);
            }
            else if (Unsafe.SizeOf<T>() == 2)
            {
                ushort x = (ushort)((Unsafe.As<T, ushort>(ref @enum) & ~Unsafe.As<T, ushort>(ref flag))
                    | (-Unsafe.As<bool, byte>(ref on) & Unsafe.As<T, ushort>(ref flag)));
                return Unsafe.As<ushort, T>(ref x);
            }
            else if (Unsafe.SizeOf<T>() == 4)
            {
                uint x = (Unsafe.As<T, uint>(ref @enum) & ~Unsafe.As<T, uint>(ref flag))
                   | ((uint)-Unsafe.As<bool, byte>(ref on) & Unsafe.As<T, uint>(ref flag));
                return Unsafe.As<uint, T>(ref x);
            }
            else
            {
                ulong x = (Unsafe.As<T, ulong>(ref @enum) & ~Unsafe.As<T, ulong>(ref flag))
                   | ((ulong)-(long)Unsafe.As<bool, byte>(ref on) & Unsafe.As<T, ulong>(ref flag));
                return Unsafe.As<ulong, T>(ref x);
            }
        }

        public static string FindSaveGameDirectory()
        {
            try
            {
                // GOG
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify), @"appdata\LocalLow\THQNOnline\Kingdoms of Amalur Re-Reckoning\autocloud\save");
                if (Directory.Exists(directory))
                {
                    return directory;
                }
                // Steam
                directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify), "Steam", "userdata");
                if (Directory.Exists(directory) && Directory.GetDirectories(directory) is { Length: 1 } userDirs
                        && (Directory.Exists(directory = Path.Combine(userDirs[0], @"1041720\remote\autocloud\save")) || Directory.Exists(directory = Path.Combine(userDirs[0], @"102500\remote"))))
                {
                    return directory;
                }
            }
            catch
            {
            }
            return Environment.CurrentDirectory;
        }
    }
}
