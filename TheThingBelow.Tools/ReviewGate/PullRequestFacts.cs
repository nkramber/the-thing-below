using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>One commit of the pull request, with the paths that it changes.</summary>
/// <param name="Sha">The full hash of the commit.</param>
/// <param name="Files">The paths that the commit changes, from the root of the checkout.</param>
public sealed record CommitFacts(string Sha, IReadOnlyList<string> Files);

/// <summary>
/// The facts of one pull request that the `review-gate` command reads. The workflow collects
/// them from GitHub and from git, and it writes them to one JSON file. The command runs from
/// `main` and never runs code of the head (D-15).
/// </summary>
/// <param name="Number">The GitHub number of the pull request, not a roadmap id (D-13).</param>
/// <param name="Body">The description of the pull request.</param>
/// <param name="Labels">The name of each label on the pull request.</param>
/// <param name="Files">Each path that the pull request changes, from the merge base to the head.</param>
/// <param name="Commits">Each commit from the merge base to the head, oldest first.</param>
/// <param name="DecisionsDiff">
/// The unified diff of `docs/decisions.md` from the merge base to the head. It is empty when the
/// pull request does not change that file.
/// </param>
public sealed record PullRequestFacts(
    int Number,
    string Body,
    IReadOnlyList<string> Labels,
    IReadOnlyList<string> Files,
    IReadOnlyList<CommitFacts> Commits,
    string DecisionsDiff)
{
    /// <summary>Reads the facts of one pull request from a JSON file.</summary>
    /// <param name="path">The path of the JSON file that the workflow wrote.</param>
    /// <returns>The facts of that pull request.</returns>
    /// <exception cref="InvalidOperationException">A field is absent, empty, or of the wrong kind.</exception>
    public static PullRequestFacts Read(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        string text = File.ReadAllText(path);
        using JsonDocument document = JsonDocument.Parse(text);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"The file '{path}' holds {root.ValueKind} at its root, and the command needs an object.");
        }

        int number = ReadNumber(root, "number", path);
        List<CommitFacts> commits = ReadCommits(root, path);
        List<string> files = ReadStringList(root, "files", path);
        if (files.Count == 0)
        {
            throw new InvalidOperationException(
                $"The field 'files' of '{path}' is empty, and a pull request changes at least one path.");
        }

        return new PullRequestFacts(
            number,
            ReadString(root, "body", path),
            ReadStringList(root, "labels", path),
            files,
            commits,
            ReadString(root, "decisionsDiff", path));
    }

    private static JsonElement ReadField(JsonElement owner, string field, string path)
    {
        if (!owner.TryGetProperty(field, out JsonElement value))
        {
            throw new InvalidOperationException(
                $"The file '{path}' holds no field '{field}'. An absent field is an error (T-2).");
        }

        return value;
    }

    private static int ReadNumber(JsonElement owner, string field, string path)
    {
        JsonElement value = ReadField(owner, field, path);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int number) || number <= 0)
        {
            throw new InvalidOperationException(
                $"The field '{field}' of '{path}' is '{value}', and the command needs a number above zero.");
        }

        return number;
    }

    private static string ReadString(JsonElement owner, string field, string path)
    {
        JsonElement value = ReadField(owner, field, path);
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                $"The field '{field}' of '{path}' holds {value.ValueKind}, and the command needs a string.");
        }

        return value.GetString() ?? throw new InvalidOperationException(
            $"The field '{field}' of '{path}' holds no text.");
    }

    private static List<string> ReadStringList(JsonElement owner, string field, string path)
    {
        JsonElement value = ReadField(owner, field, path);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"The field '{field}' of '{path}' holds {value.ValueKind}, and the command needs an array.");
        }

        List<string> items = [];
        foreach (JsonElement item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    $"The field '{field}' of '{path}' holds an item of kind {item.ValueKind}, and each item is a string.");
            }

            items.Add(item.GetString() ?? throw new InvalidOperationException(
                $"The field '{field}' of '{path}' holds an item with no text."));
        }

        return items;
    }

    private static List<CommitFacts> ReadCommits(JsonElement owner, string path)
    {
        JsonElement value = ReadField(owner, "commits", path);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"The field 'commits' of '{path}' holds {value.ValueKind}, and the command needs an array.");
        }

        List<CommitFacts> commits = [];
        int index = 0;
        foreach (JsonElement item in value.EnumerateArray())
        {
            string position = index.ToString(CultureInfo.InvariantCulture);
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    $"The commit at position {position} of '{path}' holds {item.ValueKind}, and each commit is an object.");
            }

            commits.Add(new CommitFacts(
                ReadString(item, "sha", $"{path}, commit {position}"),
                ReadStringList(item, "files", $"{path}, commit {position}")));
            index++;
        }

        if (commits.Count == 0)
        {
            throw new InvalidOperationException(
                $"The field 'commits' of '{path}' is empty, and a pull request holds at least one commit.");
        }

        return commits;
    }
}
