using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Maps;

/// <summary>The memory of each map of a run, which lasts past each exit (D-385, D-555).</summary>
/// <remarks>
/// The party enters a map on its spawn point, and the memory of that map decides which enemy is
/// dead, which door is open, and what stays in each chest (D-555, D-1216). The walked tiles and the
/// place of each enemy and each NPC start again at each entry. The memory sits in the ordinal order
/// of the map ids (G-4).
/// </remarks>
public sealed class PlaceMemory
{
    private readonly SortedDictionary<string, PlaceState> places;

    private PlaceMemory(SortedDictionary<string, PlaceState> places)
    {
        this.places = places;
    }

    /// <summary>Gives the memory of a new run, which holds nothing.</summary>
    /// <returns>The memory.</returns>
    public static PlaceMemory Start() => new(new SortedDictionary<string, PlaceState>(StringComparer.Ordinal));

    /// <summary>Gives the memory of a stored run, and checks each value against the maps of this build (T-2).</summary>
    /// <param name="maps">The maps of the run, from the content of this build.</param>
    /// <param name="values">The stored values, or no value on a snapshot before save format 18.</param>
    /// <param name="source">What the values came from, such as `the save`, for the error.</param>
    /// <param name="drift">The drift rule of the resume (D-1111).</param>
    /// <returns>The memory.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">
    /// Two values name one map or one thing, a count is below 1, or a snapshot of this build names
    /// a map, an enemy, a door, a chest, or a thing of a chest that the content lacks (T-2).
    /// </exception>
    /// <remarks>
    /// A save of another build drops each value that the content of this build no longer holds,
    /// and a log line names each change (D-1111, D-1113). A count above the count of its chest
    /// entry takes that count.
    /// </remarks>
    public static PlaceMemory Resume(MapSet maps, IReadOnlyList<PlaceValues>? values, string source, ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(drift);

        var places = new SortedDictionary<string, PlaceState>(StringComparer.Ordinal);
        foreach (PlaceValues value in values ?? [])
        {
            ArgumentNullException.ThrowIfNull(value);
            Refuse(places.ContainsKey(value.Map.Value), source, $"it holds the memory of the map '{value.Map.Value}' two times");
            if (!maps.TryFind(value.Map, out GameMap? found))
            {
                Refuse(!drift.Adjusts, source, $"it holds the memory of the map '{value.Map.Value}', which the maps of the run lack, and they are {maps.DescribeIds()}");
                drift.Note(LogSubsystems.World, "the content of this build holds no map of a stored memory, and the memory leaves", [new LogField("map", value.Map.Value)]);
                continue;
            }

            places.Add(value.Map.Value, ResumePlace(found!, value, source, drift));
        }

        return new PlaceMemory(places);
    }

    /// <summary>Gives the memory of one map, and makes an empty one when the run holds none yet.</summary>
    /// <param name="map">The id of the map.</param>
    /// <returns>The memory of that map.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public PlaceState Of(ContentId map)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (!this.places.TryGetValue(map.Value, out PlaceState? place))
        {
            place = new PlaceState(map);
            this.places.Add(map.Value, place);
        }

