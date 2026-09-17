using System;
using System.Collections.Generic;
using System.Text;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The size rules of the context budget (D-583, D-611). A session reads the start set in full:
/// the instructions file, the top entry of the handoff, and the skills of the task. Each file of
/// that set has a byte limit, and a file above its limit is a finding.
/// </summary>
public static class SizeRules
{
    /// <summary>The bytes of one kilobyte. The three limits below are the one place of the code.</summary>
    public const int Kilobyte = 1024;

    /// <summary>The limit of `CLAUDE.md` and `AGENTS.md`, 16 KB (D-583, D-611).</summary>
    public const int InstructionsLimitBytes = 16 * Kilobyte;

    /// <summary>The limit of the top entry of the handoff, 5 KB (D-583, D-611).</summary>
    public const int HandoffEntryLimitBytes = 5 * Kilobyte;

    /// <summary>The limit of one file of a skill, 36 KB (D-583, D-611).</summary>
    public const int SkillLimitBytes = 36 * Kilobyte;

    /// <summary>The two instructions files. D-20 keeps them identical, and each one is a start set file.</summary>
    public static readonly IReadOnlyList<string> InstructionsPaths = ["AGENTS.md", "CLAUDE.md"];

    /// <summary>The folder of the skills of this project (D-21).</summary>
    public const string SkillsFolder = ".claude/skills/";

    /// <summary>Reads the start set of the checkout, and gives every size finding.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>Each finding, in the order of the paths.</returns>
    /// <exception cref="InvalidOperationException">An instructions file, the handoff, or every skill file is absent.</exception>
    public static IReadOnlyList<Finding> Check(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        List<Finding> findings = [];
        foreach (string path in InstructionsPaths)
        {
            if (!documents.Holds(path))
            {
                throw new InvalidOperationException($"The instructions file '{path}' is absent (T-2).");
            }

            AddWhenAboveLimit(findings, path, 1, "SIZE 1", ByteCount(documents.ReadLines(path)), InstructionsLimitBytes);
        }

        findings.AddRange(CheckHandoffEntry(documents));
        findings.AddRange(CheckSkills(documents));
        return findings;
    }

    /// <summary>
    /// Counts the bytes of a text. Each line ending counts as one byte, so a checkout with a
    /// carriage return gives the number that each CI leg gives.
    /// </summary>
    /// <param name="lines">Each line of the text, with no line ending.</param>
    /// <returns>The number of UTF-8 bytes.</returns>
    public static int ByteCount(IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        int bytes = 0;
        foreach (string line in lines)
        {
            bytes += Encoding.UTF8.GetByteCount(line) + 1;
        }

        return bytes;
    }

    private static IReadOnlyList<Finding> CheckHandoffEntry(DocumentSet documents)
    {
        string path = SessionNumberRules.HandoffPaths[0];
        if (!documents.Holds(path))
        {
            throw new InvalidOperationException($"The handoff file '{path}' is absent (T-2).");
        }

        IReadOnlyList<string> lines = documents.ReadLines(path);
        int first = IndexOfHeading(lines, 0);
        if (first < 0)
        {
            throw new InvalidOperationException(
                $"The handoff '{path}' holds no line of the form '## Session <number>:' (T-2, D-18).");
        }

        int next = IndexOfHeading(lines, first + 1);
        int end = next < 0 ? lines.Count : next;
        List<string> entry = [];
        for (int index = first; index < end; index++)
        {
            entry.Add(lines[index]);
        }

        List<Finding> findings = [];
        AddWhenAboveLimit(findings, path, first + 1, "SIZE 2", ByteCount(entry), HandoffEntryLimitBytes);
        return findings;
    }

    private static IReadOnlyList<Finding> CheckSkills(DocumentSet documents)
    {
        List<Finding> findings = [];
        int skillFiles = 0;
        foreach (string path in documents.Documents)
        {
            if (!path.StartsWith(SkillsFolder, StringComparison.Ordinal))
            {
                continue;
            }

            skillFiles++;
            AddWhenAboveLimit(findings, path, 1, "SIZE 3", ByteCount(documents.ReadLines(path)), SkillLimitBytes);
        }

        if (skillFiles == 0)
        {
            throw new InvalidOperationException($"The checkout holds no skill file under '{SkillsFolder}' (T-2, D-21).");
        }

        return findings;
    }

    private static void AddWhenAboveLimit(List<Finding> findings, string path, int line, string rule, int bytes, int limit)
    {
        if (bytes <= limit)
        {
            return;
        }

        string subject = rule == "SIZE 2" ? "the top entry holds" : "the file holds";
        findings.Add(new Finding(
            path,
            line,
            rule,
            $"{subject} {bytes} bytes, and the limit is {limit} bytes (D-583)"));
    }

    private static int IndexOfHeading(IReadOnlyList<string> lines, int from)
    {
        for (int index = from; index < lines.Count; index++)
        {
            if (SessionNumberRules.IsSessionHeading(lines[index]))
            {
                return index;
            }
        }

        return -1;
    }
}
