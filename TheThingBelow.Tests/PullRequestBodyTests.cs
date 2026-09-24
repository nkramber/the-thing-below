using System;
using System.IO;
using TheThingBelow.Tools.Screenplay;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The Screenplay section of a PR description: the insert, the replace, and the limit of
/// GitHub (D-1016).
/// </summary>
public sealed class PullRequestBodyTests
{
    private const string Body = "## Summary\n\nText.\n\n## Documents\n\n- A line.\n";

    [Fact]
    public void ABodyWithNoMarkersGetsTheSectionAtItsEnd()
    {
        string result = PullRequestBody.Insert(Body, "## Screenplay\n\nOne.\n");

        Assert.Equal(Body + "\n<!-- screenplay:start -->\n## Screenplay\n\nOne.\n<!-- screenplay:end -->\n", result);
    }

    [Fact]
    public void ABodyWithNoLineEndAtItsEndGetsOneBeforeTheSection()
    {
        string result = PullRequestBody.Insert("Text.", "S.\n");

        Assert.Equal("Text.\n\n<!-- screenplay:start -->\nS.\n<!-- screenplay:end -->\n", result);
    }

    [Fact]
    public void ASecondRunReplacesTheSectionAndKeepsTheRest()
    {
        string first = PullRequestBody.Insert(Body, "Old.\n") + "## After\n";

        string second = PullRequestBody.Insert(first, "New.\n");

        Assert.Equal(Body + "\n<!-- screenplay:start -->\nNew.\n<!-- screenplay:end -->\n## After\n", second);
    }

    [Fact]
    public void AStartMarkerWithNoEndMarkerFails()
    {
        InvalidDataException error = Assert.Throws<InvalidDataException>(
            () => PullRequestBody.Insert(Body + PullRequestBody.StartMarker + "\n", "S.\n"));

        Assert.Contains(PullRequestBody.StartMarker, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEndMarkerBeforeTheStartMarkerFails()
    {
        string body = $"{PullRequestBody.EndMarker}\n{PullRequestBody.StartMarker}\n";

        _ = Assert.Throws<InvalidDataException>(() => PullRequestBody.Insert(body, "S.\n"));
    }

    [Fact]
    public void AMarkerTwoTimesFails()
    {
        string body = $"{PullRequestBody.StartMarker}\n{PullRequestBody.StartMarker}\n{PullRequestBody.EndMarker}\n";

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => PullRequestBody.Insert(body, "S.\n"));

        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABodyAboveTheLimitOfGitHubFailsWithItsSizeAndTheLimit()
    {
        // Exit test 5 of PR-50 (D-1016).
        string section = new string('x', PullRequestBody.MaxCharacters) + "\n";

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => PullRequestBody.Insert(Body, section));

        Assert.Contains("65536", error.Message, StringComparison.Ordinal);
        int size = Body.Length + 1 + PullRequestBody.StartMarker.Length + 1 + section.Length + PullRequestBody.EndMarker.Length + 1;
        Assert.Contains($"holds {size} characters", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABodyAtTheLimitPasses()
    {
        string marked = PullRequestBody.Insert(string.Empty, string.Empty);
        string section = new string('x', PullRequestBody.MaxCharacters - marked.Length - 1) + "\n";

        string result = PullRequestBody.Insert(string.Empty, section);

        Assert.Equal(PullRequestBody.MaxCharacters, result.Length);
    }
}
