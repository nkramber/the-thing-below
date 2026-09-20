using System;
using System.Collections.Generic;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// DOCS 1: the Documents rows of the PR template, the table of the `one-pr-one-session`
/// skill, and the list of the review gate are one list (D-579, D-581). The check runs on
/// every PR, so a documents PR that changes one of the three never merges with the other two
/// behind it.
/// </summary>
public static class DocumentRowRules
{
    /// <summary>The template, whose Documents section holds one line for each row.</summary>
    public const string TemplatePath = ".github/pull_request_template.md";

    /// <summary>The skill, whose Documents gate holds one table row for each row.</summary>
    public const string SkillPath = ".claude/skills/one-pr-one-session/SKILL.md";

    /// <summary>The heading of the section of the skill that holds the table.</summary>
    public const string SkillHeading = "## 3. Documents gate";

    /// <summary>Reads the template and the skill, and gives each row that differs from the gate.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>Each finding, in the order of the two files.</returns>
    /// <exception cref="InvalidOperationException">A file or a section is absent (T-2).</exception>
    public static IReadOnlyList<Finding> Check(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        List<Finding> findings = [];
        Compare(findings, TemplatePath, ReadTemplateRows(documents));
        Compare(findings, SkillPath, ReadSkillRows(documents));
        return findings;
    }

    /// <summary>Reads the row name of one line of the Documents section of the template.</summary>
    /// <param name="line">One line, such as "- `docs/design.md`:".</param>
    /// <returns>The row name, or null when the line is not a row.</returns>
    public static string? TemplateRowOf(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        string text = line.Trim();
        if (!text.StartsWith("- ", StringComparison.Ordinal) || !text.EndsWith(':'))
        {
            return null;
        }

        return text[2..^1].Replace("`", string.Empty, StringComparison.Ordinal).Trim();
    }

    /// <summary>Reads the row name of one table line of the Documents gate of the skill.</summary>
    /// <param name="line">One line, such as "| `docs/design.md` | intent |".</param>
    /// <returns>The row name, or null when the line is not a row with a path.</returns>
    public static string? SkillRowOf(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        string text = line.Trim();
        if (!text.StartsWith('|'))
        {
            return null;
        }

        string[] cells = text.Split('|');
        if (cells.Length < 2 || !cells[1].Contains('`', StringComparison.Ordinal))
        {
            return null;
        }

        return cells[1].Replace("`", string.Empty, StringComparison.Ordinal).Trim();
    }

    private static IReadOnlyList<string> ReadTemplateRows(DocumentSet documents)
    {
        IReadOnlyList<string> lines = ReadSection(documents, TemplatePath, DocumentRules.SectionHeading);
        List<string> rows = [];
        foreach (string line in lines)
        {
            string? row = TemplateRowOf(line);
            if (row is not null)
            {
                rows.Add(row);
            }
        }

        return rows;
    }

    private static IReadOnlyList<string> ReadSkillRows(DocumentSet documents)
    {
        IReadOnlyList<string> lines = ReadSection(documents, SkillPath, SkillHeading);
        List<string> rows = [];
        foreach (string line in lines)
        {
            string? row = SkillRowOf(line);
            if (row is not null)
            {
                rows.Add(row);
            }
        }

        return rows;
    }

    private static IReadOnlyList<string> ReadSection(DocumentSet documents, string path, string heading)
    {
        if (!documents.Holds(path))
        {
            throw new InvalidOperationException($"The file '{path}' is absent from the checkout (T-2).");
        }

        string text = string.Join('\n', documents.ReadLines(path));
        return MarkdownSection.ReadLines(text, heading)
            ?? throw new InvalidOperationException($"The file '{path}' holds no `{heading}` section (T-2).");
    }

    private static void Compare(List<Finding> findings, string path, IReadOnlyList<string> rows)
    {
        IReadOnlyList<string> wanted = DocumentRules.RequiredRows;
        int count = Math.Min(rows.Count, wanted.Count);
        for (int index = 0; index < count; index += 1)
        {
            if (!string.Equals(rows[index], wanted[index], StringComparison.Ordinal))
            {
                findings.Add(new Finding(
                    path,
                    1,
                    "DOCS 1",
                    $"row {index + 1} of the Documents rows is `{rows[index]}`, and the review gate holds `{wanted[index]}` there (D-579, D-581)"));
                return;
            }
        }

        if (rows.Count != wanted.Count)
        {
            findings.Add(new Finding(
                path,
                1,
                "DOCS 1",
                $"the file holds {rows.Count} Documents rows, and the review gate holds {wanted.Count} (D-579, D-581)"));
        }
    }
}
