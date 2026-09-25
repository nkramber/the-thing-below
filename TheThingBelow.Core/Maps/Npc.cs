using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Maps;

/// <summary>How one NPC moves on its map (D-1137, D-1138).</summary>
public enum NpcMove
{
    /// <summary>
    /// The NPC steps at random inside a list of tile rectangles, one step or one pause on each
    /// pace tick (D-1138).
    /// </summary>
    Wander,

    /// <summary>
    /// The NPC walks a route of the patrol form, and it waits at each tile of it (D-739,
    /// D-1138). A route of one tile stands still (D-740).
    /// </summary>
    Route,

    /// <summary>
    /// The NPC takes the step that brings it closest to a target NPC, inside its own tile
    /// rectangles, on each pace tick (D-1138).
    /// </summary>
    Chase,
}

/// <summary>The names of the NPC moves, as a map file writes them (D-1138).</summary>
public static class NpcMoves
{
    /// <summary>Every move, in one fixed order for a walk of them (G-4).</summary>
    public static readonly NpcMove[] All = [NpcMove.Wander, NpcMove.Route, NpcMove.Chase];

    /// <summary>The names of every move, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "wander, route, chase";

    /// <summary>Gives the move of one name.</summary>
    /// <param name="name">The name, such as `wander`.</param>
    /// <param name="move">The move of that name, when the name names one.</param>
    /// <returns>True when the name names a move.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out NpcMove move)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (NpcMove candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                move = candidate;
                return true;
            }
        }

        move = NpcMove.Route;
        return false;
    }

    /// <summary>Gives the name of one move, which a map file and an error use (T-2).</summary>
    /// <param name="move">The move.</param>
    /// <returns>The name, such as `chase`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no move (T-2).</exception>
    public static string NameOf(NpcMove move) => move switch
    {
        NpcMove.Wander => "wander",
        NpcMove.Route => "route",
        NpcMove.Chase => "chase",
        _ => throw new ArgumentOutOfRangeException(nameof(move), move, "the value names no NPC move (D-1138)"),
    };
}

/// <summary>One tile of the route of an NPC, and the ticks that the NPC waits there (D-1138).</summary>
/// <param name="At">The tile.</param>
/// <param name="WaitTicks">The count of world ticks that the NPC waits on arrival, which is zero or more.</param>
public readonly record struct RouteStop(TilePoint At, int WaitTicks);

/// <summary>
/// One NPC that a map places: its id, its facing, its step, and its move (D-112, D-1137,
/// D-1138).
/// </summary>
/// <remarks>
/// The map file holds each NPC in its `npcs` array, so one rule file still holds each place for
/// the author, the review, and the content hash (D-495, D-528). An NPC is solid, and no NPC
/// steps onto the lead, a wall, a thing, or another NPC (D-1139). PR-14 walks each NPC on a
/// stream of its own (D-1137, T-7).
/// <para>
/// Each move reads its own fields alone, so a field of another move never passes in silence
/// (T-2). A wander NPC and a chaser read a start tile, the rectangles, and the pace, and a
/// chaser adds its target. A route NPC reads its tiles, and it starts on the first one.
/// </para>
/// </remarks>
public sealed class Npc
{
    /// <summary>The kind of the id of an NPC, which a talk trigger names too (D-646, D-1005).</summary>
    public const string IdKind = SceneTrigger.NpcKind;

    private readonly TileArea[] areas;
    private readonly RouteStop[] route;

    private Npc(
        ContentId id,
        StepDirection facing,
        int stepTicks,
        NpcMove move,
        TilePoint start,
        TileArea[] areas,
        int? paceTicks,
        RouteStop[] route,
        ContentId? target)
    {
        this.Id = id;
        this.Facing = facing;
        this.StepTicks = stepTicks;
        this.Move = move;
        this.Start = start;
        this.areas = areas;
        this.PaceTicks = paceTicks;
        this.route = route;
        this.Target = target;
    }

    /// <summary>The permanent content id of this NPC, of the kind `npc` (D-166, D-646).</summary>
    public ContentId Id { get; }

    /// <summary>The direction that this NPC faces when it enters its map (D-716).</summary>
    public StepDirection Facing { get; }

