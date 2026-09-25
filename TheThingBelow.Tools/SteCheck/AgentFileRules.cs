using System;
using System.Collections.Generic;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The two rules of the instructions files. AGENTS 1: `CLAUDE.md` and `AGENTS.md` stay identical
/// (D-20). AGENTS 2: each file of the test command holds the Smoke filter one time (D-592). The
/// ste-check job runs on every PR, and a docs-only PR skips the build and test job, so these
/// rules read the files on the PR shape that breaks them most often (D-600, D-857).
/// </summary>
public static class AgentFileRules
{
    /// <summary>The instructions file that one provider reads (D-20).</summary>
    public const string ClaudePath = "CLAUDE.md";

    /// <summary>The instructions file that the other provider reads (D-20).</summary>
    public const string AgentsPath = "AGENTS.md";

    /// <summary>The id of the rule, as the `ste-writing` skill names it.</summary>
    public const string RuleId = "AGENTS 1";

    /// <summary>The id of the rule of the test command, as the `ste-writing` skill names it.</summary>
    public const string TestCommandRuleId = "AGENTS 2";

    /// <summary>
    /// The option that excludes the Smoke category from a test command. It matches
    /// `TestCategories.ExcludeSmoke` of Tests, and a test holds the match (D-592).
    /// </summary>
    public const string ExcludeSmoke = "--filter-not-trait \"Category=Smoke\"";

    /// <summary>
    /// Each file of the skip set that `TestFilterTests` reads. Each one holds the test command
    /// with <see cref="ExcludeSmoke"/> exactly one time. A docs-only PR skips that test, so this
    /// rule reads the same count on every PR (D-857).
    /// </summary>
    public static readonly IReadOnlyList<string> TestCommandPaths =
    [
        ClaudePath,
        AgentsPath,
        ".claude/skills/csharp-conventions/SKILL.md",
    ];

    /// <summary>Reads the two instructions files, and gives one finding when they differ.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>No finding, or one finding at the first line that differs.</returns>
    /// <exception cref="InvalidOperationException">One of the two files is absent.</exception>
    public static IReadOnlyList<Finding> Check(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        foreach (string path in new[] { ClaudePath, AgentsPath })
        {
            if (!documents.Holds(path))
            {
                throw new InvalidOperationException($"The instructions file '{path}' is absent (D-20, T-2).");
            }
        }

        IReadOnlyList<string> claudeLines = documents.ReadLines(ClaudePath);
        IReadOnlyList<string> agentsLines = documents.ReadLines(AgentsPath);
        int shared = Math.Min(claudeLines.Count, agentsLines.Count);
        for (int index = 0; index < shared; index++)
        {
            if (!string.Equals(claudeLines[index], agentsLines[index], StringComparison.Ordinal))
            {
                return [DifferenceAt(index + 1, $"line {index + 1} differs from line {index + 1} of `{ClaudePath}`")];
            }
        }

        if (claudeLines.Count != agentsLines.Count)
        {
            return [DifferenceAt(
                shared + 1,
                $"the file holds {agentsLines.Count} lines, and `{ClaudePath}` holds {claudeLines.Count}")];
        }

        // A read of lines drops each line end, so two files that differ in a line end alone, or
        // in the end of the last line, pass the loop above. The test of D-20 compares the whole
        // text, so this rule compares each byte too (F-112).
        if (!documents.ReadBytes(ClaudePath).AsSpan().SequenceEqual(documents.ReadBytes(AgentsPath)))
        {
            return [DifferenceAt(
                Math.Max(agentsLines.Count, 1),
                $"each line matches `{ClaudePath}`, and the bytes differ in a line end or in the end of the file")];
        }

        return [];
    }

    /// <summary>Reads each file of <see cref="TestCommandPaths"/>, and gives one finding for each file with another count.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>Each finding, in the order of <see cref="TestCommandPaths"/>.</returns>
    /// <exception cref="InvalidOperationException">One of the files is absent.</exception>
    public static IReadOnlyList<Finding> CheckTestCommand(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        List<Finding> findings = [];
        foreach (string path in TestCommandPaths)
        {
            if (!documents.Holds(path))
            {
                throw new InvalidOperationException($"The file '{path}' of the test command is absent (D-592, T-2).");
            }

            int count = 0;
            int firstLine = 1;
            IReadOnlyList<string> lines = documents.ReadLines(path);
            for (int index = 0; index < lines.Count; index++)
            {
                for (int at = lines[index].IndexOf(ExcludeSmoke, StringComparison.Ordinal);
                     at >= 0;
                     at = lines[index].IndexOf(ExcludeSmoke, at + 1, StringComparison.Ordinal))
                {
                    count += 1;
                    if (count == 1)
                    {
                        firstLine = index + 1;
                    }
                }
            }

            if (count != 1)
            {
                findings.Add(new Finding(
                    path,
                    firstLine,
                    TestCommandRuleId,
                    $"the file holds the option `{ExcludeSmoke}` {count} time(s), and the test command holds it one time (D-592, D-857)"));
            }
        }

        return findings;
    }

    private static Finding DifferenceAt(int line, string detail)
    {
        return new Finding(
            AgentsPath,
            line,
            RuleId,
            $"{detail}. Edit both files together, because they stay identical (D-20)");
    }
}
