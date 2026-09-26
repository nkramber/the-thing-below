using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The maps of one run, by id (D-528). A run starts on one map of the set, and the entry to a map
/// finds the map here, so a snapshot and a record name a map by its id alone (D-166, D-1133).
/// </summary>
/// <remarks>
/// The host passes every map of its content, and a test passes the maps of its own run. The
/// go-to-map command of a development build enters a map of the set, and PR-35 builds the travel
/// between places (D-113, D-1133).
/// </remarks>
public sealed class MapSet
{
    // The key is the text of the id, and the order is ordinal, so every machine walks the set in
    // one order (F-39, G-4). A lookup by key stays legal in Core (D-615).
    private readonly SortedDictionary<string, GameMap> maps;

    private MapSet(SortedDictionary<string, GameMap> maps)
    {
        this.maps = maps;
    }

    /// <summary>Every map of the set, in the ordinal order of its id (F-39, G-4).</summary>
    public IEnumerable<GameMap> All => this.maps.Values;

    /// <summary>The count of maps in the set.</summary>
    public int Count => this.maps.Count;

    /// <summary>Makes the set from the maps of the host.</summary>
    /// <param name="maps">One map for each id, and one map at least.</param>
    /// <returns>The set.</returns>
    /// <exception cref="ArgumentNullException">The list or one map is null (T-2).</exception>
    /// <exception cref="ArgumentException">The list is empty, or two maps carry the same id (T-2).</exception>
    public static MapSet Of(IEnumerable<GameMap> maps)
    {
        ArgumentNullException.ThrowIfNull(maps);

        SortedDictionary<string, GameMap> byId = new(StringComparer.Ordinal);
        foreach (GameMap map in maps)
        {
            ArgumentNullException.ThrowIfNull(map);
            if (!byId.TryAdd(map.Id.Value, map))
            {
                throw new ArgumentException($"The host passed two maps with the id '{map.Id.Value}' (T-2, D-166).", nameof(maps));
            }
        }

        if (byId.Count == 0)
        {
            throw new ArgumentException("The host passed no map, and a run stands on a map (T-2, D-528).", nameof(maps));
        }

        return new MapSet(byId);
    }

    /// <summary>Finds the map of one id.</summary>
    /// <param name="id">The id of the map, such as `map.fixture_dungeon`.</param>
    /// <param name="map">The map, or null when the set holds no map with that id.</param>
    /// <returns>True when the set holds a map with that id.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public bool TryFind(ContentId id, out GameMap? map)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.maps.TryGetValue(id.Value, out map);
    }

    /// <summary>Gives the ids of every map of the set, for an error (T-2).</summary>
    /// <returns>The ids in ordinal order, separated by a comma.</returns>
    public string DescribeIds() => string.Join(", ", this.maps.Keys);
}
