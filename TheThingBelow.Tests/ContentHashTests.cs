using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The content hash covers the files of `content/rules/` and no other file (D-495, D-648).
/// A run record and a snapshot hold the value (G-5).
/// </summary>
public sealed class ContentHashTests
{
    [Fact]
    public void AFileOutsideTheRuleFolderNeverReachesTheHash()
    {
        // An art batch, a music batch, an effect batch, or a text edit must never break a
        // stored run record or a replay fixture (D-495).
        string withoutArt = ContentHash.Compute([RuleFile("rules/a.json", "one")]);

        string withArt = ContentHash.Compute(
        [
            RuleFile("rules/a.json", "one"),
            RuleFile("sprites/palette.json", "a palette"),
            RuleFile("strings/en.json", "a table"),
            RuleFile("music/first.json", "a track"),
        ]);

        Assert.Equal(withoutArt, withArt);
    }

    [Fact]
    public void AChangeOfARuleFileChangesTheHash()
    {
        string before = ContentHash.Compute([RuleFile("rules/a.json", "one")]);

        string after = ContentHash.Compute([RuleFile("rules/a.json", "two")]);

        Assert.NotEqual(before, after);
    }

    [Fact]
    public void ANewRuleFileChangesTheHash()
    {
        string before = ContentHash.Compute([RuleFile("rules/a.json", "one")]);

        string after = ContentHash.Compute([RuleFile("rules/a.json", "one"), RuleFile("rules/b.json", "two")]);

        Assert.NotEqual(before, after);
    }

    [Fact]
    public void TheOrderOfTheHostNeverReachesTheHash()
    {
        // The file system gives its own order, and it differs by platform (G-4, F-39).
        IReadOnlyList<ContentFile> forward =
        [
            RuleFile("rules/a.json", "one"),
            RuleFile("rules/b.json", "two"),
            RuleFile("rules/c.json", "three"),
        ];

        IReadOnlyList<ContentFile> backward =
        [
            RuleFile("rules/c.json", "three"),
            RuleFile("rules/b.json", "two"),
            RuleFile("rules/a.json", "one"),
        ];

        Assert.Equal(ContentHash.Compute(forward), ContentHash.Compute(backward));
    }

    [Fact]
    public void ThePathOfAFileReachesTheHash()
    {
        string first = ContentHash.Compute([RuleFile("rules/a.json", "one")]);

        string second = ContentHash.Compute([RuleFile("rules/b.json", "one")]);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void AShiftOfTheBoundaryBetweenTwoFilesChangesTheHash()
    {
        // Each part carries its length, so no two sets give one value through a shift of the
        // boundary between a path and the bytes that follow it.
        string first = ContentHash.Compute([RuleFile("rules/ab.json", "one"), RuleFile("rules/c.json", "two")]);

        string second = ContentHash.Compute([RuleFile("rules/a.json", "bone"), RuleFile("rules/c.json", "two")]);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ThePathOfAFileTwoTimesFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => ContentHash.Compute([RuleFile("rules/a.json", "one"), RuleFile("rules/a.json", "two")]));

        Assert.Equal("rules/a.json", error.File);
        Assert.Contains("two times", error.Message);
    }

    [Fact]
    public void TheHashIsSixtyFourHexadecimalCharacters()
    {
        string hash = ContentHash.Compute([RuleFile("rules/a.json", "one")]);

        Assert.Equal(64, hash.Length);
        Assert.All(hash, character => Assert.True(character is (>= '0' and <= '9') or (>= 'a' and <= 'f')));
    }

    private static ContentFile RuleFile(string path, string body) =>
        new(path, Encoding.UTF8.GetBytes(body));
}
