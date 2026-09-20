using System;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The party on one map: the tile of the lead, the facing, the step that runs now, and every
/// tile that the party walked (D-106, D-567).
/// </summary>
/// <remarks>
/// Core keeps the lead on a whole tile through the whole step. <see cref="StepTicks"/> counts
/// the ticks of the step that runs, and Game slides the sprite from the tile of the lead to
/// the next tile across them (D-203). The lead reaches the next tile on the last tick of the
/// step, and the walked record takes that tile then (D-567).
/// <para>
/// The lead walks the map in every case, in the party or in the reserve (D-292, D-306).
/// </para>
/// </remarks>
public sealed class MapState
{
    private StepDirection? wanted;

    private MapState(
        GameMap map,
        TilePoint leadAt,
        StepDirection facing,
        StepDirection? stepping,
        int stepTicks,
        WalkedTiles walked)
    {
        this.Map = map;
        this.LeadAt = leadAt;
        this.Facing = facing;
        this.Stepping = stepping;
        this.StepTicks = stepTicks;
        this.Walked = walked;
    }

    /// <summary>The map that the party stands on (D-528).</summary>
    public GameMap Map { get; }

    /// <summary>The tile of the lead, which is always a whole tile (D-203).</summary>
    public TilePoint LeadAt { get; private set; }

    /// <summary>The direction that the lead faces, which its sprite draws (D-207).</summary>
    public StepDirection Facing { get; private set; }

    /// <summary>The direction of the step that runs now, or no value while the lead stands.</summary>
    public StepDirection? Stepping { get; private set; }

    /// <summary>
    /// The count of ticks of the step that runs, from 0 to <see cref="MapRules.TicksPerStep"/>
    /// minus one. The value is zero while the lead stands.
    /// </summary>
    public int StepTicks { get; private set; }

    /// <summary>Every tile that the party walked on this map (D-567).</summary>
    public WalkedTiles Walked { get; }

    /// <summary>The sight range of the party here, which the time of day of the map gives (D-193).</summary>
    public int SightRange => MapRules.PartySightRange(this.Map.Time);

