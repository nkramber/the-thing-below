using System;
using System.Reflection;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The place of the view of the map (D-106, D-717, F-52). The tests read the built Game
/// assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// The world viewport is 640 by 360 art pixels, which holds 20 by 11.25 tiles of 32 pixels
/// (D-633, D-634). Game computes the position itself, so every value below reads with no
/// engine.
/// </remarks>
public sealed class MapCameraTests
{
    private const string TypeName = "TheThingBelow.Game.Ui.MapCamera";
    private const int ViewWidth = 640;
    private const int ViewHeight = 360;
    private const int TilePixels = 32;

    [Fact]
    public void TheViewHoldsTwentyTilesAcross()
    {
        // D-633: the world draws at 2x, so the frame of 1280 by 720 holds 20 by 11.25 tiles.
        Assert.Equal(TilePixels, Constant("TilePixels"));
        Assert.Equal(20, ViewWidth / TilePixels);
    }

    [Fact]
    public void TheViewNeverScrollsPastTheWestOrTheNorthEdgeOfALargeMap()
    {
        // Exit test 2 of section 7.3 of `phase-2-first-playable.md` (D-106).
        (int x, int y) = Place(MapState.Enter(TestMaps.Large));

        Assert.Equal(0, x);
        Assert.Equal(0, y);
    }

    [Fact]
    public void TheViewNeverScrollsPastTheEastOrTheSouthEdgeOfALargeMap()
    {
        TilePoint corner = new(TestMaps.Large.Width - 2, TestMaps.Large.Height - 2);

        (int x, int y) = Place(At(TestMaps.Large, corner));

        Assert.Equal((TestMaps.Large.Width * TilePixels) - ViewWidth, x);
        Assert.Equal((TestMaps.Large.Height * TilePixels) - ViewHeight, y);
    }

    [Fact]
    public void TheViewFollowsTheLeadInTheMiddleOfALargeMap()
    {
        TilePoint middle = new(TestMaps.Large.Width / 2, TestMaps.Large.Height / 2);

        (int x, _) = Place(At(TestMaps.Large, middle));

        int leadCenter = (middle.X * TilePixels) + (TilePixels / 2);
        Assert.Equal(leadCenter - (ViewWidth / 2), x);
    }

    [Fact]
    public void AMapSmallerThanTheViewSitsCentered()
    {
        // Exit test 3 of section 7.3, and F-52: the rule lives in Game and never in the
        // engine, because no page of the Godot docs states the centering (D-717).
        (int x, int y) = Place(MapState.Enter(TestMaps.Small));

        Assert.Equal(((TestMaps.Small.Width * TilePixels) - ViewWidth) / 2, x);
        Assert.Equal(((TestMaps.Small.Height * TilePixels) - ViewHeight) / 2, y);
        Assert.True(x < 0);
        Assert.True(y < 0);
    }

    [Fact]
    public void AMapSmallerThanTheViewNeverMovesWithTheLead()
    {
        MapState party = MapState.Enter(TestMaps.Small);
        (int x, int y) = Place(party);

        party.Want(StepDirection.East);
        for (int tick = 0; tick <= MapRules.TicksPerStep; tick += 1)
        {
            party.Advance();
            Assert.Equal((x, y), Place(party));
        }
    }

    [Fact]
    public void TheLeadSlidesForwardOnEveryTickOfAStepAndNeverPastTheNextTile()
    {
        // D-203: Core keeps the lead on a whole tile, and Game draws the slide. Every value
        // is a whole art pixel, so no sprite draws between two pixels (D-715).
        MapState party = MapState.Enter(TestMaps.Large);
        int start = LeadX(party);
        party.Want(StepDirection.East);
        party.Advance();

        int last = start;
        for (int tick = 1; tick <= MapRules.TicksPerStep; tick += 1)
        {
            int now = LeadX(party);
            Assert.True(now >= last, $"The lead moved back at tick {tick} (D-203).");
            Assert.True(now - start <= TilePixels, $"The lead passed the next tile at tick {tick} (D-203).");
            last = now;
            party.Advance();
        }

        Assert.Equal(start + TilePixels, LeadX(party));
    }

    [Fact]
    public void ALeadThatStandsSitsOnTheEdgeOfItsTile()
    {
        MapState party = MapState.Enter(TestMaps.Room);

        Assert.Equal(TestMaps.Room.Spawn.X * TilePixels, LeadX(party));
        Assert.Equal(TestMaps.Room.Spawn.Y * TilePixels, Call<int>("LeadY", party));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(7, 14)]
    [InlineData(15, 32)]
    public void TheSlideOfAStepGrowsWithTheTicksOfThatStep(int stepTicks, int pixels)
    {
        Assert.Equal(pixels, Call<int>("SlideOf", 0, 1, stepTicks, MapRules.TicksPerStep));
        Assert.Equal(-pixels, Call<int>("SlideOf", 0, -1, stepTicks, MapRules.TicksPerStep));
    }

    [Theory]
    [InlineData(30, 0, 0)]
    [InlineData(30, 15, 16)]
    [InlineData(30, 29, 30)]
    [InlineData(40, 20, 16)]
    public void TheSlideOfAnEnemyReadsTheStepCountOfItsOwnRecord(int ticksPerStep, int stepTicks, int pixels)
    {
        // D-742: each enemy carries the count of ticks of its own step, so the slide reads
        // that count and not the count of the party.
        Assert.Equal(pixels, Call<int>("SlideOf", 0, 1, stepTicks, ticksPerStep));
    }

    [Theory]
    [InlineData(1280, 640, 0, 0)]
    [InlineData(1280, 640, 640, 336)]
    [InlineData(1280, 640, 1248, 640)]
    [InlineData(288, 640, 64, -176)]
    public void TheAxisRuleClampsALargeMapAndCentersASmallOne(
        int mapPixels,
        int viewPixels,
        int leadPixels,
        int wanted)
    {
        Assert.Equal(wanted, Call<int>("AxisOf", mapPixels, viewPixels, leadPixels));
    }

    [Fact]
    public void AViewOfNoSizeIsAnError()
    {
        MapState party = MapState.Enter(TestMaps.Room);

        TargetInvocationException error = Assert.Throws<TargetInvocationException>(
            () => Method("Of").Invoke(null, [party, 0, ViewHeight]));

        Assert.IsType<ArgumentOutOfRangeException>(error.InnerException);
    }

    private static MapState At(GameMap map, TilePoint at)
    {
        WalkedTiles walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(at);
        return MapState.Resume(map, at, StepDirection.South, null, 0, walked, null, null, null, "the test");
    }

    private static int LeadX(MapState party) => Call<int>("LeadX", party);

    /// <summary>Gives the place of the view as its two pixels, through the Game assembly.</summary>
    private static (int X, int Y) Place(MapState party)
    {
        object place = Method("Of").Invoke(null, [party, ViewWidth, ViewHeight])
            ?? throw new InvalidOperationException("The camera gave no place (T-2).");

        Type type = place.GetType();
        return (
            (int)type.GetProperty("X")!.GetValue(place)!,
            (int)type.GetProperty("Y")!.GetValue(place)!);
    }

    private static T Call<T>(string name, params object[] arguments) =>
        (T)Method(name).Invoke(null, arguments)!;

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(TypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"The camera holds no method '{name}' (T-2).");

    private static int Constant(string name) =>
        (int)GameAssemblyFile.Type(TypeName)
            .GetField(name, BindingFlags.Public | BindingFlags.Static)!
            .GetRawConstantValue()!;
}
