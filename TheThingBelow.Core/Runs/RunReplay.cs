using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// Plays a run record again. A replay on the same simulation version and content hash gives
/// the state hash of the run that wrote the record (G-5, T-7).
/// </summary>
/// <remarks>
/// A replay runs in Tools and in Tests with no Godot, because Core holds every rule (D-100,
/// D-493). The replay viewer of a development build plays a record in Game (D-175).
/// </remarks>
public static class RunReplay
{
    /// <summary>The list that a tick with no line of the record gets.</summary>
    private static readonly IReadOnlyList<Intent> NoIntents = [];

    /// <summary>Plays a record from its snapshot to its end tick, with no other map to enter.</summary>
    /// <param name="record">The record of the run (G-5).</param>
    /// <param name="contentHash">The content hash that this host loaded (D-648).</param>
    /// <param name="map">
    /// The map of the snapshot of the record, which the host read from its content by
    /// <see cref="RunSnapshot.MapIdOrFirst"/> (D-166).
    /// </param>
    /// <param name="battleContent">The battle rules and the fixture of this build (D-766).</param>
    /// <param name="notices">The notice file of this build (D-985).</param>
    /// <param name="story">The story content of this build (D-540).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of this host. A release host passes
    /// <see cref="DebugIntentHandlers.None"/>, and it then refuses a record with a debug
    /// intent (D-260, D-492).
    /// </param>
    /// <returns>The state at the end tick of the record.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="RunRecordException">This build cannot replay the record (T-2).</exception>
    /// <exception cref="SimulationException">
    /// A rule refused an intent of the record, and the message names the intent and the tick
    /// (T-2).
    /// </exception>
    public static RunState Play(
        RunRecord record,
        string contentHash,
        GameMap map,
        BattleContent battleContent,
        NoticeList notices,
        StoryContent story,
        DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(map);

        return Play(record, contentHash, MapSet.Of([map]), battleContent, notices, story, debugHandlers);
    }

    /// <summary>Plays a record from its snapshot to its end tick, on the maps of a set (D-1133).</summary>
    /// <param name="record">The record of the run (G-5).</param>
    /// <param name="contentHash">The content hash that this host loaded (D-648).</param>
    /// <param name="maps">
    /// Every map that the party can enter in the run, such as every map of the content. The set
    /// holds the map of the snapshot and each map that an intent of the record enters (D-166).
    /// </param>
    /// <param name="battleContent">The battle rules and the fixture of this build (D-766).</param>
    /// <param name="notices">The notice file of this build (D-985).</param>
    /// <param name="story">The story content of this build (D-540).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of this host. A release host passes
    /// <see cref="DebugIntentHandlers.None"/>, and it then refuses a record with a debug
    /// intent (D-260, D-492).
    /// </param>
    /// <returns>The state at the end tick of the record.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="RunRecordException">This build cannot replay the record (T-2).</exception>
    /// <exception cref="SimulationException">
    /// A rule refused an intent of the record, and the message names the intent and the tick
    /// (T-2).
    /// </exception>
    public static RunState Play(
        RunRecord record,
        string contentHash,
        MapSet maps,
        BattleContent battleContent,
        NoticeList notices,
        StoryContent story,
        DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrEmpty(contentHash);
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(story);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        record.Header.CheckAgainstThisBuild(contentHash);

        Simulation simulation = Simulation.Resume(record.Header.Seed, record.Snapshot, maps, battleContent, notices, story, debugHandlers);
        int next = 0;

        while (simulation.Tick < record.EndTick)
        {
            long tick = simulation.Tick + 1;
            if (next < record.Ticks.Count && record.Ticks[next].Tick == tick)
            {
                simulation.Step(record.Ticks[next].Intents);
                next += 1;
            }
            else
            {
                simulation.Step(NoIntents);
            }
        }

        return simulation.State;
    }
}
