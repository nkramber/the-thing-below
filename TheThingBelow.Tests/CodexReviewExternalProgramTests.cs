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
