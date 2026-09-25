using System;
using System.Collections.Generic;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// One enemy on the map as a run holds it: its body, its facing, the step that runs, the leg
/// of its route, and its grace time (D-738, D-750).
/// </summary>
/// <remarks>
/// Core keeps the body on whole tiles through the whole step, as it does for the party, and
/// Game slides the sprite across the ticks of the step (D-203). A step of this enemy takes
/// the count of ticks that its record gives (D-742).
/// <para>
/// <see cref="MapPatrols"/> holds the walk rule, the sight rule, and the encounter. This
/// class holds the values of one enemy and the smallest changes to them (D-168).
/// </para>
/// <para>
/// The route values hold the leg of the route, and they stay at zero and at true for an
/// enemy of an area, which walks at random inside a rectangle (D-741).
/// </para>
/// </remarks>
public sealed class PatrolState
{
    private PatrolState(
        Patrol patrol,
        PatrolStation station,
        TilePoint at,
        StepDirection facing,
        StepDirection? stepping,
        int stepTicks,
        int target,
        bool forward,
        int graceTicks,
        bool dead)
    {
        this.Patrol = patrol;
        this.Station = station;
        this.At = at;
        this.Facing = facing;
        this.Stepping = stepping;
        this.StepTicks = stepTicks;
        this.Target = target;
        this.Forward = forward;
        this.GraceTicks = graceTicks;
        this.Dead = dead;
    }

    /// <summary>The record of this enemy, as the map file holds it (D-738).</summary>
    public Patrol Patrol { get; }

    /// <summary>The station that the time of day of the map picked (D-743).</summary>
    public PatrolStation Station { get; }

    /// <summary>The anchor tile of the body, which is always a whole tile (D-203, D-737).</summary>
    public TilePoint At { get; private set; }

    /// <summary>The direction that this enemy faces, which carries its sight (D-208, D-718).</summary>
    public StepDirection Facing { get; private set; }

    /// <summary>The direction of the step that runs now, or no value while the enemy stands.</summary>
    public StepDirection? Stepping { get; private set; }

    /// <summary>
    /// The count of ticks of the step that runs, from 0 to the step of the record minus one
    /// (D-742). The value is zero while the enemy stands.
    /// </summary>
    public int StepTicks { get; private set; }

    /// <summary>
    /// The index of the route tile that this enemy walks toward (D-739). The value is zero
    /// for an enemy of an area.
    /// </summary>
    public int Target { get; private set; }

    /// <summary>
    /// True while the enemy walks up the list of route tiles, and false while it walks back
    /// down the list (D-739). The value is true for an enemy of an area.
    /// </summary>
    public bool Forward { get; private set; }

    /// <summary>
    /// The count of world ticks of the grace time that remain. No battle with this enemy
    /// starts while the count is above zero (D-381, D-748).
    /// </summary>
    public int GraceTicks { get; private set; }

    /// <summary>
    /// True when the party killed this enemy. A killed enemy stays dead, and no exit brings
    /// it back (D-555). A won battle sets it (D-531).
    /// </summary>
    public bool Dead { get; private set; }

    /// <summary>The tiles that this enemy holds (D-206, D-737).</summary>
    public EnemyBody Body => new(this.At, this.Patrol.Size);

    /// <summary>True while this enemy stands still, because its route holds one tile (D-740).</summary>
    public bool Stands => this.Station.Area is null && this.Station.Tiles.Count == 1;

    /// <summary>Puts one enemy on its station at the start of a run (D-739, D-741).</summary>
    /// <param name="patrol">The record of the enemy (D-738).</param>
    /// <param name="station">The station that the time of day of the map picked (D-743).</param>
    /// <returns>The state, at the start tile of the station.</returns>
    /// <exception cref="ArgumentNullException">The record or the station is null (T-2).</exception>
    public static PatrolState Enter(Patrol patrol, PatrolStation station)
    {
        ArgumentNullException.ThrowIfNull(patrol);
        ArgumentNullException.ThrowIfNull(station);

        int target = station.Area is null && station.Tiles.Count > 1 ? 1 : 0;
        return new PatrolState(patrol, station, station.Start, patrol.Facing, null, 0, target, true, 0, false);
    }

