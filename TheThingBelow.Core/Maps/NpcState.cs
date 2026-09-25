using System;
using System.Collections.Generic;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// One NPC on the map as a run holds it: its tile, its facing, the step that runs, the leg of
/// its route, and the ticks that it waits (D-1137, D-1138).
/// </summary>
/// <remarks>
/// Core keeps the NPC on a whole tile through the whole step, as it does for the party and an
/// enemy, and Game slides the sprite across the ticks of the step (D-203). A step takes the
/// count of ticks that the record gives (D-821).
/// <para>
/// <see cref="MapNpcs"/> holds the walk rule and the blocking rule. This class holds the values
/// of one NPC and the smallest changes to them (D-168).
/// </para>
/// <para>
/// The wait counts down on each world tick, also during a step. A wander NPC and a chaser set it
/// to the pace after each choice, so one choice falls on each pace tick (D-1138). A route NPC sets
/// it to the wait of a route tile when it arrives there, and it starts the next leg when the wait
/// ends. A route NPC also waits at its first tile when it enters the map, as if it arrived there.
/// </para>
/// <para>
/// The route values hold the leg of the route, and they stay at zero and at true for a wander
/// NPC and a chaser, as for an enemy of an area (D-741).
/// </para>
/// <para>
/// A story scene can move an NPC out of its home (D-1006). The NPC then walks home after the
/// story scene, one step at a time on a shortest path, and it moves in its normal way when it
/// arrives (D-1140). A route NPC that arrives on its route rejoins it.
/// </para>
/// </remarks>
public sealed class NpcState
{
    private NpcState(
        Npc npc,
        TilePoint at,
        StepDirection facing,
        StepDirection? stepping,
        int stepTicks,
        int target,
        bool forward,
        int waitTicks,
        bool walksHome)
    {
        this.Npc = npc;
        this.At = at;
        this.Facing = facing;
        this.Stepping = stepping;
        this.StepTicks = stepTicks;
        this.Target = target;
        this.Forward = forward;
        this.WaitTicks = waitTicks;
        this.WalksHome = walksHome;
    }

    /// <summary>The record of this NPC, as the map file holds it (D-1137).</summary>
    public Npc Npc { get; }

    /// <summary>The tile of this NPC, which is always a whole tile (D-203).</summary>
    public TilePoint At { get; private set; }

    /// <summary>The direction that this NPC faces, which its sprite draws (D-207).</summary>
    public StepDirection Facing { get; private set; }

    /// <summary>The direction of the step that runs now, or no value while the NPC stands.</summary>
    public StepDirection? Stepping { get; private set; }

    /// <summary>
    /// The count of ticks of the step that runs, from 0 to the step of the record minus one
    /// (D-821). The value is zero while the NPC stands.
    /// </summary>
    public int StepTicks { get; private set; }

    /// <summary>
    /// The index of the route tile that a route NPC walks toward (D-739). The value is zero for
    /// a wander NPC and a chaser.
    /// </summary>
    public int Target { get; private set; }

    /// <summary>
    /// True while a route NPC walks up the list of route tiles, and false while it walks back
    /// down the list (D-739). The value is true for a wander NPC and a chaser.
    /// </summary>
    public bool Forward { get; private set; }

    /// <summary>
    /// The count of world ticks before the next choice of a wander NPC or a chaser, or before the
    /// next leg of a route NPC (D-1138). The NPC starts no step while the count is above zero.
    /// </summary>
    public int WaitTicks { get; private set; }

    /// <summary>
    /// True while this NPC walks back to its home after a story scene moved it out (D-1140). The
    /// NPC stands outside its home then, and it takes no wander, route, or chase step.
    /// </summary>
    public bool WalksHome { get; private set; }

    /// <summary>The tile where the step that runs ends, or no value while the NPC stands (D-203).</summary>
    public TilePoint? StepEnd => this.Stepping is StepDirection running ? this.At.Step(running) : null;

    /// <summary>True while this NPC stands still for good, because its route holds one tile (D-740).</summary>
    public bool Stands => this.Npc.Move == NpcMove.Route && this.Npc.Route.Count == 1;

