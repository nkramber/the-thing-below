using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheThingBelow.Tools.ChangedPaths;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Branch protection matches a required status check by its name. A matrix job that a
/// condition skips never expands its matrix, so it reports the literal name template and
/// not the name of each leg (F-85). These tests read the committed workflow files and hold
/// the rule that every required check has one stable name (D-682, D-683).
/// </summary>
public sealed class CiWorkflowGateTests
{
    /// <summary>The workflow that holds every check of the PR gate except the review gate.</summary>
    private const string CiWorkflowPath = ".github/workflows/ci.yml";

    /// <summary>The workflow that holds the review gate check (D-15).</summary>
    private const string ReviewGateWorkflowPath = ".github/workflows/review-gate.yml";

    /// <summary>The mark of a workflow expression, which makes a name that can change.</summary>
    private const string ExpressionMark = "${{";

    /// <summary>The gate job of each matrix family, and the matrix job that it reads (D-683).</summary>
    public static TheoryData<string, string> GateJobs { get; } = new TheoryData<string, string>
    {
        { "build-test-format-gate", "build-test-format" },
        { "replay-identity-gate", "replay-identity" },
        { "smoke-gate", "smoke" },
        { "coverage-gate", "coverage" },
        { "det-lint-gate", "det-lint" },
        { "screen-test-gate", "screen-test" },
    };

    [Theory]
    [InlineData("changed paths")]
    [InlineData("build, test, and format")]
    [InlineData("coverage report")]
    [InlineData("det-lint")]
    [InlineData("replay-identity")]
    [InlineData("screen-test")]
    [InlineData("smoke")]
    [InlineData("ste-check")]
    public void EachRequiredCheckNameIsTheLiteralNameOfOneJob(string checkName)
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> jobs = ReadJobs(CiWorkflowPath);

        string[] owners = jobs
            .Where(job => NameOf(job.Key, job.Value) == checkName)
            .Select(job => job.Key)
            .ToArray();

