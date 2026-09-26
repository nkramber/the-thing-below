using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// Finds a softlock: a state where no intent that the state accepts changes the state other than
/// the tick (D-1179).
/// </summary>
/// <remarks>
/// The check steps a copy of the run with no intent, and a copy with each accepted intent. Each
/// copy starts from a snapshot of the run, as a replay does (D-651). The tick of each copy rises
/// by one, so two equal state hashes mean that the intent changed nothing else. The check stops
/// at the first intent that changes the state.
/// </remarks>
public static class SoftlockCheck
{
    /// <summary>Tells whether no accepted intent changes the state other than the tick.</summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="simulation">The run. The check changes nothing in it.</param>
    /// <param name="accepted">The intents that the state accepts (D-1179).</param>
    /// <returns>True when the run is in a softlock.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static bool Holds(ContentSet content, Simulation simulation, IReadOnlyList<Intent> accepted)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(accepted);

        RunSnapshot snapshot = simulation.Snapshot();
        ulong still = StepCopy(content, simulation.State.Seed, snapshot, []);
        foreach (Intent intent in accepted)
        {
            if (StepCopy(content, simulation.State.Seed, snapshot, [intent]) != still)
            {
                return false;
            }
        }

        return true;
    }

    private static ulong StepCopy(ContentSet content, ulong seed, RunSnapshot snapshot, IReadOnlyList<Intent> intents)
    {
        Simulation copy = Simulation.Resume(seed, snapshot, MapSet.Of(content.Maps), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
        copy.Step(intents);
        return copy.StateHash();
    }
}
