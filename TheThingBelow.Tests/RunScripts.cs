using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Tests;

/// <summary>
/// The scripts of intents that the run tests play. A script is legal: it opens the menu only
/// while the menu is closed, and it closes the menu only while the menu is open (D-650).
/// </summary>
/// <remarks>
/// The generator below is a stream of Tests, and it never touches a stream of the run under
/// test. Each failure of a seed loop names its seed (T-3).
/// </remarks>
public static class RunScripts
{
    /// <summary>The id of the debug intent that the tests of the seam use (D-260, D-492).</summary>
    public static readonly ContentId DebugWalkPatrol =
        ContentId.Parse("debug.walk_patrol", "TheThingBelow.Tests/RunScripts.cs", "DebugWalkPatrol");

    /// <summary>The stream number that the generator of a script takes, apart from the run.</summary>
    private const ulong ScriptSequence = 0x5343524950543031;

    /// <summary>Makes the intents of every tick of a run, from a seed.</summary>
    /// <param name="seed">The seed of the script, which the failure of a seed loop names.</param>
    /// <param name="tickCount">The count of ticks of the run.</param>
    /// <returns>One list for each tick, from tick 1 to the tick count. Most lists are empty.</returns>
    public static IReadOnlyList<IReadOnlyList<Intent>> Make(ulong seed, int tickCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tickCount);

        Pcg32 generator = Pcg32.FromSeed(seed, ScriptSequence);
        List<IReadOnlyList<Intent>> script = [];
        bool menuOpen = false;

        for (int tick = 0; tick < tickCount; tick += 1)
        {
            // One tick in twenty carries an intent, so a run holds long stretches with no
            // line and the compaction of F-10 has something to compact.
            if (generator.Next() % 20 != 0)
            {
                script.Add([]);
                continue;
            }

            script.Add([Intent.OfPlayer(menuOpen ? IntentIds.CloseMenu : IntentIds.OpenMenu)]);
            menuOpen = !menuOpen;
        }

        return script;
    }

    /// <summary>Plays a script, and records it (G-5).</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="contentHash">The content hash that the header names (D-648).</param>
    /// <param name="script">The intents of each tick, which <see cref="Make"/> gave.</param>
    /// <param name="saveEvery">The count of ticks between two saves, or zero for no save (D-651).</param>
    /// <returns>The run at its last tick, and the record of it.</returns>
    public static (Simulation Run, RunRecorder Recorder) Play(
        ulong seed,
        string contentHash,
        IReadOnlyList<IReadOnlyList<Intent>> script,
        int saveEvery)
    {
        ArgumentNullException.ThrowIfNull(script);

        Simulation run = Simulation.Start(seed, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild(contentHash, seed), run.Snapshot());

        foreach (IReadOnlyList<Intent> intents in script)
        {
            run.Step(intents);
            recorder.Step(run.Tick, intents);

            if (saveEvery > 0 && run.Tick % saveEvery == 0)
            {
                recorder.Save(run.Snapshot());
            }
        }

        return (run, recorder);
    }
}
