using System;
using System.Collections.Generic;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>
/// The rules of the `review-override` label (D-16, D-71, D-239, D-401, D-560, D-609, D-700). The label
/// exempts a documentation pull request from the review of the other provider, and from nothing
/// else. Two conditions hold together: every changed path is in the eligible set, and the pull
/// request changes no row of a decision table.
/// </summary>
public static class OverrideRules
{
    /// <summary>The name of the label on the repository (D-67).</summary>
    public const string LabelName = "review-override";

    /// <summary>The path of the decision register, which D-401 and D-609 read.</summary>
    public const string DecisionsPath = "docs/decisions.md";

    /// <summary>
    /// The folders of the eligible set. Each path under one of them is eligible, except each
    /// path of <see cref="HarnessSettingsPaths"/>.
    /// </summary>
    public static readonly IReadOnlyList<string> EligibleFolders = ["docs/", ".claude/"];

    /// <summary>The single files of the eligible set.</summary>
    public static readonly IReadOnlyList<string> EligibleFiles =
    [
        "CLAUDE.md",
        "AGENTS.md",
        "README.md",
        ".github/pull_request_template.md",
    ];

    /// <summary>The path of every gate, which D-560 keeps out of the eligible set.</summary>
    public const string WorkflowFolder = ".github/workflows/";

    /// <summary>
    /// The settings file of the harness, which D-700 keeps out of the eligible set. It can hold
    /// a hook that runs a command in each session, so it takes the guard of a workflow file.
    /// </summary>
    public const string HarnessSettingsPath = ".claude/settings.json";

    /// <summary>
    /// The local settings file of the harness. It holds the same hooks and permissions as
    /// <see cref="HarnessSettingsPath"/>, so a commit of it takes the same guard. Git ignores
    /// it, and a forced add still reaches this rule (D-1086).
    /// </summary>
    public const string HarnessLocalSettingsPath = ".claude/settings.local.json";

    /// <summary>The settings files of the harness, which the eligible set leaves out (D-700, D-1086).</summary>
    public static readonly IReadOnlyList<string> HarnessSettingsPaths = [HarnessSettingsPath, HarnessLocalSettingsPath];

