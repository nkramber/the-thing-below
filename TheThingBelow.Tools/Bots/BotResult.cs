using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>The result of one bot run (D-64, D-1182).</summary>
/// <param name="Seed">The seed of the run, which repeats it.</param>
/// <param name="Policy">The policy of the run.</param>
/// <param name="End">How the run ended.</param>
/// <param name="Tick">The tick of the run at its end.</param>
/// <param name="Played">The count of ticks that the runner played, which the tick budget reads. A reload after a wipe moves the tick back, and this count goes on.</param>
/// <param name="GoalPlayed">The count of played ticks when the goal flag came on, or no value.</param>
/// <param name="Wipes">The count of wipes, each one with a reload (D-1181).</param>
/// <param name="Message">The error of a crash, or the tick and the intents of a softlock, or no value.</param>
/// <param name="Battles">The turns and the outcome of each battle (D-1182).</param>
/// <param name="Record">The run record from the last snapshot to the end, which a replay repeats (T-7).</param>
/// <param name="StateHash">The state hash of the run at its end, which a replay of the record gives again (G-5).</param>
public sealed record BotResult(
    ulong Seed,
    BotPolicyKind Policy,
    BotEnd End,
    long Tick,
    long Played,
    long? GoalPlayed,
    int Wipes,
    string? Message,
    IReadOnlyList<BattleCount> Battles,
    RunRecord Record,
    ulong StateHash);
