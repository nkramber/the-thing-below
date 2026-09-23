using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>One issue comment of the pull request, as the `codex-review` command reads it.</summary>
/// <param name="Login">The login of the author. The REST API names Gitar `gitar-bot[bot]`.</param>
/// <param name="CreatedAt">The time of the comment, in UTC, such as `2026-09-23T06:53:26Z`.</param>
/// <param name="UpdatedAt">The time of the last edit, in UTC.</param>
/// <param name="IsDashboard">True when the body holds the `Code Review` block of Gitar.</param>
/// <param name="IsRequest">True when the body is the request `Gitar review` alone.</param>
public sealed record GitarComment(string Login, string CreatedAt, string UpdatedAt, bool IsDashboard, bool IsRequest);

/// <summary>One review thread of the pull request.</summary>
/// <param name="IsResolved">True when the thread is resolved.</param>
/// <param name="Author">The login of the first comment. The GraphQL API names Gitar `gitar-bot`.</param>
public sealed record GitarThread(bool IsResolved, string Author);

/// <summary>The facts of the Gitar pass of one pull request (D-14).</summary>
/// <param name="Comments">Each issue comment of the pull request.</param>
/// <param name="CheckStatuses">The status of each Gitar check run on the tip of the branch.</param>
/// <param name="Threads">Each review thread of the pull request.</param>
/// <param name="PushTime">
/// The time of the push that brought the effective head: the first check suite of a commit from
/// the effective head to the tip. Null when no such suite exists.
/// </param>
public sealed record GitarFacts(
    IReadOnlyList<GitarComment> Comments,
    IReadOnlyList<string> CheckStatuses,
    IReadOnlyList<GitarThread> Threads,
    string? PushTime);

/// <summary>
/// The part of a complete Gitar pass that a machine can read (D-14, D-705). The pass is current
/// for the effective head, no Gitar check runs, and no review thread of Gitar is open. A claim of
/// the dashboard text needs an answer too, and the author session checks it with the
/// `gitar-review` skill, because no rule can read the prose of a claim.
/// </summary>
public static class GitarPass
{
    /// <summary>The login of Gitar in the REST API.</summary>
    public const string RestLogin = "gitar-bot[bot]";

    /// <summary>The login of Gitar in the GraphQL API.</summary>
    public const string GraphLogin = "gitar-bot";

    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;

    /// <summary>A time of the GitHub API in UTC. The form of each time is the same, so an ordinal order is the order of the times.</summary>
    private static readonly Regex UtcTime = new Regex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z$", Options);

    /// <summary>The statuses of a check run that did not complete.</summary>
    private static readonly IReadOnlyList<string> RunningStatuses = ["queued", "in_progress", "pending", "waiting", "requested"];

    /// <summary>Gives each reason why the Gitar pass is not complete.</summary>
    /// <param name="facts">The facts of the pass.</param>
    /// <returns>Each reason. An empty list means that the machine part of the pass is complete.</returns>
    /// <exception cref="InvalidOperationException">A time is not in the UTC form of the GitHub API (T-2).</exception>
    public static IReadOnlyList<string> Check(GitarFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        List<string> reasons = [];

        string? dashboard = null;
        string? request = null;
        foreach (GitarComment comment in facts.Comments)
        {
            RequireTime(comment.CreatedAt);
            RequireTime(comment.UpdatedAt);
            if (comment.IsDashboard && comment.Login == RestLogin && Later(comment.UpdatedAt, dashboard))
            {
                dashboard = comment.UpdatedAt;
            }

            if (comment.IsRequest && Later(comment.CreatedAt, request))
            {
                request = comment.CreatedAt;
            }
        }

        if (dashboard is null)
        {
            reasons.Add("the PR holds no Gitar dashboard comment, so no Gitar review exists (D-14).");
        }

        if (facts.PushTime is null)
        {
            reasons.Add("no commit from the effective head to the tip has a check suite, so the push time is unknown (T-2).");
        }
        else
        {
            RequireTime(facts.PushTime);
        }

        if (dashboard is not null && facts.PushTime is not null)
        {
            if (string.CompareOrdinal(dashboard, facts.PushTime) <= 0)
            {
                reasons.Add($"the Gitar dashboard has its last edit at {dashboard}, not after the push at {facts.PushTime}, so the review is stale (D-705).");
            }
            else if (request is not null
                && string.CompareOrdinal(request, facts.PushTime) > 0
                && string.CompareOrdinal(dashboard, request) <= 0)
            {
                reasons.Add($"the request `Gitar review` at {request} has no dashboard edit after it, so the manual review did not end (D-705).");
            }
        }

        foreach (string status in facts.CheckStatuses)
        {
            if (Contains(RunningStatuses, status))
            {
                reasons.Add($"a Gitar check on the tip has the status `{status}`, so the review runs now (D-705).");
            }
        }

        int open = 0;
        foreach (GitarThread thread in facts.Threads)
        {
            if (!thread.IsResolved && thread.Author == GraphLogin)
            {
                open++;
            }
        }

        if (open > 0)
        {
            reasons.Add($"{open} review thread(s) of Gitar are open. Answer each one, then resolve it (D-14, D-67).");
        }

        return reasons;
    }

