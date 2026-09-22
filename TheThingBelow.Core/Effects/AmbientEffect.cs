using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Core.Effects;

/// <summary>The four ambient kinds of region one (D-187). The ambience of a map matches its kind (D-424).</summary>
public enum AmbientKind
{
    /// <summary>Snow and wind.</summary>
    Snow,

    /// <summary>Fog and mist.</summary>
    Fog,

    /// <summary>Fire with embers and smoke.</summary>
    Fire,

    /// <summary>Dust with drips and motes.</summary>
    Dust,
}

/// <summary>
/// The weather of one or more maps: streams of particles over the view, and layers of fog
/// above the figures (D-187, D-202, D-885). The same weather plays over the battle backdrop of
/// the map (D-205).
/// </summary>
/// <remarks>
/// No rule reads an ambient file, so the file lies outside the rule folder (D-495, D-522). The
/// file names each map that it serves, and a map file never names an effect (D-519). A map
/// takes one ambient file or none, and its time of day never changes it (D-202).
/// <para>
/// A capture file has the same form, in <see cref="CaptureFolder"/>. A capture of the screen
/// test loads it in place of the weather of the map, and the shipped game never does (D-889).
/// </para>
/// </remarks>
public sealed class AmbientEffect
{
    /// <summary>The folder of the ambient files, under `content/`.</summary>
    public const string Folder = "effects/ambient/";

    /// <summary>The folder of the ambient files that only the captures of the screen test load (D-889).</summary>
    public const string CaptureFolder = "effects/ambient-captures/";

    private AmbientEffect(
        string file,
        ContentId id,
        AmbientKind kind,
        IReadOnlyList<ContentId> maps,
        bool lit,
        IReadOnlyList<StreamEmitter> emitters,
        IReadOnlyList<FogLayer> fogs)
    {
        this.File = file;
        this.Id = id;
        this.Kind = kind;
        this.Maps = maps;
        this.Lit = lit;
        this.Emitters = emitters;
        this.Fogs = fogs;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the effect, such as `effect.dust_fixture_dungeon`.</summary>
    public ContentId Id { get; }

    /// <summary>The ambient kind (D-187).</summary>
    public AmbientKind Kind { get; }

    /// <summary>The maps that this weather serves, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Maps { get; }

    /// <summary>True when the scene light falls on the particles and the fog, as on a figure (D-183).</summary>
    public bool Lit { get; }

    /// <summary>The streams of the weather, from the north-west corner of the view, in the order of the file.</summary>
    public IReadOnlyList<StreamEmitter> Emitters { get; }

    /// <summary>The layers of fog, from the lowest, in the order of the file. Each one is a full-screen pass (D-523).</summary>
    public IReadOnlyList<FogLayer> Fogs { get; }

    /// <summary>True when the file lies in the folder of the capture files (D-889).</summary>
    public bool IsCapture => this.File.StartsWith(CaptureFolder, StringComparison.Ordinal);

    /// <summary>The count of live particles of the weather, which the effect budget counts (D-523).</summary>
    public int Particles => StreamEmitter.ParticlesOf(this.Emitters);

    /// <summary>Tells whether a content path is an ambient file or a capture file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in one of the two folders.</returns>
    public static bool IsAmbientFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal)
            || path.StartsWith(CaptureFolder, StringComparison.Ordinal);
    }

    /// <summary>Gives the name of an ambient kind, as a file writes it.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `snow`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(AmbientKind kind) => kind switch
    {
        AmbientKind.Snow => "snow",
        AmbientKind.Fog => "fog",
        AmbientKind.Fire => "fire",
        AmbientKind.Dust => "dust",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no ambient kind (D-187)"),
    };

    /// <summary>Reads one ambient effect from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The effect.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static AmbientEffect Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        AmbientEffect effect = Read(ref reader);
        reader.ReadFileEnd();
        return effect;
    }

    private static AmbientEffect Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        AmbientKind? kind = null;
        List<ContentId>? maps = null;
        bool? lit = null;
        List<StreamEmitter>? emitters = null;
        List<FogLayer>? fogs = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "id":
                    id = reader.ReadContentId(HitEffect.IdKind);
                    break;
                case "kind":
                    kind = KindOf(ref reader, reader.ReadString());
                    break;
                case "maps":
                    maps = ReadMaps(ref reader);
                    break;
                case "lit":
                    lit = reader.ReadBoolean();
                    break;
                case "emitters":
                    emitters = StreamEmitter.ReadList(ref reader);
                    break;
                case "fogs":
                    fogs = FogLayer.ReadList(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        List<ContentId> served = reader.Require(maps, depth, "maps");
        if (served.Count == 0)
        {
            throw reader.RefuseField(depth, "maps", "the effect serves no map, and each weather serves one map or more (D-202)");
        }

        List<StreamEmitter> streams = reader.Require(emitters, depth, "emitters");
        List<FogLayer> layers = reader.Require(fogs, depth, "fogs");
        if (streams.Count == 0 && layers.Count == 0)
        {
            throw reader.RefuseField(depth, "emitters", "the effect holds no emitter and no fog, and a weather shows one or more");
        }

        return new AmbientEffect(
            reader.File,
            reader.Require(id, depth, "id"),
            reader.RequireValue(kind, depth, "kind"),
            served,
            reader.RequireValue(lit, depth, "lit"),
            streams,
            layers);
    }

    private static AmbientKind KindOf(ref ContentReader reader, string name) => name switch
    {
        "snow" => AmbientKind.Snow,
        "fog" => AmbientKind.Fog,
        "fire" => AmbientKind.Fire,
        "dust" => AmbientKind.Dust,
        _ => throw reader.Refuse($"the kind is '{name}', and a weather is 'snow', 'fog', 'fire', or 'dust' (D-187)"),
    };

    private static List<ContentId> ReadMaps(ref ContentReader reader)
    {
        var maps = new List<ContentId>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, maps.Count))
        {
            ContentId id = reader.ReadContentId("map");
            foreach (ContentId before in maps)
            {
                if (string.CompareOrdinal(before.Value, id.Value) == 0)
                {
                    throw reader.Refuse($"the effect names the map '{id.Value}' two times");
                }
            }

            maps.Add(id);
        }

        return maps;
    }
}
