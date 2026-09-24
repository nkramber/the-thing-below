using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The entries and the cursor of the main list: the order of D-992, the dim entries of D-988, and
/// one cursor for the keyboard, the gamepad, and the mouse (D-219, D-872). The tests read the built
/// Game assembly (D-614).
/// </summary>
public sealed class MainListTests
{
    [Fact]
    public void TheListTakesTheOrderOfD992()
    {
        Assert.Equal(["Party", "Lessons", "Gear", "Items", "Status", "Log", "Save", "Settings"], Entries());
    }

    [Fact]
    public void TheEntriesOfLaterPrsAreDimAndTheOthersAreLive()
    {
        // D-988: PR-12, PR-13, PR-14, and PR-16 make the four dim entries live.
        List<string> live = [];
        foreach (string entry in Entries())
        {
            if ((bool)GameValue.Static("MainList", "IsLive", GameValue.Enum("MenuEntry", entry))!)
            {
                live.Add(entry);
            }
        }

        Assert.Equal(["Party", "Status", "Log", "Settings"], live);
    }

    [Fact]
    public void TheCursorSkipsEachDimEntryAndWrapsAtEachEnd()
    {
        GameValue list = GameValue.New("MainList");
        Assert.Equal("Party", list.Name("Current"));

        List<string> visited = [];
        for (int step = 0; step < 4; step += 1)
        {
            list.Call("Move", 1);
            visited.Add(list.Name("Current"));
        }

        Assert.Equal(["Status", "Log", "Settings", "Party"], visited);
        list.Call("Move", -1);
        Assert.Equal("Settings", list.Name("Current"));
    }

    [Fact]
    public void TheMouseAndTheKeysDriveOneCursor()
    {
        // Exit test 4 of PR-62: a point of the mouse moves the cursor that a key moves next (D-872).
        GameValue list = GameValue.New("MainList");

        Assert.True((bool)list.Call("Point", 5)!);
        Assert.Equal("Log", list.Name("Current"));
        list.Call("Move", 1);
        Assert.Equal("Settings", list.Name("Current"));
    }

    [Fact]
    public void APointOnADimEntryLeavesTheCursor()
    {
        GameValue list = GameValue.New("MainList");

        Assert.False((bool)list.Call("Point", 1)!);
        Assert.Equal(0, list.Read<int>("Cursor"));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Call("Point", 8));
    }

    [Theory]
    [InlineData("Party", "Party")]
    [InlineData("Status", "Status")]
    [InlineData("Log", "Log")]
    [InlineData("Settings", "Settings")]
    public void EachLiveEntryOpensItsWindow(string entry, string window)
    {
        object opened = GameValue.Static("MainList", "WindowOf", GameValue.Enum("MenuEntry", entry))!;

        Assert.Equal(window, opened.ToString());
    }

    [Fact]
    public void ADimEntryOpensNoWindow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.Static("MainList", "WindowOf", GameValue.Enum("MenuEntry", "Gear")));
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
