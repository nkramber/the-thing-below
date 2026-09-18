using System;
using System.Collections.Generic;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>The position of one random stream at the moment of a snapshot (D-166, D-259).</summary>
/// <param name="Stream">The number of the stream (G-4).</param>
/// <param name="State">The state word of the generator.</param>
/// <param name="Increment">The increment word of the generator, which the seed split set.</param>
public sealed record StreamPosition(StreamId Stream, ulong State, ulong Increment);

/// <summary>
/// The whole state of a run at the end of one tick. A record holds one snapshot and the
/// intents after it, so its size stays bounded (F-10, D-651).
/// </summary>
/// <remarks>
/// A snapshot holds the tick and the position of every stream (D-166). A replay starts from
/// a snapshot and applies the intents that follow it, so a snapshot must hold every value
/// that a rule reads. PR-43 writes a snapshot to a save file (D-259, D-494).
/// <para>
/// The world values below are the world of Phase 1: a patrol that walks on a fixed tick
/// while no menu is open (D-162, D-650). PR-7 replaces them with the tile map and the
/// position of the party.
/// </para>
/// </remarks>
/// <param name="Tick">The count of ticks since the start of the run (D-164, D-650).</param>
/// <param name="MenuOpen">True while a menu is open and the world waits (D-162).</param>
/// <param name="WorldTick">The count of ticks in which the world ran (D-650).</param>
/// <param name="PatrolBeats">The count of beats that the patrol walked.</param>
/// <param name="PatrolChoice">The direction of the last beat, from 0 to 3.</param>
/// <param name="Streams">The position of every stream, in the order of `RandomStreams.All`.</param>
public sealed record RunSnapshot(
    long Tick,
    bool MenuOpen,
    long WorldTick,
    int PatrolBeats,
    int PatrolChoice,
    IReadOnlyList<StreamPosition> Streams)
{
    /// <summary>Fails when the snapshot cannot describe a state of a run (T-2).</summary>
    /// <param name="source">What the snapshot came from, such as `the record`, for the error.</param>
    /// <exception cref="ArgumentException">A value is outside its range, or a stream is absent.</exception>
    /// <remarks>
    /// A read of a file or of a record makes a snapshot from values that this build did not
    /// write, so the check runs on every path that makes one from the outside (T-2).
    /// </remarks>
    public void Check(string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(this.Streams);

        Refuse(this.Tick < 0, source, $"the tick is {this.Tick}, which is below zero");
        Refuse(this.WorldTick < 0, source, $"the world tick is {this.WorldTick}, which is below zero");
        Refuse(
            this.WorldTick > this.Tick,
            source,
            $"the world tick is {this.WorldTick}, and the tick is {this.Tick}, which is lower");
        Refuse(this.PatrolBeats < 0, source, $"the patrol beats are {this.PatrolBeats}, which is below zero");
        Refuse(
            this.PatrolChoice < 0 || this.PatrolChoice >= WorldRules.PatrolChoiceCount,
            source,
            $"the patrol choice is {this.PatrolChoice}, and the range is 0 to {WorldRules.PatrolChoiceCount - 1}");
        Refuse(
            this.Streams.Count != RandomStreams.All.Count,
            source,
            $"it holds {this.Streams.Count} streams, and a run holds {RandomStreams.All.Count}");

        for (int index = 0; index < this.Streams.Count; index += 1)
        {
            StreamPosition position = this.Streams[index];
            ArgumentNullException.ThrowIfNull(position);
            Refuse(
                position.Stream != RandomStreams.All[index],
                source,
                $"the stream at position {index} is '{position.Stream}', and a run holds '{RandomStreams.All[index]}' there");
        }
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The snapshot of {source} is not a state of a run: {reason}.");
        }
    }
}
