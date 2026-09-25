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
        string? cell = TemplateCellOf(line);
        return cell is null ? null : NameOf(cell);
    }

    /// <summary>Reads the row name of one table line of the Documents gate of the skill.</summary>
    /// <param name="line">One line, such as "| `docs/design.md` | intent |".</param>
    /// <returns>The row name, or null when the line is not a row with a path.</returns>
    public static string? SkillRowOf(string line)
    {
        string? cell = SkillCellOf(line);
        return cell is null ? null : NameOf(cell);
    }

    /// <summary>
    /// Gives the written form of a row: each path in backticks, joined by "and". The gate test
    /// of the skill looks for this exact cell, so DOCS 1 reads this form (F-112).
    /// </summary>
    /// <param name="row">The row name, as `DocumentRules.RequiredRows` holds it.</param>
    /// <returns>The form, such as "`CLAUDE.md` and `AGENTS.md`".</returns>
    public static string CellOf(string row)
    {
        ArgumentException.ThrowIfNullOrEmpty(row);
        return "`" + string.Join("` and `", row.Split(" and ")) + "`";
    }

    private static string? TemplateCellOf(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        string text = line.Trim();
        if (!text.StartsWith("- ", StringComparison.Ordinal) || !text.EndsWith(':'))
        {
            return null;
        }

        return text[2..^1].Trim();
    }

    private static string? SkillCellOf(string line)
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

        return cells[1].Trim();
    }

    private static string NameOf(string cell) => cell.Replace("`", string.Empty, StringComparison.Ordinal).Trim();

    private static IReadOnlyList<string> ReadTemplateRows(DocumentSet documents)
    {
        IReadOnlyList<string> lines = ReadSection(documents, TemplatePath, DocumentRules.SectionHeading);
        List<string> cells = [];
        foreach (string line in lines)
        {
            string? cell = TemplateCellOf(line);
            if (cell is not null)
            {
                cells.Add(cell);
            }
        }

        return cells;
    }

    private static IReadOnlyList<string> ReadSkillRows(DocumentSet documents)
    {
        IReadOnlyList<string> lines = ReadSection(documents, SkillPath, SkillHeading);
        List<string> cells = [];
        foreach (string line in lines)
        {
            string? cell = SkillCellOf(line);
            if (cell is not null)
            {
                cells.Add(cell);
            }
        }

        return cells;
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

    /// <summary>Compares the written cell of each row with the gate, first by name, then by form.</summary>
    private static void Compare(List<Finding> findings, string path, IReadOnlyList<string> cells)
    {
        IReadOnlyList<string> wanted = DocumentRules.RequiredRows;
        int count = Math.Min(cells.Count, wanted.Count);
        for (int index = 0; index < count; index += 1)
        {
            string name = NameOf(cells[index]);
            if (!string.Equals(name, wanted[index], StringComparison.Ordinal))
            {
                findings.Add(new Finding(
                    path,
                    1,
                    "DOCS 1",
                    $"row {index + 1} of the Documents rows is `{name}`, and the review gate holds `{wanted[index]}` there (D-579, D-581)"));
                return;
            }

            string form = CellOf(wanted[index]);
            if (!string.Equals(cells[index], form, StringComparison.Ordinal))
            {
                findings.Add(new Finding(
                    path,
                    1,
                    "DOCS 1",
                    $"row {index + 1} of the Documents rows reads {cells[index]}, and the gate test reads the form {form} (D-581, F-112)"));
                return;
            }
        }

        if (cells.Count != wanted.Count)
        {
            findings.Add(new Finding(
                path,
                1,
                "DOCS 1",
                $"the file holds {cells.Count} Documents rows, and the review gate holds {wanted.Count} (D-579, D-581)"));
        }
    }
}