    /// <summary>Puts the party on a map at its spawn point (D-528).</summary>
    /// <param name="map">The map to enter.</param>
    /// <returns>The state, with the spawn tile walked.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static MapState Enter(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(map.Spawn);
        return new MapState(map, map.Spawn, StepDirection.South, null, 0, walked);
    }

    /// <summary>Puts the party back on a map from the values of a snapshot (D-166, D-651).</summary>
    /// <param name="map">The map of the snapshot, which the content set of this build holds.</param>
    /// <param name="leadAt">The tile of the lead.</param>
    /// <param name="facing">The direction that the lead faces.</param>
    /// <param name="stepping">The direction of the step that ran, or no value.</param>
    /// <param name="stepTicks">The count of ticks of that step.</param>
    /// <param name="walked">The walked tiles of the snapshot.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">The map, the record, or the source is null (T-2).</exception>
    /// <exception cref="ArgumentException">A value describes no state of a party on this map (T-2).</exception>
    public static MapState Resume(
        GameMap map,
        TilePoint leadAt,
        StepDirection facing,
        StepDirection? stepping,
        int stepTicks,
        WalkedTiles walked,
        string source)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(walked);
        ArgumentException.ThrowIfNullOrEmpty(source);

        Refuse(!map.Holds(leadAt), source, $"the lead stands at {leadAt}, and the map '{map.Id.Value}' is {map.Width} by {map.Height} tiles");
        Refuse(
            !TileKinds.CanWalk(map.TileAt(leadAt)),
            source,
            $"the lead stands at {leadAt}, which is a {TileKinds.NameOf(map.TileAt(leadAt))} tile");
        Refuse(
            walked.Width != map.Width || walked.Height != map.Height,
            source,
            $"the walked tiles are {walked.Width} by {walked.Height}, and the map '{map.Id.Value}' is {map.Width} by {map.Height}");
        Refuse(
            !walked.WasWalked(leadAt),
            source,
            $"the lead stands at {leadAt}, and the walked tiles do not hold that tile (D-567)");
        Refuse(
            stepping is null && stepTicks != 0,
            source,
            $"the lead stands on no step, and the step ticks are {stepTicks}");
        Refuse(
            stepping is not null && (stepTicks < 0 || stepTicks >= MapRules.TicksPerStep),
            source,
            $"the step ticks are {stepTicks}, and the range of a step is 0 to {MapRules.TicksPerStep - 1}");

        if (stepping is StepDirection direction)
        {
            TilePoint target = leadAt.Step(direction);
            Refuse(
                !MapRules.CanEnter(map, target),
                source,
                $"the lead steps {StepDirections.NameOf(direction)} from {leadAt}, and the tile {target} takes no step");
        }

        return new MapState(map, leadAt, facing, stepping, stepTicks, walked);
    }

    /// <summary>
    /// Reads the move intent of this tick (D-493). The lead turns to that direction, and the
    /// next world tick starts the step.
    /// </summary>
    /// <param name="direction">The direction of the intent.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value names no direction (T-2).</exception>
    /// <remarks>
    /// The player holds a key, and Game makes one intent for each tick that the key is down.
    /// Thus a move intent while a step runs is the normal case, and it chains the next step
    /// (D-716). Two move intents in one tick leave the last one, which keeps the order of the
    /// record and the order of the rules the same (T-7).
    /// </remarks>
    public void Want(StepDirection direction)
    {
        switch (direction)
        {
            case StepDirection.North:
            case StepDirection.South:
            case StepDirection.East:
            case StepDirection.West:
                this.wanted = direction;
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    "the value names no step direction (D-716)");
        }
    }

    /// <summary>Runs the party for one world tick: the step that runs, and then the next one.</summary>
    /// <param name="walked">Takes the tile that the lead reached on this tick, or no value.</param>
    /// <param name="started">Takes the direction of the step that started on this tick, or no value.</param>
    /// <returns>True when the lead moved to a new tile on this tick.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    /// <remarks>
    /// A menu pauses the world, so the world step of a run calls this method only while no menu
    /// is open (D-162, D-650). The wanted direction lasts one tick alone, so a step
    /// never starts from an intent of an earlier tick.
    /// </remarks>
    public bool Advance(out TilePoint walked, out StepDirection? started)
    {
        walked = this.LeadAt;
        started = null;
        bool arrived = false;

        if (this.Stepping is StepDirection running)
        {
            this.StepTicks = checked(this.StepTicks + 1);
            if (this.StepTicks >= MapRules.TicksPerStep)
            {
                this.LeadAt = this.LeadAt.Step(running);
                this.Walked.Mark(this.LeadAt);
                this.Stepping = null;
                this.StepTicks = 0;
                walked = this.LeadAt;
                arrived = true;
            }
        }

        if (this.Stepping is null && this.wanted is StepDirection next)
        {
            // The lead turns whether or not it can move, so a push against a wall turns it
            // and the player reads the direction of the party from the sprite (D-207).
            this.Facing = next;
            if (MapRules.CanEnter(this.Map, this.LeadAt.Step(next)))
            {
                this.Stepping = next;
                this.StepTicks = 0;
                started = next;
            }
        }

        this.wanted = null;
        return arrived;
    }

    /// <summary>Adds every value of this state to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    /// <remarks>
    /// The hash holds the id of the map and never its content, because the content hash
    /// covers the map file (D-166, D-495, D-648).
    /// </remarks>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddText(this.Map.Id.Value);
        hasher.AddInt32(this.LeadAt.X);
        hasher.AddInt32(this.LeadAt.Y);
        hasher.AddInt32((int)this.Facing);
        hasher.AddBoolean(this.Stepping.HasValue);
        hasher.AddInt32(this.Stepping.HasValue ? (int)this.Stepping.Value : 0);
        hasher.AddInt32(this.StepTicks);
        this.Walked.Hash(hasher);
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The map state of {source} is not a state of a run: {reason} (T-2).", nameof(source));
        }
    }
}
