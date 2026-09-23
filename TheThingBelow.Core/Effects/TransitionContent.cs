using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// The transition files and the transition table of one build, checked across files, and the
/// pick of the transition of each fight (D-195, D-196, D-934 to D-941). No rule reads them (D-495).
/// </summary>
/// <remarks>
/// The checks that span files:
/// <list type="bullet">
/// <item>The library holds one file for each look (D-195).</item>
/// <item>Each id of the table names a transition file, and each cover names a palette key (D-181).</item>
/// <item>A pool holds no transition of a fixed kind, so a special kind reads the same in each region (D-934).</item>
/// <item>Each map of the rules lies in one region, and each map of the table is a map of the rules (D-936).</item>
/// </list>
/// </remarks>
public sealed class TransitionContent
{
    /// <summary>The full-screen passes of a transition, which the effect budget counts on each map (D-523, D-923).</summary>
    /// <remarks>
    /// The transition and each fade draw in one pass over the frame, and one plays at a time. The
    /// map stays on screen under the transition, and the fight is on screen under its fade, so the
    /// passes of a map with this one pass are the most of any frame (D-920, D-939).
    /// </remarks>
    public const int FullScreenPasses = 1;

    private readonly SortedDictionary<string, Transition> transitionOf;
    private readonly SortedDictionary<string, TransitionRegion> regionOf;

    private TransitionContent(
        IReadOnlyList<Transition> transitions,
        TransitionTable table,
        SortedDictionary<string, Transition> transitionOf,
        SortedDictionary<string, TransitionRegion> regionOf)
    {
        this.Transitions = transitions;
        this.Table = table;
        this.transitionOf = transitionOf;
        this.regionOf = regionOf;
    }

    /// <summary>Every transition, in the order of its path (F-39).</summary>
    public IReadOnlyList<Transition> Transitions { get; }

    /// <summary>The table of the kinds, the regions, and the fade (D-934, D-936, D-938).</summary>
    public TransitionTable Table { get; }

    /// <summary>Gives one transition by its id.</summary>
    /// <param name="id">The id, such as `transition.shatter`.</param>
    /// <returns>The transition.</returns>
    /// <exception cref="ContentException">No transition file holds the id (T-2).</exception>
    public Transition TransitionOf(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.transitionOf.TryGetValue(id.Value, out Transition? found)
            ? found
            : throw ContentException.ForFile(Transition.Folder, $"no transition file holds the id '{id.Value}' (D-195)");
    }

    /// <summary>Gives the transition of a fight (D-934, D-935, D-937).</summary>
    /// <param name="kind">The kind of the encounter, from <see cref="EncounterKinds.Of"/>.</param>
    /// <param name="map">The id of the map of the encounter, which names the region of the pool.</param>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="tick">The tick of the run that starts the fight.</param>
    /// <param name="lastCommon">The id of the transition of the last common fight that the screen showed, or null.</param>
    /// <returns>The transition.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ContentException">No region holds the map (D-936, T-2).</exception>
    /// <remarks>
    /// A fixed kind takes its transition from the table. A common fight takes one from the pool of
    /// its region, with a hash of the seed and the tick, and it skips the last pick of a common
    /// fight. The hash draws on no rule stream, so the pick never changes the state (G-4, D-935).
    /// </remarks>
    public Transition Pick(EncounterKind kind, ContentId map, ulong seed, long tick, ContentId? lastCommon)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (kind != EncounterKind.Common)
        {
            return this.TransitionOf(this.Table.FixedOf(kind));
        }

        if (!this.regionOf.TryGetValue(map.Value, out TransitionRegion? region))
        {
            throw ContentException.ForField(TransitionTable.Path, "regions", $"no region holds the map '{map.Value}', and each map belongs to one (D-936)");
        }

        var choices = new List<ContentId>(region.Pool.Count);
        foreach (ContentId entry in region.Pool)
        {
            if (lastCommon is null || string.CompareOrdinal(entry.Value, lastCommon.Value) != 0)
            {
                choices.Add(entry);
            }
        }

