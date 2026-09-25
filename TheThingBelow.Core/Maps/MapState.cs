using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Maps;

/// <summary>What one world tick did to the party on the map (D-203, D-747).</summary>
/// <param name="Arrived">True when the lead reached a new tile on this tick.</param>
/// <param name="At">The tile of the lead at the end of the tick.</param>
/// <param name="Started">The direction of the step that started on this tick, or no value.</param>
/// <param name="Bumped">
/// The id of the enemy that the party stepped into, or no value. A step into a body starts
/// the encounter at once, with the party as the side that reached the other (D-747).
/// </param>
public readonly record struct PartyStep(bool Arrived, TilePoint At, StepDirection? Started, ContentId? Bumped);

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
        WalkedTiles walked,
        MapPatrols patrols,
        MapNpcs npcs)
    {
        this.Map = map;
        this.LeadAt = leadAt;
        this.Facing = facing;
        this.Stepping = stepping;
        this.StepTicks = stepTicks;
        this.Walked = walked;
        this.Patrols = patrols;
        this.Npcs = npcs;
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

    /// <summary>Every enemy of the map, the mark, and the encounter (D-738, D-749).</summary>
    public MapPatrols Patrols { get; }

    /// <summary>Every NPC of the map, which is solid and walks on the NPC stream (D-1137, D-1139).</summary>
    public MapNpcs Npcs { get; }

    /// <summary>Puts the party on a map at its spawn point (D-528).</summary>
    /// <param name="map">The map to enter.</param>
    /// <returns>The state, with the spawn tile walked.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static MapState Enter(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(map.Spawn);
        return new MapState(map, map.Spawn, StepDirection.South, null, 0, walked, MapPatrols.Enter(map), MapNpcs.Enter(map));
    }

    /// <summary>Puts the party back on a map from the values of a snapshot (D-166, D-651).</summary>
    /// <param name="map">The map of the snapshot, which the content set of this build holds.</param>
    /// <param name="leadAt">The tile of the lead.</param>
    /// <param name="facing">The direction that the lead faces.</param>
    /// <param name="stepping">The direction of the step that ran, or no value.</param>
    /// <param name="stepTicks">The count of ticks of that step.</param>
    /// <param name="walked">The walked tiles of the snapshot.</param>
    /// <param name="enemies">
    /// The stored values of each enemy that the map places, or no value on a snapshot of
    /// save format 2. With no value, each enemy starts on the start tile of its station
    /// (D-654, D-750).
    /// </param>
    /// <param name="mark">The mark of a sight of the snapshot, or no value (D-745).</param>
    /// <param name="encounter">The encounter of the snapshot, or no value (D-749).</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">The map, the record, or the source is null (T-2).</exception>
    /// <exception cref="ArgumentException">A value describes no state of a party on this map (T-2).</exception>
    /// <remarks>Each NPC of the map starts on its start tile, as on a snapshot before save format 15 (D-1137).</remarks>
    public static MapState Resume(
        GameMap map,
        TilePoint leadAt,
        StepDirection facing,
        StepDirection? stepping,
        int stepTicks,
        WalkedTiles walked,
        IReadOnlyList<PatrolValues>? enemies,
        SightMark? mark,
        MapEncounter? encounter,
        string source) =>
        Resume(map, new LeadValues(leadAt, facing, stepping, stepTicks), walked, enemies, mark, encounter, null, source, ResumeDrift.Of(SnapshotOrigin.ThisBuild, 0));

    /// <summary>
    /// Puts the party back on a map from the values of a snapshot that this build or another
    /// build wrote (D-166, D-651, D-1111).
    /// </summary>
    /// <param name="map">The map of the snapshot, which the content set of this build holds.</param>
    /// <param name="lead">The tile, the facing, and the step of the lead.</param>
    /// <param name="walked">The walked tiles of the snapshot.</param>
    /// <param name="enemies">The stored values of each enemy, or no value on a snapshot of save format 2 (D-654, D-750).</param>
    /// <param name="mark">The mark of a sight of the snapshot, or no value (D-745).</param>
    /// <param name="encounter">The encounter of the snapshot, or no value (D-749).</param>
    /// <param name="npcs">The stored values of each NPC, or no value on a snapshot before save format 15, whose NPCs start on their start tiles (D-1137).</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <param name="drift">The build of the snapshot, and the log of each change (D-1111).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">The map, the record, the source, or the drift is null (T-2).</exception>
    /// <exception cref="ArgumentException">A value describes no state of a party on this map (T-2).</exception>
    /// <remarks>
    /// A snapshot of another build can follow an edit of the map (D-1111). The walked tiles
    /// take the size of the map of this build. A lead off the map, on a tile that takes no
    /// step, on the body of an enemy, or on an NPC moves to the spawn point, and a step into a
    /// tile that takes no step or that an NPC holds ends. Each change logs a warning, and the
    /// strict checks then run.
    /// </remarks>
    public static MapState Resume(
        GameMap map,
        LeadValues lead,
        WalkedTiles walked,
        IReadOnlyList<PatrolValues>? enemies,
        SightMark? mark,
        MapEncounter? encounter,
        IReadOnlyList<NpcValues>? npcs,
        string source,
        ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(lead);
        ArgumentNullException.ThrowIfNull(walked);
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(drift);

        MapPatrols patrols = enemies is null
            ? MapPatrols.Enter(map)
            : MapPatrols.Resume(map, enemies, mark, encounter, source, drift);
        MapNpcs mapNpcs = MapNpcs.Resume(map, npcs, patrols, source, drift);
        if (drift.Adjusts)
        {
            walked = FitWalked(map, walked, drift);
            lead = FitLead(map, lead, walked, patrols, mapNpcs, drift);
        }

        TilePoint leadAt = lead.At;
        StepDirection? stepping = lead.Stepping;
        int stepTicks = lead.StepTicks;

        Refuse(!map.Holds(leadAt), source, $"the lead stands at {leadAt}, and the map '{map.Id.Value}' is {map.Width} by {map.Height} tiles");
        Refuse(
            !TileKinds.CanWalk(map.TileAt(leadAt)),
            source,
            $"the lead stands at {leadAt}, which is a {TileKinds.NameOf(map.TileAt(leadAt))} tile");
        Refuse(
            map.HoldsSolidThing(leadAt),
            source,
            $"the lead stands at {leadAt}, which holds a solid thing (D-1142)");
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

        // The body of an enemy blocks every step into it, so no state of a run holds the
        // party and an enemy on one tile (D-747, T-2).
        if (patrols.TryEnemyAt(leadAt, out PatrolState? found))
        {
            Refuse(
                true,
                source,
                $"the lead stands at {leadAt}, and the enemy '{found!.Patrol.Id.Value}' holds the body {found.Body} there");
        }

        // An NPC is solid, so no state of a run holds the lead and an NPC on one tile, or a step
        // of the lead into the tile of an NPC (D-1139, T-2).
        if (mapNpcs.TryNpcAt(leadAt, out NpcState? person) || (stepping is StepDirection walking && mapNpcs.TryNpcAt(leadAt.Step(walking), out person)))
        {
            Refuse(
                true,
                source,
                $"the lead stands at {leadAt} or steps from it, and the NPC '{person!.Npc.Id.Value}' at {person.At} holds that tile (D-1139)");
        }

        return new MapState(map, leadAt, lead.Facing, stepping, stepTicks, walked, patrols, mapNpcs);
    }

    /// <summary>Gives the walked tiles the size of the map of this build, and logs a change (D-1111).</summary>
    private static WalkedTiles FitWalked(GameMap map, WalkedTiles walked, ResumeDrift drift)
    {
        if (walked.Width == map.Width && walked.Height == map.Height)
        {
            return walked;
        }

        drift.Note(
            LogSubsystems.World,
            "the map of this build has another size than the walked tiles of the save, and the walked tiles take the new size",
            [new LogField("map", map.Id.Value), LogField.OfNumber("stored_width", walked.Width), LogField.OfNumber("stored_height", walked.Height), LogField.OfNumber("width", map.Width), LogField.OfNumber("height", map.Height)]);
        return walked.Resized(map.Width, map.Height);
    }

    /// <summary>
    /// Moves a lead that the map of this build no longer holds to the spawn point, and ends a
    /// step into a tile that takes no step. Each change logs a warning (D-1111).
    /// </summary>
    private static LeadValues FitLead(GameMap map, LeadValues lead, WalkedTiles walked, MapPatrols patrols, MapNpcs npcs, ResumeDrift drift)
    {
        string? reason = null;
        if (!map.Holds(lead.At))
        {
            reason = "the tile of the lead lies off the map of this build";
        }
        else if (!TileKinds.CanWalk(map.TileAt(lead.At)))
        {
            reason = "the tile of the lead takes no step in the map of this build";
        }
        else if (map.HoldsSolidThing(lead.At))
        {
            reason = "a solid thing of this build holds the tile of the lead";
        }
        else if (patrols.TryEnemyAt(lead.At, out _))
        {
            reason = "an enemy of this build holds the tile of the lead";
        }
        else if (npcs.TryNpcAt(lead.At, out _))
        {
            reason = "an NPC of this build holds the tile of the lead";
        }

        if (reason is not null)
        {
            drift.Note(
                LogSubsystems.World,
                $"{reason}, and the lead moves to the spawn point",
                [new LogField("map", map.Id.Value), new LogField("stored_tile", lead.At.ToString()), new LogField("tile", map.Spawn.ToString())]);
            walked.Mark(map.Spawn);
            return new LeadValues(map.Spawn, lead.Facing, null, 0);
        }

        if (lead.Stepping is StepDirection direction && (!MapRules.CanEnter(map, lead.At.Step(direction)) || npcs.TryNpcAt(lead.At.Step(direction), out _)))
        {
            drift.Note(
                LogSubsystems.World,
                "the step of the lead reaches a tile that takes no step or that an NPC holds in the map of this build, and the step ends",
                [new LogField("map", map.Id.Value), new LogField("tile", lead.At.ToString()), new LogField("direction", StepDirections.NameOf(direction))]);
            return lead with { Stepping = null, StepTicks = 0 };
        }

        return lead;
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
    /// <returns>What the tick did to the party (D-203, D-747).</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    /// <remarks>
    /// A menu pauses the world, so the world step of a run calls this method only while no menu
    /// is open (D-162, D-650). The wanted direction lasts one tick alone, so a step
    /// never starts from an intent of an earlier tick.
    /// <para>
    /// While an encounter runs, no map system ticks, so no step of the party runs either
    /// (D-531). The wanted direction still ends here, so no intent of an older tick starts a
    /// step when the encounter ends.
    /// </para>
    /// </remarks>
    public PartyStep Advance()
    {
        TilePoint walked = this.LeadAt;
        StepDirection? started = null;
        ContentId? bumped = null;
        bool arrived = false;

        if (this.Patrols.Encounter is not null)
        {
            this.wanted = null;
            return new PartyStep(false, this.LeadAt, null, null);
        }

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

        // A beat that ended waits for the lead to stand still, and the encounter starts on the
        // next tick, so the lead starts no new step here (D-1094).
        bool encounterDue = this.Patrols.Mark is SightMark { TicksLeft: 0 };
        if (this.Stepping is null && !encounterDue && this.wanted is StepDirection next)
        {
            // The lead turns whether or not it can move, so a push against a wall turns it
            // and the player reads the direction of the party from the sprite (D-207).
            this.Facing = next;
            TilePoint target = this.LeadAt.Step(next);
            if (this.Patrols.TryEnemyAt(target, out PatrolState? found))
            {
                // The body blocks the step, and the step starts the encounter at once. This
                // is the sneak of D-265 in the hands of the player (D-747). Inside the grace
                // time of a flee, the body still blocks the step, and no encounter starts
                // (D-381, D-1085).
                if (found!.GraceTicks == 0)
                {
                    bumped = found.Patrol.Id;
                }
            }
            // An NPC is solid, so a step into one turns the lead alone, with no bump and no
            // encounter (D-1139).
            else if (MapRules.CanEnter(this.Map, target) && !this.Npcs.TryNpcAt(target, out _))
            {
                this.Stepping = next;
                this.StepTicks = 0;
                started = next;
            }
        }

        this.wanted = null;
        return new PartyStep(arrived, walked, started, bumped);
    }

    /// <summary>
    /// Ends the wanted direction of this tick (D-493). The run calls it at the end of each tick,
    /// also a tick in which a menu, a battle, or a story scene held the world, so a move intent
    /// never starts a step on a later tick (T-7).
    /// </summary>
    /// <remarks>
    /// The snapshot and the state hash leave the wanted direction out, so a direction that lived
    /// past its tick made a resumed run differ from the live run with equal hashes (G-5).
    /// </remarks>
    internal void EndTick() => this.wanted = null;

    /// <summary>
    /// Ends the step that runs and the wanted step, so the lead stands still on its tile while a
    /// story scene runs (D-1009). Each NPC ends its step too, and it stands still on its tile
    /// (D-1139).
    /// </summary>
    /// <remarks>
    /// A tile trigger fires when the lead arrives, and the same tick can start the next step of
    /// the player. The story scene starts from the tile of the trigger, so that step ends here.
    /// </remarks>
    internal void HoldStill()
    {
        this.Stepping = null;
        this.StepTicks = 0;
        this.wanted = null;
        this.Npcs.HoldStill();
    }

    /// <summary>Moves the lead one tile in a move step of a story scene, and marks the tile walked (D-567, D-1012).</summary>
    /// <param name="direction">The direction of the tile, which the lead then faces.</param>
    internal void WalkInScene(StepDirection direction)
    {
        this.Facing = direction;
        this.LeadAt = this.LeadAt.Step(direction);
        this.Walked.Mark(this.LeadAt);
    }

    /// <summary>Turns the lead in a face step of a story scene (D-1000).</summary>
    /// <param name="direction">The direction.</param>
    internal void FaceInScene(StepDirection direction) => this.Facing = direction;

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
        this.Patrols.Hash(hasher);
        this.Npcs.Hash(hasher);
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The map state of {source} is not a state of a run: {reason} (T-2).", nameof(source));
        }
    }
}

/// <summary>The tile, the facing, and the step of the lead in a snapshot (D-166, D-203).</summary>
/// <param name="At">The tile of the lead.</param>
/// <param name="Facing">The direction that the lead faces.</param>
/// <param name="Stepping">The direction of the step that ran, or no value.</param>
/// <param name="StepTicks">The count of ticks of that step.</param>
public sealed record LeadValues(TilePoint At, StepDirection Facing, StepDirection? Stepping, int StepTicks);