        return place;
    }

    /// <summary>Gives the values of each map whose memory holds something, in the ordinal order of the map ids (D-166).</summary>
    /// <returns>The values.</returns>
    public IReadOnlyList<PlaceValues> Values()
    {
        List<PlaceValues> values = [];
        foreach (PlaceState place in this.places.Values)
        {
            if (!place.IsEmpty)
            {
                values.Add(place.Values());
            }
        }

        return values;
    }

    /// <summary>Adds the memory of each map that holds something to the state hash, in the order of <see cref="Values"/> (G-5).</summary>
    /// <param name="hasher">The hasher.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    /// <remarks>An empty memory takes no part, so a map that the party only entered hashes as a map that it never entered.</remarks>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        int count = 0;
        foreach (PlaceState place in this.places.Values)
        {
            count += place.IsEmpty ? 0 : 1;
        }

        hasher.AddInt32(count);
        foreach (PlaceState place in this.places.Values)
        {
            if (!place.IsEmpty)
            {
                place.Hash(hasher);
            }
        }
    }

    private static PlaceState ResumePlace(GameMap map, PlaceValues value, string source, ResumeDrift drift)
    {
        var place = new PlaceState(map.Id);
        foreach (ContentId enemy in value.Dead)
        {
            Refuse(place.IsDead(enemy), source, $"the memory of '{map.Id.Value}' holds the dead enemy '{enemy.Value}' two times");
            if (Keeps(!map.PlacesPatrol(enemy), map, enemy, "the map places no such enemy", source, drift))
            {
                place.MarkDead(enemy);
            }
        }

        foreach (ContentId door in value.Doors)
        {
            Refuse(place.IsOpen(door), source, $"the memory of '{map.Id.Value}' holds the open door '{door.Value}' two times");
            if (Keeps(!map.HoldsThing(door, MapThingKind.Door), map, door, "the map holds no such door", source, drift))
            {
                place.Open(door);
            }
        }

        foreach (ChestValues chest in value.Chests)
        {
            Refuse(place.LeftIn(chest.Chest) is not null, source, $"the memory of '{map.Id.Value}' holds the chest '{chest.Chest.Value}' two times");
            MapThing? thing = ChestOf(map, chest.Chest);
            if (Keeps(thing is null, map, chest.Chest, "the map holds no such chest", source, drift))
            {
                place.Keep(chest.Chest, LeftOf(map, thing!, chest, source, drift));
            }
        }

        return place;
    }

    /// <summary>Checks each thing that stays in a chest against the entries of the chest of this build (D-385, D-1111).</summary>
    private static List<ChestLeft> LeftOf(GameMap map, MapThing chest, ChestValues values, string source, ResumeDrift drift)
    {
        ChestContents contents = chest.Contents
            ?? throw new InvalidOperationException($"The chest '{chest.Id.Value}' holds no contents, and the reader of a map refuses such a chest (D-1220, T-2).");
        List<ChestLeft> kept = [];
        foreach (ChestLeft left in values.Left)
        {
            ArgumentNullException.ThrowIfNull(left);
            Refuse(left.Count < 1, source, $"the chest '{chest.Id.Value}' keeps {left.Count} of '{left.Thing.Value}', and a kept count is at least 1");
            foreach (ChestLeft earlier in kept)
            {
                Refuse(string.CompareOrdinal(earlier.Thing.Value, left.Thing.Value) == 0, source, $"the chest '{chest.Id.Value}' keeps '{left.Thing.Value}' two times");
            }

            int? most = MostOf(contents, left.Thing);
            if (!Keeps(most is null, map, left.Thing, $"the chest '{chest.Id.Value}' holds no entry or fallback of that thing", source, drift))
            {
                continue;
            }

            if (left.Count > most!.Value)
            {
                Refuse(!drift.Adjusts, source, $"the chest '{chest.Id.Value}' keeps {left.Count} of '{left.Thing.Value}', above the count {most.Value} of its entry");
                drift.Note(LogSubsystems.World, "the chest of this build holds fewer of a kept thing, and the kept count takes the new count", [new LogField("map", map.Id.Value), new LogField("chest", chest.Id.Value), new LogField("thing", left.Thing.Value), LogField.OfNumber("stored_count", left.Count), LogField.OfNumber("count", most.Value)]);
                kept.Add(left with { Count = most.Value });
                continue;
            }

            kept.Add(left);
        }

        return kept;
    }

    /// <summary>Gives the most copies of one thing that a chest can keep: the count of its entry, or 1 for the fallback of a lesson entry.</summary>
    private static int? MostOf(ChestContents contents, ContentId thing)
    {
        foreach (ChestEntry entry in contents.Entries)
        {
            if (string.CompareOrdinal(entry.Thing.Value, thing.Value) == 0 && entry.Fallback is null)
            {
                return entry.Count;
            }

            if (entry.Fallback is ContentId fallback && string.CompareOrdinal(fallback.Value, thing.Value) == 0)
            {
                return 1;
            }
        }

        return null;
    }

    private static MapThing? ChestOf(GameMap map, ContentId id)
    {
        foreach (MapThing thing in map.Things)
        {
            if (thing.Kind == MapThingKind.Chest && string.CompareOrdinal(thing.Id.Value, id.Value) == 0)
            {
                return thing;
            }
        }

        return null;
    }

    /// <summary>
    /// Tells whether a stored id stays. A misfit refuses a snapshot of this build, and a snapshot of
    /// another build drops the id with a log line (D-1111).
    /// </summary>
    private static bool Keeps(bool misfit, GameMap map, ContentId id, string reason, string source, ResumeDrift drift)
    {
        if (!misfit)
        {
            return true;
        }

        Refuse(!drift.Adjusts, source, $"the memory of '{map.Id.Value}' names '{id.Value}', and {reason}");
        drift.Note(LogSubsystems.World, $"the memory of a map names a thing that this build lacks, and it leaves: {reason}", [new LogField("map", map.Id.Value), new LogField("thing", id.Value)]);
        return false;
    }

    private static void Refuse(bool refused, string source, string reason)
    {
        if (refused)
        {
            throw new ArgumentException($"The snapshot of {source} is not a state of a run: {reason} (T-2, D-555).");
        }
    }
}
