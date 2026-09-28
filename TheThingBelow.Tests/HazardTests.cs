using System;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The tests of the deep snow and the ice of PR-64, and of the load check of a field of ice (D-1232, D-1233).</summary>
public sealed class HazardTests
{
    private const ulong Seed = 0x64;

    /// <summary>A map whose row of ice has no way out: each slide ends on the ice again (D-1232).</summary>
    private const string ClosedIceFile = """
    {
     "comment": "A closed row of ice, which the load refuses.",
     "id": "map.test_closed_ice",
     "region": "region.test",
     "label": "label.test_room",
     "time": "day",
     "dark": false,
     "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "reopen": [],
     "terrain": [
      "######",
      "#.####",
      "######",
      "#====#",
      "######"
     ],
     "things": [
      { "id": "spawn_point.test_closed_ice_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [], "triggers": []
    }
    """;

    [Fact]
    public void AStepIntoDeepSnowAndAStepOutOfItEachTakeDoubleTime()
    {
        // D-1233: the step from (2, 1) onto the snow and the step from the snow each take 32 ticks,
        // and a step on bare ground takes 16.
        Simulation run = Start();
        Assert.Equal(MapRules.TicksPerStep, TicksOfStep(run, StepDirection.East));
        Assert.Equal(MapRules.SnowStepTicks, TicksOfStep(run, StepDirection.East));
        Assert.Equal(MapRules.SnowStepTicks, TicksOfStep(run, StepDirection.East));
        Assert.Equal(MapRules.SnowStepTicks, TicksOfStep(run, StepDirection.East));
        Assert.Equal(MapRules.TicksPerStep, TicksOfStep(run, StepDirection.East));
        Assert.Equal(new TilePoint(6, 1), run.State.Party.LeadAt);
    }

    [Fact]
    public void TheLengthOfAStepComesFromTheMapAndTheStep()
    {
        // D-1233: the rule reads the start tile and the next tile alone.
        GameMap yard = TrapMaps.Yard;
        Assert.Equal(MapRules.TicksPerStep, MapRules.PartyStepTicks(yard, new TilePoint(1, 1), StepDirection.East));
        Assert.Equal(MapRules.SnowStepTicks, MapRules.PartyStepTicks(yard, new TilePoint(2, 1), StepDirection.East));
        Assert.Equal(MapRules.SnowStepTicks, MapRules.PartyStepTicks(yard, new TilePoint(4, 1), StepDirection.East));
        Assert.Equal(MapRules.SnowStepTicks, MapRules.PartyStepTicks(yard, new TilePoint(3, 2), StepDirection.North));
        Assert.Equal(MapRules.TicksPerStep, MapRules.PartyStepTicks(yard, new TilePoint(5, 1), StepDirection.East));
    }

    [Fact]
    public void AResumeInsideAStepOfSnowKeepsItsLongerRange()
    {
        // D-1233, T-2: a step of snow can stand at 20 ticks, and a step on bare ground cannot.
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 1);
        run.Step([HubWalks.Move(StepDirection.East)]);
        for (int tick = 0; tick < 20; tick += 1)
        {
            run.Step([]);
        }

