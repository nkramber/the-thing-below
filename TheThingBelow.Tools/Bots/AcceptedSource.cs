using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// Gives the intents that the state of a run accepts. The bot job reads the query of Core, and a
/// test plants a list of its own to prove that a softlock fails the job (D-1179, T-3).
/// </summary>
/// <param name="simulation">The run.</param>
/// <returns>The intents.</returns>
public delegate IReadOnlyList<Intent> AcceptedSource(Simulation simulation);

/// <summary>The source of the accepted intents of the bot job.</summary>
public static class AcceptedSources
{
    /// <summary>Gives the intents of the query of Core (D-1179).</summary>
    /// <param name="simulation">The run.</param>
    /// <returns>The intents.</returns>
    public static IReadOnlyList<Intent> OfCore(Simulation simulation) => simulation.Accepted();
}

/// <summary>What one play of the `bots` command runs (D-1180).</summary>
/// <param name="Policy">The policy of every run.</param>
/// <param name="Runs">The count of runs.</param>
/// <param name="FirstSeed">The seed of the first run. Each later run takes the next seed.</param>
/// <param name="OutFolder">The folder of the results, the summary, and the records of the failed runs.</param>
/// <param name="Leg">The name of the leg, which each failure line names.</param>
public sealed record BotPlan(BotPolicyKind Policy, int Runs, ulong FirstSeed, string OutFolder, string Leg);
