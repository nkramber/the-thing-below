using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tests;

/// <summary>
/// A fixture pull request in a temporary folder: the JSON file of facts and the files of the
/// head. The gate cannot run live on the pull request that creates it, so these fixtures are the
/// proof of the command (F-37, D-500).
/// </summary>
public sealed class ReviewGateFixture : IDisposable
{
    /// <summary>The GitHub number of the fixture pull request.</summary>
    public const int Number = 21;

    /// <summary>The commit that changes code, which is the effective head (D-610).</summary>
    public const string HeadSha = "a1b2c3d4e5f60718293a4b5c6d7e8f9012345678";

    /// <summary>The commit that changes the metadata set alone, and moves no head (D-610).</summary>
    public const string MetadataSha = "b2c3d4e5f60718293a4b5c6d7e8f90123456789a";

    /// <summary>A commit of code before the effective head, which a stale record names.</summary>
    public const string OlderSha = "c3d4e5f60718293a4b5c6d7e8f90123456789ab2";

    /// <summary>
    /// The writer options of the facts file. `JsonSerializer` needs runtime reflection, and
    /// `Directory.Build.props` turns that off for every program of the solution (D-647,
    /// F-36). Thus the fixture writes each field itself, as the workflow does.
    /// </summary>
    private static readonly JsonWriterOptions WriteOptions = new JsonWriterOptions { Indented = true };

    private ReviewGateFixture(string root)
    {
        Root = root;
        HeadFiles = Path.Combine(root, "head-files");
        FactsPath = Path.Combine(root, "pull-request.json");
    }

    /// <summary>The full path of the temporary folder of this fixture.</summary>
    public string Root { get; }

    /// <summary>The folder that holds the files of the head, as data alone.</summary>
    public string HeadFiles { get; }

    /// <summary>The path of the JSON file with the facts of the pull request.</summary>
    public string FactsPath { get; }

    /// <summary>Builds a fixture that passes every rule from RG 1 to RG 8.</summary>
    /// <returns>The fixture. The caller disposes it.</returns>
    public static ReviewGateFixture Build()
    {
        string root = Path.Combine(Path.GetTempPath(), "review-gate-" + Guid.NewGuid().ToString("N"));
        ReviewGateFixture fixture = new ReviewGateFixture(root);
        Directory.CreateDirectory(Path.Combine(fixture.HeadFiles, "docs", "reviews"));
        fixture.WriteFacts(PassingFacts());
        fixture.WriteRecord(Record(HeadSha, "Ready for owner merge"));
        return fixture;
    }

    /// <summary>Gives the facts of a pull request that passes every rule.</summary>
    /// <returns>The facts of the fixture pull request.</returns>
    public static PullRequestFacts PassingFacts() => new PullRequestFacts(
        Number,
        Body(),
        [],
        ["TheThingBelow.Core/Rules.cs", "docs/design.md", "docs/reviews/pr-21.md", "docs/session-handoff.md"],
        [
            new CommitFacts(HeadSha, ["TheThingBelow.Core/Rules.cs", "docs/design.md"]),
            new CommitFacts(MetadataSha, ["docs/reviews/pr-21.md", "docs/session-handoff.md"]),
        ],
        string.Empty);

    /// <summary>Gives the facts of a documentation pull request that carries the label.</summary>
    /// <returns>The facts of a pull request in the override set (D-16, D-71, D-239).</returns>
    public static PullRequestFacts LabeledFacts() => PassingFacts() with
    {
        Labels = [OverrideRules.LabelName],
        Files = ["docs/design.md", "docs/session-handoff.md"],
        Commits = [new CommitFacts(HeadSha, ["docs/design.md", "docs/session-handoff.md"])],
    };

    /// <summary>Builds the Documents section of a description (D-581).</summary>
    /// <param name="changes">
    /// The text after the colon of each row that the test changes. A null value removes the line
    /// of that row.
    /// </param>
    /// <returns>The description of the pull request.</returns>
    public static string Body(IReadOnlyDictionary<string, string?>? changes = null)
    {
        Dictionary<string, string?> rows = new Dictionary<string, string?>(DefaultRows, StringComparer.Ordinal);
        if (changes is not null)
        {
            foreach (KeyValuePair<string, string?> change in changes)
            {
                rows[change.Key] = change.Value;
            }
        }

        List<string> lines = ["## Summary", string.Empty, "The one concern of this PR.", string.Empty, "## Documents", string.Empty];
        foreach (string row in DocumentRules.RequiredRows)
        {
            if (rows[row] is string content)
            {
                lines.Add($"- `{row}`: {content}");
            }
        }

        return string.Join('\n', lines) + "\n";
    }

