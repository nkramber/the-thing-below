using System.Collections.Generic;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// The rules that Core takes, and that each tool of D-502 takes in part. Every rule comes
/// from the determinism guardrails: G-2, G-3, G-4, T-7, F-35, F-36, and F-39.
/// </summary>
public static class CoreRules
{
    /// <summary>DL 1: no float type. Core uses integer math and basis points (G-2, D-169).</summary>
    public static readonly BannedSymbolRule FloatTypes = new(
        "DL 1",
        "Core uses integer math alone, and a fraction takes basis points (G-2, D-169, T-7).",
        types: ["System.Single", "System.Double", "System.Decimal", "System.Half"],
        namespaces: [],
        members: [],
        memberNames: []);

    /// <summary>DL 2: no clock. The seed and the tick are the only sources of time (G-3).</summary>
    public static readonly BannedSymbolRule Clock = new(
        "DL 2",
        "Core reads no clock. The seed and the tick are the only sources of time (G-3, T-7).",
        types:
        [
            "System.DateTime",
            "System.DateTimeOffset",
            "System.DateOnly",
            "System.TimeOnly",
            "System.TimeProvider",
            "System.Diagnostics.Stopwatch",
        ],
        namespaces: [],
        members: ["System.Environment.TickCount", "System.Environment.TickCount64"],
        memberNames: []);

    /// <summary>DL 3: no OS random. Each stream splits from the run seed (G-3, G-4).</summary>
    public static readonly BannedSymbolRule OsRandom = new(
        "DL 3",
        "Core takes each random value from a seeded stream, and never from the OS (G-3, G-4).",
        types: ["System.Random"],
        namespaces: [],
        members: ["System.Guid.NewGuid"],
        memberNames: []);

    /// <summary>DL 4: no reflection. `typeof` alone stays legal, for an attribute (F-36).</summary>
    /// <remarks>
    /// `JsonSerializer` is the one reflection path that the content reader could take, and
    /// D-647 refuses it. Core reads content with a hand reader on `Utf8JsonReader`, which is
    /// a scanner with no metadata and no type map.
    /// </remarks>
    public static readonly BannedSymbolRule Reflection = new(
        "DL 4",
        "Core runs with no reflection. The content reader is a hand reader on `Utf8JsonReader`, and it calls `JsonSerializer` nowhere (F-36, D-647).",
        types: ["System.Activator", "System.Text.Json.JsonSerializer", SymbolNames.DynamicType],
        namespaces: ["System.Reflection"],
        members:
        [
            "System.Object.GetType",
            "System.Type.GetType",
            "System.Type.GetConstructor",
            "System.Type.GetConstructors",
            "System.Type.GetField",
            "System.Type.GetFields",
            "System.Type.GetInterfaces",
            "System.Type.GetMember",
            "System.Type.GetMembers",
            "System.Type.GetMethod",
            "System.Type.GetMethods",
            "System.Type.GetProperties",
            "System.Type.GetProperty",
            "System.Type.InvokeMember",
            "System.Type.MakeGenericType",
            "System.Type.Assembly",
            "System.Type.Module",
        ],
        memberNames: []);

    /// <summary>DL 5: neither hash path of F-35. Core holds its own hash function.</summary>
    public static readonly BannedSymbolRule HashPaths = new(
        "DL 5",
        "The hash classes defer to the OS libraries, and a string hash code is not stable across runs. Core holds its own hash function (F-35, G-1, T-7).",
        types: ["System.HashCode"],
        namespaces: ["System.Security.Cryptography", "System.IO.Hashing"],
        members: [],
        memberNames: ["GetHashCode"]);

    /// <summary>
    /// DL 10: no thread, no task, and no SIMD vector. A thread makes the order of two steps
    /// follow the machine, and the lane count of a vector follows the processor (T-7).
    /// </summary>
    public static readonly BannedSymbolRule Threads = new(
        "DL 10",
        "Core runs on one thread, and it computes with whole numbers alone. A thread or a task makes the order follow the machine, and a vector makes the lane count follow the processor (T-7, G-4).",
        types:
        [
            "System.Numerics.Vector",
            "System.Numerics.Vector2",
            "System.Numerics.Vector3",
            "System.Numerics.Vector4",
            "System.Numerics.Matrix3x2",
            "System.Numerics.Matrix4x4",
            "System.Numerics.Quaternion",
            "System.Numerics.Plane",
        ],
        namespaces: ["System.Threading", "System.Runtime.Intrinsics"],
        members: [],
        memberNames: []);

    /// <summary>
    /// DL 11: no file, no network, and no OS service. The reference test of Core cannot see such
    /// a call, because `System.Runtime` holds `File`, `Environment`, and `OperatingSystem` too.
    /// A line end of the OS in hashed text would give another hash on another leg (G-1, T-7).
    /// </summary>
    public static readonly BannedSymbolRule FileAndSystem = new(
        "DL 11",
        "Core reads no file, no network, and no OS service, and it writes no line end of the OS. The host passes each byte and each value in (G-1, D-100, T-7).",
        types:
        [
            "System.AppContext",
            "System.Console",
            "System.Diagnostics.Process",
            "System.Environment",
            "System.OperatingSystem",
            "System.Runtime.InteropServices.RuntimeInformation",
        ],
        namespaces: ["System.IO", "System.Net", "Microsoft.Win32"],
        members: [],
        memberNames: ["AppendLine", "WriteLine"]);

    /// <summary>Every rule of Core: DL 1 to DL 7, DL 10, and DL 11.</summary>
    /// <returns>The nine rules, in the order of their ids.</returns>
    public static IReadOnlyList<ILintRule> All() =>
    [
        FloatTypes,
        Clock,
        OsRandom,
        Reflection,
        HashPaths,
        new StringOrderRule(),
        new CollectionWalkRule(),
        Threads,
        FileAndSystem,
    ];

    /// <summary>
    /// The rules of the debug commands, which change a run inside a tick: the float types, the
    /// clock, the OS random, and the file and OS rule (D-724, G-1).
    /// </summary>
    /// <returns>The four rules, in the order of their ids.</returns>
    public static IReadOnlyList<ILintRule> DebugCommands() => [FloatTypes, Clock, OsRandom, FileAndSystem];

    /// <summary>
    /// The rules that a tool of D-502 takes: the float types, the clock, and the OS random.
    /// The output of such a tool goes to a test that compares it on every CI leg.
    /// </summary>
    /// <returns>The three rules, in the order of their ids.</returns>
    public static IReadOnlyList<ILintRule> ComparedOutputTools() => [FloatTypes, Clock, OsRandom];
}