    /// <summary>The count of world ticks that one step of this NPC takes, one of the enemy counts (D-821).</summary>
    public int StepTicks { get; }

    /// <summary>How this NPC moves (D-1138).</summary>
    public NpcMove Move { get; }

    /// <summary>
    /// The tile where this NPC stands when it enters its map: the start tile of a wander NPC or a
    /// chaser, or the first tile of a route (D-1138).
    /// </summary>
    public TilePoint Start { get; }

    /// <summary>
    /// The rectangles of the range of a wander NPC or a chaser, in the order of the file, or none
    /// for a route NPC (D-1138, G-4).
    /// </summary>
    public IReadOnlyList<TileArea> Areas => this.areas;

    /// <summary>
    /// The count of world ticks from one choice of a wander NPC or a chaser to the next, or no
    /// value for a route NPC, which waits at each tile of its route in its place (D-1138).
    /// </summary>
    public int? PaceTicks { get; }

    /// <summary>
    /// The tiles of the route of a route NPC, in order, or none for every other move (D-739,
    /// D-1138, G-4).
    /// </summary>
    public IReadOnlyList<RouteStop> Route => this.route;

    /// <summary>The NPC that a chaser closes on, or no value for every other move (D-1138).</summary>
    public ContentId? Target { get; }

    /// <summary>Tells whether one tile lies inside a rectangle of the range of this NPC (D-1138).</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when a rectangle holds the tile. A route NPC has no rectangle, so it gives false.</returns>
    public bool RangeHolds(TilePoint at)
    {
        foreach (TileArea area in this.areas)
        {
            if (area.Holds(at))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads the `npcs` array of a map file (D-1137).</summary>
    /// <param name="reader">The reader of the map file, at the start of the array.</param>
    /// <returns>Each NPC, in the order of the file.</returns>
    /// <exception cref="ContentException">An entry breaks a rule of the reader (G-6, T-2).</exception>
    /// <remarks>
    /// The reader checks each record alone. <see cref="NpcLayout"/> checks each NPC against the
    /// ground, the things, and the other NPCs of its map.
    /// </remarks>
    public static List<Npc> ReadAll(ref ContentReader reader)
    {
        List<Npc> npcs = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, npcs.Count))
        {
            npcs.Add(Read(ref reader));
        }

        return npcs;
    }