    /// <summary>Gives the `gh` arguments that list each issue comment of the pull request, on every page.</summary>
    /// <param name="repo">The owner and the name, such as `nkramber/the-thing-below`.</param>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <returns>The arguments. The output has one JSON object on each line.</returns>
    public static IReadOnlyList<string> CommentArguments(string repo, string number)
    {
        ArgumentException.ThrowIfNullOrEmpty(repo);
        ArgumentException.ThrowIfNullOrEmpty(number);
        return
        [
            "api", "--paginate", $"repos/{repo}/issues/{number}/comments?per_page=100", "--jq",
            ".[] | {login: .user.login, created: .created_at, updated: .updated_at, " +
            "dashboard: (.body | test(\"<b>Code Review</b>\")), request: (.body | test(\"^\\\\s*gitar review\\\\s*$\"; \"i\"))}",
        ];
    }

    /// <summary>
    /// Gives the `gh` arguments that list each review thread of the pull request. The query reads
    /// every page through `$endCursor`, so a thread after the first 100 counts too.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <returns>The arguments. The output has one JSON object on each line.</returns>
    public static IReadOnlyList<string> ThreadArguments(string owner, string name, string number)
    {
        ArgumentException.ThrowIfNullOrEmpty(owner);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(number);
        return
        [
            "api", "graphql", "--paginate", "-F", $"owner={owner}", "-F", $"name={name}", "-F", $"number={number}", "-f",
            "query=query($owner: String!, $name: String!, $number: Int!, $endCursor: String) { repository(owner: $owner, name: $name) { " +
            "pullRequest(number: $number) { reviewThreads(first: 100, after: $endCursor) { " +
            "nodes { isResolved comments(first: 1) { nodes { author { login } } } } pageInfo { hasNextPage endCursor } } } } }",
            "--jq", ".data.repository.pullRequest.reviewThreads.nodes[] | {resolved: .isResolved, author: (.comments.nodes[0].author.login // \"\")}",
        ];
    }

    /// <summary>Gives the `gh` arguments that list the status of each Gitar check run of one commit, on every page.</summary>
    /// <param name="repo">The owner and the name of the repository.</param>
    /// <param name="sha">The commit.</param>
    /// <returns>The arguments. The output has one status on each line.</returns>
    public static IReadOnlyList<string> CheckRunArguments(string repo, string sha)
    {
        ArgumentException.ThrowIfNullOrEmpty(repo);
        ArgumentException.ThrowIfNullOrEmpty(sha);
        return
        [
            "api", "--paginate", $"repos/{repo}/commits/{sha}/check-runs?per_page=100", "--jq",
            $".check_runs[] | select(.app.slug == \"{GraphLogin}\") | .status",
        ];
    }

    /// <summary>Gives the `gh` arguments that list the creation time of each check suite of one commit, on every page.</summary>
    /// <param name="repo">The owner and the name of the repository.</param>
    /// <param name="sha">The commit.</param>
    /// <returns>The arguments. The output has one time on each line.</returns>
    public static IReadOnlyList<string> CheckSuiteArguments(string repo, string sha)
    {
        ArgumentException.ThrowIfNullOrEmpty(repo);
        ArgumentException.ThrowIfNullOrEmpty(sha);
        return ["api", "--paginate", $"repos/{repo}/commits/{sha}/check-suites?per_page=100", "--jq", ".check_suites[].created_at"];
    }