        RunSnapshot snowy = run.Snapshot();
        Assert.Equal(20, snowy.Map!.StepTicks);
        Simulation resumed = Simulation.Resume(Seed, snowy, TrapMaps.Yard, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        Assert.Equal(run.StateHash(), resumed.StateHash());

        RunSnapshot bare = snowy with { Map = snowy.Map with { LeadX = 5, Facing = StepDirection.East, Stepping = StepDirection.East, Walked = MarkWalked(snowy.Map.Walked, new TilePoint(5, 1)) } };
        ArgumentException error = Assert.Throws<ArgumentException>(() => Simulation.Resume(Seed, bare, TrapMaps.Yard, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));
        Assert.Contains("D-1233", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLeadSlidesOverIceUntilAWallStopsItAndThePlayerCannotTurn()
    {
        // D-1232: a step east from (1, 3) slides to (5, 3), where the wall at (6, 3) stops it. A push to
        // the north on each tick of the slide never turns the lead.
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.South, 2);
        run.Step([HubWalks.Move(StepDirection.East)]);
        for (int tick = 0; tick < 200 && run.State.Party.Stepping is not null; tick += 1)
        {
            run.Step([HubWalks.Move(StepDirection.North)]);
            if (run.State.Party.LeadAt.X < 5)
            {
                Assert.Equal(StepDirection.East, run.State.Party.Facing);
            }
        }

        Assert.Equal(new TilePoint(5, 3), run.State.Party.LeadAt);
        Assert.Equal(TileKind.Ice, TrapMaps.Yard.TileAt(run.State.Party.LeadAt));
    }

    [Fact]
    public void ASlideEndsOnTheFirstTileThatIsNotIce()
    {
        // D-1232: a step south onto the ice at (2, 3) slides on to the ground at (2, 4) and stops there.
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Walk(run, StepDirection.South, 1);

        HubWalks.Walk(run, StepDirection.South, 1);

        Assert.Equal(new TilePoint(2, 4), run.State.Party.LeadAt);
        Assert.Null(run.State.Party.Stepping);
    }

    [Fact]
    public void TheLeadStandingOnIceStepsAnyWay()
    {
        // D-1232: a slide that a wall stopped leaves the lead on the ice, and a step north then leaves it.
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.South, 2);
        HubWalks.Walk(run, StepDirection.East, 1);
        Assert.Equal(new TilePoint(5, 3), run.State.Party.LeadAt);

        HubWalks.Walk(run, StepDirection.North, 1);

        Assert.Equal(new TilePoint(5, 2), run.State.Party.LeadAt);
    }

    [Fact]
    public void TheLoadRefusesAFieldOfIceWithNoWayOut()
    {
        // D-1232: the row of ice at row 3 holds a stop at each end, and each slide ends at the other.
        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("test-closed-ice.json", ClosedIceFile));

        Assert.Contains("(1, 3)", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1232", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EachFieldOfIceOfTheYardAndOfTheFixtureDungeonHasAWayOut()
    {
        // D-1232: the load passes, and the check finds no fault.
        Assert.Null(IceFields.FaultOf(TrapMaps.Yard));
        Assert.Null(IceFields.FaultOf(TestMaps.FixtureDungeon));
        Assert.Equal(TileKind.Ice, TestMaps.FixtureDungeon.TileAt(new TilePoint(10, 14)));
    }

    [Fact]
    public void TheFixtureDungeonHoldsAPatchOfEachHazard()
    {
        // D-1238: the bots walk each hazard of the fixture dungeon.
        GameMap dungeon = TestMaps.FixtureDungeon;
        Assert.Equal(TileKind.Snow, dungeon.TileAt(new TilePoint(17, 5)));
        Assert.Equal(TileKind.Ice, dungeon.TileAt(new TilePoint(11, 16)));
        Assert.Equal(TileKind.BadAir, dungeon.TileAt(new TilePoint(26, 19)));
    }

    private static Simulation Start() =>
        Simulation.Start(Seed, TrapMaps.Yard, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    /// <summary>Walks one step, and gives the count of ticks from the start of the step to the arrival.</summary>
    private static int TicksOfStep(Simulation run, StepDirection direction)
    {
        run.Step([HubWalks.Move(direction)]);
        Assert.NotNull(run.State.Party.Stepping);
        int ticks = 0;
        while (run.State.Party.Stepping is not null)
        {
            run.Step([]);
            ticks += 1;
            Assert.True(ticks <= MapRules.SnowStepTicks, $"The step from {run.State.Party.LeadAt} ran past {MapRules.SnowStepTicks} ticks.");
        }

        return ticks;
    }

    private static string[] MarkWalked(System.Collections.Generic.IReadOnlyList<string> walked, TilePoint at)
    {
        string[] rows = [.. walked];
        char[] row = rows[at.Y].ToCharArray();
        row[at.X] = 'x';
        rows[at.Y] = new string(row);
        return rows;
    }
}
