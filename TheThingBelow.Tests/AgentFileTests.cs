using System.IO;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// `CLAUDE.md` and `AGENTS.md` hold the same rules for every session, and they stay
/// identical (D-20). This test fails on a change to one file alone.
/// </summary>
public sealed class AgentFileTests
{
    [Fact]
    public void ClaudeFileAndAgentsFileAreIdentical()
    {
        string claudeText = File.ReadAllText(RepositoryRoot.PathTo("CLAUDE.md"));
        string agentsText = File.ReadAllText(RepositoryRoot.PathTo("AGENTS.md"));

        Assert.Equal(claudeText, agentsText);
    }
}
