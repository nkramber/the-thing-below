using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The kind of one map, which its rule file gives (D-112, D-1131).</summary>
/// <remarks>
/// A hub and a dungeon take one map code path (D-112). The kind decides what the load lets a
/// map hold: a hub holds services, and a dungeon holds none (D-1131). PR-14 adds the autosave
/// on the entry to a hub (D-224, D-1132).
/// </remarks>
public enum MapKind
{
    /// <summary>A place where the party recovers, saves, and meets people (D-112).</summary>
    Hub,

    /// <summary>A place of enemies and loot, which holds no service (D-1131).</summary>
    Dungeon,
}

/// <summary>The names of the map kinds, as a map file writes them (D-112).</summary>
public static class MapKinds
{
    /// <summary>Every kind, in one fixed order for a walk of them (G-4).</summary>
    public static readonly MapKind[] All = [MapKind.Hub, MapKind.Dungeon];

    /// <summary>The names of every kind, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "hub, dungeon";

    /// <summary>Gives the kind of one name.</summary>
    /// <param name="name">The name, such as `hub`.</param>
    /// <param name="kind">The kind of that name, when the name names one.</param>
    /// <returns>True when the name names a kind.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out MapKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (MapKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = MapKind.Dungeon;
        return false;
    }

    /// <summary>Gives the name of one kind, which a map file and an error use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `dungeon`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(MapKind kind) => kind switch
    {
        MapKind.Hub => "hub",
        MapKind.Dungeon => "dungeon",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map kind (D-112)"),
    };
}
