using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The path of the menu stack: which window opens where, back, and the close of the menu (D-162,
/// D-211, D-986). The tests read the built Game assembly (D-614).
/// </summary>
public sealed class MenuPathTests
{
    [Fact]
    public void AFixtureMenuOpensStacksASecondWindowAndClosesEachWithBack()
    {
        // Exit test 1 of PR-62: the main list, then the party window over it. Back closes the
        // party window, and a second back closes the main list and the menu (D-211).
        GameValue path = GameValue.New("MenuPath");

        Open(path, "MainList");
        Open(path, "Party");
        Assert.Equal(["MainList", "Party"], Windows(path));
        Assert.Equal("Party", path.Name("Top"));

        Assert.False((bool)path.Call("Back")!);
        Assert.Equal(["MainList"], Windows(path));
        Assert.True(path.Read<bool>("IsOpen"));

        Assert.True((bool)path.Call("Back")!);
        Assert.False(path.Read<bool>("IsOpen"));
    }

    [Theory]
    [InlineData("Party")]
    [InlineData("Status")]
    [InlineData("Log")]
    [InlineData("Settings")]
    public void EachTaskWindowOpensOverTheMainListAlone(string window)
    {
        GameValue path = GameValue.New("MenuPath");

        InvalidOperationException alone = Assert.Throws<InvalidOperationException>(() => Open(path, window));
        Assert.Contains("main list", alone.Message, StringComparison.Ordinal);

        Open(path, "MainList");
        Open(path, window);
        Assert.Equal(window, path.Name("Top"));

        Assert.Throws<InvalidOperationException>(() => Open(path, window));
    }

    [Fact]
    public void TheDungeonMapOpensFromTheWalkAloneAndTheMapActionClosesIt()
    {
        // D-986: the map action opens the map screen from the walk, and it never opens over a window.
        GameValue path = GameValue.New("MenuPath");
        Open(path, "DungeonMap");

        Assert.Throws<InvalidOperationException>(() => Open(path, "MainList"));
        Assert.Throws<InvalidOperationException>(() => Open(path, "Status"));

        path.Call("CloseAll");
        Assert.False(path.Read<bool>("IsOpen"));
    }

    [Fact]
    public void TheMenuActionClosesEveryWindow()
    {
        // D-162: the menu action closes the menu from any depth.
        GameValue path = GameValue.New("MenuPath");
        Open(path, "MainList");
        Open(path, "Log");

        path.Call("CloseAll");

        Assert.Empty(Windows(path));
    }

    [Fact]
    public void ABackWithNoWindowAndATopWithNoWindowAreErrors()
    {
        GameValue path = GameValue.New("MenuPath");

        Assert.Throws<InvalidOperationException>(() => path.Call("Back"));
        Assert.Throws<InvalidOperationException>(() => path.Name("Top"));
    }

    private static void Open(GameValue path, string window) => path.Call("Open", GameValue.Enum("MenuWindowKind", window));

    private static List<string> Windows(GameValue path)
    {
        List<string> names = [];
        foreach (object window in path.Read<IEnumerable>("Windows"))
        {
            names.Add(window.ToString()!);
        }

        return names;
    }
}
