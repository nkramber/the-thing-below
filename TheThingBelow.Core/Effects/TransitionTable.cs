using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>One region of the transition table: its maps and the pool of its common fights (D-934, D-936).</summary>
/// <param name="Id">The id of the region, of the kind `region`, such as `region.one`.</param>
/// <param name="Maps">The ids of the maps of the region, in the order of the file.</param>
/// <param name="Pool">The ids of the transitions of its common fights, in the order of the file.</param>
public sealed record TransitionRegion(ContentId Id, IReadOnlyList<ContentId> Maps, IReadOnlyList<ContentId> Pool);

/// <summary>
/// The table that gives each fight its transition: one transition for each fixed kind, a pool of
/// each region for its common fights, and the fade of D-938 (D-196, D-934, D-936). No rule reads it
/// (D-495).
/// </summary>
/// <remarks>
/// This reader checks the file alone. <see cref="TransitionContent"/> checks each id of it against
/// the transition files and the maps (T-2).
/// </remarks>
public sealed class TransitionTable
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "effects/transition-table.json";

    /// <summary>The kind of the id of each region (D-646).</summary>
    public const string RegionIdKind = "region";

    /// <summary>The fewest transitions of a pool, so a pick can always skip the last one (D-935).</summary>
    public const int FewestInPool = 2;

    /// <summary>The most ticks of the fade, ten seconds, so a typing fault never holds a fight for minutes (T-2).</summary>
    public const int MostFadeTicks = 600;

    private TransitionTable(int fadeTicks, char backCover, IReadOnlyList<ContentId> fixedKinds, IReadOnlyList<TransitionRegion> regions)
    {
        this.FadeTicks = fadeTicks;
        this.BackCover = backCover;
        this.FixedKinds = fixedKinds;
        this.Regions = regions;
    }

    /// <summary>The length of the fade from the cover color, into a fight and back to the map, in ticks (D-938, D-939).</summary>
    public int FadeTicks { get; }

    /// <summary>The palette key of the black that the screen shows after a win or a flee, which the map fades in from (D-938).</summary>
    public char BackCover { get; }

    /// <summary>The transition of each fixed kind, in the order of <see cref="EncounterKinds.Fixed"/> (D-934, D-940).</summary>
    public IReadOnlyList<ContentId> FixedKinds { get; }

    /// <summary>The regions, in the order of the file (D-936).</summary>
    public IReadOnlyList<TransitionRegion> Regions { get; }

    /// <summary>Gives the transition of one fixed kind (D-934).</summary>
    /// <param name="kind">A kind of <see cref="EncounterKinds.Fixed"/>.</param>
    /// <returns>The id of its transition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The kind is common, which the pool of a region serves (D-934, T-2).</exception>
    public ContentId FixedOf(EncounterKind kind)
    {
        int index = Array.IndexOf(EncounterKinds.Fixed, kind);
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "a common fight takes a transition from the pool of its region, and no fixed transition (D-934)");
        }

        return this.FixedKinds[index];
    }

    /// <summary>Reads the table from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The table.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static TransitionTable Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        TransitionTable table = Read(ref reader);
        reader.ReadFileEnd();
        return table;
    }

    private static TransitionTable Read(ref ContentReader reader)
    {
        string? comment = null;
        int? fade = null;
        string? back = null;
        ContentId?[]? kinds = null;
        List<TransitionRegion>? regions = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "fade_ticks":
                    fade = reader.ReadInt();
                    break;
                case "back_cover":
                    back = reader.ReadString();
                    break;
                case "kinds":
                    kinds = ReadKinds(ref reader);
                    break;
                case "regions":
                    regions = ReadRegions(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        int fadeTicks = reader.RequireInt(fade, depth, "fade_ticks");
        if (fadeTicks < 1 || fadeTicks > MostFadeTicks)
        {
            throw reader.RefuseField(depth, "fade_ticks", $"the fade is {fadeTicks} ticks, and it takes 1 to {MostFadeTicks} (D-938)");
        }

        string backKey = reader.Require(back, depth, "back_cover");
        if (backKey.Length != 1)
        {
            throw reader.RefuseField(depth, "back_cover", $"the cover is '{backKey}', and a cover names one palette key of one character (D-181)");
        }

        ContentId?[] read = reader.Require(kinds, depth, "kinds");
        var fixedKinds = new List<ContentId>(read.Length);
        for (int index = 0; index < read.Length; index += 1)
        {
            string name = EncounterKinds.NameOf(EncounterKinds.Fixed[index]);
            fixedKinds.Add(read[index] ?? throw reader.RefuseField(depth, "kinds", $"the table names no transition for the kind '{name}', and each of {EncounterKinds.FixedNames} takes one (D-934)"));
        }

        List<TransitionRegion> readRegions = reader.Require(regions, depth, "regions");
        if (readRegions.Count == 0)
        {
            throw reader.RefuseField(depth, "regions", "the table holds no region, and each map of the rules belongs to one (D-936)");
        }

        return new TransitionTable(fadeTicks, backKey[0], fixedKinds, readRegions);
    }

    private static ContentId?[] ReadKinds(ref ContentReader reader)
    {
        var kinds = new ContentId?[EncounterKinds.Fixed.Length];
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string name))
        {
            if (!EncounterKinds.TryFixedOf(name, out EncounterKind kind))
            {
                string reason = string.CompareOrdinal(name, EncounterKinds.NameOf(EncounterKind.Common)) == 0
                    ? "a common fight takes a transition from the pool of its region (D-934)"
                    : $"the table takes the kinds {EncounterKinds.FixedNames} (D-196, D-934)";
                throw reader.Refuse($"the kind '{name}' is no fixed kind of encounter: {reason}");
            }

            kinds[Array.IndexOf(EncounterKinds.Fixed, kind)] = reader.ReadContentId(Transition.IdKind);
        }

        return kinds;
    }

    private static List<TransitionRegion> ReadRegions(ref ContentReader reader)
    {
        var regions = new List<TransitionRegion>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, regions.Count))
        {
            regions.Add(ReadRegion(ref reader));
        }

        return regions;
    }

    private static TransitionRegion ReadRegion(ref ContentReader reader)
    {
        ContentId? id = null;
        List<ContentId>? maps = null;
        List<ContentId>? pool = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "region":
                    id = reader.ReadContentId(RegionIdKind);
                    break;
                case "maps":
                    maps = ReadIds(ref reader, "map");
                    break;
                case "pool":
                    pool = ReadIds(ref reader, Transition.IdKind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId region = reader.Require(id, depth, "region");
        // A region can hold no map yet, such as a later region before its first map. The load
        // still gives each map of the rules one region (D-936).
        List<ContentId> readMaps = reader.Require(maps, depth, "maps");

        List<ContentId> readPool = reader.Require(pool, depth, "pool");
        if (readPool.Count < FewestInPool)
        {
            throw reader.RefuseField(depth, "pool", $"the pool of the region '{region.Value}' holds {readPool.Count} transitions, and a pool holds {FewestInPool} or more, so a pick can skip the last one (D-935)");
        }

        return new TransitionRegion(region, readMaps, readPool);
    }

    private static List<ContentId> ReadIds(ref ContentReader reader, string kind)
    {
        var ids = new List<ContentId>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, ids.Count))
        {
            ContentId id = reader.ReadContentId(kind);
            foreach (ContentId before in ids)
            {
                if (string.CompareOrdinal(before.Value, id.Value) == 0)
                {
                    throw reader.Refuse($"the list names '{id.Value}' two times");
                }
            }

            ids.Add(id);
        }

        return ids;
    }
}
