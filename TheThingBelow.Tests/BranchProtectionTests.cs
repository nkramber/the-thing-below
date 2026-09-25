using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The required checks of `main`, as `docs/runbooks/branch-protection.json` records them
/// (D-931). A required context matches by name alone, so a second job with the same name could
/// report it (F-147).
/// </summary>
public sealed class BranchProtectionTests
{
    private const string RecordPath = "docs/runbooks/branch-protection.json";

    private const string WorkflowFolder = ".github/workflows";

    /// <summary>The check of the Gitar app, which no workflow of this repository runs (D-1123).</summary>
    private const string GitarContext = "Gitar";

    private static readonly Regex JobKey = new Regex(@"^  ([A-Za-z0-9_-]+):\s*$", RegexOptions.CultureInvariant);

    private static readonly Regex JobName = new Regex(@"^    name:\s*(.+?)\s*$", RegexOptions.CultureInvariant);

    [Fact]
    public void EachRequiredContextOfAWorkflowIsTheNameOfOneJob()
    {
        IReadOnlyList<string> contexts = RequiredContexts();
        List<(string Name, string Where)> jobs = JobNames();

        Assert.True(jobs.Count >= 10, $"The search found {jobs.Count} jobs in the workflows, so it broke (T-2).");
        foreach (string context in contexts.Where(context => context != GitarContext))
        {
            string[] matches = jobs.Where(job => job.Name == context).Select(job => job.Where).ToArray();
            Assert.True(
                matches.Length == 1,
                $"The required context '{context}' is the name of {matches.Length} jobs: {string.Join(", ", matches)}. " +
                "A context matches by name alone, so each one needs exactly one job (F-147).");
        }
    }

    /// <summary>The Gitar pass is required on each head, and no workflow can report its context (D-1123).</summary>
    [Fact]
    public void TheGitarCheckIsRequiredAndNoWorkflowJobTakesItsName()
    {
        Assert.Contains(GitarContext, RequiredContexts());
        Assert.DoesNotContain(JobNames(), job => string.Equals(job.Name, GitarContext, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> RequiredContexts()
    {
        using JsonDocument record = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(RecordPath)));
        JsonElement contexts = record.RootElement.GetProperty("main").GetProperty("required_status_checks").GetProperty("contexts");
        return contexts.EnumerateArray()
            .Select(context => context.GetString() ?? throw new InvalidOperationException($"`{RecordPath}` holds a context that is not text (T-2)."))
            .ToArray();
    }

    /// <summary>
    /// Reads the check name of each job of each workflow: its `name` key, or its key when it has
    /// no name. The read takes the indent of this repository: jobs at 2 spaces, and keys of a
    /// job at 4 spaces.
    /// </summary>
    private static List<(string Name, string Where)> JobNames()
    {
        List<(string Name, string Where)> jobs = [];
        foreach (string path in Directory.GetFiles(RepositoryRoot.PathTo(WorkflowFolder), "*.yml").Order(StringComparer.Ordinal))
        {
            string[] lines = File.ReadAllLines(path);
            int start = Array.FindIndex(lines, line => line == "jobs:");
            if (start < 0)
            {
                throw new InvalidOperationException($"The workflow '{path}' holds no `jobs:` key (T-2).");
            }

            string? key = null;
            string? name = null;
            for (int index = start + 1; index < lines.Length; index += 1)
            {
                Match keyMatch = JobKey.Match(lines[index]);
                if (keyMatch.Success)
                {
                    AddJob(jobs, path, key, name);
                    key = keyMatch.Groups[1].Value;
                    name = null;
                    continue;
                }

                Match nameMatch = JobName.Match(lines[index]);
                if (key is not null && name is null && nameMatch.Success)
                {
                    name = nameMatch.Groups[1].Value;
                }
            }

            AddJob(jobs, path, key, name);
        }

        return jobs;
    }

    private static void AddJob(List<(string Name, string Where)> jobs, string path, string? key, string? name)
    {
        if (key is not null)
        {
            jobs.Add((name ?? key, $"{Path.GetFileName(path)} job `{key}`"));
        }
    }
}
