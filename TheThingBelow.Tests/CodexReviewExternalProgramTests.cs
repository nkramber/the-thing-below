using System;
using System.IO;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The runner of the external programs of the `codex-review` command (T-2).</summary>
public sealed class CodexReviewExternalProgramTests
{
    [Fact]
    public void AProgramGivesItsOutputAndItsExitCode()
    {
        ProgramResult result = ExternalProgram.Run("git", ["--version"], Path.GetTempPath(), null);

        Assert.Equal(0, result.ExitCode);
        Assert.StartsWith("git version", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOutputFileTakesTheOutput()
    {
        string file = Path.Combine(Path.GetTempPath(), $"codex-review-output-{Guid.NewGuid():N}.txt");
        try
        {
            ProgramResult result = ExternalProgram.Run("git", ["--version"], Path.GetTempPath(), file);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Output);
            Assert.StartsWith("git version", File.ReadAllText(file), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// A removed variable leaves the environment of the program alone. This process keeps it, so
    /// no other process, such as a run of another repository, loses a key (D-932).
    /// </summary>
    [Fact]
    public void ARemovedVariableLeavesTheProgramAlone()
    {
        string name = $"THE_THING_BELOW_TEST_KEY_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(name, "a-test-value");
        try
        {
            ProgramResult kept = ExternalProgram.Run("printenv", [name], Path.GetTempPath(), null);
            ProgramResult removed = ExternalProgram.Run("printenv", [name], Path.GetTempPath(), null, [name]);

            Assert.Equal("a-test-value", kept.Output.Trim());
            Assert.Equal(string.Empty, removed.Output.Trim());
            Assert.NotEqual(0, removed.ExitCode);
            Assert.Equal("a-test-value", Environment.GetEnvironmentVariable(name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void ACheckedRunThatFailsNamesTheCommandAndTheCode()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => ExternalProgram.RunChecked("git", ["no-such-command"], Path.GetTempPath()));

        Assert.Contains("`git no-such-command`", fault.Message, StringComparison.Ordinal);
        Assert.Contains("exit code 1", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AProgramThatDoesNotExistIsAFault()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => ExternalProgram.Run("the-thing-below-no-such-program", [], Path.GetTempPath(), null));

        Assert.Contains("did not start", fault.Message, StringComparison.Ordinal);
    }
}
