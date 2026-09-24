using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The place and the marks of the dungeon map screen: 16 frame pixels for each walked tile, the
/// doors, the save points, and the party (D-567, D-982, D-993). The tests read the built Game
/// assembly (D-614).
/// </summary>
public sealed class DungeonMapLayoutTests
{
    [Fact]
    public void TheFixtureDungeonDrawsAtSixteenPixelsInTheMiddleOfTheFrame()
    {
        // D-982: 40 by 24 tiles draw at 640 by 384 frame pixels.
        GameValue layout = Layout(MapState.Enter(TestMaps.FixtureDungeon));

        Assert.Equal((320, 168, 640, 384), (layout.Read<int>("Left"), layout.Read<int>("Top"), layout.Read<int>("Width"), layout.Read<int>("Height")));
        Assert.Equal(80, (int)GameValue.Constant("DungeonMapLayout", "MostColumns"));
        Assert.Equal(45, (int)GameValue.Constant("DungeonMapLayout", "MostRows"));
    }

    [Fact]
    public void TheScreenShowsEachWalkedTileWithItsDoorAndItsSavePoint()
    {
        // Exit test 6 of PR-62 (D-567, D-993): the save point at (9, 6) and the door at (11, 4).
        MapState party = MapState.Enter(TestMaps.FixtureDungeon);
        party.Walked.Mark(new TilePoint(9, 6));
        party.Walked.Mark(new TilePoint(11, 4));
        party.Walked.Mark(new TilePoint(5, 4));
        GameValue layout = Layout(party);

        Assert.Equal("Party", MarkAt(layout, party.LeadAt));
        Assert.Equal("SavePoint", MarkAt(layout, new TilePoint(9, 6)));
        Assert.Equal("Door", MarkAt(layout, new TilePoint(11, 4)));
        Assert.Equal("Floor", MarkAt(layout, new TilePoint(5, 4)));
    }

    [Fact]
    public void ATileThatThePartyNeverWalkedDrawsNothingItsDoorIncluded()
    {
        // D-567: the door of the vault at (18, 17) stays off the map until the party walks it.
        MapState party = MapState.Enter(TestMaps.FixtureDungeon);
        GameValue layout = Layout(party);

        Assert.Equal("None", MarkAt(layout, new TilePoint(18, 17)));
        Assert.Equal("None", MarkAt(layout, new TilePoint(0, 0)));
    }

    [Fact]
    public void AMapOfEightyByFortyFiveShowsWhole()
    {
        GameValue layout = Layout(MapState.Enter(TestMaps.OpenOf("map.test_widest", 80, 45)));

        Assert.Equal((0, 0), (layout.Read<int>("Left"), layout.Read<int>("Top")));
    }

    [Theory]
    [InlineData(81, 45)]
    [InlineData(80, 46)]
    public void AMapLargerThanTheFrameIsAnErrorThatNamesItsSize(int width, int height)
    {
        // D-982: the screen builds no pan, so a larger map fails with its size (T-2).
        MapState party = MapState.Enter(TestMaps.OpenOf("map.test_wide", width, height));

        ArgumentException error = Assert.Throws<ArgumentException>(() => Layout(party));

        Assert.Contains($"{width} by {height}", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-982", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATileOutsideTheMapIsAnError()
    {
        GameValue layout = Layout(MapState.Enter(TestMaps.FixtureDungeon));

        Assert.Throws<ArgumentOutOfRangeException>(() => MarkAt(layout, new TilePoint(40, 0)));
    }

    private static GameValue Layout(MapState party)
    {
        return GameValue.Of(GameValue.Static("DungeonMapLayout", "Of", party)!);
    }

    private static string MarkAt(GameValue layout, TilePoint at) => layout.Call("MarkAt", at)!.ToString()!;
}
