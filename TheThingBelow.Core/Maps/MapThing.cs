using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Maps;

/// <summary>The kind of one thing that a map file places on a tile (D-386, D-528).</summary>
/// <remarks>
/// The map file holds every thing that a rule reads, so one file holds each place for the
/// author, for the review, and for the content hash (D-528). PR-16 adds the rules that open a
/// door, a lock, and a chest, the save of a save point, and the exit (D-1131, D-1216). PR-64 adds
/// the rules that show, fire, and disarm a trap (D-1226, D-1228). A service point holds a service of a hub (D-1142).
/// PR-35 adds the entrance and the gate of the overworld (D-1243). PR-110 adds the mark of a place with no map yet (D-1271).
/// </remarks>
public enum MapThingKind
{
    /// <summary>A door, which stands in a doorway.</summary>
    Door,

    /// <summary>A lock on a doorway. The map says whether a Theft drill opens it (D-386).</summary>
    Lock,

    /// <summary>A chest with entries and gold that the party takes (D-1220).</summary>
    Chest,

    /// <summary>
    /// A trap, which fires one time when the lead steps onto it (D-1226, D-1229). A Theft drill shows
    /// it near the lead, and a confirm then disarms it (D-386, D-1228).
    /// </summary>
    Trap,

    /// <summary>
    /// A save point, where the party saves, on a hub and in a dungeon (D-58, D-1221). It restores
    /// nothing. The party swap works anywhere outside a fight, so a save point marks no swap place
    /// (D-1134).
    /// </summary>
    SavePoint,

    /// <summary>The tile where the party starts on this map. One map holds one (D-528).</summary>
    SpawnPoint,

    /// <summary>A named tile that a story scene or a later rule points at (D-528).</summary>
    Marker,

    /// <summary>
    /// A solid thing that holds one service of a hub, such as a bed (D-1131, D-1142). The lead faces
    /// it and never walks onto it. A save point holds the save, and no service does (D-1221).
    /// </summary>
    ServicePoint,

    /// <summary>The exit of a map, which enters the map that it names when the party steps onto it (D-1216).</summary>
    Exit,

    /// <summary>
    /// The entrance of a place on the overworld, which enters the map of that place when the party
    /// steps onto it (D-1243). The party arrives on the spawn point of that map.
    /// </summary>
    Entrance,

    /// <summary>
    /// A gate of the overworld, which the lead passes only while its condition holds (D-1243). A
    /// confirm at a closed gate posts its notice (D-1257).
    /// </summary>
    Gate,

    /// <summary>
    /// The mark of a place on the overworld whose map does not exist yet (D-1270, D-1271). It draws
    /// the place, and no rule reads it: the lead walks over it (D-1272). The PR of the place changes
    /// its kind to an entrance on the same tile.
    /// </summary>
    Mark,
}

/// <summary>The condition and the notice of one gate of the overworld (D-1243, D-1257).</summary>
/// <param name="Condition">The condition of the PR-68 form, which opens the gate while it holds (D-543).</param>
/// <param name="Notice">The notice that a confirm at the closed gate posts (D-1257).</param>
public sealed record MapGate(Condition Condition, ContentId Notice);