    /// <summary>Reads the label set of the pull request.</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <returns>True when the pull request carries the `review-override` label.</returns>
    public static bool CarriesLabel(PullRequestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        foreach (string label in facts.Labels)
        {
            if (string.Equals(label, LabelName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>RG 1: every changed path of a labeled pull request is in the eligible set.</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <returns>The result of the rule.</returns>
    public static GateCheck CheckPaths(PullRequestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (!CarriesLabel(facts))
        {
            return new GateCheck("RG 1", GateResult.Pass, $"the PR carries no `{LabelName}` label.");
        }

        List<string> outside = [];
        foreach (string file in facts.Files)
        {
            if (!IsEligible(file))
            {
                outside.Add(file);
            }
        }

        if (outside.Count == 0)
        {
            return new GateCheck(
                "RG 1",
                GateResult.Pass,
                $"the PR carries the `{LabelName}` label, and every changed path is in the eligible set (D-71, D-239).");
        }

        return new GateCheck(
            "RG 1",
            GateResult.Fault,
            $"the PR carries the `{LabelName}` label, and it changes {Describe(outside)}. {ReasonFor(outside[0])}.");
    }

    /// <summary>RG 2: a labeled pull request changes no row of a decision table (D-401, D-609).</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <returns>The result of the rule.</returns>
    public static GateCheck CheckDecisionRows(PullRequestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (!CarriesLabel(facts))
        {
            return new GateCheck("RG 2", GateResult.Pass, $"the PR carries no `{LabelName}` label.");
        }

        IReadOnlyList<string> rows = ChangedDecisionRows(facts.DecisionsDiff);
        if (rows.Count == 0)
        {
            return new GateCheck(
                "RG 2",
                GateResult.Pass,
                $"the PR changes no row of a decision table in `{DecisionsPath}` (D-609).");
        }

        return new GateCheck(
            "RG 2",
            GateResult.Fault,
            $"the PR changes {rows.Count} row(s) of a decision table, and the first one is `{Shorten(rows[0])}`. " +
            $"A PR that adds or revises a decision row takes the review of the other provider (D-401, D-609).");
    }

    /// <summary>
    /// Reads a unified diff of `docs/decisions.md` and gives each decision row that it adds or
    /// removes. The head line and the divider line of a table are not decision rows (D-609).
    /// </summary>
    /// <param name="diff">The unified diff of the decision register, or an empty text.</param>
    /// <returns>Each changed row, in the order of the diff.</returns>
    public static IReadOnlyList<string> ChangedDecisionRows(string diff)
    {
        ArgumentNullException.ThrowIfNull(diff);
        List<string> rows = [];
        foreach (string line in diff.Split('\n'))
        {
            if (line.StartsWith("+++", StringComparison.Ordinal) || line.StartsWith("---", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.Length < 2 || (line[0] != '+' && line[0] != '-'))
            {
                continue;
            }

            string content = line[1..].Trim();
            if (IsDecisionRow(content))
            {
                rows.Add(content);
            }
        }

        return rows;
    }

    /// <summary>Reads one line of the register and says whether it is a decision row.</summary>
    /// <param name="line">One line of `docs/decisions.md`, with no leading space.</param>
    /// <returns>True when the line is a row of a decision table.</returns>
    public static bool IsDecisionRow(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (!line.StartsWith('|'))
        {
            return false;
        }

        bool divider = true;
        foreach (char letter in line)
        {
            if (letter != '|' && letter != '-' && letter != ':' && letter != ' ')
            {
                divider = false;
                break;
            }
        }

        if (divider)
        {
            return false;
        }

        string firstCell = line.Split('|')[1].Trim();
        return !string.Equals(firstCell, "Id", StringComparison.Ordinal)
            && !string.Equals(firstCell, "#", StringComparison.Ordinal);
    }

    private static bool IsEligible(string file)
    {
        // The settings files of `.claude/` take the review, so the refusal comes before the
        // folder match (D-700, D-1086).
        if (IsHarnessSettings(file))
        {
            return false;
        }

        foreach (string folder in EligibleFolders)
        {
            if (file.StartsWith(folder, StringComparison.Ordinal))
            {
                return true;
            }
        }

        foreach (string eligible in EligibleFiles)
        {
            if (string.Equals(file, eligible, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string ReasonFor(string path)
    {
        if (path.StartsWith(WorkflowFolder, StringComparison.Ordinal))
        {
            return "Each gate lives in a workflow file, so a workflow change takes the review (D-560)";
        }

        if (IsHarnessSettings(path))
        {
            return "A settings file of the harness can hold a hook that runs a command, so it takes the review (D-700, D-1086)";
        }

        return "A path outside the eligible set takes the review of the other provider (D-16, D-71)";
    }

    /// <summary>Reads whether one path is a settings file of the harness, in any case of its letters.</summary>
    /// <param name="path">A path from the root of the checkout, with forward slashes, as git gives it.</param>
    /// <returns>True when the path names <see cref="HarnessSettingsPath"/> or <see cref="HarnessLocalSettingsPath"/>.</returns>
    public static bool IsHarnessSettings(string path)
    {
        // macOS and Windows resolve a case variant of the path to the same settings file, so the
        // compare ignores case (D-700, D-1086).
        foreach (string settings in HarnessSettingsPaths)
        {
            if (string.Equals(path, settings, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Describe(IReadOnlyList<string> paths)
    {
        string first = $"`{paths[0]}`";
        return paths.Count == 1 ? first : $"{first} and {paths.Count - 1} other path(s)";
    }

    private static string Shorten(string row) => row.Length <= 60 ? row : row[..60] + "...";
}
