using System;
using System.Collections.Generic;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The world work of one tick. A menu pauses the world, so <see cref="Simulation"/> calls
/// this system only while no menu is open (D-162, D-650).
/// </summary>
/// <remarks>
/// The world of this build is the party on a tile map. The party walks one tile at a time,
/// and a step takes a fixed count of ticks (D-106, D-164, D-203). The map runs in real time,
/// so PR-8 walks each patrol here on the same tick, whether or not the player moves (D-162).
/// </remarks>
public static class WorldRules
{
    /// <summary>Runs the world for one tick.</summary>
    /// <param name="state">The state of the run, which the system changes.</param>
    /// <param name="log">The log entries of this tick, which this system adds to (D-179).</param>
    /// <exception cref="ArgumentNullException">The state or the list is null (T-2).</exception>
    /// <exception cref="OverflowException">A count passes its range (T-2).</exception>
    /// <remarks>
    /// A step of the party takes the debug level, because the party walks four tiles a
    /// second and a log file of the info level holds the changes that a report follows
    /// (D-179).
    /// </remarks>
    public static void Step(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        state.CountWorldTick();

        MapState party = state.Party;
        bool arrived = party.Advance(out TilePoint walked, out StepDirection? started);

        if (arrived)
        {
            log.Add(Entry(state, "the party reached a tile", walked, party));
        }

        if (started is StepDirection direction)
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

    private static LogEntry Entry(RunState state, string message, TilePoint at, MapState party) =>
        new(
            LogLevel.Debug,
            message,
            state.Tick,
            LogSubsystems.World,
            [
                LogField.OfNumber("x", at.X),
                LogField.OfNumber("y", at.Y),
                LogField.OfNumber("walked", party.Walked.Count),
                LogField.OfNumber("world-tick", state.WorldTick),
            ]);
}