    private static Npc Read(ref ContentReader reader)
    {
        ContentId? id = null;
        string? facing = null;
        int? stepTicks = null;
        string? move = null;
        int? x = null;
        int? y = null;
        List<TileArea>? areas = null;
        int? paceTicks = null;
        ContentId? target = null;
        List<RouteStop>? tiles = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "move":
                    move = reader.ReadString();
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "areas":
                    areas = ReadAreas(ref reader);
                    break;
                case "pace_ticks":
                    paceTicks = reader.ReadInt();
                    break;
                case "target":
                    target = reader.ReadContentId(IdKind);
                    break;
                case "tiles":
                    tiles = ReadStops(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string moveName = reader.Require(move, depth, "move");
        if (!NpcMoves.TryOf(moveName, out NpcMove parsedMove))
        {
            throw reader.RefuseField(depth, "move", $"the NPC '{readId.Value}' takes the move '{moveName}', and an NPC takes one of {NpcMoves.EveryName} (D-1138)");
        }

        // Each move reads its own fields alone, so a field of another move never passes in
        // silence (T-2).
        bool ranged = parsedMove != NpcMove.Route;
        RefuseField(ref reader, depth, readId, "x", x.HasValue, ranged, parsedMove);
        RefuseField(ref reader, depth, readId, "y", y.HasValue, ranged, parsedMove);
        RefuseField(ref reader, depth, readId, "areas", areas is not null, ranged, parsedMove);
        RefuseField(ref reader, depth, readId, "pace_ticks", paceTicks.HasValue, ranged, parsedMove);
        RefuseField(ref reader, depth, readId, "target", target is not null, parsedMove == NpcMove.Chase, parsedMove);
        RefuseField(ref reader, depth, readId, "tiles", tiles is not null, parsedMove == NpcMove.Route, parsedMove);

        string facingName = reader.Require(facing, depth, "facing");
        if (!StepDirections.TryOf(facingName, out StepDirection parsedFacing))
        {
            throw reader.RefuseField(depth, "facing", $"the NPC '{readId.Value}' faces '{facingName}', and a facing is one of {StepDirections.EveryName} (D-716)");
        }

        // A step that the tile of 32 art pixels does not divide moves a sprite by an uneven count
        // of pixels on each tick, so an NPC takes the counts of an enemy (D-821).
        int readStepTicks = reader.RequireInt(stepTicks, depth, "step_ticks");
        if (!MapRules.IsEnemyStep(readStepTicks))
        {
            throw reader.RefuseField(depth, "step_ticks", $"the NPC '{readId.Value}' steps in {readStepTicks} ticks, and an NPC steps in {string.Join(", ", MapRules.EnemyStepTicks)} (D-821)");
        }

        if (parsedMove == NpcMove.Route)
        {
            RouteStop[] stops = [.. tiles!];
            if (stops.Length == 0)
            {
                throw reader.RefuseField(depth, "tiles", $"the NPC '{readId.Value}' holds no route tile, and a route holds at least one. A route of one tile stands still (D-739, D-740)");
            }

            return new Npc(readId, parsedFacing, readStepTicks, parsedMove, stops[0].At, [], null, stops, null);
        }

        TileArea[] range = [.. areas!];
        if (range.Length == 0)
        {
            throw reader.RefuseField(depth, "areas", $"the NPC '{readId.Value}' holds no rectangle, and a range holds at least one (D-1138)");
        }

        // A pace below the step would ask for a new step before the last one ends (D-1138).
        if (paceTicks!.Value < readStepTicks)
        {
            throw reader.RefuseField(depth, "pace_ticks", $"the NPC '{readId.Value}' takes a pace of {paceTicks.Value} ticks, and its step takes {readStepTicks}. A pace is at least one step (D-1138)");
        }

        if (target is ContentId chased && string.CompareOrdinal(chased.Value, readId.Value) == 0)
        {
            throw reader.RefuseField(depth, "target", $"the NPC '{readId.Value}' chases itself, and a chaser names another NPC (D-1138)");
        }

        return new Npc(readId, parsedFacing, readStepTicks, parsedMove, new TilePoint(x!.Value, y!.Value), range, paceTicks, [], target);
    }

    private static void RefuseField(ref ContentReader reader, int depth, ContentId id, string field, bool present, bool wanted, NpcMove move)
    {
        if (present && !wanted)
        {
            throw reader.RefuseField(depth, field, $"the NPC '{id.Value}' takes the move '{NpcMoves.NameOf(move)}', which reads no field '{field}' (T-2)");
        }

        if (!present && wanted)
        {
            throw reader.RefuseField(depth, field, "the field is absent");
        }
    }

    private static List<TileArea> ReadAreas(ref ContentReader reader)
    {
        List<TileArea> areas = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, areas.Count))
        {
            areas.Add(ReadArea(ref reader));
        }

        return areas;
    }

    private static TileArea ReadArea(ref ContentReader reader)
    {
        int? x = null;
        int? y = null;
        int? width = null;
        int? height = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "width":
                    width = reader.ReadInt();
                    break;
                case "height":
                    height = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        TileArea area = new(
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"));
        if (!area.IsRectangle())
        {
            throw reader.Refuse($"the rectangle {area} holds no tile, and a rectangle starts inside the map and holds at least one tile (D-1138)");
        }

        return area;
    }

    private static List<RouteStop> ReadStops(ref ContentReader reader)
    {
        List<RouteStop> stops = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, stops.Count))
        {
            stops.Add(ReadStop(ref reader));
        }

        return stops;
    }

    private static RouteStop ReadStop(ref ContentReader reader)
    {
        int? x = null;
        int? y = null;
        int? waitTicks = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "wait_ticks":
                    waitTicks = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        int wait = reader.RequireInt(waitTicks, depth, "wait_ticks");
        if (wait < 0)
        {
            throw reader.RefuseField(depth, "wait_ticks", $"the route tile waits {wait} ticks, which is below zero (D-1138)");
        }

        return new RouteStop(new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")), wait);
    }
}
