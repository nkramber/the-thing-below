using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// Every enemy of one map, the mark of a sight, and the encounter that runs (D-37, D-208,
/// D-531).
/// </summary>
/// <remarks>
/// The map runs in real time, so each enemy walks on the tick whether or not the player moves
/// (D-162). The time of day of the map picks the station of each enemy, and an enemy that no
/// station names is not on the map at that time (D-743).
/// <para>
/// One enemy that sees the party starts a mark for a beat, and the encounter starts when that
/// beat ends (D-208, D-745). A step of the party into a body starts the encounter at once,
/// with the party as the side that reached the other (D-747).
/// </para>
/// <para>
/// While the encounter holds, no map system ticks, so the patrols and the grace time all
/// stand still (D-531). The encounter becomes a battle, and the end of the battle ends the
/// encounter as a win or a flee (D-522, D-767).
/// </para>
/// </remarks>
public sealed class MapPatrols
{
    // The order of this list is the order of the map file, so every machine walks the
    // enemies in one order and the draws of the exploration stream never differ (G-4, T-7).
    private readonly PatrolState[] patrols;

    private MapPatrols(PatrolState[] patrols, SightMark? mark, MapEncounter? encounter)
    {
        this.patrols = patrols;
        this.Mark = mark;
        this.Encounter = encounter;
    }

    /// <summary>Every enemy that the time of day of the map places, in the order of the file (G-4).</summary>
    public IReadOnlyList<PatrolState> All => this.patrols;

    /// <summary>The mark of a sight that runs now, or no value (D-208, D-745).</summary>
    public SightMark? Mark { get; private set; }

    /// <summary>The encounter that runs now, or no value (D-531, D-749).</summary>
    public MapEncounter? Encounter { get; private set; }

    /// <summary>Puts every enemy of one map on its station at the start of a run (D-743).</summary>
    /// <param name="map">The map that the run opens.</param>
    /// <returns>The enemies, with no mark and no encounter.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static MapPatrols Enter(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        List<PatrolState> placed = [];
        foreach (Patrol patrol in map.Patrols)
        {
            PatrolStation? station = patrol.StationOf(map.Time);
            if (station is not null)
            {
                placed.Add(PatrolState.Enter(patrol, station));
            }
        }