    /// <summary>Puts one enemy back from the values of a snapshot (D-166, D-750).</summary>
    /// <param name="patrol">The record of the enemy, from the map of this build.</param>
    /// <param name="station">The station that the time of day of the map picked.</param>
    /// <param name="map">The map, for the check of the body and of the step (T-2).</param>
    /// <param name="values">The values of the snapshot.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">A value describes no state of this enemy (T-2).</exception>
    public static PatrolState Resume(
        Patrol patrol,
        PatrolStation station,
        GameMap map,
        PatrolValues values,
        string source)
    {
        ArgumentNullException.ThrowIfNull(patrol);
        ArgumentNullException.ThrowIfNull(station);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (MisfitOf(patrol, station, map, values) is string misfit)
        {
            Refuse(true, source, misfit);
        }

        return new PatrolState(
            patrol,
            station,
            new TilePoint(values.X, values.Y),
            values.Facing,
            values.Stepping,
            values.StepTicks,
            values.Target,
            values.Forward,
            values.GraceTicks,
            values.Dead);
    }

    /// <summary>
    /// Puts one enemy on the start tile of its station again, as a run starts it, and keeps
    /// its death. A resume of a save of another build calls it for an enemy whose stored place
    /// the edited map no longer takes (D-1111).
    /// </summary>
    /// <param name="patrol">The record of the enemy, from the map of this build.</param>
    /// <param name="station">The station that the time of day of the map picked.</param>
    /// <param name="dead">True when the save holds the enemy as dead (D-555).</param>
    /// <returns>The state, at the start tile of the station.</returns>
    /// <exception cref="ArgumentNullException">The record or the station is null (T-2).</exception>
    public static PatrolState EnterAgain(Patrol patrol, PatrolStation station, bool dead)
    {
        PatrolState entered = Enter(patrol, station);
        entered.Dead = dead;
        return entered;
    }

    /// <summary>
    /// Gives the first reason why stored values describe no state of this enemy on this map,
    /// or no value when they fit (T-2, D-1111).
    /// </summary>
    /// <param name="patrol">The record of the enemy, from the map of this build.</param>
    /// <param name="station">The station that the time of day of the map picked.</param>
    /// <param name="map">The map, for the check of the body and of the step.</param>
    /// <param name="values">The values of the snapshot.</param>
    /// <returns>The reason, or null.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static string? MisfitOf(Patrol patrol, PatrolStation station, GameMap map, PatrolValues values)
    {
        ArgumentNullException.ThrowIfNull(patrol);
        ArgumentNullException.ThrowIfNull(station);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(values);

        var at = new TilePoint(values.X, values.Y);
        EnemyBody body = new(at, patrol.Size);
        string name = patrol.Id.Value;

        if (!MapRules.CanPlace(map, body))
        {
            return $"the enemy '{name}' holds the body {body}, and that body takes no step on the map '{map.Id.Value}'";
        }

        if (values.GraceTicks < 0 || values.GraceTicks > MapRules.GraceTicks)
        {
            return $"the enemy '{name}' holds {values.GraceTicks} grace ticks, and the range is 0 to {MapRules.GraceTicks}";
        }

        if (values.Stepping is null && values.StepTicks != 0)
        {
            return $"the enemy '{name}' stands on no step, and its step ticks are {values.StepTicks}";
        }

        if (values.Stepping is not null && (values.StepTicks < 0 || values.StepTicks >= patrol.StepTicks))
        {
            return $"the enemy '{name}' holds {values.StepTicks} step ticks, and the range of its step is 0 to {patrol.StepTicks - 1}";
        }

        string? place = station.Area is TileArea area
            ? AreaMisfitOf(area, values, body, name)
            : RouteMisfitOf(station, values, at, name);
        if (place is not null)
        {
            return place;
        }

        if (values.Stepping is StepDirection stepping)
        {
            EnemyBody next = body.Step(stepping);
            if (!MapRules.CanPlace(map, next))
            {
                return $"the enemy '{name}' steps {StepDirections.NameOf(stepping)} to the body {next}, which takes no step";
            }
        }

        return null;
    }

    /// <summary>
    /// Counts one tick of the grace time after a flee (D-381, D-748). The count stops at
    /// zero, and a battle with this enemy can start again from that tick.
    /// </summary>
    public void CountGrace()
    {
        if (this.GraceTicks > 0)
        {
            this.GraceTicks -= 1;
        }
    }

    /// <summary>Starts the grace time of a flee (D-381, D-748).</summary>
    public void StartGrace() => this.GraceTicks = MapRules.GraceTicks;

    /// <summary>Marks the enemy dead after a won battle (D-555). It never walks or sees again.</summary>
    public void Defeat() => this.Dead = true;

    /// <summary>
    /// Counts one tick of the step that runs, and moves the body when the step ends (D-742).
    /// </summary>
    /// <returns>True when the body reached a new tile on this tick.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public bool CountStep()
    {
        if (this.Stepping is not StepDirection running)
        {
            return false;
        }

        this.StepTicks = checked(this.StepTicks + 1);
        if (this.StepTicks < this.Patrol.StepTicks)
        {
            return false;
        }

        this.At = this.At.Step(running);
        this.Stepping = null;
        this.StepTicks = 0;
        if (this.Station.Area is null && this.At == this.Station.Tiles[this.Target])
        {
            this.TakeNextLeg();
        }

        return true;
    }