    /// <summary>Builds a review record with the head and the verdict of a test.</summary>
    /// <param name="head">The value of the head field of the Identity list.</param>
    /// <param name="verdict">The verdict name of the Verdict section.</param>
    /// <returns>The text of the record.</returns>
    public static string Record(string head, string verdict) => string.Join(
        '\n',
        $"# PR-{Number} review",
        string.Empty,
        "Date: 2026-09-17",
        string.Empty,
        "## Identity",
        string.Empty,
        $"- PR: {Number}",
        "- Target: `main`",
        $"- Head: `{head}`",
        string.Empty,
        "## Findings",
        string.Empty,
        "No finding.",
        string.Empty,
        "## Verdict",
        string.Empty,
        $"**{verdict}.** This verdict applies to head `{head}`.",
        string.Empty);

    /// <summary>Writes the JSON file of facts that the command reads.</summary>
    /// <param name="facts">The facts of the pull request.</param>
    public void WriteFacts(PullRequestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        using MemoryStream bytes = new();
        using (Utf8JsonWriter writer = new(bytes, WriteOptions))
        {
            writer.WriteStartObject();
            writer.WriteNumber("number", facts.Number);
            writer.WriteString("body", facts.Body);
            WriteStrings(writer, "labels", facts.Labels);
            WriteStrings(writer, "files", facts.Files);
            writer.WriteStartArray("commits");
            foreach (CommitFacts commit in facts.Commits)
            {
                writer.WriteStartObject();
                writer.WriteString("sha", commit.Sha);
                WriteStrings(writer, "files", commit.Files);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteString("decisionsDiff", facts.DecisionsDiff);
            writer.WriteEndObject();
        }

        File.WriteAllBytes(FactsPath, bytes.ToArray());
    }

    private static void WriteStrings(Utf8JsonWriter writer, string field, IReadOnlyList<string> values)
    {
        writer.WriteStartArray(field);
        foreach (string value in values)
        {
            writer.WriteStringValue(value);
        }

        writer.WriteEndArray();
    }

    /// <summary>Writes the review record into the files of the head.</summary>
    /// <param name="text">The text of the record.</param>
    public void WriteRecord(string text) =>
        File.WriteAllText(Path.Combine(HeadFiles, "docs", "reviews", $"pr-{Number}.md"), text);

    /// <summary>Removes the review record from the files of the head.</summary>
    public void DeleteRecord() =>
        File.Delete(Path.Combine(HeadFiles, "docs", "reviews", $"pr-{Number}.md"));

    /// <summary>Removes the temporary folder of this fixture.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    private static readonly IReadOnlyDictionary<string, string?> DefaultRows =
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            // Each line agrees with the changed paths of `PassingFacts` and of `LabeledFacts`,
            // because RG 7 reads the truth of a line against the diff (D-577).
            ["docs/design.md"] = "Changed: `docs/design.md`. The section on the gate names the command.",
            ["docs/decisions.md"] = "No change needed because `docs/decisions.md` holds no answer of this PR.",
            ["docs/questions.md"] = "No change needed because `docs/questions.md` holds no question of this PR.",
            ["docs/roadmaps/"] = "No change needed because `docs/roadmaps/area-tools.md` names the command already.",
            ["docs/world/"] = "Not applicable because this PR holds no lore and no place.",
            ["docs/runbooks/"] = "No change needed because `docs/runbooks/session-context.md` needs no new step.",
            ["docs/reviews/"] = "No change needed because `docs/reviews/pr-21.md` comes from the reviewer of this PR.",
            ["docs/session-handoff.md"] = "Changed: `docs/session-handoff.md`. The entry of this session is at the top.",
            ["CLAUDE.md and AGENTS.md"] = "No change needed because `CLAUDE.md` and `AGENTS.md` hold no rule that this PR changes.",
            [".claude/skills/ and .claude/agents/"] = "No change needed because `.claude/skills/pr-review/SKILL.md` names the rule already.",
            [".github/pull_request_template.md"] = "No change needed because `.github/pull_request_template.md` holds each line of the gate.",
            ["README.md"] = "No change needed because `README.md` describes the project, and this PR changes no description.",
        };
}
