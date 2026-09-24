using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The world work of one tick. A menu pauses the world, so <see cref="Simulation"/> calls
/// this system only while no menu is open (D-162, D-650).
/// </summary>
/// <remarks>
/// The world of this build is the party and the enemies on a tile map. The party walks one
/// tile at a time, and a step takes a fixed count of ticks (D-106, D-164, D-203). The map
/// runs in real time, so each enemy walks here on the same tick, whether or not the player
/// moves (D-162).
/// <para>
/// The tick runs in one fixed order: the beat of a mark, the party, the encounter of a step
/// into a body, and then the enemies and the sight (D-168). An encounter starts its battle on
/// the same tick. While the encounter runs, no map system ticks, so the patrols and the grace
/// time all stand still (D-531). A snapshot of save format 3 can hold an encounter with no
/// battle, and the next world step starts that battle (D-765).
/// </para>
/// <para>
/// A story scene holds the map still as a battle does, and the tick runs its steps alone
/// (D-1009). With no story scene, the tick first reads the entry trigger and the battle end
/// trigger that wait, and it reads the tile trigger of a tile that the party reached after
/// the encounter of the same step (D-1004).
/// </para>
/// <para>
/// A step of the party and a step of an enemy each take the debug level, and a sight and an
/// encounter take the info level. Thus a log file of a player holds the events that a report
/// follows, and not the walk (D-179, D-751).
/// </para>
/// </remarks>
public static class WorldRules
{
    /// <summary>Runs the world for one tick.</summary>
    /// <param name="state">The state of the run, which the system changes.</param>
    /// <param name="log">The log entries of this tick, which this system adds to (D-179).</param>
    /// <exception cref="ArgumentNullException">The state or the list is null (T-2).</exception>
    /// <exception cref="OverflowException">A count passes its range (T-2).</exception>
    public static void Step(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        state.CountWorldTick();

        // A battle holds the map still until the wait intent of Game ends it (D-522, D-531).
        if (state.Battle is not null)
        {
            return;
        }

        // A story scene holds the map still until its last step ends (D-1009).
        if (state.Story.Running)
        {
            StoryRules.Advance(state, log);
            return;
        }

        if (StoryRules.FireWaiting(state, log))
        {
            return;
        }

        MapState party = state.Party;
        MapPatrols patrols = party.Patrols;

        if (patrols.Encounter is null && patrols.CountBeat(party))
        {
            log.Add(EncounterEntry(state, "the beat of a mark ended and an encounter started"));
        }

        PartyStep step = party.Advance();
        AddPartyEntries(state, party, step, log);

        if (step.Bumped is ContentId bumped)
        {
            patrols.StartBump(bumped);
            log.Add(EncounterEntry(state, "the party stepped into an enemy and an encounter started"));
        }

        // While an encounter runs, no enemy walks and no enemy sees the party. The encounter
        // becomes a battle on this tick (D-531).
        if (patrols.Encounter is not null)
        {
            BattleTurns.Begin(state, log);
            return;
        }

        // A story scene that starts on the tile holds the patrols still from this tick (D-1009).
        if (step.Arrived && StoryRules.FireTile(state, step.At, log))
        {
            return;
        }

        AddEnemyEntries(state, patrols, party, log);
    }

    private static void AddPartyEntries(RunState state, MapState party, PartyStep step, List<LogEntry> log)
    {
        if (step.Arrived)
        {
            log.Add(new LogEntry(
                LogLevel.Debug,
                "the party reached a tile",
                state.Tick,
                LogSubsystems.World,
                [
                    LogField.OfNumber("x", step.At.X),
                    LogField.OfNumber("y", step.At.Y),
                    LogField.OfNumber("walked", party.Walked.Count),
                    LogField.OfNumber("world-tick", state.WorldTick),
                ]));
        }

        if (step.Started is StepDirection direction)
        {
            log.Add(new LogEntry(
                LogLevel.Debug,
                $"the party started a step to the {StepDirections.NameOf(direction)}",
                state.Tick,
                LogSubsystems.World,
                [
                    LogField.OfNumber("x", party.LeadAt.X),
                    LogField.OfNumber("y", party.LeadAt.Y),
                    new LogField("direction", StepDirections.NameOf(direction)),
                ]));
        }
    }

    /// <summary>
    /// Walks every enemy of the map, and reads the sight of each one (D-718, D-742). One
    /// mark runs at a time, so an enemy sees nothing while a mark runs (D-208).
    /// </summary>
    private static void AddEnemyEntries(RunState state, MapPatrols patrols, MapState party, List<LogEntry> log)
    {
        List<PatrolState> moved = [];
        patrols.Walk(
            party.Map,
            party,
            state.Stream(StreamId.Exploration),
            state.Context("patrols"),
            moved);

        foreach (PatrolState patrol in moved)
        {
            log.Add(new LogEntry(
                LogLevel.Debug,
                "an enemy reached a tile",
                state.Tick,
                LogSubsystems.World,
                [
                    new LogField("enemy", patrol.Patrol.Id.Value),
                    LogField.OfNumber("x", patrol.At.X),
                    LogField.OfNumber("y", patrol.At.Y),
                    new LogField("facing", StepDirections.NameOf(patrol.Facing)),
                ]));
        }

        if (patrols.Mark is not null || !patrols.TrySight(party.Map, party, state.Characters.TorchHeld, out PatrolState? seen))
        {
            return;
        }

        patrols.StartMark(seen!);
        log.Add(new LogEntry(
            LogLevel.Info,
            "an enemy saw the party and the mark of a beat started",
            state.Tick,
            LogSubsystems.World,
            [
                new LogField("enemy", seen!.Patrol.Id.Value),
                LogField.OfNumber("x", seen.At.X),
                LogField.OfNumber("y", seen.At.Y),
                LogField.OfNumber("beat", MapRules.BeatTicks),
            ]));
    }

    private static LogEntry EncounterEntry(RunState state, string message)
    {
        MapEncounter encounter = state.Party.Patrols.Encounter
            ?? throw new InvalidOperationException("The map holds no encounter, and this entry reports one (T-2).");

        return new LogEntry(
            LogLevel.Info,
            message,
            state.Tick,
            LogSubsystems.World,
            [
                new LogField("enemy", encounter.Enemy.Value),
                new LogField("group", encounter.Group.Value),
                new LogField("behind", EncounterSides.NameOf(encounter.Behind)),
            ]);
    }
}