        return new MapPatrols([.. placed], null, null);
    }

    /// <summary>Puts every enemy back from the values of a snapshot (D-166, D-750).</summary>
    /// <param name="map">The map of the snapshot, from the content of this build.</param>
    /// <param name="values">The stored values, one for each enemy that the map places.</param>
    /// <param name="mark">The mark of the snapshot, or no value.</param>
    /// <param name="encounter">The encounter of the snapshot, or no value.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The enemies, at the values of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of this map (T-2).</exception>
    /// <remarks>
    /// The list holds one value for each enemy that the time of day of the map places, in the
    /// order of the map file. A list of another shape names a save of another content set, so
    /// the load fails with the map and the count (T-2).
    /// </remarks>
    public static MapPatrols Resume(
        GameMap map,
        IReadOnlyList<PatrolValues> values,
        SightMark? mark,
        MapEncounter? encounter,
        string source)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentException.ThrowIfNullOrEmpty(source);

        List<Patrol> placed = [];
        List<PatrolStation> stations = [];
        foreach (Patrol patrol in map.Patrols)
        {
            PatrolStation? station = patrol.StationOf(map.Time);
            if (station is not null)
            {
                placed.Add(patrol);
                stations.Add(station);
            }
        }

        Refuse(
            values.Count != placed.Count,
            source,
            $"it holds {values.Count} enemies, and the map '{map.Id.Value}' places {placed.Count} at the time {TimesOfDay.NameOf(map.Time)}");

        PatrolState[] states = new PatrolState[placed.Count];
        for (int index = 0; index < placed.Count; index += 1)
        {
            PatrolValues stored = values[index];
            ArgumentNullException.ThrowIfNull(stored);
            Refuse(
                string.CompareOrdinal(stored.Enemy.Value, placed[index].Id.Value) != 0,
                source,
                $"the enemy at position {index} is '{stored.Enemy.Value}', and the map '{map.Id.Value}' places '{placed[index].Id.Value}' there");
            states[index] = PatrolState.Resume(placed[index], stations[index], map, stored, source);
        }

        var built = new MapPatrols(states, mark, encounter);
        built.CheckMarkAndEncounter(source);
        return built;
    }

    /// <summary>
    /// Finds the live enemy that holds one tile now, or that steps into it on this tick
    /// (D-737, D-747).
    /// </summary>
    /// <param name="at">The tile.</param>
    /// <param name="found">The enemy, or null when no enemy reaches that tile.</param>
    /// <returns>True when a live enemy holds that tile or steps into it.</returns>
    /// <remarks>
    /// A step that runs counts as the tile of its end too. Without that rule, the party and
    /// an enemy can start a step into one tile on two ticks and then hold one tile (T-2).
    /// </remarks>
    public bool TryEnemyAt(TilePoint at, out PatrolState? found)
    {
        foreach (PatrolState patrol in this.patrols)
        {
            if (patrol.Dead)
            {
                continue;
            }

            if (patrol.Body.Holds(at))
            {
                found = patrol;
                return true;
            }

            if (patrol.Stepping is StepDirection stepping && patrol.Body.Step(stepping).Holds(at))
            {
                found = patrol;
                return true;
            }
        }

        found = null;
        return false;
    }

    /// <summary>Runs every enemy of the map for one world tick (D-162, D-742).</summary>
    /// <param name="map">The map that the enemies stand on.</param>
    /// <param name="party">The party, which blocks a step of an enemy.</param>
    /// <param name="stream">The exploration stream, which an enemy of an area draws from (G-4).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <param name="moved">Takes each enemy that reached a new tile on this tick, for the log.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public void Walk(
        GameMap map,
        MapState party,
        RandomStream stream,
        RunContext context,
        List<PatrolState> moved)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(moved);

        foreach (PatrolState patrol in this.patrols)
        {
            if (patrol.Dead)
            {
                continue;
            }

            patrol.CountGrace();
            if (patrol.CountStep())
            {
                moved.Add(patrol);
            }

            if (patrol.Stepping is not null)
            {
                continue;
            }

            StepDirection? wanted = patrol.Station.Area is null
                ? patrol.NextOnRoute()
                : this.Wander(map, party, patrol, stream, context);
            if (wanted is not StepDirection next)
            {
                continue;
            }

            // The enemy faces the way that it wants to go, whether or not the step runs, so
            // the player reads its next move from the sprite (D-207).
            patrol.Turn(next);
            if (this.Clear(map, party, patrol, next))
            {
                patrol.Begin(next);
            }
        }
    }

    /// <summary>Finds the first enemy of the map that sees the party (D-718, D-737).</summary>
    /// <param name="map">The map that both stand on.</param>
    /// <param name="party">The party.</param>
    /// <param name="torchHeld">True while the party holds the torch out, which widens the sight of each patrol of a dark map (D-1063).</param>
    /// <param name="seen">The enemy that sees the party, or null.</param>
    /// <returns>True when an enemy sees the party.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <remarks>
    /// The walk reads the enemies in the order of the map file, so the first one of that
    /// order starts the encounter and the answer never follows a hash (G-4, T-7). An enemy
    /// inside its grace time sees nothing, because no battle with it starts (D-381).
    /// </remarks>
    public bool TrySight(GameMap map, MapState party, bool torchHeld, out PatrolState? seen)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(party);

        foreach (PatrolState patrol in this.patrols)
        {
            if (patrol.Dead || patrol.GraceTicks > 0)
            {
                continue;
            }

            TilePoint from = patrol.Body.Nearest(party.LeadAt);
            if (MapSight.PatrolSees(map, from, patrol.Facing, MapRules.PatrolSightRange(map, patrol.Patrol, torchHeld), party.LeadAt))
            {
                seen = patrol;
                return true;
            }
        }

        seen = null;
        return false;
    }

    /// <summary>Starts the mark of a sight, which runs for a beat (D-208, D-745).</summary>
    /// <param name="seen">The enemy that saw the party.</param>
    /// <exception cref="ArgumentNullException">The enemy is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">A mark or an encounter already runs (T-2).</exception>
    public void StartMark(PatrolState seen)
    {
        ArgumentNullException.ThrowIfNull(seen);

        if (this.Mark is not null || this.Encounter is not null)
        {
            throw new InvalidOperationException(
                $"The map already holds a mark or an encounter, and the enemy '{seen.Patrol.Id.Value}' saw the party (D-208, T-2).");
        }

        this.Mark = new SightMark(seen.Patrol.Id, MapRules.BeatTicks);
    }

    /// <summary>Counts one tick of the beat, and starts the encounter when the beat ends (D-745).</summary>
    /// <param name="party">The party, for the side that reached the other from behind (D-746).</param>
    /// <returns>True when the encounter started on this tick.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public bool CountBeat(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        if (this.Mark is not SightMark running)
        {
            return false;
        }

        int left = checked(running.TicksLeft - 1);
        if (left > 0)
        {
            this.Mark = new SightMark(running.Enemy, left);
            return false;
        }

        PatrolState patrol = this.Find(running.Enemy);
        this.Mark = null;
        this.Encounter = new MapEncounter(patrol.Patrol.Id, patrol.Patrol.Group, BehindOf(party, patrol));
        return true;
    }

    /// <summary>
    /// Starts the encounter of a step of the party into a body (D-747). The step takes no
    /// beat, because the party made the approach itself.
    /// </summary>
    /// <param name="enemy">The id of the enemy that the party stepped into.</param>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">An encounter already runs (T-2).</exception>
    public void StartBump(ContentId enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);

        if (this.Encounter is not null)
        {
            throw new InvalidOperationException(
                $"The map already holds an encounter, and the party stepped into the enemy '{enemy.Value}' (D-747, T-2).");
        }

        PatrolState patrol = this.Find(enemy);

        // The mark of another enemy ends here, because one encounter runs at a time and this
        // one started (D-531).
        this.Mark = null;
        this.Encounter = new MapEncounter(patrol.Patrol.Id, patrol.Patrol.Group, EncounterSide.Party);
    }

    /// <summary>
    /// Ends the encounter as a flee, and starts the grace time of that enemy (D-381, D-748,
    /// D-749).
    /// </summary>
    /// <returns>The id of the enemy that the party fled from.</returns>
    /// <exception cref="InvalidOperationException">No encounter runs (T-2).</exception>
    public ContentId Flee()
    {
        if (this.Encounter is not MapEncounter running)
        {
            throw new InvalidOperationException(
                "The map holds no encounter, and a flee ends one (D-381, T-2).");
        }

        PatrolState patrol = this.Find(running.Enemy);
        patrol.StartGrace();
        this.Encounter = null;
        return patrol.Patrol.Id;
    }

    /// <summary>Ends the encounter as a win, and marks its enemy dead (D-531, D-555).</summary>
    /// <exception cref="InvalidOperationException">No encounter runs (T-2).</exception>
    public void Defeat()
    {
        if (this.Encounter is not MapEncounter running)
        {
            throw new InvalidOperationException(
                "The map holds no encounter, and a win ends one (D-531, T-2).");
        }

        this.Find(running.Enemy).Defeat();
        this.Encounter = null;
    }

    /// <summary>Gives the stored values of every enemy, in the order of the map file (D-750).</summary>
    /// <returns>One value for each enemy that the map places.</returns>
    public IReadOnlyList<PatrolValues> Values()
    {
        List<PatrolValues> values = [];
        foreach (PatrolState patrol in this.patrols)
        {
            values.Add(new PatrolValues(
                patrol.Patrol.Id,
                patrol.At.X,
                patrol.At.Y,
                patrol.Facing,
                patrol.Stepping,
                patrol.StepTicks,
                patrol.Target,
                patrol.Forward,
                patrol.GraceTicks,
                patrol.Dead));
        }

        return values;
    }

    /// <summary>Adds every value of the enemies to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.patrols.Length);
        foreach (PatrolState patrol in this.patrols)
        {
            patrol.Hash(hasher);
        }

        hasher.AddBoolean(this.Mark is not null);
        hasher.AddText(this.Mark is null ? string.Empty : this.Mark.Enemy.Value);
        hasher.AddInt32(this.Mark is null ? 0 : this.Mark.TicksLeft);
        hasher.AddBoolean(this.Encounter is not null);
        hasher.AddText(this.Encounter is null ? string.Empty : this.Encounter.Enemy.Value);
        hasher.AddText(this.Encounter is null ? string.Empty : this.Encounter.Group.Value);
        hasher.AddInt32(this.Encounter is null ? 0 : (int)this.Encounter.Behind);
    }

    /// <summary>
    /// Gives the side that reached the other from behind (D-265, D-746). A patrol ambushes
    /// the party when it stands in the quarter behind the facing of the party, and the party
    /// sneaks a patrol on the same rule.
    /// </summary>
    private static EncounterSide BehindOf(MapState party, PatrolState patrol)
    {
        TilePoint at = patrol.Body.Nearest(party.LeadAt);
        bool sneak = MapSight.BehindFacing(at, patrol.Facing, party.LeadAt);
        bool ambush = MapSight.BehindFacing(party.LeadAt, party.Facing, at);

        // Both sides behind, and neither side behind, each leave the first blow to the rules
        // of the fight (D-746).
        if (sneak == ambush)
        {
            return EncounterSide.None;
        }

        return sneak ? EncounterSide.Party : EncounterSide.Enemy;
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The enemies of {source} are not a state of a run: {reason} (T-2).", nameof(source));
        }
    }

    /// <summary>
    /// Picks the next direction of an enemy of an area (D-741). The enemy keeps its direction
    /// while the step holds the body inside the area, and it draws a new direction from the
    /// exploration stream when the step does not.
    /// </summary>
    private StepDirection? Wander(
        GameMap map,
        MapState party,
        PatrolState patrol,
        RandomStream stream,
        RunContext context)
    {
        if (this.Clear(map, party, patrol, patrol.Facing))
        {
            return patrol.Facing;
        }

        // One draw picks where the walk of the four directions starts, so the count of draws
        // of one tick never follows the shape of the area (G-4, T-7).
        int draw = stream.NextInt(StepDirections.All.Length, context);
        for (int turn = 0; turn < StepDirections.All.Length; turn += 1)
        {
            StepDirection candidate = StepDirections.All[(draw + turn) % StepDirections.All.Length];
            if (this.Clear(map, party, patrol, candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// Tells whether one enemy can step in one direction. The terrain, the area, the party,
    /// and every other body each block the step (D-206, D-741, D-747).
    /// </summary>
    private bool Clear(GameMap map, MapState party, PatrolState patrol, StepDirection direction)
    {
        EnemyBody next = patrol.Body.Step(direction);
        if (!MapRules.CanPlace(map, next))
        {
            return false;
        }

        if (patrol.Station.Area is TileArea area && !area.HoldsBody(next))
        {
            return false;
        }

        if (next.Holds(party.LeadAt))
        {
            return false;
        }

        // The party stands on a whole tile through its step, and it reaches the next tile on
        // the last tick of that step. Thus the tile of its end blocks an enemy too (D-203).
        if (party.Stepping is StepDirection walking && next.Holds(party.LeadAt.Step(walking)))
        {
            return false;
        }

        foreach (PatrolState other in this.patrols)
        {
            if (ReferenceEquals(other, patrol) || other.Dead)
            {
                continue;
            }

            if (MapRules.BodiesOverlap(other.Body, next))
            {
                return false;
            }

            if (other.Stepping is StepDirection stepping && MapRules.BodiesOverlap(other.Body.Step(stepping), next))
            {
                return false;
            }
        }

        return true;
    }

    private void CheckMarkAndEncounter(string source)
    {
        if (this.Mark is SightMark mark)
        {
            Refuse(
                mark.TicksLeft < 1 || mark.TicksLeft > MapRules.BeatTicks,
                source,
                $"the mark of the enemy '{mark.Enemy.Value}' holds {mark.TicksLeft} ticks, and the range of a beat is 1 to {MapRules.BeatTicks}");
            Refuse(
                this.Encounter is not null,
                source,
                "it holds a mark and an encounter, and one encounter ends every mark");
            Refuse(!this.Holds(mark.Enemy), source, $"the mark names the enemy '{mark.Enemy.Value}', which this map does not place");
        }

        if (this.Encounter is MapEncounter encounter)
        {
            Refuse(
                !this.Holds(encounter.Enemy),
                source,
                $"the encounter names the enemy '{encounter.Enemy.Value}', which this map does not place");
            PatrolState patrol = this.Find(encounter.Enemy);
            Refuse(
                string.CompareOrdinal(patrol.Patrol.Group.Value, encounter.Group.Value) != 0,
                source,
                $"the encounter names the group '{encounter.Group.Value}', and the enemy '{encounter.Enemy.Value}' takes the group '{patrol.Patrol.Group.Value}'");
        }
    }

    private bool Holds(ContentId enemy)
    {
        foreach (PatrolState patrol in this.patrols)
        {
            if (string.CompareOrdinal(patrol.Patrol.Id.Value, enemy.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private PatrolState Find(ContentId enemy)
    {
        foreach (PatrolState patrol in this.patrols)
        {
            if (string.CompareOrdinal(patrol.Patrol.Id.Value, enemy.Value) == 0)
            {
                return patrol;
            }
        }

        throw new InvalidOperationException(
            $"This map places no enemy with the id '{enemy.Value}', and a mark or an encounter names one (T-2).");
    }
}