    /// <summary>Puts one NPC on its start tile when the party enters its map (D-1138).</summary>
    /// <param name="npc">The record of the NPC.</param>
    /// <returns>The state, at the start tile. A route NPC waits the ticks of its first tile.</returns>
    /// <exception cref="ArgumentNullException">The record is null (T-2).</exception>
    public static NpcState Enter(Npc npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        bool route = npc.Move == NpcMove.Route;
        int target = route && npc.Route.Count > 1 ? 1 : 0;
        int wait = route ? npc.Route[0].WaitTicks : 0;
        return new NpcState(npc, npc.Start, npc.Facing, null, 0, target, true, wait, false);
    }

    /// <summary>Puts one NPC back from the values of a snapshot (D-166, D-259).</summary>
    /// <param name="npc">The record of the NPC, from the map of this build.</param>
    /// <param name="map">The map, for the check of the tile and of the step (T-2).</param>
    /// <param name="values">The values of the snapshot.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">A value describes no state of this NPC (T-2).</exception>
    /// <remarks>
    /// The check reads this NPC alone. <see cref="MapNpcs"/> checks the NPCs against each other,
    /// against the enemies, and the map state checks them against the lead.
    /// </remarks>
    public static NpcState Resume(Npc npc, GameMap map, NpcValues values, string source)
    {
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (MisfitOf(npc, map, values) is string misfit)
        {
            throw new ArgumentException($"The NPC state of {source} is not a state of a run: {misfit} (T-2).", nameof(source));
        }

        return new NpcState(
            npc,
            new TilePoint(values.X, values.Y),
            values.Facing,
            values.Stepping,
            values.StepTicks,
            values.Target,
            values.Forward,
            values.WaitTicks,
            values.WalksHome);
    }

    /// <summary>
    /// Gives the first reason why stored values describe no state of this NPC on this map, or
    /// no value when they fit (T-2, D-1111).
    /// </summary>
    /// <param name="npc">The record of the NPC, from the map of this build.</param>
    /// <param name="map">The map, for the check of the tile and of the step.</param>
    /// <param name="values">The values of the snapshot.</param>
    /// <returns>The reason, or null.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static string? MisfitOf(Npc npc, GameMap map, NpcValues values)
    {
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(values);

        var at = new TilePoint(values.X, values.Y);
        string name = npc.Id.Value;

        if (!IsOpen(map, at))
        {
            return $"the NPC '{name}' stands at {at}, which takes no step or holds a thing on the map '{map.Id.Value}' (D-1139)";
        }

        if (values.Stepping is null && values.StepTicks != 0)
        {
            return $"the NPC '{name}' stands on no step, and its step ticks are {values.StepTicks}";
        }

        if (values.Stepping is not null && (values.StepTicks < 0 || values.StepTicks >= npc.StepTicks))
        {
            return $"the NPC '{name}' holds {values.StepTicks} step ticks, and the range of its step is 0 to {npc.StepTicks - 1}";
        }

        // A blocked step of the walk home waits one step (D-1140).
        int mostWait = values.WalksHome ? Math.Max(MostWaitOf(npc), npc.StepTicks) : MostWaitOf(npc);
        if (values.WaitTicks < 0 || values.WaitTicks > mostWait)
        {
            return $"the NPC '{name}' waits {values.WaitTicks} ticks, and the range of its wait is 0 to {mostWait} (D-1138)";
        }

        string? place;
        if (values.WalksHome)
        {
            place = HomeMisfitOf(npc, values, at, name);
        }
        else if (npc.Move == NpcMove.Route)
        {
            place = RouteMisfitOf(npc, values, at, name);
        }
        else
        {
            place = RangeMisfitOf(npc, values, at, name);
        }

        if (place is not null)
        {
            return place;
        }

        // The walk home leaves the range on its way, so a step of it reads the ground alone (D-1140).
        if (values.Stepping is StepDirection stepping)
        {
            TilePoint end = at.Step(stepping);
            if (!IsOpen(map, end) || (npc.Move != NpcMove.Route && !values.WalksHome && !npc.RangeHolds(end)))
            {
                return $"the NPC '{name}' steps {StepDirections.NameOf(stepping)} to {end}, which it never walks onto (D-1138, D-1139)";
            }
        }

        return null;
    }

