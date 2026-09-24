using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>The position of one random stream at the moment of a snapshot (D-166, D-259).</summary>
/// <param name="Stream">The number of the stream (G-4).</param>
/// <param name="State">The state word of the generator.</param>
/// <param name="Increment">The increment word of the generator, which the seed split set.</param>
public sealed record StreamPosition(StreamId Stream, ulong State, ulong Increment);

/// <summary>The party on one map at the moment of a snapshot (D-166, D-528, D-567).</summary>
/// <remarks>
/// The snapshot holds the id of the map and never its content, so a load reads the map of
/// this build and a content change never breaks a save (D-166, D-495).
/// </remarks>
/// <param name="Map">The id of the map that the party stands on (D-528).</param>
/// <param name="LeadX">The column of the lead (D-106).</param>
/// <param name="LeadY">The row of the lead.</param>
/// <param name="Facing">The direction that the lead faces (D-207, D-716).</param>
/// <param name="Stepping">The direction of the step that ran, or no value while the lead stood.</param>
/// <param name="StepTicks">The count of ticks of that step (D-203).</param>
/// <param name="Walked">Every tile that the party walked, one string for each row (D-567).</param>
/// <param name="Enemies">
/// The stored values of each enemy that the map places, in the order of the map file
/// (D-750). The value is absent on a snapshot of save format 2, which predates the enemies,
/// and the migration then puts each enemy on the start tile of its station (D-654).
/// </param>
/// <param name="Mark">The mark of a sight that ran, or no value (D-745).</param>
/// <param name="Encounter">The encounter that ran, or no value (D-749).</param>
public sealed record MapSnapshot(
    ContentId Map,
    int LeadX,
    int LeadY,
    StepDirection Facing,
    StepDirection? Stepping,
    int StepTicks,
    IReadOnlyList<string> Walked,
    IReadOnlyList<PatrolValues>? Enemies,
    SightMark? Mark,
    MapEncounter? Encounter);

/// <summary>The characters of the party and their pack in a snapshot (D-765).</summary>
/// <param name="Characters">Each character, in slot order.</param>
/// <param name="Pack">Each item of the pack, in the order of the fixture file.</param>
/// <param name="LessonPack">The owned lessons that no character carries, from save format 10 (D-1024). Null in a snapshot of an older format.</param>
/// <param name="AtSwapPlace">True when the party stood at a swap place, from save format 10 (D-1030). False in a snapshot of an older format.</param>
public sealed record PartySnapshot(IReadOnlyList<CharacterValues> Characters, IReadOnlyList<PackValues> Pack, IReadOnlyList<ContentId>? LessonPack, bool AtSwapPlace);

