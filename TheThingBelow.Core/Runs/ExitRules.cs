using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Runs;

/// <summary>The rule of an exit and an entrance: the arrival on it enters the map that it names (D-1216, D-1243).</summary>
/// <remarks>
/// The exit of a place leads to the overworld, and the party arrives on the marker that the exit
/// names (D-1255). An entrance of the overworld leads to a place, and the party arrives on the
/// spawn point of that place. The exit restores nothing (D-1217). The memory of the map that the
/// party leaves keeps each killed enemy, each open door, and each chest (D-555).
/// </remarks>
public static class ExitRules
{
    /// <summary>Enters the map that an exit or an entrance names, on the arrival of the lead on it (D-1216, D-1243).</summary>
    /// <param name="state">The run, with the lead on the tile of the exit or the entrance.</param>
    /// <param name="exit">The exit or the entrance.</param>
    /// <param name="bumped">The enemy that a step of the same tick reached, or no value. That step starts no encounter.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">The state, the exit, or the log is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// The thing names no map, or the run cannot enter its map, such as a map that the maps of the
    /// run lack or a marker that the map lacks (D-1133, D-1255, T-2).
    /// </exception>
    public static void Leave(RunState state, MapThing exit, ContentId? bumped, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(exit);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context($"exit/{exit.Id.Value}");
        ContentId to = exit.To
            ?? throw new SimulationException($"an arrival on the thing '{exit.Id.Value}', which names no map, and the reader of a map gives each exit and each entrance one (D-1216, D-1243)", context);
        ContentId from = state.Party.Map.Id;
        if (exit.Arrive is ContentId marker)
        {
            state.EnterMapAt(to, marker, context);
        }
        else
        {
            state.EnterMap(to, context);
        }

        var fields = new List<LogField> { new(MapThingKinds.NameOf(exit.Kind), exit.Id.Value), new("from", from.Value), new("to", to.Value) };
        if (bumped is ContentId passed)
        {
            fields.Add(new LogField("passed-enemy", passed.Value));
        }

        log.Add(new LogEntry(LogLevel.Info, $"the party took an {MapThingKinds.NameOf(exit.Kind)} and entered its map", state.Tick, LogSubsystems.World, fields));
    }
}