        Assert.True(
            owners.Length == 1,
            $"The workflow '{CiWorkflowPath}' holds {owners.Length} jobs named '{checkName}'. " +
            $"Branch protection requires that name, so one job is the contract (F-85, D-682).");
    }

    [Fact]
    public void TheReviewGateJobHasALiteralName()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> jobs = ReadJobs(ReviewGateWorkflowPath);

        Assert.Equal("review-gate", NameOf("review-gate", jobs["review-gate"]));
    }

    [Theory]
    [MemberData(nameof(GateJobs))]
    public void EachGateJobAlwaysRunsAndReadsItsMatrixJob(string gateJob, string matrixJob)
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> jobs = ReadJobs(CiWorkflowPath);
        IReadOnlyList<string> lines = jobs[gateJob];

        // `always()` makes the check report on a run that a fault or a cancel stopped. A
        // gate that a condition skips reports Success and hides that fault (T-2).
        Assert.Equal("always()", ValueOf(gateJob, lines, "if"));
        Assert.Equal($"[changed-paths, {matrixJob}]", ValueOf(gateJob, lines, "needs"));
        Assert.DoesNotContain(ExpressionMark, NameOf(gateJob, lines), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryJobNameWithAnExpressionHasOneGateJobThatReadsIt()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> jobs = ReadJobs(CiWorkflowPath);

        foreach ((string jobId, IReadOnlyList<string> lines) in jobs)
        {
            if (!NameOf(jobId, lines).Contains(ExpressionMark, StringComparison.Ordinal))
            {
                continue;
            }

            string[] gates = jobs
                .Where(gate => ValueOf(gate.Key, gate.Value, "needs") == $"[changed-paths, {jobId}]")
                .Select(gate => gate.Key)
                .ToArray();

            Assert.True(
                gates.Length == 1,
                $"The name of the job '{jobId}' holds an expression, and {gates.Length} gate jobs " +
                $"read it. Such a job reports no stable check name, so it needs one gate (F-85).");
        }
    }

    [Fact]
    public void ThePreviousHeadRuleReadsEachCheckThatADocsOnlyChangeSkips()
    {
        // A docs-only push skips a job only when that check passed on the previous head
        // (D-858). A skipped job that the list misses would skip on a red head (T-2).
        IReadOnlyDictionary<string, IReadOnlyList<string>> jobs = ReadJobs(CiWorkflowPath);

        SortedSet<string> skipped = new SortedSet<string>(StringComparer.Ordinal);
        foreach ((string jobId, IReadOnlyList<string> lines) in jobs)
        {
            if (ValueOf(jobId, lines, "if") != "needs.changed-paths.outputs.documents-alone != 'true'")
            {
                continue;
            }

            KeyValuePair<string, IReadOnlyList<string>> gate = jobs.Single(
                job => ValueOf(job.Key, job.Value, "needs") == $"[changed-paths, {jobId}]");
            skipped.Add(NameOf(gate.Key, gate.Value));
        }

        Assert.Equal(
            PreviousHeadChecks.SkippedCheckNames.OrderBy(name => name, StringComparer.Ordinal),
            skipped);
    }

    [Fact]
    public void EachJobThatADocsOnlyChangeSkipsHasOneGateJob()
    {
        // A plain job that a condition skips reports `skipped`, and the rule of the previous
        // head reads that as no pass. A gate reports `success` on a docs-only head, so two
        // docs-only pushes in a row can both skip (D-858).
        IReadOnlyDictionary<string, IReadOnlyList<string>> jobs = ReadJobs(CiWorkflowPath);

        foreach ((string jobId, IReadOnlyList<string> lines) in jobs)
        {
            if (ValueOf(jobId, lines, "if") != "needs.changed-paths.outputs.documents-alone != 'true'")
            {
                continue;
            }

            string[] gates = jobs
                .Where(gate => ValueOf(gate.Key, gate.Value, "needs") == $"[changed-paths, {jobId}]")
                .Select(gate => gate.Key)
                .ToArray();

            Assert.True(
                gates.Length == 1,
                $"The job '{jobId}' skips on a docs-only change, and {gates.Length} gate jobs read it (D-858).");
            Assert.Equal("always()", ValueOf(gates[0], jobs[gates[0]], "if"));
        }
    }

    [Fact]
    public void TheChangedPathsJobRunsTheCommandOfTools()
    {
        // D-859: the command of Tools decides, and the workflow collects the facts alone.
        string text = File.ReadAllText(RepositoryRoot.PathTo(CiWorkflowPath));

        Assert.Contains(
            $"-- {ChangedPathsCommand.Name} ",
            text,
            StringComparison.Ordinal);
        Assert.Contains("checks: read", string.Join('\n', ReadJobs(CiWorkflowPath)["changed-paths"]), StringComparison.Ordinal);
    }

    /// <summary>Reads the `jobs` map of a workflow file as the lines of each job.</summary>
    /// <param name="workflowPath">The path of the workflow under the root of the checkout.</param>
    /// <returns>The lines of each job, by the id of the job.</returns>
    /// <exception cref="InvalidOperationException">The file holds no `jobs` key.</exception>
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ReadJobs(string workflowPath)
    {
        string path = RepositoryRoot.PathTo(workflowPath);
        string[] lines = File.ReadAllLines(path);
        int start = Array.IndexOf(lines, "jobs:");
        if (start < 0)
        {
            throw new InvalidOperationException($"The file '{path}' holds no `jobs` key (T-2).");
        }

        Dictionary<string, IReadOnlyList<string>> jobs =
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        List<string>? current = null;
        foreach (string line in lines.Skip(start + 1))
        {
            if (line.Length > 0 && line[0] != ' ')
            {
                break;
            }

            if (IsJobHeader(line))
            {
                current = new List<string>();
                jobs.Add(line.Trim().TrimEnd(':'), current);
                continue;
            }

            current?.Add(line);
        }

        return jobs;
    }

    /// <summary>Reads whether a line names a job, at the one indent level of a job id.</summary>
    /// <param name="line">One line of the `jobs` map.</param>
    /// <returns>True when the line is the header of a job.</returns>
    private static bool IsJobHeader(string line)
    {
        return line.Length > 3 &&
            line.StartsWith("  ", StringComparison.Ordinal) &&
            line[2] != ' ' &&
            line.EndsWith(':');
    }

    /// <summary>Reads the `name` of a job, which is the name of its check run.</summary>
    /// <param name="jobId">The id of the job, for the error message.</param>
    /// <param name="lines">The lines of the job.</param>
    /// <returns>The value of the `name` key.</returns>
    /// <exception cref="InvalidOperationException">The job holds no `name` key.</exception>
    private static string NameOf(string jobId, IReadOnlyList<string> lines)
    {
        return ValueOf(jobId, lines, "name") ??
            throw new InvalidOperationException($"The job '{jobId}' holds no `name` key (T-2).");
    }

    /// <summary>Reads a key of a job, at the one indent level of the keys of the job.</summary>
    /// <param name="jobId">The id of the job, for the error message.</param>
    /// <param name="lines">The lines of the job.</param>
    /// <param name="key">The name of the key, such as `if`.</param>
    /// <returns>The value of the key, or null when the job holds no such key.</returns>
    /// <exception cref="InvalidOperationException">The job holds the key more than one time.</exception>
    private static string? ValueOf(string jobId, IReadOnlyList<string> lines, string key)
    {
        string mark = $"    {key}: ";
        string[] matches = lines
            .Where(line => line.StartsWith(mark, StringComparison.Ordinal))
            .Select(line => line.Substring(mark.Length).Trim())
            .ToArray();

        if (matches.Length > 1)
        {
            throw new InvalidOperationException(
                $"The job '{jobId}' holds {matches.Length} `{key}` keys, and one is the contract (T-2).");
        }

        return matches.Length == 1 ? matches[0] : null;
    }
}
