using System;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;

namespace TheThingBelow.Tests;

/// <summary>
/// The runs that the save tests store and load. A save holds a snapshot of a real run, so
/// each test starts from the rules and never from values by hand (D-259, T-3).
/// </summary>
public static class SaveRuns
{
    /// <summary>The seed of every run of the save tests.</summary>
    public const ulong Seed = 0x0000000000bada55;

    /// <summary>The content hash that the header of a test save names (D-648).</summary>
    public const string ContentHash = "5ce12c645b6f5150e71d3775ada1c32fdbfca9372f8dac8008a5e1220f3c15f3";

    /// <summary>The count of ticks of the run that the stored fixture saves hold.</summary>
    public const int FixtureTicks = 120;

    /// <summary>Plays a run of the script of <see cref="Seed"/> and gives its state.</summary>
    /// <param name="tickCount">The count of ticks to play.</param>
    /// <returns>The run at that tick.</returns>
    public static Simulation Play(int tickCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tickCount);

        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);
        foreach (IReadOnlyList<Intent> intents in RunScripts.Make(Seed, tickCount))
        {
            run.Step(intents);
        }

        return run;
    }

    /// <summary>Makes the save of a run of this build, at the tick that the count gives.</summary>
    /// <param name="tickCount">The count of ticks to play before the save.</param>
    /// <returns>The header of this build and the snapshot of the run.</returns>
    public static SaveDocument SaveAfter(int tickCount) =>
        new(SaveHeader.ForThisBuild(ContentHash, Seed), Play(tickCount).Snapshot());
}