/// <summary>
/// The whole state of a run at the end of one tick. A record holds one snapshot and the
/// intents after it, so its size stays bounded (F-10, D-651).
/// </summary>
/// <remarks>
/// A snapshot holds the tick and the position of every stream (D-166). A replay starts from
/// a snapshot and applies the intents that follow it, so a snapshot must hold every value
/// that a rule reads. PR-43 writes a snapshot to a save file (D-259, D-494).
/// <para>
/// The world of this build is the party and the enemies on a tile map (D-100, D-106). A
/// snapshot of save format 1 predates the map, so its map value is absent and the migration
/// of `RunState` puts the party on the spawn point of the first map (D-166, D-654).
/// </para>
/// <para>
/// A snapshot of save format 2 predates the enemies, so its enemy list holds none. Its
/// migration puts each enemy of the map on the start tile of its station, which is where a
/// new run puts it (D-654, D-750).
/// </para>
/// <para>
/// A snapshot before save format 4 holds no party, and its migration starts the party of the
/// fixture at full health (D-166, D-765). One snapshot covers the map and the battle, so a
/// battle that runs, or that waits for the screen, takes its place in the snapshot (D-531).
/// </para>
/// <para>
/// A snapshot before save format 8 holds no notice log, and its migration starts the log empty
/// (D-166, D-985).
/// </para>
/// <para>
/// A snapshot before save format 9 holds no story state, and its migration starts with no
/// flag on and no story scene (D-166, D-540).
/// </para>
/// </remarks>
/// <param name="Tick">The count of ticks since the start of the run (D-164, D-650).</param>
/// <param name="MenuOpen">True while a menu is open and the world waits (D-162).</param>
/// <param name="WorldTick">The count of ticks in which the world ran (D-650).</param>
/// <param name="Map">The party on its map, or no value on a snapshot of save format 1 (D-166).</param>
/// <param name="Characters">The characters and the pack, or no value on a snapshot before save format 4 (D-765).</param>
/// <param name="Battle">The battle, or no value when none runs (D-531).</param>
/// <param name="Notices">The notice log, oldest first, or no value on a snapshot before save format 8 (D-985).</param>
/// <param name="Story">The flags, the story scene that runs, and the events that fire a trigger, or no value on a snapshot before save format 9 (D-540, D-542).</param>
/// <param name="Streams">The position of every stream, in the order of `RandomStreams.All`.</param>
public sealed record RunSnapshot(
    long Tick,
    bool MenuOpen,
    long WorldTick,
    MapSnapshot? Map,
    PartySnapshot? Characters,
    BattleValues? Battle,
    IReadOnlyList<ContentId>? Notices,
    StoryValues? Story,
    IReadOnlyList<StreamPosition> Streams)
{
    /// <summary>
    /// The id of the map to load for this snapshot (D-166). A snapshot of save format 1
    /// holds no map, and its migration puts the party on the first map (D-654).
    /// </summary>
    public ContentId MapIdOrFirst => this.Map is null ? MapIds.FirstMap : this.Map.Map;

    /// <summary>Fails when the snapshot cannot describe a state of a run (T-2).</summary>
    /// <param name="source">What the snapshot came from, such as `the record`, for the error.</param>
    /// <exception cref="ArgumentException">
    /// A value is outside its range, a stream is absent, or an increment is even.
    /// </exception>
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
        this.CheckMap(source);
        Refuse(
            this.Battle is not null && (this.Characters is null || (this.Map?.Encounter is null && !this.StoryWaitsForBattle())),
            source,
            "it holds a battle with no party, or with no encounter and no story scene that waits for it, and a battle needs a party and one of the two (D-531, D-998)");
        Refuse(
            this.Notices is not null && this.Notices.Count > NoticeLog.MostEntries,
            source,
            $"it holds {this.Notices?.Count} notices in the log, and the log keeps {NoticeLog.MostEntries} (D-984)");
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

            // `RandomStreams.Open` gives each stream one increment, from its number alone.
            // The check runs here, so the reader of a record names the line of the fault and
            // no stream of another sequence reaches a replay (T-2, G-18, T-7).
            ulong increment = RandomStreams.IncrementOf(position.Stream);
            Refuse(
                position.Increment != increment,
                source,
                $"the increment of the stream '{position.Stream}' is {position.Increment}, and every stream of that number takes the increment {increment}");
        }
    }

    /// <summary>
    /// Checks the values of the map that a rule reads. The walked tiles and the map itself
    /// take their full check in `MapState.Resume`, which holds the map of this build and can
    /// compare the two (T-2, D-567).
    /// </summary>
    private void CheckMap(string source)
    {
        if (this.Map is null)
        {
            return;
        }

        MapSnapshot map = this.Map;
        ArgumentNullException.ThrowIfNull(map.Map);
        ArgumentNullException.ThrowIfNull(map.Walked);
        Refuse(map.LeadX < 0, source, $"the column of the lead is {map.LeadX}, which is below zero");
        Refuse(map.LeadY < 0, source, $"the row of the lead is {map.LeadY}, which is below zero");
        Refuse(
            map.Stepping is null && map.StepTicks != 0,
            source,
            $"the lead stands on no step, and the step ticks are {map.StepTicks}");
        Refuse(
            map.Stepping is not null && (map.StepTicks < 0 || map.StepTicks >= MapRules.TicksPerStep),
            source,
            $"the step ticks are {map.StepTicks}, and the range of a step is 0 to {MapRules.TicksPerStep - 1}");
        Refuse(map.Walked.Count == 0, source, "the walked tiles hold no row, and a map holds at least one");

        // The values of one enemy take their full check in `MapPatrols.Resume`, which holds
        // the map of this build and can read the record of each enemy (T-2, D-750).
        foreach (PatrolValues enemy in map.Enemies ?? [])
        {
            ArgumentNullException.ThrowIfNull(enemy);
            ArgumentNullException.ThrowIfNull(enemy.Enemy);
        }

        Refuse(
            map.Mark is not null && map.Encounter is not null,
            source,
            "it holds a mark and an encounter, and one encounter ends every mark");
        Refuse(
            map.Enemies is null && (map.Mark is not null || map.Encounter is not null),
            source,
            "it holds no enemy list and it holds a mark or an encounter, and both name an enemy");
    }

    /// <summary>Tells whether a story scene waits for the battle of its start battle step (D-998).</summary>
    private bool StoryWaitsForBattle() => this.Story?.Scene?.Phase == ScenePhase.Battle;

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The snapshot of {source} is not a state of a run: {reason}.");
        }
    }
}
