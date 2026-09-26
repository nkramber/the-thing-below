using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Maps;

/// <summary>Every NPC of one map, and the rules of its walk (D-1137, D-1138, D-1139).</summary>
/// <remarks>
/// The map runs in real time, so each NPC walks on the world tick whether or not the player
/// moves (D-162). A menu, a battle, an encounter, and a story scene each hold the world, so no
/// NPC walks then (D-531, D-1009, D-1139).
/// <para>
/// An NPC is solid (D-1139). No NPC steps onto the lead, a wall, a thing, another NPC, or the
/// body of an enemy, and a blocked step becomes a pause. The lead never walks through an NPC,
/// and an enemy never walks through one either.
/// </para>
/// <para>
/// A wander NPC draws once from the NPC stream on each pace tick, and no other move draws
/// (D-1137, G-4). Thus the count of draws of one tick follows the count of wander NPCs whose
/// pace ends, and never the shape of a range or a blocked step (T-7).
/// </para>
/// <para>
/// An NPC that a story scene moved out of its home walks home in place of its move, on the
/// shortest path of <see cref="NpcPaths"/>, and it takes no draw (D-1140).
/// </para>
/// </remarks>
public sealed class MapNpcs
{
    /// <summary>
    /// The count of values of one draw of a wander NPC: the four directions of
    /// <see cref="StepDirections.All"/>, and a pause (D-1138).
    /// </summary>
    public const int WanderChoices = 5;

    // The order of this list is the order of the map file, so every machine walks the NPCs in
    // one order and the draws of the NPC stream never differ (G-4, T-7).
    private readonly NpcState[] npcs;

    private MapNpcs(NpcState[] npcs)
    {
        this.npcs = npcs;
    }

    /// <summary>Every NPC that the map places, in the order of the file (G-4).</summary>
    public IReadOnlyList<NpcState> All => this.npcs;

    /// <summary>Puts every NPC of one map on its start tile (D-1138).</summary>
    /// <param name="map">The map that the party enters.</param>
    /// <returns>The NPCs.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static MapNpcs Enter(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        NpcState[] entered = new NpcState[map.Npcs.Count];
        for (int index = 0; index < entered.Length; index += 1)
        {
            entered[index] = NpcState.Enter(map.Npcs[index]);
        }

