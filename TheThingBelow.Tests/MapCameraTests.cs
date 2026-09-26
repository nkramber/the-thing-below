using System;
using System.Collections.Generic;
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
        Assert.Equal(TestMaps.Room.Spawn.Y * TilePixels, Call<int>("LeadY", party, 0));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(7, 14)]
    [InlineData(16, 32)]
    public void TheSlideOfAStepGrowsWithTheTicksOfThatStep(int stepTicks, int pixels)
    {
        Assert.Equal(pixels, Call<int>("SlideOf", 0, 1, stepTicks, MapRules.TicksPerStep, 0));
        Assert.Equal(-pixels, Call<int>("SlideOf", 0, -1, stepTicks, MapRules.TicksPerStep, 0));
    }

    [Theory]
    [InlineData(32, 0, 0)]
    [InlineData(32, 15, 15)]
    [InlineData(32, 31, 31)]
    [InlineData(64, 20, 10)]
    public void TheSlideOfAnEnemyReadsTheStepCountOfItsOwnRecord(int ticksPerStep, int stepTicks, int pixels)
    {
        // D-742: each enemy carries the count of ticks of its own step, so the slide reads
        // that count and not the count of the party.
        Assert.Equal(pixels, Call<int>("SlideOf", 0, 1, stepTicks, ticksPerStep, 0));
    }

    [Theory]
    [InlineData(StepDirection.East, 8, 80, 64)]
    [InlineData(StepDirection.West, 8, 48, 64)]
    [InlineData(StepDirection.South, 12, 96, 88)]
    public void AnNpcSlidesAcrossTheTicksOfItsOwnStep(StepDirection stepping, int stepTicks, int x, int y)
    {
        // D-203, D-1138: the barmaid of the test yard steps in 16 ticks, so 8 ticks of her step
        // move her half a tile, 16 art pixels, and Core keeps her on her tile until the step ends.
        GameMap map = HubMaps.Of(npcs: HubMaps.Walker(tiles: """{ "x": 1, "y": 2, "wait_ticks": 0 }, { "x": 3, "y": 2, "wait_ticks": 0 }, { "x": 3, "y": 5, "wait_ticks": 0 }"""));
        Npc record = map.Npcs[0];
        int target = stepping == StepDirection.West ? 0 : stepping == StepDirection.East ? 1 : 2;
        TilePoint at = stepping == StepDirection.South ? new TilePoint(3, 2) : new TilePoint(2, 2);
        NpcState npc = NpcState.Resume(record, map, new NpcValues(record.Id, at.X, at.Y, stepping, stepping, stepTicks, target, stepping != StepDirection.West, 0, false), "the test");

        Assert.Equal(x, Call<int>("NpcX", npc, 0));
        Assert.Equal(y, Call<int>("NpcY", npc, 0));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void EachTickOfAnEvenStepMovesTheSameCountOfPixels(int ticksPerStep)
    {
        // D-821. A step that divides the tile of 32 art pixels, or that 32 divides, moves the
        // sprite by one count on each tick. A step of 15, 30, or 40 ticks once moved it by two
        // counts, which showed as a hitch at each tile.
        // A step of 64 ticks moves 1 pixel every other tick, so it compares two ticks at a time.
        int period = Math.Max(1, ticksPerStep / TilePixels);
        var moves = new SortedSet<int>();
        for (int tick = period; tick <= ticksPerStep; tick += period)
        {
            int now = tick == ticksPerStep ? TilePixels : Call<int>("SlideOf", 0, 1, tick, ticksPerStep, 0);
            int before = Call<int>("SlideOf", 0, 1, tick - period, ticksPerStep, 0);
            moves.Add(now - before);
        }

        Assert.Single(moves);
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(500, 9)]
    [InlineData(999, 9)]
    public void ThePartOfATickMovesTheSlideBetweenTwoTicks(int tickPart, int pixels)
    {
        // D-820. The party steps in 16 ticks, 2 art pixels a tick, so tick 4 sits at 8 pixels,
        // and half of the next tick adds one whole pixel.
        Assert.Equal(pixels, Call<int>("SlideOf", 0, 1, 4, MapRules.TicksPerStep, tickPart));
        Assert.Equal(-pixels, Call<int>("SlideOf", 0, -1, 4, MapRules.TicksPerStep, tickPart));
    }

    [Fact]
    public void ThePartOfATickNeverPassesTheNextTile()
    {
        // D-820. The last tick of a step and the largest part stay short of the next tile.
        Assert.Equal(TilePixels - 1, Call<int>("SlideOf", 0, 1, MapRules.TicksPerStep - 1, MapRules.TicksPerStep, 999));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void APartOutsideOneTickFails(int tickPart)
    {
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => Call<int>("SlideOf", 0, 1, 0, MapRules.TicksPerStep, tickPart));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
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
            () => Method("Of").Invoke(null, [party, 0, ViewHeight, 0]));

        Assert.IsType<ArgumentOutOfRangeException>(error.InnerException);
    }

    private static MapState At(GameMap map, TilePoint at)
    {
        WalkedTiles walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(at);
        return MapState.Resume(map, at, StepDirection.South, null, 0, walked, null, null, null, "the test");
    }

    private static int LeadX(MapState party) => Call<int>("LeadX", party, 0);

    /// <summary>Gives the place of the view as its two pixels, through the Game assembly.</summary>
    private static (int X, int Y) Place(MapState party)
    {
        object place = Method("Of").Invoke(null, [party, ViewWidth, ViewHeight, 0])
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
