using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>The kind of one thing that a map file places on a tile (D-386, D-528).</summary>
/// <remarks>
/// The map file holds every thing that a rule reads, so one file holds each place for the
/// author, for the review, and for the content hash (D-528). PR-16 and PR-64 add the rules
/// that open a door, a lock, and a chest, and that fire a trap.
/// </remarks>
public enum MapThingKind
{
    /// <summary>A door, which stands in a doorway.</summary>
    Door,

    /// <summary>A lock on a doorway. The map says whether a Theft drill opens it (D-386).</summary>
    Lock,

    /// <summary>A chest with content that the party takes.</summary>
    Chest,

    /// <summary>A trap, which a Theft drill reveals and disarms (D-386).</summary>
    Trap,

    /// <summary>A save point, where the party saves and swaps the reserve (D-58).</summary>
    SavePoint,

    /// <summary>The tile where the party starts on this map. One map holds one (D-528).</summary>
    SpawnPoint,

    /// <summary>A named tile that a story scene or a later rule points at (D-528).</summary>
    Marker,
}

/// <summary>One thing on one tile of a map (D-528).</summary>
/// <param name="Id">The permanent content id of the thing (D-166, D-646).</param>
/// <param name="Kind">What the thing is.</param>
/// <param name="At">The tile that the thing sits on.</param>
/// <param name="Pickable">
/// True when a Theft drill opens this lock (D-386). The field holds false for every other
/// kind, and the reader refuses the field on a thing that is not a lock (T-2).
/// </param>
public sealed record MapThing(ContentId Id, MapThingKind Kind, TilePoint At, bool Pickable);

/// <summary>The names of the thing kinds, and the tile that each kind sits on (D-528).</summary>
public static class MapThingKinds
{
    /// <summary>Every kind, in one fixed order for a walk of them (G-4).</summary>
    public static readonly MapThingKind[] All =
    [
        MapThingKind.Door,
        MapThingKind.Lock,
        MapThingKind.Chest,
        MapThingKind.Trap,
        MapThingKind.SavePoint,
        MapThingKind.SpawnPoint,
        MapThingKind.Marker,
    ];

    /// <summary>The names of every kind, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "door, lock, chest, trap, save_point, spawn_point, marker";

    /// <summary>Gives the kind of one name.</summary>
    /// <param name="name">The name, such as `save_point`.</param>
    /// <param name="kind">The kind of that name, when the name names one.</param>
    /// <returns>True when the name names a kind.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out MapThingKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (MapThingKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = MapThingKind.Marker;
        return false;
    }

    /// <summary>Gives the name of one kind, which a map file and an error use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `spawn_point`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(MapThingKind kind) => kind switch
    {
        MapThingKind.Door => "door",
        MapThingKind.Lock => "lock",
        MapThingKind.Chest => "chest",
        MapThingKind.Trap => "trap",
        MapThingKind.SavePoint => "save_point",
        MapThingKind.SpawnPoint => "spawn_point",
        MapThingKind.Marker => "marker",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map thing kind (D-528)"),
    };

    /// <summary>Gives the one tile kind that a thing of this kind sits on (D-528, T-2).</summary>
    /// <param name="kind">The kind of the thing.</param>
    /// <returns>The kind of the tile under the thing.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    /// <remarks>
    /// A door and a lock stand in a doorway, because a doorway is the gap in a wall that
    /// holds them. Every other thing sits on open ground. A thing on a tile of another kind
    /// fails the load of the map, and the message names the map, the tile, and the kind.
    /// </remarks>
    public static TileKind TileOf(MapThingKind kind) => kind switch
    {
        MapThingKind.Door => TileKind.Doorway,
        MapThingKind.Lock => TileKind.Doorway,
        MapThingKind.Chest => TileKind.Floor,
        MapThingKind.Trap => TileKind.Floor,
        MapThingKind.SavePoint => TileKind.Floor,
        MapThingKind.SpawnPoint => TileKind.Floor,
        MapThingKind.Marker => TileKind.Floor,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map thing kind (D-528)"),
    };
}
