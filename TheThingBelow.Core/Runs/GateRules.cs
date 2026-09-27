using System;
using System.Collections.Generic;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The confirm rule of a gate of the overworld (D-1243, D-1257).</summary>
/// <remarks>
/// A step onto a closed gate turns the lead alone, as a step into a wall does, and `MapState`
/// reads the condition for that step. The player learns why through a confirm at the gate, which
/// posts the notice of the gate, as a confirm at a locked door does (D-1257). An open gate takes
/// no confirm.
/// </remarks>
public static class GateRules
{
    /// <summary>Posts the notice of a closed gate that the lead faces (D-1257).</summary>
    /// <param name="state">The run, with the lead standing and facing the gate.</param>
    /// <param name="gate">The gate.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when the gate is closed and its notice posted. False for an open gate, which takes no confirm.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The thing is not a gate, or the notice file lacks its notice (T-2).</exception>
    public static bool Confirm(RunState state, MapThing gate, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(gate);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context($"gate/{gate.Id.Value}");
        MapGate rule = gate.Gate
            ?? throw new SimulationException($"a confirm at the thing '{gate.Id.Value}', which holds no gate, and the reader of a map gives each gate its condition and its notice (D-1243)", context);
        if (rule.Condition.Holds(state.Story.Flags))
        {
            return false;
        }

        NoticeRules.Post(state, rule.Notice, context, log);
        log.Add(new LogEntry(
            LogLevel.Info,
            "the lead confirmed at a closed gate, and its notice posted",
            state.Tick,
            LogSubsystems.World,
            [new LogField("gate", gate.Id.Value), new LogField("notice", rule.Notice.Value)]));
        return true;
    }
}