    /// <summary>
    /// Reads the comments that the command gets from `gh api`, one JSON object on each line, with
    /// the fields `login`, `created`, `updated`, `dashboard`, and `request`.
    /// </summary>
    /// <param name="lines">The output of the command.</param>
    /// <returns>Each comment in the order of the lines.</returns>
    /// <exception cref="InvalidOperationException">A line is no JSON object, or a field is absent or of the wrong kind (T-2).</exception>
    public static IReadOnlyList<GitarComment> ParseComments(string lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        List<GitarComment> comments = [];
        foreach (string line in SplitLines(lines))
        {
            using JsonDocument document = ReadObject(line);
            JsonElement root = document.RootElement;
            comments.Add(new GitarComment(
                ReadString(root, "login", line),
                ReadString(root, "created", line),
                ReadString(root, "updated", line),
                ReadBool(root, "dashboard", line),
                ReadBool(root, "request", line)));
        }

        return comments;
    }

    /// <summary>
    /// Reads the review threads that the command gets from `gh api graphql`, one JSON object on
    /// each line, with the fields `resolved` and `author`.
    /// </summary>
    /// <param name="lines">The output of the command.</param>
    /// <returns>Each thread in the order of the lines.</returns>
    /// <exception cref="InvalidOperationException">A line is no JSON object, or a field is absent or of the wrong kind (T-2).</exception>
    public static IReadOnlyList<GitarThread> ParseThreads(string lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        List<GitarThread> threads = [];
        foreach (string line in SplitLines(lines))
        {
            using JsonDocument document = ReadObject(line);
            JsonElement root = document.RootElement;
            threads.Add(new GitarThread(ReadBool(root, "resolved", line), ReadString(root, "author", line)));
        }

        return threads;
    }

    /// <summary>Gives the earliest time of a list, one time on each line.</summary>
    /// <param name="lines">The creation time of each check suite.</param>
    /// <returns>The earliest time, or null when the list is empty.</returns>
    /// <exception cref="InvalidOperationException">A line is not in the UTC form of the GitHub API (T-2).</exception>
    public static string? Earliest(string lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        string? earliest = null;
        foreach (string line in SplitLines(lines))
        {
            RequireTime(line);
            if (earliest is null || string.CompareOrdinal(line, earliest) < 0)
            {
                earliest = line;
            }
        }

        return earliest;
    }

    /// <summary>Splits an output into its lines, with the empty lines removed.</summary>
    /// <param name="text">The output of a command.</param>
    /// <returns>Each line with text, with the white space at each end removed.</returns>
    public static IReadOnlyList<string> SplitLines(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool Later(string time, string? current)
    {
        return current is null || string.CompareOrdinal(time, current) > 0;
    }

    private static bool Contains(IReadOnlyList<string> values, string value)
    {
        foreach (string item in values)
        {
            if (string.Equals(item, value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void RequireTime(string time)
    {
        if (!UtcTime.IsMatch(time))
        {
            throw new InvalidOperationException(
                $"The time '{time}' is not in the UTC form of the GitHub API, such as `2026-09-23T06:53:26Z` (T-2).");
        }
    }

    private static JsonDocument ReadObject(string line)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(line);
        }
        catch (JsonException fault)
        {
            throw new InvalidOperationException($"The line '{line}' is no JSON object (T-2). {fault.Message}", fault);
        }

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            document.Dispose();
            throw new InvalidOperationException($"The line '{line}' is no JSON object (T-2).");
        }

        return document;
    }

    private static string ReadString(JsonElement root, string field, string line)
    {
        if (!root.TryGetProperty(field, out JsonElement value) || value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"The line '{line}' has no text field `{field}` (T-2).");
        }

        return value.GetString() ?? throw new InvalidOperationException($"The line '{line}' has no text field `{field}` (T-2).");
    }

    private static bool ReadBool(JsonElement root, string field, string line)
    {
        if (!root.TryGetProperty(field, out JsonElement value)
            || (value.ValueKind != JsonValueKind.True && value.ValueKind != JsonValueKind.False))
        {
            throw new InvalidOperationException($"The line '{line}' has no true or false field `{field}` (T-2).");
        }

        return value.GetBoolean();
    }
}
