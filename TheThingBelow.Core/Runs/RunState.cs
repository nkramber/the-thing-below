using System;
using System.Collections.Generic;
using TheThingBelow.Core.Hashing;
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
/// The world of Phase 1 is a patrol that walks on a fixed beat. PR-7 replaces it with the
/// tile map, the party, and the sight of the map (D-100, D-106).
/// </para>
/// </remarks>
public sealed class RunState
{
    // The streams sit in the order of `RandomStreams.All`, so a walk of them is the same on
    // every machine and a snapshot writes them in one order (G-4, T-7).
    private readonly RandomStream[] streams;

    private RunState(ulong seed, RandomStream[] streams, RunSnapshot start)
    {
        this.Seed = seed;
        this.streams = streams;
        this.Tick = start.Tick;
        this.MenuOpen = start.MenuOpen;
        this.WorldTick = start.WorldTick;
        this.PatrolBeats = start.PatrolBeats;
        this.PatrolChoice = start.PatrolChoice;
    }

    /// <summary>The seed that started the run (G-3).</summary>
    public ulong Seed { get; }

    /// <summary>The count of ticks since the start of the run (D-164, D-650).</summary>
    public long Tick { get; private set; }

    /// <summary>True while a menu is open and the world waits (D-162).</summary>
    public bool MenuOpen { get; private set; }

    /// <summary>The count of ticks in which the world ran (D-650).</summary>
    public long WorldTick { get; private set; }

    /// <summary>The count of beats that the patrol walked.</summary>
    public int PatrolBeats { get; private set; }

    /// <summary>The direction of the last beat of the patrol, from 0 to 3.</summary>
    public int PatrolChoice { get; private set; }

    /// <summary>Starts a new run from a seed, at tick zero.</summary>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <returns>The state, with every stream at its first value.</returns>
    public static RunState Start(ulong seed)
    {
        RandomStream[] streams = new RandomStream[RandomStreams.All.Count];
        for (int index = 0; index < streams.Length; index += 1)
        {
            streams[index] = RandomStreams.Open(seed, RandomStreams.All[index]);
        }

        RunSnapshot start = new(0, false, 0, 0, 0, ReadPositions(streams));
        return new RunState(seed, streams, start);
    }

    /// <summary>Starts a run again from a snapshot (D-259, D-651).</summary>
    /// <param name="seed">The seed of the run, which the record header holds (G-5).</param>
    /// <param name="snapshot">The snapshot, which the caller checked (T-2).</param>
    /// <returns>The state, with every stream at the position of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">The snapshot is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run (T-2).</exception>
    public static RunState Resume(ulong seed, RunSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        snapshot.Check("this run");

        RandomStream[] streams = new RandomStream[snapshot.Streams.Count];
        for (int index = 0; index < streams.Length; index += 1)
        {
            StreamPosition position = snapshot.Streams[index];
            streams[index] = new RandomStream(
                position.Stream,
                Pcg32.FromSnapshot(position.State, position.Increment));
        }

        return new RunState(seed, streams, snapshot);
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
            this.PatrolBeats,
            this.PatrolChoice,
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
        hasher.AddInt32(this.PatrolBeats);
        hasher.AddInt32(this.PatrolChoice);

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

    /// <summary>Walks the patrol one beat, and draws its direction from the map stream (G-4).</summary>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The count of beats passes the range of an `int` (T-2).</exception>
    public void WalkPatrol(RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (this.PatrolBeats == int.MaxValue)
        {
            throw new SimulationException(
                $"a patrol beat after {int.MaxValue} beats, which passes the range of the count",
                context);
        }

        this.PatrolBeats += 1;
        this.PatrolChoice = this.Stream(StreamId.Exploration)
            .NextInt(WorldRules.PatrolChoiceCount, context);
    }

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