    /// <summary>
    /// Tells whether an NPC can stand on one tile of a map: the tile takes a step and holds no
    /// thing at all (D-1139). The load proves each tile of a range and of a route with the same rule.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="at">The tile, which can lie outside the map.</param>
    /// <returns>True when the NPC can stand there.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static bool IsOpen(GameMap map, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);

        return MapRules.CanEnter(map, at) && map.ThingsAt(at).Count == 0;
    }

    /// <summary>Tells whether this NPC holds one tile now, or steps into it (D-1139).</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when the tile is the tile of the NPC or the end of its step.</returns>
    /// <remarks>
    /// A step that runs counts as the tile of its end too, as for an enemy. Without that rule,
    /// two bodies can start a step into one tile on two ticks and then hold one tile (T-2).
    /// </remarks>
    public bool Holds(TilePoint at) => this.At == at || this.StepEnd == at;

    /// <summary>Counts one world tick of the wait (D-1138). The count stops at zero.</summary>
    public void CountWait()
    {
        if (this.WaitTicks > 0)
        {
            this.WaitTicks -= 1;
        }
    }

    /// <summary>
    /// Counts one tick of the step that runs, and moves the NPC when the step ends (D-821). A
    /// route NPC that reaches its route tile starts the wait of that tile and takes the next leg
    /// (D-739, D-1138).
    /// </summary>
    /// <returns>True when the NPC reached a new tile on this tick.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public bool CountStep()
    {
        if (this.Stepping is not StepDirection running)
        {
            return false;
        }

        this.StepTicks = checked(this.StepTicks + 1);
        if (this.StepTicks < this.Npc.StepTicks)
        {
            return false;
        }

        this.At = this.At.Step(running);
        this.Stepping = null;
        this.StepTicks = 0;
        if (this.WalksHome)
        {
            if (this.Npc.HomeHolds(this.At))
            {
                this.ArriveHome();
            }

            return true;
        }

        if (this.Npc.Move == NpcMove.Route && this.At == this.Npc.Route[this.Target].At)
        {
            this.WaitTicks = this.Npc.Route[this.Target].WaitTicks;
            this.TakeNextLeg();
        }

        return true;
    }

    /// <summary>Starts the wait of one pace after a choice of a wander NPC or a chaser (D-1138).</summary>
    /// <exception cref="InvalidOperationException">This NPC walks a route, which has no pace (T-2).</exception>
    public void WaitPace()
    {
        this.WaitTicks = this.Npc.PaceTicks
            ?? throw new InvalidOperationException(
                $"The NPC '{this.Npc.Id.Value}' walks a route, and a route waits at its tiles in place of a pace (D-1138, T-2).");
    }

    /// <summary>Waits one step after a step of the walk home that a body blocked, and then the NPC searches again (D-1140).</summary>
    public void WaitBlocked() => this.WaitTicks = this.Npc.StepTicks;

    /// <summary>
    /// Ends the step that runs, and turns this NPC to the lead that talks with it (D-1139). The NPC
    /// stays on <see cref="At"/>, the tile that a talk reads.
    /// </summary>
    /// <param name="toward">The direction from this NPC to the lead.</param>
    public void FaceTalker(StepDirection toward)
    {
        this.EndStep();
        this.Turn(toward);
    }

    /// <summary>Moves this NPC one tile in a move step of a story scene, with no step to draw (D-1006, D-1012).</summary>
    /// <param name="at">The tile, which the story scene proved is free.</param>
    /// <param name="facing">The direction of the step, which the NPC then faces.</param>
    /// <remarks>The story scene calls <see cref="SettleAfterScene"/> when the path of the move ends.</remarks>
    public void MoveInScene(TilePoint at, StepDirection facing)
    {
        this.EndStep();
        this.At = at;
        this.Facing = facing;
    }

    /// <summary>
    /// Settles this NPC at the end of the path of a move step (D-1140). Outside its home, it walks
    /// home after the story scene. Inside its home, a route NPC rejoins its route. The wait ends in
    /// both cases, so the NPC moves on the first world tick after the story scene.
    /// </summary>
    public void SettleAfterScene()
    {
        this.WaitTicks = 0;
        if (this.Npc.HomeHolds(this.At))
        {
            this.ArriveHome();
            return;
        }

        this.WalksHome = true;
    }

    /// <summary>Starts a step of this NPC in one direction (D-821).</summary>
    /// <param name="direction">The direction of the step, which the caller proved is clear (D-1139).</param>
    public void Begin(StepDirection direction)
    {
        this.Facing = direction;
        this.Stepping = direction;
        this.StepTicks = 0;
    }

    /// <summary>
    /// Turns this NPC to one direction with no step (D-207). A blocked NPC faces the way that it
    /// wants to go, as a blocked enemy does.
    /// </summary>
    /// <param name="direction">The direction that the NPC faces from this tick.</param>
    public void Turn(StepDirection direction) => this.Facing = direction;

    /// <summary>
    /// Ends the step that runs, so the NPC stands still on its tile (D-1139). The NPC stays on
    /// <see cref="At"/>, the whole tile that Core held through the step, as the lead does when a
    /// story scene starts (D-1009). The wait and the route leg stay.
    /// </summary>
    public void EndStep()
    {
        this.Stepping = null;
        this.StepTicks = 0;
    }

    /// <summary>Gives the direction of the next step of a route, or no value for a standing NPC.</summary>
    /// <returns>The direction toward the route tile of <see cref="Target"/>.</returns>
    /// <exception cref="InvalidOperationException">This NPC walks a range, not a route (T-2).</exception>
    /// <remarks>
    /// The load of the map proved that each leg is straight and on one axis, so this call always
    /// finds a direction while the route holds more than one tile (D-739).
    /// </remarks>
    public StepDirection? NextOnRoute()
    {
        if (this.Npc.Move != NpcMove.Route)
        {
            throw new InvalidOperationException(
                $"The NPC '{this.Npc.Id.Value}' takes the move '{NpcMoves.NameOf(this.Npc.Move)}', which holds no route leg (D-1138, T-2).");
        }

        if (this.Stands)
        {
            return null;
        }

        TilePoint goal = this.Npc.Route[this.Target].At;
        if (!PatrolStation.TryLeg(this.At, goal, out StepDirection direction, out _))
        {
            throw new InvalidOperationException(
                $"The NPC '{this.Npc.Id.Value}' stands at {this.At}, and the route tile {this.Target} is {goal}. The load of a map proves that each leg is straight (D-739, T-2).");
        }

        return direction;
    }

    /// <summary>Gives the stored values of this NPC (D-259).</summary>
    /// <returns>The values, which a snapshot holds.</returns>
    public NpcValues Values() =>
        new(this.Npc.Id, this.At.X, this.At.Y, this.Facing, this.Stepping, this.StepTicks, this.Target, this.Forward, this.WaitTicks, this.WalksHome);

    /// <summary>Adds every value of this NPC to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddText(this.Npc.Id.Value);
        hasher.AddInt32(this.At.X);
        hasher.AddInt32(this.At.Y);
        hasher.AddInt32((int)this.Facing);
        hasher.AddBoolean(this.Stepping.HasValue);
        hasher.AddInt32(this.Stepping.HasValue ? (int)this.Stepping.Value : 0);
        hasher.AddInt32(this.StepTicks);
        hasher.AddInt32(this.Target);
        hasher.AddBoolean(this.Forward);
        hasher.AddInt32(this.WaitTicks);
        hasher.AddBoolean(this.WalksHome);
    }

    /// <summary>
    /// Gives the longest wait of one NPC: the pace of a wander NPC or a chaser, or the longest
    /// wait of a route tile (D-1138).
    /// </summary>
    private static int MostWaitOf(Npc npc)
    {
        if (npc.PaceTicks is int pace)
        {
            return pace;
        }

        int most = 0;
        foreach (RouteStop stop in npc.Route)
        {
            most = Math.Max(most, stop.WaitTicks);
        }

        return most;
    }

    /// <summary>
    /// Ends the walk home on a tile of the home (D-1140). A route NPC rejoins its route on the
    /// first leg that holds the tile: on a route tile, it waits there and takes the next leg, as on
    /// an arrival. Between two route tiles, it walks on toward the tile of its direction (D-739).
    /// </summary>
    private void ArriveHome()
    {
        this.WalksHome = false;
        if (this.Npc.Move != NpcMove.Route)
        {
            return;
        }

        IReadOnlyList<RouteStop> route = this.Npc.Route;
        int leg = this.Npc.RouteLegOf(this.At)
            ?? throw new InvalidOperationException($"The NPC '{this.Npc.Id.Value}' arrived home at {this.At}, and no leg of its route holds that tile (D-1140, T-2).");
        for (int stop = 0; stop < route.Count; stop += 1)
        {
            if (route[stop].At == this.At)
            {
                this.Target = stop;
                this.WaitTicks = route[stop].WaitTicks;
                this.TakeNextLeg();
                return;
            }
        }

        this.Target = this.Forward ? leg + 1 : leg;
    }

    /// <summary>
    /// Takes the next leg of the route. The NPC walks to the last tile of the list, and then it
    /// walks back down the list, as a patrol does (D-739).
    /// </summary>
    private void TakeNextLeg()
    {
        IReadOnlyList<RouteStop> route = this.Npc.Route;
        if (route.Count < 2)
        {
            return;
        }

        if (this.Forward)
        {
            if (this.Target + 1 < route.Count)
            {
                this.Target += 1;
                return;
            }

            this.Forward = false;
            this.Target = route.Count - 2;
            return;
        }

        if (this.Target - 1 >= 0)
        {
            this.Target -= 1;
            return;
        }

        this.Forward = true;
        this.Target = 1;
    }

    private static string? HomeMisfitOf(Npc npc, NpcValues values, TilePoint at, string name)
    {
        if (npc.HomeHolds(at))
        {
            return $"the NPC '{name}' walks home, and it stands at {at}, which its home holds. The walk home ends on arrival (D-1140)";
        }

        if (values.Target < 0 || values.Target >= Math.Max(npc.Route.Count, 1) || (npc.Move != NpcMove.Route && (values.Target != 0 || !values.Forward)))
        {
            return $"the NPC '{name}' walks home, and it holds the route leg {values.Target} and the direction {values.Forward}";
        }

        return null;
    }

    private static string? RangeMisfitOf(Npc npc, NpcValues values, TilePoint at, string name)
    {
        if (!npc.RangeHolds(at))
        {
            return $"the NPC '{name}' stands at {at}, and no rectangle of its range holds that tile (D-1138)";
        }

        if (values.Target != 0 || !values.Forward)
        {
            return $"the NPC '{name}' walks a range, and it holds the route leg {values.Target} and the direction {values.Forward}";
        }

        return null;
    }

    private static string? RouteMisfitOf(Npc npc, NpcValues values, TilePoint at, string name)
    {
        IReadOnlyList<RouteStop> route = npc.Route;
        if (values.Target < 0 || values.Target >= route.Count)
        {
            return $"the NPC '{name}' walks toward the route tile {values.Target}, and its route holds {route.Count} tiles";
        }

        if (route.Count == 1)
        {
            if (at != route[0].At)
            {
                return $"the NPC '{name}' stands at {at}, and its route holds the one tile {route[0].At}";
            }

            if (values.Stepping is StepDirection stepping)
            {
                return $"the NPC '{name}' stands on a route of one tile, and it steps {StepDirections.NameOf(stepping)}";
            }

            return null;
        }

        if (at == route[values.Target].At)
        {
            return $"the NPC '{name}' stands on the route tile {values.Target}, and a walk takes the next leg on arrival (D-739)";
        }

        if (!PatrolStation.TryLeg(at, route[values.Target].At, out _, out _))
        {
            return $"the NPC '{name}' stands at {at}, and the route tile {values.Target} is {route[values.Target].At}. The two lie on no straight leg";
        }

        return null;
    }
}
