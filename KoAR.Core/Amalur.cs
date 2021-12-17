using Ionic.Zlib;
using System;
using System.Collections.Generic;
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

namespace KoAR.Core;

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
        using var gemsStream = archive.GetEntry("gemDefinitions.json")!.Open();
        GemDefinitions = JsonSerializer.DeserializeAsync<GemDefinition[]>(gemsStream, jsonOptions).AsTask().Result!.ToDictionary(def => def.TypeId);
        using var itemsStream = archive.GetEntry("definitions.json")!.Open();
        ItemDefinitions = JsonSerializer.DeserializeAsync<ItemDefinition.WireFormat[]>(itemsStream, jsonOptions).AsTask().Result!.ToDictionary(wf => wf.TypeId, wf => (ItemDefinition)wf);
#if DEBUG
        using var simTypesStream = archive.GetEntry("simtype.csv")!.Open();
        using var reader = new StreamReader(simTypesStream);
        SimTypes = reader
            .ReadLines()
            .Select(l => l.Split(','))
            .ToDictionary(arr => uint.Parse(arr[0]), arr => arr[1]);
        using var magicStream = archive.GetEntry("magic.json")!.Open();
        MagicTypeIds = JsonSerializer.DeserializeAsync<uint[]>(magicStream, jsonOptions).AsTask().Result!;
#endif
    }

    public static IReadOnlyDictionary<uint, Buff> Buffs { get; }
    public static IReadOnlyDictionary<uint, GemDefinition> GemDefinitions { get; }
    public static IReadOnlyDictionary<uint, ItemDefinition> ItemDefinitions { get; }
    public static IReadOnlyDictionary<uint, QuestItemDefinition> QuestItemDefinitions { get; }
#if DEBUG
    public static IReadOnlyDictionary<uint, string> SimTypes { get; }
    public static IReadOnlyList<uint> MagicTypeIds { get; }
#endif
    public static ReadOnlySpan<uint> PlayerTypeIds => MemoryMarshal.Cast<byte, uint>((ReadOnlySpan<byte>)new byte[16]{
            0x6D, 0x38, 0x0A, 0x00, // playerHumanMale
            0x6E, 0x38, 0x0A, 0x00, // playerHumanFemale
            0x6F, 0x38, 0x0A, 0x00, // playerElfMale
            0x70, 0x38, 0x0A, 0x00,  // playerElfFemale
        });

    public static Buff GetBuff(uint buffId) => Buffs.GetValueOrDefault(buffId, new() { Id = buffId, Name = "Unknown" });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T SetFlag<T>(this T @enum, T flag, bool on) where T : struct, Enum
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

    public static IEnumerable<string> ReadLines(this TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    public static int ReadAll(this ZlibStream stream, Span<byte> buffer)
    {
        var totalAmountRead = 0;
        while (stream.Read(buffer) is int read and > 0)
        {
            totalAmountRead += read;
            buffer = buffer[read..];
        }
        return totalAmountRead;
    }
}
