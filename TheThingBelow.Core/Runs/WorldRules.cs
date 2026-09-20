using System;
using System.Collections.Generic;
using TheThingBelow.Core.Logging;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The world work of one tick. A menu pauses the world, so <see cref="Simulation"/> calls
/// this system only while no menu is open (D-162, D-650).
/// </summary>
/// <remarks>
/// The world of Phase 1 is one patrol that walks on a fixed beat, whether or not the player
/// moves (D-162). PR-7 replaces it with the tile map, the movement of the party, and the
/// sight of the map.
/// </remarks>
public static class WorldRules
{
    /// <summary>The count of world ticks between two beats of a patrol, which is half a second (D-164).</summary>
    public const int TicksPerPatrolBeat = 30;

    /// <summary>The count of directions that a patrol can take on one beat.</summary>
    public const int PatrolChoiceCount = 4;

    /// <summary>Runs the world for one tick.</summary>
    /// <param name="state">The state of the run, which the system changes.</param>
    /// <param name="log">The log entries of this tick, which this system adds to (D-179).</param>
    /// <exception cref="ArgumentNullException">The state or the list is null (T-2).</exception>
    /// <exception cref="SimulationException">A count passes its range (T-2).</exception>
    /// <remarks>
    /// A beat takes the debug level, because the patrol walks two times a second and a log
    /// file of the info level holds the changes that a report follows (D-179).
    /// </remarks>
    public static void Step(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        state.CountWorldTick();
        if (state.WorldTick % TicksPerPatrolBeat == 0)
        {
            state.WalkPatrol(state.Context("world/patrol"));
            log.Add(new LogEntry(
                LogLevel.Debug,
                "the patrol walked one beat",
                state.Tick,
                LogSubsystems.World,
                [
                    LogField.OfNumber("beats", state.PatrolBeats),
                    LogField.OfNumber("choice", state.PatrolChoice),
                    LogField.OfNumber("world-tick", state.WorldTick),
                ]));
        }
    }
}