    /// <summary>Starts a step of this enemy in one direction (D-742).</summary>
    /// <param name="direction">The direction of the step, which the caller proved is clear.</param>
    public void Begin(StepDirection direction)
    {
        this.Facing = direction;
        this.Stepping = direction;
        this.StepTicks = 0;
    }

    /// <summary>
    /// Turns this enemy to one direction with no step (D-207). A blocked patrol faces the
    /// way that it wants to go, so the player reads its next move from the sprite.
    /// </summary>
    /// <param name="direction">The direction that the enemy faces from this tick.</param>
    public void Turn(StepDirection direction) => this.Facing = direction;

    /// <summary>Gives the direction of the next step of a route, or no value for a standing enemy.</summary>
    /// <returns>The direction toward the route tile of <see cref="Target"/>.</returns>
    /// <exception cref="InvalidOperationException">This enemy walks an area, not a route (T-2).</exception>
    /// <remarks>
    /// The load of the map proved that each leg is straight and on one axis, so this call
    /// always finds a direction while the route holds more than one tile (D-739).
    /// </remarks>
    public StepDirection? NextOnRoute()
    {
        if (this.Station.Area is not null)
        {
            throw new InvalidOperationException(
                $"The enemy '{this.Patrol.Id.Value}' walks an area, and an area holds no route leg (D-741, T-2).");
        }

        if (this.Stands)
        {
            return null;
        }

        if (!PatrolStation.TryLeg(this.At, this.Station.Tiles[this.Target], out StepDirection direction, out _))
        {
            throw new InvalidOperationException(
                $"The enemy '{this.Patrol.Id.Value}' stands at {this.At}, and the route tile {this.Target} is {this.Station.Tiles[this.Target]}. The load of a map proves that each leg is straight (D-739, T-2).");
        }

        return direction;
    }

    /// <summary>Adds every value of this enemy to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddText(this.Patrol.Id.Value);
        hasher.AddInt32(this.At.X);
        hasher.AddInt32(this.At.Y);
        hasher.AddInt32((int)this.Facing);
        hasher.AddBoolean(this.Stepping.HasValue);
        hasher.AddInt32(this.Stepping.HasValue ? (int)this.Stepping.Value : 0);
        hasher.AddInt32(this.StepTicks);
        hasher.AddInt32(this.Target);
        hasher.AddBoolean(this.Forward);
        hasher.AddInt32(this.GraceTicks);
        hasher.AddBoolean(this.Dead);
    }

    /// <summary>
    /// Takes the next leg of the route. The enemy walks to the last tile of the list, and
    /// then it walks back down the list (D-739).
    /// </summary>
    private void TakeNextLeg()
    {
        IReadOnlyList<TilePoint> route = this.Station.Tiles;
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

    private static string? AreaMisfitOf(TileArea area, PatrolValues values, EnemyBody body, string name)
    {
        if (!area.HoldsBody(body))
        {
            return $"the enemy '{name}' holds the body {body}, and its area is {area}";
        }

        if (values.Target != 0 || !values.Forward)
        {
            return $"the enemy '{name}' walks an area, and it holds the route leg {values.Target} and the direction {values.Forward}";
        }

        return null;
    }

    private static string? RouteMisfitOf(
        PatrolStation station,
        PatrolValues values,
        TilePoint at,
        string name)
    {
        IReadOnlyList<TilePoint> route = station.Tiles;
        if (values.Target < 0 || values.Target >= route.Count)
        {
            return $"the enemy '{name}' walks toward the route tile {values.Target}, and its route holds {route.Count} tiles";
        }

        if (route.Count == 1)
        {
            if (at != route[0])
            {
                return $"the enemy '{name}' stands at {at}, and its route holds the one tile {route[0]}";
            }

            if (values.Stepping is StepDirection stepping)
            {
                return $"the enemy '{name}' stands on a route of one tile, and it steps {StepDirections.NameOf(stepping)}";
            }

            return null;
        }

        if (at == route[values.Target])
        {
            return $"the enemy '{name}' stands on the route tile {values.Target}, and a walk takes the next leg on arrival (D-739)";
        }

        if (!PatrolStation.TryLeg(at, route[values.Target], out _, out _))
        {
            return $"the enemy '{name}' stands at {at}, and the route tile {values.Target} is {route[values.Target]}. The two lie on no straight leg";
        }

        return null;
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The enemy state of {source} is not a state of a run: {reason} (T-2).", nameof(source));
        }
    }
}
