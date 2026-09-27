using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TheThingBelow.Core.Battles;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// Writes the result line of each run and the summary of the runs of one policy on one leg
/// (D-1180, D-1182). The summary is Markdown, which the bot job adds to the page of its run.
/// </summary>
public static class BotSummary
{
    /// <summary>Gives the result line of one run: the policy, the seed, the end, the ticks, and each battle.</summary>
    /// <param name="result">The result of the run.</param>
    /// <returns>The line, with no line end.</returns>
    public static string LineOf(BotResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        StringBuilder line = new();
        line.Append(CultureInfo.InvariantCulture, $"{BotPolicyKinds.NameOf(result.Policy)} seed={result.Seed} end={BotEnds.NameOf(result.End)} tick={result.Tick} played={result.Played}");
        line.Append(CultureInfo.InvariantCulture, $" goal={(result.GoalPlayed is long goal ? goal.ToString(CultureInfo.InvariantCulture) : "none")} wipes={result.Wipes} battles=");
        List<string> battles = [];
        foreach (BattleCount battle in result.Battles)
        {
            battles.Add($"{battle.Turns}:{Battle.OutcomeName(battle.Outcome)}");
        }

        line.Append(battles.Count == 0 ? "none" : string.Join(',', battles));
        return line.ToString();
    }

    /// <summary>Gives the Markdown summary of the runs of one policy on one leg (D-1182).</summary>
    /// <param name="policy">The policy.</param>
    /// <param name="leg">The name of the leg, such as `ubuntu-24.04`.</param>
    /// <param name="totals">The totals of the runs.</param>
    /// <param name="seconds">The wall time of the runs, in whole seconds, for the count of D-1180.</param>
    /// <returns>The text, with a line end after each line.</returns>
    public static string Markdown(BotPolicyKind policy, string leg, BotTotals totals, long seconds)
    {
        ArgumentNullException.ThrowIfNull(leg);
        ArgumentNullException.ThrowIfNull(totals);

        StringBuilder text = new();
        text.Append(CultureInfo.InvariantCulture, $"### Bot runs: {BotPolicyKinds.NameOf(policy)} on {leg}\n\n");
        text.Append(CultureInfo.InvariantCulture, $"- Runs: {totals.Runs}, in {seconds} seconds.\n");
        text.Append(CultureInfo.InvariantCulture, $"- Ends: {Counts(totals.Ends)}.\n");
        text.Append(CultureInfo.InvariantCulture, $"- Played ticks to the goal: {Spread(totals.Goals)}.\n");
        text.Append(CultureInfo.InvariantCulture, $"- Battles: {totals.Turns.Count}. Outcomes: {Counts(totals.Outcomes)}.\n");
        text.Append(CultureInfo.InvariantCulture, $"- Turns in a battle: {Spread(totals.Turns)}.\n");
        return text.ToString();
    }

    private static string Counts(IReadOnlyDictionary<string, int> counts)
    {
        if (counts.Count == 0)
        {
            return "none";
        }

        List<string> parts = [];
        foreach (KeyValuePair<string, int> entry in counts)
        {
            parts.Add($"{entry.Key} {entry.Value}");
        }

        return string.Join(", ", parts);
    }

    /// <summary>Gives the minimum, the median, and the maximum, or `none` for an empty list. The median of an even count is the lower middle value.</summary>
    private static string Spread(IReadOnlyList<long> values)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        List<long> sorted = [.. values];
        sorted.Sort();
        long median = sorted[(sorted.Count - 1) / 2];
        return string.Create(CultureInfo.InvariantCulture, $"minimum {sorted[0]}, median {median}, maximum {sorted[^1]}");
    }
}
