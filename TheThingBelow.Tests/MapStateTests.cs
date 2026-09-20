using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The party on one map: the step, the facing, and the walked record (D-203, D-567, D-716).
/// Core keeps the lead on a whole tile through the whole step.
/// </summary>
public sealed class MapStateTests
{
    [Fact]
    public void ThePartyEntersAMapOnItsSpawnPoint()
    {
        MapState party = MapState.Enter(TestMaps.Room);

        Assert.Equal(TestMaps.Room.Spawn, party.LeadAt);
        Assert.Null(party.Stepping);
        Assert.Equal(0, party.StepTicks);
        Assert.Equal(StepDirection.South, party.Facing);
    }

    [Fact]
    public void AStepStartsOnTheTickOfItsIntent()
    {
        MapState party = MapState.Enter(TestMaps.Room);

        party.Want(StepDirection.East);
        Assert.True(party.Advance(out _, out StepDirection? started) is false);

        Assert.Equal(StepDirection.East, started);
        Assert.Equal(StepDirection.East, party.Stepping);
        Assert.Equal(0, party.StepTicks);
    }

    [Fact]
    public void TheLeadStaysOnItsTileUntilTheLastTickOfTheStep()
    {
        MapState party = MapState.Enter(TestMaps.Room);
        TilePoint start = party.LeadAt;
        party.Want(StepDirection.East);
        party.Advance(out _, out _);

        for (int tick = 1; tick < MapRules.TicksPerStep; tick += 1)
        {
            Assert.False(party.Advance(out _, out _));
            Assert.Equal(start, party.LeadAt);
        }

        Assert.True(party.Advance(out TilePoint walked, out _));
        Assert.Equal(start.Step(StepDirection.East), party.LeadAt);
        Assert.Equal(party.LeadAt, walked);
        Assert.True(party.Walked.WasWalked(party.LeadAt));
    }

    [Fact]
    public void AHeldDirectionChainsTheNextStep()
    {
        // Game makes one intent for each tick that the key is down, so a move intent while a
        // step runs is the normal case (D-716).
        MapState party = MapState.Enter(TestMaps.Room);
        TilePoint start = party.LeadAt;

        for (int tick = 0; tick <= MapRules.TicksPerStep * 2; tick += 1)
        {
            party.Want(StepDirection.East);
            party.Advance(out _, out _);
        }

        Assert.Equal(new TilePoint(start.X + 2, start.Y), party.LeadAt);
        Assert.Equal(3, party.Walked.Count);
    }

    [Fact]
    public void AStepIntoAWallTurnsTheLeadAndMovesIt()
    {
        // A push against a wall turns the lead, so the player reads the direction of the
        // party from its sprite (D-207).
        MapState party = MapState.Enter(TestMaps.Room);
        TilePoint start = party.LeadAt;

        party.Want(StepDirection.North);
        party.Advance(out _, out StepDirection? started);
        party.Want(StepDirection.West);
        party.Advance(out _, out StepDirection? blocked);

        Assert.Equal(StepDirection.North, started);
        Assert.Null(blocked);
        Assert.Equal(StepDirection.North, party.Stepping);
        Assert.Equal(start, party.LeadAt);
    }

    [Fact]
    public void AStepIntoAWallFromAStandingLeadTurnsItAlone()
    {
        MapState party = MapState.Resume(
            TestMaps.Room,
            new TilePoint(1, 1),
            StepDirection.South,
            null,
            0,
            WalkedOf(TestMaps.Room, new TilePoint(1, 1)),
            "the test");

        party.Want(StepDirection.West);
        bool arrived = party.Advance(out _, out StepDirection? started);

        Assert.False(arrived);
        Assert.Null(started);
        Assert.Null(party.Stepping);
        Assert.Equal(StepDirection.West, party.Facing);
        Assert.Equal(new TilePoint(1, 1), party.LeadAt);
    }

    [Fact]
    public void AWantedDirectionLastsOneTickAlone()
    {
        // No step starts from an intent of an earlier tick, so a replay that drops a tick
        // never gains a step (T-7).
        MapState party = MapState.Enter(TestMaps.Room);

        party.Want(StepDirection.East);
        for (int tick = 0; tick <= MapRules.TicksPerStep; tick += 1)
        {
            party.Advance(out _, out _);
        }

        Assert.Null(party.Stepping);
    }

    [Fact]
    public void ADirectionThatNamesNoStepIsAnError()
    {
        MapState party = MapState.Enter(TestMaps.Room);

        Assert.Throws<ArgumentOutOfRangeException>(() => party.Want((StepDirection)9));
    }

    [Fact]
    public void TheSightRangeComesFromTheTimeOfDayOfTheMap()
    {
        Assert.Equal(MapRules.PartySightRange(TimeOfDay.Day), MapState.Enter(TestMaps.Room).SightRange);
    }

    [Fact]
    public void AResumeOnAnotherMapSizeIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            TestMaps.Room,
            TestMaps.Room.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedTiles.Empty(4, 4),
            "the save"));

        Assert.Contains("the save", error.Message, StringComparison.Ordinal);
        Assert.Contains("4 by 4", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeWithTheLeadOnAWallIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            TestMaps.Room,
            new TilePoint(0, 0),
            StepDirection.South,
            null,
            0,
            WalkedOf(TestMaps.Room, new TilePoint(0, 0)),
            "the save"));

        Assert.Contains("wall", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeWithTheLeadOutsideTheMapIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            TestMaps.Room,
            new TilePoint(40, 40),
            StepDirection.South,
            null,
            0,
            WalkedTiles.Empty(TestMaps.Room.Width, TestMaps.Room.Height),
            "the save"));

        Assert.Contains("(40, 40)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeWithATileThatTheWalkedRecordLacksIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            TestMaps.Room,
            TestMaps.Room.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedTiles.Empty(TestMaps.Room.Width, TestMaps.Room.Height),
            "the save"));

        Assert.Contains("walked tiles", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeOfAStepIntoAWallIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            TestMaps.Room,
            new TilePoint(1, 1),
            StepDirection.West,
            StepDirection.West,
            3,
            WalkedOf(TestMaps.Room, new TilePoint(1, 1)),
            "the save"));

        Assert.Contains("takes no step", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeOfStandingLeadWithStepTicksIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            TestMaps.Room,
            TestMaps.Room.Spawn,
            StepDirection.South,
            null,
            4,
            WalkedOf(TestMaps.Room, TestMaps.Room.Spawn),
            "the save"));

        Assert.Contains("step ticks are 4", error.Message, StringComparison.Ordinal);
    }

    private static WalkedTiles WalkedOf(GameMap map, TilePoint at)
    {
        WalkedTiles walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(at);
        return walked;
    }
}
