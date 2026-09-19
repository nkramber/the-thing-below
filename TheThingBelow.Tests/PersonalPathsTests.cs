using System;
using System.Collections.Generic;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rule that hides the folders of the person in the text of a crash file (D-170). A crash
/// file goes to the studio by mail, and it holds no personal data.
/// </summary>
public sealed class PersonalPathsTests
{
    [Fact]
    public void AFolderOfThePersonBecomesThePlaceholder()
    {
        IReadOnlyList<string> folders = ["/Users/a-person"];

        string hidden = PersonalPaths.Hide(
            "the game could not write the file (the path '/Users/a-person/Library/x/slot.json')", folders);

        Assert.DoesNotContain("a-person", hidden, StringComparison.Ordinal);
        Assert.Contains($"{PersonalPaths.Placeholder}/Library/x/slot.json", hidden, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLongestFolderGoesFirst()
    {
        // The data folder lies inside the home folder, and the rule must not leave a part of
        // the longer path in the text.
        IReadOnlyList<string> folders = ["/home/a-person", "/home/a-person/.local/share"];

        string hidden = PersonalPaths.Hide("the file '/home/a-person/.local/share/x/log.json'", folders);

        Assert.Equal($"the file '{PersonalPaths.Placeholder}/x/log.json'", hidden);
    }

    [Fact]
    public void ABackslashFolderAlsoHidesItsSlashForm()
    {
        // Godot writes a slash in every path, also on Windows, and the environment of Windows
        // writes a backslash (F-33).
        IReadOnlyList<string> folders = ["C:\\Users\\a-person\\AppData\\Roaming"];

        string hidden = PersonalPaths.Hide(
            "the two paths 'C:\\Users\\a-person\\AppData\\Roaming\\x' and 'C:/Users/a-person/AppData/Roaming/x'",
            folders);

        Assert.DoesNotContain("a-person", hidden, StringComparison.Ordinal);
    }

    [Fact]
    public void AFolderOfFewCharactersStaysAsItIs()
    {
        // A home folder of one character would match text that names no folder at all.
        IReadOnlyList<string> folders = ["/"];

        Assert.Equal("the file 'a/b/c'", PersonalPaths.Hide("the file 'a/b/c'", folders));
    }

    [Fact]
    public void ATextWithNoFolderStaysAsItIs()
    {
        IReadOnlyList<string> folders = ["/Users/a-person"];

        Assert.Equal(
            "the intent 'menu.open' names no rule of this build",
            PersonalPaths.Hide("the intent 'menu.open' names no rule of this build", folders));
    }

    [Fact]
    public void TheFoldersOfThisSystemHoldNoShortValue()
    {
        foreach (string folder in PersonalPaths.FoldersOfThisSystem())
        {
            Assert.True(folder.Length >= PersonalPaths.ShortestFolder);
        }
    }

    [Fact]
    public void ATextThatIsNullIsAnError()
    {
        Assert.Throws<ArgumentNullException>(() => PersonalPaths.Hide(null!, ["/Users/a-person"]));
        Assert.Throws<ArgumentNullException>(() => PersonalPaths.Hide("a text", null!));
    }
}
