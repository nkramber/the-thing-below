using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The live state of one run: the tick, the menu, the world values, and every random stream
/// (D-168). <see cref="Simulation"/> steps it, and <see cref="RunSnapshot"/> stores it.
/// </summary>
/// <remarks>
/// The tick is the one time line of a run, and it rises on every step, a step with a menu
/// open included (D-650). The world values rise only while no menu is open, because a menu
/// pauses the world (D-162).
/// <para>
/// The world of this run is the party on a tile map (D-100, D-106). The party walks one tile
/// at a time, and the record of the walked tiles grows with it (D-567, D-716).
/// </para>
/// </remarks>
public sealed class RunState
{
    // The streams sit in the order of `RandomStreams.All`, so a walk of them is the same on
    // every machine and a snapshot writes them in one order (G-4, T-7).
    private readonly RandomStream[] streams;

    // The events of the battle wait here until Game takes them. They are output, and not
    // state, so no snapshot and no hash reads them (D-168, D-532).
    private readonly List<BattleEvent> events = [];

    private RunState(
        ulong seed,
        RandomStream[] streams,
        long tick,
        bool menuOpen,
        long worldTick,
        MapState map,
        BattleContent battleContent,
        PartyState characters,
        Battle? battle)
    {
        this.BattleContent = battleContent;
        this.Characters = characters;
        this.Battle = battle;
        this.Seed = seed;
        this.streams = streams;
        this.Tick = tick;
        this.MenuOpen = menuOpen;
        this.WorldTick = worldTick;
        this.Party = map;
    }

    /// <summary>The seed that started the run (G-3).</summary>
    public ulong Seed { get; }

    /// <summary>The count of ticks since the start of the run (D-164, D-650).</summary>
    public long Tick { get; private set; }

    /// <summary>True while a menu is open and the world waits (D-162).</summary>
    public bool MenuOpen { get; private set; }

    /// <summary>The count of ticks in which the world ran (D-650).</summary>
    public long WorldTick { get; private set; }

    /// <summary>The party on its map: the lead, the step that runs, and the walked tiles (D-106, D-567).</summary>
    public MapState Party { get; }

    /// <summary>The battle rules and the fixture of this run (D-757, D-766).</summary>
    public BattleContent BattleContent { get; }

    /// <summary>The characters of the party and their pack, which last between battles (D-36, D-765).</summary>
    public PartyState Characters { get; }

    /// <summary>The battle that runs, or that ended and waits for the screen, or no value (D-522, D-531).</summary>
    public Battle? Battle { get; private set; }

    /// <summary>Starts a new run from a seed, at tick zero, on one map.</summary>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <param name="map">The map that the run opens, with the party on its spawn point (D-528).</param>
    /// <param name="battleContent">The battle rules and the fixture, which hold every group that the map names (D-766).</param>
    /// <returns>The state, with every stream at its first value.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static RunState Start(ulong seed, GameMap map, BattleContent battleContent)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        battleContent.RequireGroupsOf(map);

        RandomStream[] streams = new RandomStream[RandomStreams.All.Count];
        for (int index = 0; index < streams.Length; index += 1)
        {
            streams[index] = RandomStreams.Open(seed, RandomStreams.All[index]);
        }

