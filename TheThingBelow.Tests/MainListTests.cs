using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The entries and the cursor of the main list: the order of D-992 with no save entry (D-1143),
/// and one cursor for the keyboard, the gamepad, and the mouse (D-219, D-872). The tests read the
/// built Game assembly (D-614).
/// </summary>
public sealed class MainListTests
{
    [Fact]
    public void TheListTakesTheOrderOfD992WithNoSaveEntry()
    {
        // D-1143: a save opens only from the save service of a hub and from a save point.
        Assert.Equal(["Party", "Lessons", "Gear", "Items", "Status", "Log", "Settings"], Entries());
        Assert.False(Enum.IsDefined(GameAssemblyFile.Type(GameValue.Ui + "MenuEntry"), "Save"));
    }

    [Fact]
    public void TheCursorVisitsEachEntryAndWrapsAtEachEnd()
    {
        // D-988, D-1143: no entry is dim any more, so the cursor skips none.
        GameValue list = GameValue.New("MainList");
        Assert.Equal("Party", list.Name("Current"));

        List<string> visited = [];
        for (int step = 0; step < 7; step += 1)
        {
            list.Call("Move", 1);
            visited.Add(list.Name("Current"));
        }

        Assert.Equal(["Lessons", "Gear", "Items", "Status", "Log", "Settings", "Party"], visited);
        list.Call("Move", -1);
        Assert.Equal("Settings", list.Name("Current"));
    }

    [Fact]
    public void TheMouseAndTheKeysDriveOneCursor()
    {
        // Exit test 4 of PR-62: a point of the mouse moves the cursor that a key moves next (D-872).
        GameValue list = GameValue.New("MainList");

        list.Call("Point", 5);
        Assert.Equal("Log", list.Name("Current"));
        list.Call("Move", 1);
        Assert.Equal("Settings", list.Name("Current"));
    }

    [Fact]
    public void APointOutsideTheListIsAnErrorAndLeavesTheCursor()
    {
        GameValue list = GameValue.New("MainList");

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Call("Point", 7));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Call("Point", -1));
        Assert.Equal(0, list.Read<int>("Cursor"));
    }

    [Theory]
    [InlineData("Party", "Party")]
    [InlineData("Lessons", "Lessons")]
    [InlineData("Gear", "Gear")]
    [InlineData("Items", "Items")]
    [InlineData("Status", "Status")]
    [InlineData("Log", "Log")]
    [InlineData("Settings", "Settings")]
    public void EachEntryOpensItsWindow(string entry, string window)
    {
        object opened = GameValue.Static("MainList", "WindowOf", GameValue.Enum("MenuEntry", entry))!;

        Assert.Equal(window, opened.ToString());
    }

    [Fact]
    public void AValueOutsideTheEntriesOpensNoWindow()
    {
        object outside = Enum.ToObject(GameAssemblyFile.Type(GameValue.Ui + "MenuEntry"), 99);

        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.Static("MainList", "WindowOf", outside));
    }

    [Fact]
    public void AStepOtherThanOneIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("MainList").Call("Move", 2));
    }

    private static List<string> Entries()
    {
        List<string> names = [];
        foreach (object entry in (IEnumerable)GameValue.StaticProperty("MainList", "Entries")!)
        {
            names.Add(entry.ToString()!);
        }

        return names;
    }
}
