using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The .NET tools that a workflow installs. Each one comes from a package file whose SHA-512
/// the workflow checks, as each other download does (D-511, D-1116).
/// </summary>
/// <remarks>
/// A `dotnet tool install` with a pinned version alone checks no digest, and it reads each
/// package source of the machine. The coverage job thus downloads the package file, checks
/// it, and installs from that folder alone with `--source`.
/// </remarks>
public sealed class ToolDownloadWorkflowTests
{
    /// <summary>The folder of the workflows.</summary>
    private const string WorkflowFolder = ".github/workflows";

    /// <summary>The workflow of the coverage job.</summary>
    private const string CiWorkflowPath = ".github/workflows/ci.yml";

    [Fact]
    public void EachToolInstallReadsTheCheckedFolderAlone()
    {
        foreach (string path in Directory.GetFiles(RepositoryRoot.PathTo(WorkflowFolder), "*.yml").Order(StringComparer.Ordinal))
        {
            string text = File.ReadAllText(path).Replace("\\\n", string.Empty, StringComparison.Ordinal);
            foreach (string line in text.Split('\n').Where(line => line.Contains("dotnet tool install", StringComparison.Ordinal)))
            {
                Assert.True(
                    line.Contains("--source ", StringComparison.Ordinal),
                    $"The workflow '{Path.GetFileName(path)}' installs a tool with no --source: '{line.Trim()}'. A tool comes from a checked package file alone (D-511, D-1116).");
            }
        }
    }

    [Fact]
    public void TheCoverageJobChecksTheSha512OfTheReportGeneratorPackage()
    {
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo(CiWorkflowPath));
        const string Mark = "  REPORT_GENERATOR_SHA512: ";
        string digest = lines.Single(line => line.StartsWith(Mark, StringComparison.Ordinal))[Mark.Length..];
        string text = string.Join('\n', lines);

        Assert.Equal(128, digest.Length);
        Assert.All(digest, letter => Assert.Contains(letter, "0123456789abcdef"));
        Assert.Contains("if [ \"$actual\" != \"$REPORT_GENERATOR_SHA512\" ]; then", text, StringComparison.Ordinal);
        Assert.Contains("--source \"$PWD/report-generator-download\"", text, StringComparison.Ordinal);
    }
}
