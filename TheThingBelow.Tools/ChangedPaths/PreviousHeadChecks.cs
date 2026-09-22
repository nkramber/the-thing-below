using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.ChangedPaths;

/// <summary>One check run of a commit, as the check runs API of GitHub gives it.</summary>
/// <param name="Name">The name of the check run, which is the name of its job.</param>
/// <param name="Status">The status, such as `completed` or `in_progress`.</param>
/// <param name="Conclusion">The conclusion, such as `success`. Null until the run completes.</param>
/// <param name="App">The slug of the app that wrote the run, such as `github-actions`.</param>
public sealed record CheckRunFacts(string Name, string Status, string? Conclusion, string App);

/// <summary>
/// The check runs of the previous head of a PR. A docs-only push skips a job only when that
/// job passed on the previous head (D-856, D-858).
/// </summary>
public static class PreviousHeadChecks
{
    /// <summary>The slug of the app that runs the workflows of this repository.</summary>
    public const string ActionsApp = "github-actions";

    /// <summary>
    /// The name of each check that a docs-only change skips. Each one must pass on the previous
    /// head. ste-check and review-gate run on every head, so they are not in the list (D-858).
    /// </summary>
    public static readonly IReadOnlyList<string> SkippedCheckNames =
    [
        "build, test, and format",
        "coverage report",
        "det-lint",
        "replay-identity",
        "screen-test",
        "smoke",
    ];

    /// <summary>Reads the check runs from a file of JSON lines, one object for each run.</summary>
    /// <param name="path">The path of the file that the workflow wrote.</param>
    /// <returns>Each check run, in the order of the file.</returns>
    /// <exception cref="InvalidOperationException">A line is not an object, or a field is absent or of the wrong kind.</exception>
    public static IReadOnlyList<CheckRunFacts> Read(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        List<CheckRunFacts> runs = [];
        string[] lines = File.ReadAllLines(path);
        for (int index = 0; index < lines.Length; index++)
        {
            if (lines[index].Length == 0)
            {
                continue;
            }

            string where = $"line {index + 1} of '{path}'";
            using JsonDocument document = JsonDocument.Parse(lines[index]);
            JsonElement run = document.RootElement;
            if (run.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    $"The {where} holds {run.ValueKind}, and the command needs an object (T-2).");
            }

            runs.Add(new CheckRunFacts(
                ReadText(run, "name", where),
                ReadText(run, "status", where),
                ReadConclusion(run, where),
                ReadText(run, "app", where)));
        }

        return runs;
    }

    /// <summary>
    /// Gives each skipped check that did not pass. A check passes when a run of that name from
    /// GitHub Actions exists, and every such run completed with the conclusion `success`.
    /// </summary>
    /// <param name="runs">The check runs of the previous head.</param>
    /// <returns>Each name of <see cref="SkippedCheckNames"/> that did not pass, with the reason.</returns>
    public static IReadOnlyList<string> FindNotPassed(IReadOnlyList<CheckRunFacts> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);

        List<string> notPassed = [];
        foreach (string name in SkippedCheckNames)
        {
            int found = 0;
            string? fault = null;
            foreach (CheckRunFacts run in runs)
            {
                if (!string.Equals(run.Name, name, StringComparison.Ordinal) ||
                    !string.Equals(run.App, ActionsApp, StringComparison.Ordinal))
                {
                    continue;
                }

                found += 1;

                // A rerun leaves the earlier run in the list when the API gives every run. Each
                // run of the name must pass, so an old failure never hides under a new pass (T-2).
                if (!string.Equals(run.Status, "completed", StringComparison.Ordinal))
                {
                    fault ??= $"'{name}' has the status '{run.Status}'";
                }
                else if (!string.Equals(run.Conclusion, "success", StringComparison.Ordinal))
                {
                    fault ??= $"'{name}' ended with '{run.Conclusion ?? "no conclusion"}'";
                }
            }

            if (found == 0)
            {
                notPassed.Add($"'{name}' has no run");
            }
            else if (fault is not null)
            {
                notPassed.Add(fault);
            }
        }

        return notPassed;
    }

    private static JsonElement ReadField(JsonElement run, string field, string where)
    {
        if (!run.TryGetProperty(field, out JsonElement value))
        {
            throw new InvalidOperationException(
                $"The {where} holds no field '{field}'. An absent field is an error (T-2).");
        }

        return value;
    }

    private static string ReadText(JsonElement run, string field, string where)
    {
        JsonElement value = ReadField(run, field, where);
        string? text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidOperationException(
                $"The field '{field}' of the {where} holds {value.ValueKind}, and the command needs text (T-2).");
        }

        return text;
    }

    private static string? ReadConclusion(JsonElement run, string where)
    {
        // The API gives null until the run completes, so null is a value here and not an
        // absent field. The field itself must be present (T-2).
        JsonElement value = ReadField(run, "conclusion", where);
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return ReadText(run, "conclusion", where);
    }
}