/// <summary>One thing on one tile of a map (D-528).</summary>
/// <param name="Id">The permanent content id of the thing (D-166, D-646).</param>
/// <param name="Kind">What the thing is.</param>
/// <param name="At">The tile that the thing sits on.</param>
/// <param name="Pickable">
/// True when a Theft drill opens this lock (D-386). The field holds false for every other
/// kind, and the reader refuses the field on a thing that is not a lock (T-2).
/// </param>
/// <param name="Key">
/// The key item that opens this lock, or no value (D-1219). A story lock names one, and a
/// pickable lock can name one. Every other kind holds no value.
/// </param>
/// <param name="To">The map that this exit or this entrance enters (D-1216, D-1243). Every other kind holds no value.</param>
/// <param name="Contents">The entries and the gold of this chest (D-1220). Every other kind holds no value.</param>
/// <param name="Harm">What this trap does when it fires (D-1226). Every other kind holds no value.</param>
/// <param name="Arrive">
/// The marker where the party arrives through this exit or this entrance (D-1255, D-1367). An
/// exit to an overworld names a marker of the overworld, and an entrance names a marker of its
/// place. Every other thing holds no value.
/// </param>
/// <param name="Gate">The condition and the notice of this gate (D-1243). Every other kind holds no value.</param>
public sealed record MapThing(ContentId Id, MapThingKind Kind, TilePoint At, bool Pickable, ContentId? Key, ContentId? To, ChestContents? Contents, TrapHarm? Harm, ContentId? Arrive, MapGate? Gate);

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
        MapThingKind.ServicePoint,
        MapThingKind.Exit,
        MapThingKind.Entrance,
        MapThingKind.Gate,
        MapThingKind.Mark,
    ];

    /// <summary>The names of every kind, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "door, lock, chest, trap, save_point, spawn_point, marker, service_point, exit, entrance, gate, mark";

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
        MapThingKind.ServicePoint => "service_point",
        MapThingKind.Exit => "exit",
        MapThingKind.Entrance => "entrance",
        MapThingKind.Gate => "gate",
        MapThingKind.Mark => "mark",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map thing kind (D-528)"),
    };

    /// <summary>The ground of every thing that sits on open ground, for the error of a wrong tile (T-2).</summary>
    public const string OpenGroundNames = "floor, grass, road, or snowfield";

    /// <summary>Tells whether a tile is open ground, where every thing but a door and a lock sits (D-1256, D-1277).</summary>
    /// <param name="tile">The kind of the tile.</param>
    /// <returns>True for the floor of a place, and for the grass, the road, and the snowfield of the overworld.</returns>
    public static bool IsOpenGround(TileKind tile) =>
        tile == TileKind.Floor || tile == TileKind.Grass || tile == TileKind.Road || tile == TileKind.Snowfield;

    /// <summary>Tells whether a thing of this kind can sit on a tile of this kind (D-528, D-1256, T-2).</summary>
    /// <param name="kind">The kind of the thing.</param>
    /// <param name="tile">The kind of the tile under the thing.</param>
    /// <returns>True for a door or a lock in a doorway, and for every other thing on open ground.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    /// <remarks>
    /// A door and a lock stand in a doorway, because a doorway is the gap in a wall that
    /// holds them. Every other thing sits on open ground: the floor of a place, or the grass, the
    /// road, or the snowfield of the overworld (D-1256, D-1277). A thing on a tile of another kind fails the load of the map, and
    /// the message names the map, the tile, and the kind.
    /// </remarks>
    public static bool CanSitOn(MapThingKind kind, TileKind tile) => kind switch
    {
        MapThingKind.Door => tile == TileKind.Doorway,
        MapThingKind.Lock => tile == TileKind.Doorway,
        MapThingKind.Chest or MapThingKind.Trap or MapThingKind.SavePoint or MapThingKind.SpawnPoint
            or MapThingKind.Marker or MapThingKind.ServicePoint or MapThingKind.Exit or MapThingKind.Entrance
            or MapThingKind.Gate or MapThingKind.Mark => IsOpenGround(tile),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map thing kind (D-528)"),
    };

    /// <summary>Gives the name of the ground that a thing of this kind sits on, for an error (T-2).</summary>
    /// <param name="kind">The kind of the thing.</param>
    /// <returns>The name, such as `doorway`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string GroundNameOf(MapThingKind kind) => kind switch
    {
        MapThingKind.Door or MapThingKind.Lock => TileKinds.NameOf(TileKind.Doorway),
        MapThingKind.Chest or MapThingKind.Trap or MapThingKind.SavePoint or MapThingKind.SpawnPoint
            or MapThingKind.Marker or MapThingKind.ServicePoint or MapThingKind.Exit or MapThingKind.Entrance
            or MapThingKind.Gate or MapThingKind.Mark => OpenGroundNames,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map thing kind (D-528)"),
    };

    /// <summary>Tells whether a thing of this kind blocks each step onto its tile (D-1142, D-1222).</summary>
    /// <param name="kind">The kind of the thing.</param>
    /// <returns>True for a door, a chest, a save point, and a service point.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    /// <remarks>
    /// A solid thing blocks the lead, a patrol, an NPC, and a story scene actor, because each of
    /// them reads <see cref="MapRules.CanEnter(GameMap, TilePoint)"/>. The lead faces a solid thing and confirms it
    /// (D-1131). An open door takes the step of the lead alone, through `MapState`, so no patrol
    /// and no NPC walks through a door (D-1142). A lock shares the tile of its door, and the door
    /// blocks it. A gate is not solid: the lead reads its condition through `MapState`, and no NPC
    /// steps onto a tile with a thing (D-1243).
    /// </remarks>
    public static bool IsSolid(MapThingKind kind) => kind switch
    {
        MapThingKind.Door => true,
        MapThingKind.Lock => false,
        MapThingKind.Chest => true,
        MapThingKind.Trap => false,
        MapThingKind.SavePoint => true,
        MapThingKind.SpawnPoint => false,
        MapThingKind.Marker => false,
        MapThingKind.ServicePoint => true,
        MapThingKind.Exit => false,
        MapThingKind.Entrance => false,
        MapThingKind.Gate => false,
        MapThingKind.Mark => false,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no map thing kind (D-528)"),
    };
}
