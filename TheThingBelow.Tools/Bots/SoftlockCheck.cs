using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// Finds a softlock: a state where no intent that the state accepts changes the state other than
/// the tick (D-1179). The runner checks each state before the policy acts.
/// </summary>
/// <remarks>
/// The trial steps a copy of the run with no intent, and a copy with each accepted intent. Each
/// copy starts from a snapshot of the run, as a replay does (D-651). The tick of each copy rises
/// by one, so two equal state hashes mean that the intent changed nothing else. The trial stops at
/// the first intent that changes the state.
/// <para>
/// A trial costs two copies of the run at least, and a trial on each tick took about 99% of the
/// time of a bot run (G-14). Thus the check skips the trial when the state accepts a toggle: the
/// open or the close of the menu, or the pause or the end of the pause of a story scene. An
/// accepted toggle always flips a flag that the state hash reads (`MenuOpen` of the run, `Paused`
/// of the story), so its trial always gives another hash, and the state is no softlock. A seed
/// loop of Tests proves that the two paths give the same answer.
/// </para>
/// </remarks>
public static class SoftlockCheck
{
    /// <summary>The intents whose trial always changes the state, because each one flips a flag that the state hash reads.</summary>
    public static readonly IReadOnlyList<ContentId> Toggles =
    [
        IntentIds.OpenMenu,
        IntentIds.CloseMenu,
        IntentIds.StoryPause,
        IntentIds.StoryResume,
    ];

    /// <summary>Tells whether no accepted intent changes the state other than the tick.</summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="simulation">The run. The check changes nothing in it.</param>
    /// <param name="accepted">The intents that the state accepts (D-1179).</param>
    /// <returns>True when the run is in a softlock.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static bool Holds(ContentSet content, Simulation simulation, IReadOnlyList<Intent> accepted)
    {
        ArgumentNullException.ThrowIfNull(accepted);

        foreach (Intent intent in accepted)
        {
            if (IsToggle(intent))
            {
                return false;
            }
        }

        return HoldsByTrial(content, simulation, accepted);
    }

    /// <summary>Tells whether no accepted intent changes the state, by the trial of each intent on a copy of the run.</summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="simulation">The run. The trial changes nothing in it.</param>
    /// <param name="accepted">The intents that the state accepts (D-1179).</param>
    /// <returns>True when the run is in a softlock.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static bool HoldsByTrial(ContentSet content, Simulation simulation, IReadOnlyList<Intent> accepted)
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

    private static bool IsToggle(Intent intent)
    {
        foreach (ContentId toggle in Toggles)
        {
            if (string.CompareOrdinal(intent.Action.Value, toggle.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static ulong StepCopy(ContentSet content, ulong seed, RunSnapshot snapshot, IReadOnlyList<Intent> intents)
    {
        Simulation copy = Simulation.Resume(seed, snapshot, MapSet.Of(content.Maps), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
        copy.Step(intents);
        return copy.StateHash();
    }
}