        return new MapNpcs(entered);
    }

    /// <summary>
    /// Puts every NPC back from the values of a snapshot that this build or another build wrote
    /// (D-166, D-259, D-1111).
    /// </summary>
    /// <param name="map">The map of the snapshot, from the content of this build.</param>
    /// <param name="values">
    /// The stored values, one for each NPC that the map placed, or no value on a snapshot before
    /// save format 15. With no value, each NPC starts on its start tile (D-1137).
    /// </param>
    /// <param name="patrols">The enemies of the map, which no NPC can stand on (D-1139).</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <param name="drift">The build of the snapshot, and the log of each change (D-1111).</param>
    /// <returns>The NPCs.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of this map (T-2).</exception>
    /// <remarks>
    /// A snapshot of this build holds one value for each NPC of the map, in the order of the map
    /// file, and a list of another shape fails with the map and the count (T-2). A snapshot of
    /// another build matches each NPC by its id instead, with the rules of an enemy (D-1111). An
    /// NPC that the save lacks starts on its start tile. An NPC that the map no longer places
    /// leaves. An NPC whose stored place its edited record no longer takes starts on its start
    /// tile again. Each change logs a warning.
    /// <para>
    /// Two NPCs on one tile, and an NPC on the body of an enemy, refuse the snapshot in both
    /// cases, because no rule moves a body off another one (D-1139, T-2).
    /// </para>
    /// </remarks>
    public static MapNpcs Resume(
        GameMap map,
        IReadOnlyList<NpcValues>? values,
        MapPatrols patrols,
        string source,
        ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(patrols);
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(drift);

        MapNpcs built;
        if (values is null)
        {
            built = Enter(map);
        }
        else if (drift.Adjusts)
        {
            built = ResumeById(map, values, source, drift);
        }
        else
        {
            built = ResumeInOrder(map, values, source);
        }

        built.CheckBodies(patrols, source);
        return built;
    }

    /// <summary>Finds the NPC that holds one tile now, or that steps into it on this tick (D-1139).</summary>
    /// <param name="at">The tile.</param>
    /// <param name="found">The NPC, or null when no NPC reaches that tile.</param>
    /// <returns>True when an NPC holds that tile or steps into it.</returns>
    /// <remarks>A talk reads the tile of an NPC at its tick, so the end of a step counts (D-1139).</remarks>
    public bool TryNpcAt(TilePoint at, out NpcState? found)
    {
        foreach (NpcState npc in this.npcs)
        {
            if (npc.Holds(at))
            {
                found = npc;
                return true;
            }
        }

        found = null;
        return false;
    }

    /// <summary>Finds the NPC whose tile is one tile now, with no regard to the end of its step (D-1139).</summary>
    /// <param name="at">The tile.</param>
    /// <param name="found">The NPC, or null when no NPC stands on that tile.</param>
    /// <returns>True when an NPC stands on that tile.</returns>
    /// <remarks>
    /// A talk reads the tile of an NPC at its tick, and Core holds an NPC on its tile through its
    /// step. Thus an NPC that steps into the faced tile is not there yet, and an NPC that steps out
    /// of it is still there (D-203, D-1139).
    /// </remarks>
    public bool TryNpcStandingOn(TilePoint at, out NpcState? found)
    {
        foreach (NpcState npc in this.npcs)
        {
            if (npc.At == at)
            {
                found = npc;
                return true;
            }
        }

        found = null;
        return false;
    }

    /// <summary>Finds one NPC of the map by its id, for a step of a story scene that names it (D-1006).</summary>
    /// <param name="id">The id of the NPC.</param>
    /// <param name="found">The NPC, or null when the map places no NPC with that id.</param>
    /// <returns>True when the map places the NPC.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public bool TryFind(ContentId id, out NpcState? found)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (NpcState npc in this.npcs)
        {
            if (string.CompareOrdinal(npc.Npc.Id.Value, id.Value) == 0)
            {
                found = npc;
                return true;
            }
        }

        found = null;
        return false;
    }

    /// <summary>Tells whether an NPC holds a tile of one body of an enemy, or steps into one (D-1139).</summary>
    /// <param name="body">The body.</param>
    /// <returns>True when a tile of the body holds an NPC or the end of its step.</returns>
    public bool Blocks(EnemyBody body)
    {
        foreach (NpcState npc in this.npcs)
        {
            if (body.Holds(npc.At) || (npc.StepEnd is TilePoint end && body.Holds(end)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Runs every NPC of the map for one world tick (D-162, D-1138).</summary>
    /// <param name="map">The map that the NPCs stand on.</param>
    /// <param name="party">The party, whose lead blocks a step of an NPC, and whose map state holds the enemies (D-1139).</param>
    /// <param name="stream">The NPC stream, which a wander NPC draws from (D-1137, G-4).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <param name="moved">Takes each NPC that reached a new tile on this tick, for the log.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The stream is not the NPC stream (G-4, T-2).</exception>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    /// <exception cref="SimulationException">An NPC that walks home finds no path home (D-1140, T-2).</exception>
    /// <remarks>
    /// Each NPC counts its wait and its step, and it chooses only while it stands and its wait
    /// is zero. An NPC outside its home takes the first step of its shortest path home, and a
    /// blocked step waits one step and searches again (D-1140). A wander NPC takes one draw of <see cref="WanderChoices"/> values: a direction or
    /// a pause. A step that leaves its range or that a body blocks becomes a pause. A route NPC
    /// steps toward its route tile, and a blocked step tries again on the next tick. A chaser
    /// takes the clear step inside its range that brings it closest to its target, the first in
    /// the order of <see cref="StepDirections.All"/> on a tie, and it pauses when no step brings
    /// it closer. A wander NPC and a chaser then wait one pace (D-1138, D-1139).
    /// </remarks>
    public void Walk(
        GameMap map,
        MapState party,
        RandomStream stream,
        RunContext context,
        List<NpcState> moved)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(moved);
        if (stream.Stream != StreamId.Npc)
        {
            throw new ArgumentException(
                $"The NPCs of the map '{map.Id.Value}' draw from the stream '{StreamId.Npc}', and the caller gave the stream '{stream.Stream}' (D-1137, G-4).",
                nameof(stream));
        }

        foreach (NpcState npc in this.npcs)
        {
            npc.CountWait();
            if (npc.CountStep())
            {
                moved.Add(npc);
            }

            if (npc.WaitTicks > 0 || npc.Stepping is not null)
            {
                continue;
            }

            if (npc.WalksHome)
            {
                this.WalkHome(map, party, npc, context);
                continue;
            }

            switch (npc.Npc.Move)
            {
                case NpcMove.Wander:
                    this.Wander(map, party, npc, stream, context);
                    break;
                case NpcMove.Route:
                    this.WalkRoute(map, party, npc);
                    break;
                case NpcMove.Chase:
                    this.Chase(map, party, npc);
                    break;
                default:
                    throw new SimulationException(
                        $"the NPC '{npc.Npc.Id.Value}' takes the move {npc.Npc.Move}, which no walk rule reads (D-1138)",
                        context);
            }
        }
    }

    /// <summary>
    /// Ends the step of each NPC, so each one stands still on its tile while a story scene runs
    /// (D-1139).
    /// </summary>
    public void HoldStill()
    {
        foreach (NpcState npc in this.npcs)
        {
            npc.EndStep();
        }
    }

    /// <summary>Gives the stored values of every NPC, in the order of the map file (D-259).</summary>
    /// <returns>One value for each NPC that the map places.</returns>
    public IReadOnlyList<NpcValues> Values()
    {
        List<NpcValues> values = [];
        foreach (NpcState npc in this.npcs)
        {
            values.Add(npc.Values());
        }

        return values;
    }

    /// <summary>Adds every value of the NPCs to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.npcs.Length);
        foreach (NpcState npc in this.npcs)
        {
            npc.Hash(hasher);
        }
    }

    /// <summary>Gives the count of steps between two tiles on a walk with no diagonal (D-716).</summary>
    private static int DistanceOf(TilePoint from, TilePoint to) =>
        checked(Math.Abs(checked(from.X - to.X)) + Math.Abs(checked(from.Y - to.Y)));

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The NPCs of {source} are not a state of a run: {reason} (T-2).", nameof(source));
        }
    }

    private static MapNpcs ResumeInOrder(GameMap map, IReadOnlyList<NpcValues> values, string source)
    {
        IReadOnlyList<Npc> placed = map.Npcs;
        Refuse(
            values.Count != placed.Count,
            source,
            $"it holds {values.Count} NPCs, and the map '{map.Id.Value}' places {placed.Count}");

        NpcState[] states = new NpcState[placed.Count];
        for (int index = 0; index < placed.Count; index += 1)
        {
            NpcValues stored = values[index];
            ArgumentNullException.ThrowIfNull(stored);
            Refuse(
                string.CompareOrdinal(stored.Npc.Value, placed[index].Id.Value) != 0,
                source,
                $"the NPC at position {index} is '{stored.Npc.Value}', and the map '{map.Id.Value}' places '{placed[index].Id.Value}' there");
            states[index] = NpcState.Resume(placed[index], map, stored, source);
        }

        return new MapNpcs(states);
    }

    /// <summary>Matches each stored NPC to the map of this build by its id, and logs each change (D-1111).</summary>
    private static MapNpcs ResumeById(GameMap map, IReadOnlyList<NpcValues> values, string source, ResumeDrift drift)
    {
        string mapId = map.Id.Value;
        for (int index = 0; index < values.Count; index += 1)
        {
            ArgumentNullException.ThrowIfNull(values[index]);
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                Refuse(
                    string.CompareOrdinal(values[earlier].Npc.Value, values[index].Npc.Value) == 0,
                    source,
                    $"it holds the NPC '{values[index].Npc.Value}' two times");
            }
        }

        IReadOnlyList<Npc> placed = map.Npcs;
        NpcState[] states = new NpcState[placed.Count];
        for (int index = 0; index < placed.Count; index += 1)
        {
            Npc npc = placed[index];
            NpcValues? stored = StoredOf(values, npc.Id.Value);
            if (stored is null)
            {
                drift.Note(LogSubsystems.World, "the map of this build places an NPC that the save lacks, and it starts on its start tile", [new LogField("npc", npc.Id.Value), new LogField("map", mapId)]);
                states[index] = NpcState.Enter(npc);
            }
            else if (NpcState.MisfitOf(npc, map, stored) is string misfit)
            {
                drift.Note(LogSubsystems.World, "the record of this build takes no stored place of the NPC, and it starts on its start tile again", [new LogField("npc", npc.Id.Value), new LogField("map", mapId), new LogField("reason", misfit)]);
                states[index] = NpcState.Enter(npc);
            }
            else
            {
                states[index] = NpcState.Resume(npc, map, stored, source);
            }
        }

        foreach (NpcValues stored in values)
        {
            if (!map.PlacesNpc(stored.Npc))
            {
                drift.Note(LogSubsystems.World, "the map of this build no longer places an NPC of the save, and the NPC leaves", [new LogField("npc", stored.Npc.Value), new LogField("map", mapId)]);
            }
        }

        return new MapNpcs(states);
    }

    private static NpcValues? StoredOf(IReadOnlyList<NpcValues> values, string id)
    {
        foreach (NpcValues stored in values)
        {
            if (string.CompareOrdinal(stored.Npc.Value, id) == 0)
            {
                return stored;
            }
        }

        return null;
    }

    /// <summary>
    /// Refuses two NPCs on one tile, and an NPC on the body of a live enemy. A step that runs
    /// counts as the tile of its end too (D-1139, T-2).
    /// </summary>
    private void CheckBodies(MapPatrols patrols, string source)
    {
        for (int index = 0; index < this.npcs.Length; index += 1)
        {
            NpcState npc = this.npcs[index];
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                NpcState other = this.npcs[earlier];
                bool shared = other.Holds(npc.At) || (npc.StepEnd is TilePoint end && other.Holds(end));
                Refuse(
                    shared,
                    source,
                    $"the NPC '{npc.Npc.Id.Value}' at {npc.At} and the NPC '{other.Npc.Id.Value}' at {other.At} hold one tile (D-1139)");
            }

            bool onEnemy = patrols.TryEnemyAt(npc.At, out PatrolState? enemy)
                || (npc.StepEnd is TilePoint stepEnd && patrols.TryEnemyAt(stepEnd, out enemy));
            Refuse(
                onEnemy,
                source,
                $"the NPC '{npc.Npc.Id.Value}' at {npc.At} shares a tile with the enemy '{enemy?.Patrol.Id.Value}' (D-1139)");
        }
    }

    /// <summary>
    /// Takes one choice of a wander NPC: one draw of a direction or a pause (D-1138). A step
    /// that leaves the range or that a body blocks becomes a pause (D-1139).
    /// </summary>
    private void Wander(GameMap map, MapState party, NpcState npc, RandomStream stream, RunContext context)
    {
        int draw = stream.NextInt(WanderChoices, context);
        npc.WaitPace();
        if (draw >= StepDirections.All.Length)
        {
            return;
        }

        StepDirection direction = StepDirections.All[draw];

        // The NPC faces the way that it wants to go, whether or not the step runs, as an enemy
        // does (D-207).
        npc.Turn(direction);
        if (this.Clear(map, party, npc, direction))
        {
            npc.Begin(direction);
        }
    }

    /// <summary>
    /// Takes the first step of the shortest path home of an NPC outside its home. The search
    /// ignores each body that moves, so a body on the path blocks the step, and the NPC waits one
    /// step and searches again (D-1140).
    /// </summary>
    private void WalkHome(GameMap map, MapState party, NpcState npc, RunContext context)
    {
        if (!NpcPaths.TryFirstStepHome(map, npc.Npc, npc.At, out StepDirection first, out _))
        {
            throw new SimulationException(
                $"the NPC '{npc.Npc.Id.Value}' walks home from {npc.At}, and no path of open ground leads to its home. A story scene moved it there (D-1140)",
                context);
        }

        npc.Turn(first);
        if (this.Clear(map, party, npc, first))
        {
            npc.Begin(first);
            return;
        }

        npc.WaitBlocked();
    }

    /// <summary>Steps a route NPC toward its route tile. A blocked step tries again on the next tick (D-739, D-1139).</summary>
    private void WalkRoute(GameMap map, MapState party, NpcState npc)
    {
        if (npc.NextOnRoute() is not StepDirection next)
        {
            return;
        }

        npc.Turn(next);
        if (this.Clear(map, party, npc, next))
        {
            npc.Begin(next);
        }
    }

    /// <summary>
    /// Takes one choice of a chaser: the clear step that brings it closest to the tile of its
    /// target, the first in the order of <see cref="StepDirections.All"/> on a tie, or a pause
    /// when no clear step brings it closer (D-1138). A chaser takes no draw.
    /// </summary>
    private void Chase(GameMap map, MapState party, NpcState npc)
    {
        npc.WaitPace();
        ContentId target = npc.Npc.Target
            ?? throw new InvalidOperationException($"The chaser '{npc.Npc.Id.Value}' names no target, and the reader of a map requires one (D-1138, T-2).");
        TilePoint goal = this.Find(target).At;

        int best = DistanceOf(npc.At, goal);
        StepDirection? chosen = null;
        foreach (StepDirection direction in StepDirections.All)
        {
            if (!this.Clear(map, party, npc, direction))
            {
                continue;
            }

            int distance = DistanceOf(npc.At.Step(direction), goal);
            if (distance < best)
            {
                best = distance;
                chosen = direction;
            }
        }

        if (chosen is StepDirection step)
        {
            npc.Begin(step);
        }
    }

    /// <summary>
    /// Tells whether one NPC can step in one direction (D-1139). The ground, a thing, the range of
    /// a wander NPC or a chaser, the lead, each other NPC, and the body of each live enemy block
    /// the step. The end of a step that runs blocks it too. The range blocks no step of the walk
    /// home, which starts outside it (D-1140).
    /// </summary>
    private bool Clear(GameMap map, MapState party, NpcState npc, StepDirection direction)
    {
        TilePoint next = npc.At.Step(direction);
        if (!NpcState.IsOpen(map, next))
        {
            return false;
        }

        if (npc.Npc.Move != NpcMove.Route && !npc.WalksHome && !npc.Npc.RangeHolds(next))
        {
            return false;
        }

        // The party stands on a whole tile through its step, and it reaches the next tile on the
        // last tick of that step. Thus the tile of its end blocks an NPC too (D-203).
        if (next == party.LeadAt || (party.Stepping is StepDirection walking && next == party.LeadAt.Step(walking)))
        {
            return false;
        }

        foreach (NpcState other in this.npcs)
        {
            if (!ReferenceEquals(other, npc) && other.Holds(next))
            {
                return false;
            }
        }

        return !party.Patrols.TryEnemyAt(next, out _);
    }

    private NpcState Find(ContentId id)
    {
        foreach (NpcState npc in this.npcs)
        {
            if (string.CompareOrdinal(npc.Npc.Id.Value, id.Value) == 0)
            {
                return npc;
            }
        }

        throw new InvalidOperationException(
            $"This map places no NPC with the id '{id.Value}', and a chaser names it. The load of a map proves each target (D-1138, T-2).");
    }
}