        var hasher = new StateHasher();
        hasher.AddText("transition.pick");
        hasher.AddUInt64(seed);
        hasher.AddInt64(tick);
        int index = (int)(hasher.Finish() % (ulong)choices.Count);
        return this.TransitionOf(choices[index]);
    }

    /// <summary>Reads the transition files and the table, and checks them against the maps and the palette.</summary>
    /// <param name="files">The transition files and the table file, which <see cref="IsTransitionContent"/> picked.</param>
    /// <param name="maps">The maps of the rules, by id.</param>
    /// <param name="palette">The palette, which each cover names a key of (D-181).</param>
    /// <param name="ids">The effect ids of the build so far, which each transition id joins (D-166).</param>
    /// <returns>The transition content.</returns>
    /// <exception cref="ContentException">A file breaks a rule of its reader, or a check across files fails (T-2).</exception>
    public static TransitionContent Load(
        IReadOnlyList<ContentFile> files,
        SortedDictionary<string, GameMap> maps,
        Palette palette,
        SortedDictionary<string, string> ids)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(ids);

        TransitionTable? table = null;
        var transitions = new List<Transition>();
        foreach (ContentFile file in files)
        {
            if (string.CompareOrdinal(file.Path, TransitionTable.Path) == 0)
            {
                table = TransitionTable.Read(file.Bytes, file.Path);
            }
            else if (Transition.IsTransitionFile(file.Path))
            {
                Transition transition = Transition.Read(file.Bytes, file.Path);
                if (!ids.TryAdd(transition.Id.Value, file.Path))
                {
                    throw ContentException.ForField(file.Path, "id", $"the file '{ids[transition.Id.Value]}' holds the id '{transition.Id.Value}' too, and one id names one effect (D-166)");
                }

                transitions.Add(transition);
            }
            else
            {
                throw ContentException.ForFile(file.Path, "the file is not a transition file, and the effect reader gave it to the transition reader");
            }
        }

        transitions.Sort(static (first, second) => string.CompareOrdinal(first.File, second.File));
        TransitionTable readTable = table ?? throw ContentException.ForFile(TransitionTable.Path, "the content set holds no such file, and each fight takes a transition (D-196)");
        SortedDictionary<string, Transition> transitionOf = LibraryOf(transitions, palette);
        if (!palette.TryColorOf(readTable.BackCover, out _))
        {
            throw ContentException.ForField(TransitionTable.Path, "back_cover", $"the palette holds no key '{readTable.BackCover}', and a cover names a palette key (D-181, L-10)");
        }

        RefuseAbsentTransition(readTable, transitionOf);
        RefuseFixedInPool(readTable);
        return new TransitionContent(transitions, readTable, transitionOf, RegionsOf(readTable, maps));
    }

    /// <summary>Tells whether a content path is a transition file or the table file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True for the table file and each file of the transition folder.</returns>
    public static bool IsTransitionContent(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return string.CompareOrdinal(path, TransitionTable.Path) == 0 || Transition.IsTransitionFile(path);
    }

    /// <summary>Gives each transition by its id, and refuses a cover with no palette key and a look with no file or two (D-181, D-195).</summary>
    private static SortedDictionary<string, Transition> LibraryOf(List<Transition> transitions, Palette palette)
    {
        var transitionOf = new SortedDictionary<string, Transition>(StringComparer.Ordinal);
        var lookFiles = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (Transition transition in transitions)
        {
            if (!palette.TryColorOf(transition.Cover, out _))
            {
                throw ContentException.ForField(transition.File, "cover", $"the palette holds no key '{transition.Cover}', and a cover names a palette key (D-181, L-10)");
            }

            string look = Transition.NameOf(transition.Look);
            if (!lookFiles.TryAdd(look, transition.File))
            {
                throw ContentException.ForField(transition.File, "look", $"the file '{lookFiles[look]}' holds the look '{look}' too, and the library of D-195 holds each look one time");
            }

            transitionOf.Add(transition.Id.Value, transition);
        }

        foreach (TransitionLook look in Transition.AllLooks)
        {
            if (!lookFiles.ContainsKey(Transition.NameOf(look)))
            {
                throw ContentException.ForFile(Transition.Folder, $"no transition file holds the look '{Transition.NameOf(look)}', and the library holds ten transitions (D-195)");
            }
        }

        return transitionOf;
    }

    /// <summary>Refuses a kind or a pool entry of the table that names no transition file (T-2).</summary>
    private static void RefuseAbsentTransition(TransitionTable table, SortedDictionary<string, Transition> transitionOf)
    {
        for (int index = 0; index < table.FixedKinds.Count; index += 1)
        {
            string id = table.FixedKinds[index].Value;
            if (!transitionOf.ContainsKey(id))
            {
                throw ContentException.ForField(TransitionTable.Path, $"kinds.{EncounterKinds.NameOf(EncounterKinds.Fixed[index])}", $"no transition file holds the id '{id}' (D-195)");
            }
        }

        for (int region = 0; region < table.Regions.Count; region += 1)
        {
            IReadOnlyList<ContentId> pool = table.Regions[region].Pool;
            for (int index = 0; index < pool.Count; index += 1)
            {
                if (!transitionOf.ContainsKey(pool[index].Value))
                {
                    throw ContentException.ForField(TransitionTable.Path, $"regions[{region}].pool[{index}]", $"no transition file holds the id '{pool[index].Value}' (D-195)");
                }
            }
        }
    }

    /// <summary>Refuses a pool that holds the transition of a fixed kind, because a special kind reads the same in each region (D-934).</summary>
    private static void RefuseFixedInPool(TransitionTable table)
    {
        for (int region = 0; region < table.Regions.Count; region += 1)
        {
            IReadOnlyList<ContentId> pool = table.Regions[region].Pool;
            for (int index = 0; index < pool.Count; index += 1)
            {
                for (int kind = 0; kind < table.FixedKinds.Count; kind += 1)
                {
                    if (string.CompareOrdinal(pool[index].Value, table.FixedKinds[kind].Value) == 0)
                    {
                        throw ContentException.ForField(
                            TransitionTable.Path,
                            $"regions[{region}].pool[{index}]",
                            $"the pool holds '{pool[index].Value}', which the kind '{EncounterKinds.NameOf(EncounterKinds.Fixed[kind])}' takes, and a pool holds no transition of a fixed kind (D-934)");
                    }
                }
            }
        }
    }

    /// <summary>Gives the region of each map, and refuses a map of the table that the rules do not hold, a map in two regions, and a map in none (D-936).</summary>
    private static SortedDictionary<string, TransitionRegion> RegionsOf(TransitionTable table, SortedDictionary<string, GameMap> maps)
    {
        var regionOf = new SortedDictionary<string, TransitionRegion>(StringComparer.Ordinal);
        var regionIds = new SortedSet<string>(StringComparer.Ordinal);
        for (int region = 0; region < table.Regions.Count; region += 1)
        {
            TransitionRegion entry = table.Regions[region];
            if (!regionIds.Add(entry.Id.Value))
            {
                throw ContentException.ForField(TransitionTable.Path, $"regions[{region}].region", $"the table names the region '{entry.Id.Value}' two times (D-936)");
            }

            for (int index = 0; index < entry.Maps.Count; index += 1)
            {
                string map = entry.Maps[index].Value;
                string field = $"regions[{region}].maps[{index}]";
                if (!maps.ContainsKey(map))
                {
                    throw ContentException.ForField(TransitionTable.Path, field, $"the rules hold no map '{map}' (D-936)");
                }

                if (!regionOf.TryAdd(map, entry))
                {
                    throw ContentException.ForField(TransitionTable.Path, field, $"the region '{regionOf[map].Id.Value}' holds the map '{map}' too, and a map belongs to one region (D-936)");
                }
            }
        }

        foreach (string map in maps.Keys)
        {
            if (!regionOf.ContainsKey(map))
            {
                throw ContentException.ForField(TransitionTable.Path, "regions", $"no region holds the map '{map}', and each map of the rules belongs to one (D-936)");
            }
        }

        return regionOf;
    }
}