        return new RunState(
            seed,
            streams,
            0,
            false,
            0,
            MapState.Enter(map),
            battleContent,
            PartyState.Start(battleContent),
            null);
    }

    /// <summary>Starts a run again from a snapshot (D-259, D-651).</summary>
    /// <param name="seed">The seed of the run, which the record header holds (G-5).</param>
    /// <param name="snapshot">The snapshot, which the caller checked (T-2).</param>
    /// <param name="map">
    /// The map of the snapshot, which the caller read from its content by
    /// <see cref="RunSnapshot.MapIdOrFirst"/> (D-166).
    /// </param>
    /// <param name="battleContent">The battle rules and the fixture, which hold every group that the map names (D-766).</param>
    /// <returns>The state, with every stream at the position of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">The snapshot or the map is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run, or the map is another map (T-2).</exception>
    /// <remarks>
    /// A snapshot of save format 1 holds no map, because it predates the tile map. Its
    /// migration puts the party on the spawn point of the first map, with that tile walked
    /// and no other (D-166, D-654). A snapshot of save format 2 holds no enemy, and its
    /// migration puts each enemy of the map on the start tile of its station (D-750).
    /// </remarks>
    public static RunState Resume(ulong seed, RunSnapshot snapshot, GameMap map, BattleContent battleContent)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        battleContent.RequireGroupsOf(map);
        snapshot.Check("this run");

        RandomStream[] streams = new RandomStream[snapshot.Streams.Count];
        for (int index = 0; index < streams.Length; index += 1)
        {
            StreamPosition position = snapshot.Streams[index];
            streams[index] = new RandomStream(
                position.Stream,
                Pcg32.FromSnapshot(position.State, position.Increment));
        }

        MapState party = ResumeMap(snapshot, map);
        return new RunState(
            seed,
            streams,
            snapshot.Tick,
            snapshot.MenuOpen,
            snapshot.WorldTick,
            party,
            battleContent,
            ResumeCharacters(snapshot, battleContent),
            ResumeBattle(snapshot, party, battleContent));
    }

    private static MapState ResumeMap(RunSnapshot snapshot, GameMap map)
    {
        if (string.CompareOrdinal(snapshot.MapIdOrFirst.Value, map.Id.Value) != 0)
        {
            throw new ArgumentException(
                $"The snapshot names the map '{snapshot.MapIdOrFirst.Value}', and the caller gave the map '{map.Id.Value}' (T-2, D-166).",
                nameof(map));
        }

        if (snapshot.Map is null)
        {
            return MapState.Enter(map);
        }

        MapSnapshot party = snapshot.Map;

        // A snapshot of save format 2 predates the enemies, so its enemy list is absent and
        // each enemy of the map starts on the start tile of its station (D-654, D-750).
        return MapState.Resume(
            map,
            new TilePoint(party.LeadX, party.LeadY),
            party.Facing,
            party.Stepping,
            party.StepTicks,
            WalkedTiles.OfRows(party.Walked, "this run"),
            party.Enemies,
            party.Mark,
            party.Encounter,
            "this run");
    }

    /// <summary>
    /// Gives the party of a snapshot. A snapshot before save format 4 holds no party, and its
    /// migration starts the party of the fixture at full health (D-166, D-765).
    /// </summary>
    private static PartyState ResumeCharacters(RunSnapshot snapshot, BattleContent battleContent)
    {
        if (snapshot.Characters is not PartySnapshot stored)
        {
            return PartyState.Start(battleContent);
        }

        return PartyState.Resume(battleContent, stored.Characters, stored.Pack, "this run");
    }

    /// <summary>Gives the battle of a snapshot, which must match the encounter of its map (D-531, T-2).</summary>
    private static Battle? ResumeBattle(RunSnapshot snapshot, MapState party, BattleContent battleContent)
    {
        if (snapshot.Battle is not BattleValues stored)
        {
            return null;
        }

        if (party.Patrols.Encounter is not MapEncounter encounter
            || string.CompareOrdinal(encounter.Enemy.Value, stored.Enemy.Value) != 0
            || string.CompareOrdinal(encounter.Group.Value, stored.Group.Value) != 0)
        {
            throw new ArgumentException(
                $"The snapshot holds a battle of the enemy '{stored.Enemy.Value}' and the group '{stored.Group.Value}', and its map holds no encounter of both (T-2, D-531).",
                nameof(snapshot));
        }

        return Battle.Resume(battleContent, stored, "this run");
    }

    /// <summary>Gives the stream of one subsystem (G-4).</summary>
    /// <param name="stream">The number of the stream.</param>
    /// <returns>The stream of that subsystem.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The run holds no such stream (T-2).</exception>
    public RandomStream Stream(StreamId stream)
    {
        foreach (RandomStream open in this.streams)
        {
            if (open.Stream == stream)
            {
                return open;
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(stream),
            stream,
            $"The run holds no stream with that number. It holds {this.streams.Length} streams.");
    }

    /// <summary>Makes the context that an error of this tick carries (T-2, G-18).</summary>
    /// <param name="subject">The ids of the thing that the rule works on.</param>
    /// <returns>The seed, the tick, and the subject.</returns>
    public RunContext Context(string subject) => new(this.Seed, this.Tick, subject);

    /// <summary>Stores the whole state, so a replay can start again from here (F-10, D-651).</summary>
    /// <returns>The snapshot.</returns>
    public RunSnapshot Snapshot() =>
        new(
            this.Tick,
            this.MenuOpen,
            this.WorldTick,
            new MapSnapshot(
                this.Party.Map.Id,
                this.Party.LeadAt.X,
                this.Party.LeadAt.Y,
                this.Party.Facing,
                this.Party.Stepping,
                this.Party.StepTicks,
                this.Party.Walked.Rows(),
                this.Party.Patrols.Values(),
                this.Party.Patrols.Mark,
                this.Party.Patrols.Encounter),
            new PartySnapshot(this.Characters.CharacterValues(), this.Characters.PackValues()),
            this.Battle?.Values(),
            ReadPositions(this.streams));

    /// <summary>Computes the state hash that a replay and the identity job compare (G-5).</summary>
    /// <returns>The hash of every value of this state.</returns>
    /// <remarks>
    /// The hash holds the simulation version, so a build with other rules never gives the
    /// hash of this build by accident (G-17, D-504).
    /// </remarks>
    public ulong StateHash()
    {
        StateHasher hasher = new();

        hasher.AddInt32(SimulationVersion.Current);
        hasher.AddUInt64(this.Seed);
        hasher.AddInt64(this.Tick);
        hasher.AddBoolean(this.MenuOpen);
        hasher.AddInt64(this.WorldTick);
        this.Party.Hash(hasher);
        this.Characters.Hash(hasher);
        hasher.AddBoolean(this.Battle is not null);
        this.Battle?.Hash(hasher);

        foreach (RandomStream stream in this.streams)
        {
            hasher.AddInt32((int)stream.Stream);
            hasher.AddUInt64(stream.Generator.State);
            hasher.AddUInt64(stream.Generator.Increment);
        }

        return hasher.Finish();
    }

    /// <summary>Opens or closes the menu, which starts or ends the pause of the world (D-162).</summary>
    /// <param name="open">True to open the menu, and false to close it.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The menu already has that state (T-2).</exception>
    /// <remarks>
    /// A second open of an open menu is an error and never a value that the rule drops. An
    /// intent that changes nothing points at a fault in the screen that made it (T-2).
    /// </remarks>
    public void SetMenuOpen(bool open, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (this.MenuOpen == open)
        {
            string state = open ? "open" : "closed";
            throw new SimulationException($"an intent that makes the menu {state}, which it already is", context);
        }

        this.MenuOpen = open;
    }

    /// <summary>Reads a move intent of this tick, which the world step then applies (D-493).</summary>
    /// <param name="direction">The direction of the step that the player asked for.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">The context is null (T-2).</exception>
    /// <exception cref="SimulationException">A menu is open, and a menu pauses the world (T-2).</exception>
    /// <remarks>
    /// A move intent while a menu is open is an error and never a value that the rule drops.
    /// The menu screen takes every input of the player, so a step intent from it points at a
    /// fault in the screen that made it (D-162, T-2).
    /// </remarks>
    public void WantStep(StepDirection direction, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (this.MenuOpen)
        {
            throw new SimulationException(
                $"a step to the {StepDirections.NameOf(direction)} while the menu is open, and a menu pauses the world (D-162)",
                context);
        }

        this.Party.Want(direction);
    }

    /// <summary>Takes every battle event since the last take, in the order of the rules (D-532).</summary>
    /// <returns>The events, which the run no longer holds.</returns>
    public IReadOnlyList<BattleEvent> TakeEvents()
    {
        BattleEvent[] taken = [.. this.events];
        this.events.Clear();
        return taken;
    }

    /// <summary>Sets the battle, or ends it with null (D-531).</summary>
    internal void SetBattle(Battle? battle) => this.Battle = battle;

    /// <summary>Adds one battle event for Game (D-168, D-532).</summary>
    internal void AddEvent(BattleEvent battleEvent) => this.events.Add(battleEvent);

    /// <summary>
    /// Counts one step of the loop (D-164, D-650). The tick is the one time line of the run,
    /// so <see cref="Simulation"/> alone counts it and no debug command moves it (D-171).
    /// </summary>
    /// <exception cref="SimulationException">The tick passes the range of a `long` (T-2).</exception>
    internal void CountTick()
    {
        if (this.Tick == long.MaxValue)
        {
            throw new SimulationException(
                $"a step after tick {long.MaxValue}, which passes the range of the tick",
                this.Context("run"));
        }

        this.Tick += 1;
    }

    /// <summary>Counts one tick in which the world ran, which a menu skips (D-650).</summary>
    internal void CountWorldTick() => this.WorldTick += 1;

    private static StreamPosition[] ReadPositions(RandomStream[] streams)
    {
        StreamPosition[] positions = new StreamPosition[streams.Length];
        for (int index = 0; index < streams.Length; index += 1)
        {
            RandomStream stream = streams[index];
            positions[index] = new StreamPosition(
                stream.Stream,
                stream.Generator.State,
                stream.Generator.Increment);
        }

        return positions;
    }
}
