using System;
using System.IO;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The folder of the saves on each of the three systems (D-465, D-481, F-33). The folder
/// carries the name of the repository, so a change of the title never moves a save.
/// </summary>
/// <remarks>
/// Each rule takes the environment of its system, so this test reads all three folders on
/// one machine. Godot writes to the same folder, and the last test compares the name in the
/// Godot project with the name in Storage (F-33).
/// </remarks>
public sealed class SaveFolderTests
{
    [Fact]
    public void TheWindowsFolderSitsInTheRoamingFolderOfThePerson() =>
        Assert.Equal(
            @"C:\Users\player\AppData\Roaming\the-thing-below",
            SaveFolder.Of(SaveSystem.Windows, @"C:\Users\player\AppData\Roaming", null, null));

    [Fact]
    public void TheMacOsFolderSitsInTheApplicationSupportFolder() =>
        Assert.Equal(
            "/Users/player/Library/Application Support/the-thing-below",
            SaveFolder.Of(SaveSystem.MacOs, null, "/Users/player", null));

    [Fact]
    public void TheLinuxFolderSitsInTheLocalShareFolder() =>
        Assert.Equal(
            "/home/player/.local/share/the-thing-below",
            SaveFolder.Of(SaveSystem.Linux, null, "/home/player", null));

    [Fact]
    public void TheLinuxFolderFollowsTheDataVariableOfTheSpecification() =>
        Assert.Equal(
            "/data/games/the-thing-below",
            SaveFolder.Of(SaveSystem.Linux, null, "/home/player", "/data/games"));

    [Fact]
    public void TheLinuxFolderDropsARelativeDataVariable()
    {
        // The XDG base directory specification says that a relative value is invalid, and
        // Godot reads the variable by the same rule (D-465, F-33).
        //
        // The comment sits in the body and never between the arrow and the expression. A
        // comment there makes `dotnet format` rewrite the line, and the rewrite takes the
        // line ending of the machine. The check then fails on the Windows leg alone (F-80).
        Assert.Equal(
            "/home/player/.local/share/the-thing-below",
            SaveFolder.Of(SaveSystem.Linux, null, "/home/player", "games/data"));
    }

    [Theory]
    [InlineData("/home/player/")]
    [InlineData("/home/player")]
    public void AValueThatEndsWithASeparatorMakesNoEmptyStep(string home) =>
        Assert.Equal(
            "/home/player/.local/share/the-thing-below",
            SaveFolder.Of(SaveSystem.Linux, null, home, null));

    [Fact]
    public void TheSavesSitInTheirOwnFolderUnderTheFolderOfTheGame()
    {
        // D-656: the crash files and the log files of PR-44 take their own folders beside it.
        Assert.Equal(
            "/home/player/.local/share/the-thing-below/saves",
            SaveFolder.SavesOf(SaveSystem.Linux, null, "/home/player", null));
        Assert.Equal(
            @"C:\Users\player\AppData\Roaming\the-thing-below\saves",
            SaveFolder.SavesOf(SaveSystem.Windows, @"C:\Users\player\AppData\Roaming", null, null));
    }

    [Fact]
    public void AnAbsentWindowsVariableFailsAndNamesIt()
    {
        StorageException error = Assert.Throws<StorageException>(
            () => SaveFolder.Of(SaveSystem.Windows, null, "/home/player", null));

        Assert.Contains(SaveFolder.WindowsVariable, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(SaveSystem.MacOs)]
    [InlineData(SaveSystem.Linux)]
    public void AnAbsentHomeVariableFailsAndNamesIt(SaveSystem system)
    {
        StorageException error = Assert.Throws<StorageException>(
            () => SaveFolder.Of(system, @"C:\Users\player\AppData\Roaming", null, null));

        Assert.Contains(SaveFolder.HomeVariable, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFolderOfThisSystemEndsWithTheNameOfTheRepository()
    {
        string folder = SaveFolder.OfThisSystem();

        Assert.EndsWith(SaveFolder.Name, folder, StringComparison.Ordinal);
        Assert.EndsWith(SaveFolder.SavesName, SaveFolder.SavesOfThisSystem(), StringComparison.Ordinal);
        Assert.True(Path.IsPathRooted(folder), $"The folder '{folder}' is no full path (T-2).");
    }

    [Fact]
    public void TheGodotProjectNamesTheSameUserFolder()
    {
        // Godot resolves `user://` from these two settings, and Storage reads the environment
        // of the system. A change of one name without the other would write a save that no
        // later session reads (D-465, F-33, T-2).
        string project = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.Game/project.godot"));

        Assert.Contains("config/use_custom_user_dir=true", project, StringComparison.Ordinal);
        Assert.Contains(
            $"config/custom_user_dir_name=\"{SaveFolder.Name}\"", project, StringComparison.Ordinal);
    }
}
