using System;
using System.IO;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Every test command of the repository excludes the Smoke category with one option, and the
/// option reads the constants of <see cref="TestCategories"/> (D-592). A misspelled trait
/// would run a smoke test in the unit job, or exclude a test from every job (T-3).
/// </summary>
public sealed class TestFilterTests
{
    [Theory]
    [InlineData("Makefile", 1)]
    [InlineData(".github/workflows/ci.yml", 2)]
    [InlineData("CLAUDE.md", 1)]
    [InlineData("AGENTS.md", 1)]
    [InlineData(".claude/skills/csharp-conventions/SKILL.md", 1)]
    public void EachTestCommandExcludesTheSmokeCategory(string path, int count)
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(path));

        int found = 0;
        for (int index = text.IndexOf(TestCategories.ExcludeSmoke, StringComparison.Ordinal);
             index >= 0;
             index = text.IndexOf(TestCategories.ExcludeSmoke, index + 1, StringComparison.Ordinal))
        {
            found += 1;
        }

        Assert.Equal(count, found);
    }

    [Fact]
    public void TheOptionReadsTheTraitAndTheCategory()
    {
        Assert.Equal("--filter-not-trait \"Category=Smoke\"", TestCategories.ExcludeSmoke);
    }
}
