using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The invisible fights of the overworld of PR-109: the step counter, the zones, and the danger
/// count in the snapshot (D-1249 to D-1251, D-1261 to D-1266).
/// </summary>
public sealed class EncounterRulesTests
{
    private const ulong Seed = 20260927;

    /// <summary>The count of seeds of each seed loop.</summary>
    private const int Seeds = 100;

    /// <summary>The walk of each run of a seed loop, which a rate of 100 ends in a fight far before its end.</summary>
    private const int LongWalk = 200;

    [Fact]
    public void TheSameSeedAndStepsGiveTheSameFights()
    {
        // Exit test 1 of PR-109 (T-7, G-4): the draw and the pick come from the seed alone.
        GameMap map = EncounterMaps.Of(100, """[{ "group": "group.one", "weight": 1 }, { "group": "group.other", "weight": 1 }]""");
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            (int? Steps, string? Group, ulong Hash) first = FirstFight(seed, map);
            (int? Steps, string? Group, ulong Hash) again = FirstFight(seed, map);

            Assert.True(first.Steps is not null, $"Seed {seed}: no fight started in {LongWalk} steps at rate 100.");
            Assert.True(first == again, $"Seed {seed}: the first run met {first} and the second run met {again}.");
        }
    }

    [Fact]
    public void AZoneAtRateZeroNeverStartsAFightAndDrawsNothing()
    {
        // Exit test 2 of PR-109, and D-1263.
        GameMap map = EncounterMaps.Of(0, "[]");
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            Simulation run = EncounterMaps.Start(seed, map);
            StreamPosition before = PositionOf(run, StreamId.Encounter);

            int? fight = EncounterMaps.WalkUntilFight(run, 50);

            Assert.True(fight is null, $"Seed {seed}: a zone at rate 0 started a fight at step {fight}.");
            Assert.True(run.State.Danger == 0, $"Seed {seed}: the danger count is {run.State.Danger}.");
            Assert.True(before == PositionOf(run, StreamId.Encounter), $"Seed {seed}: a zone at rate 0 drew on the encounter stream.");
        }
    }

    [Fact]
    public void AZoneWhoseConditionFailsStartsNoFightAndTheSameZoneFightsWhenItHolds()
    {
        // Exit test 3 of PR-109, and D-1251: at rate 10000 each live step is a certain fight.
        GameMap map = EncounterMaps.Read(EncounterMaps.TextOf(EncounterMaps.FlaggedZone(BasisPoints.One)));
        Simulation run = EncounterMaps.Start(Seed, map);
        StreamPosition before = PositionOf(run, StreamId.Encounter);

        Assert.Null(EncounterMaps.WalkUntilFight(run, 30));
        Assert.Equal(0, run.State.Danger);
        Assert.Equal(before, PositionOf(run, StreamId.Encounter));

        run.State.Story.Flags.TurnOn(OverworldMaps.Id(EncounterMaps.ZoneFlag));

        Assert.Equal(1, EncounterMaps.WalkUntilFight(run, 1));
        Battle battle = BattleRuns.BattleOf(run);
        Assert.True(battle.FromZone);
        Assert.Equal(("zone.test_wild", "group.one"), (battle.Enemy.Value, battle.Group.Id.Value));
    }

    [Fact]
    public void AFightOfAZoneStartsWithTheLeadOnItsTileAndNoNextStep()
    {
        // The playtest: the player held a direction, the fight started on the arrival, and after
        // the fight the lead walked on to the next tile. The fight now stops the step that
        // started on its tick (D-1374). At rate 10000 the first live arrival is a certain fight.
        GameMap map = EncounterMaps.Of(BasisPoints.One);
        Simulation run = EncounterMaps.Start(Seed, map);
        TilePoint from = run.State.Party.LeadAt;

        for (int tick = 0; run.State.Battle is null; tick += 1)
        {
            Assert.True(tick < 64, "No fight started in 64 ticks of a held step at rate 10000.");
            run.Step([HubWalks.Move(StepDirection.East)]);
        }

        Assert.Equal(from.Step(StepDirection.East), run.State.Party.LeadAt);
        Assert.Null(run.State.Party.Stepping);
        Assert.Equal(0, run.State.Party.StepTicks);
        Assert.Equal(StepDirection.East, run.State.Party.Facing);
    }

    [Fact]
    public void AFightSetsTheDangerCountToZero()
    {
        // Exit test 4 of PR-109, and D-1249.
        GameMap map = EncounterMaps.Of(BasisPoints.One);
        Simulation run = EncounterMaps.Start(Seed, map);

        Assert.Equal(1, EncounterMaps.WalkUntilFight(run, 1));

        Assert.Equal(0, run.State.Danger);
        Assert.NotNull(run.State.Battle);
    }

    [Fact]
    public void EachStepOfALiveZoneAddsItsRateAndTheCountStopsAtTenThousand()
    {
        // D-1261: the count grows by the rate, and a fight at the full count is certain. A rate of 1
        // gives a fight in about 125 steps, so this seed walks 3 with none.
        GameMap slow = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, slow);

        Assert.Null(EncounterMaps.WalkUntilFight(run, 3));
        Assert.Equal(3, run.State.Danger);

        GameMap sure = EncounterMaps.Of(BasisPoints.One);
        Simulation full = EncounterMaps.Start(Seed, sure);
        Assert.Equal(1, EncounterMaps.WalkUntilFight(full, 1));
        Assert.Equal(0, full.State.Danger);
    }

    [Fact]
    public void TheSnapshotHoldsTheDangerCountAcrossAReload()
    {
        // Exit test 4 of PR-109 (D-1249): the count lives in the snapshot, and the state hash holds it.
        GameMap map = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, map);
        Assert.Null(EncounterMaps.WalkUntilFight(run, 3));

        string line = RunSnapshotText.Write(run.Snapshot());
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(line), "the test");
        Simulation reloaded = EncounterMaps.Resume(Seed, map, RunSnapshotText.Read(ref reader));

        Assert.Contains("\"danger\":3", line, StringComparison.Ordinal);
        Assert.Equal(3, reloaded.State.Danger);
        Assert.Equal(run.StateHash(), reloaded.StateHash());
    }

    [Fact]
    public void TheStateHashReadsTheDangerCount()
    {
        // G-5: two runs that differ in the count alone give two hashes.
        GameMap map = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, map);
        Assert.Null(EncounterMaps.WalkUntilFight(run, 3));
        RunSnapshot snapshot = run.Snapshot();

        Simulation other = EncounterMaps.Resume(Seed, map, snapshot with { Danger = 2 });

        Assert.NotEqual(run.StateHash(), other.StateHash());
    }

    [Fact]
    public void AStepOntoASafeZoneKeepsTheDangerCount()
    {
        // D-1263: the road keeps the count, and draws nothing.
        GameMap map = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, map);
        EncounterMaps.StepOnce(run, StepDirection.East);
        EncounterMaps.StepOnce(run, StepDirection.East);
        Assert.Equal(2, run.State.Danger);
        StreamPosition before = PositionOf(run, StreamId.Encounter);

        EncounterMaps.StepOnce(run, StepDirection.West);
        EncounterMaps.StepOnce(run, StepDirection.West);

        Assert.Equal(new TilePoint(1, 2), run.State.Party.LeadAt);
        Assert.Equal(3, run.State.Danger);
        EncounterMaps.StepOnce(run, StepDirection.South);
        Assert.Equal(3, run.State.Danger);
        EncounterMaps.StepOnce(run, StepDirection.North);
        Assert.Equal(3, run.State.Danger);
        Assert.NotEqual(before, PositionOf(run, StreamId.Encounter));
    }

    [Fact]
    public void AVisitToAPlaceKeepsTheDangerCount()
    {
        // D-1264: the run holds one count, and the entry to a place and the exit back keep it.
        GameMap map = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, map);
        EncounterMaps.StepOnce(run, StepDirection.East);
        EncounterMaps.StepOnce(run, StepDirection.West);
        Assert.Equal(1, run.State.Danger);

        EncounterMaps.StepOnce(run, StepDirection.North);
        Assert.Equal("map.test_place", run.State.Party.Map.Id.Value);
        Assert.Equal(1, run.State.Danger);

        HubWalks.Walk(run, StepDirection.East, 4);

        Assert.Equal("map.test_overworld", run.State.Party.Map.Id.Value);
        Assert.Equal(1, run.State.Danger);
    }

    [Fact]
    public void TheDrawsComeFromTheEncounterStreamAlone()
    {
        // G-4: a step onto a live zone moves the encounter stream, and no other stream.
        GameMap map = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, map);
        List<StreamPosition> before = [.. run.Snapshot().Streams];

        EncounterMaps.StepOnce(run, StepDirection.East);

        IReadOnlyList<StreamPosition> after = run.Snapshot().Streams;
        for (int index = 0; index < before.Count; index += 1)
        {
            bool moved = before[index] != after[index];
            Assert.True(moved == (before[index].Stream == StreamId.Encounter), $"The stream {before[index].Stream} moved: {moved}.");
        }
    }

    [Fact]
    public void TheWeightsOfAZoneGiveEachGroupItsShare()
    {
        // D-1250: a group of weight 3 comes about three times as often as a group of weight 1. The
        // bounds are wide, so the loop holds on each seed range of the same size.
        GameMap map = EncounterMaps.Of(BasisPoints.One, """[{ "group": "group.one", "weight": 3 }, { "group": "group.other", "weight": 1 }]""");
        int one = 0;
        int other = 0;
        for (ulong seed = 1; seed <= 400; seed += 1)
        {
            Simulation run = EncounterMaps.Start(seed, map);
            Assert.True(EncounterMaps.WalkUntilFight(run, 1) == 1, $"Seed {seed}: the first step of a zone at rate 10000 started no fight.");
            if (BattleRuns.BattleOf(run).Group.Id.Value == "group.one")
            {
                one += 1;
            }
            else
            {
                other += 1;
            }
        }

        Assert.InRange(one, 240, 360);
        Assert.Equal(400, one + other);
    }

    [Fact]
    public void AGroupOfAZoneTakesTheShareOfItsWeightInTheOrderOfTheFile()
    {
        // D-1250, G-4: the draw reads the shares in the order of the file.
        EncounterZone zone = EncounterMaps.Of(100, """[{ "group": "group.one", "weight": 3 }, { "group": "group.other", "weight": 1 }]""").ZoneOf(OverworldMaps.Id("zone.test_wild"))!;

        Assert.Equal(4, zone.TotalWeight);
        Assert.Equal(["group.one", "group.one", "group.one", "group.other"], [zone.GroupAt(0).Value, zone.GroupAt(1).Value, zone.GroupAt(2).Value, zone.GroupAt(3).Value]);
        Assert.Throws<ArgumentOutOfRangeException>(() => zone.GroupAt(4));
        Assert.Throws<ArgumentOutOfRangeException>(() => zone.GroupAt(-1));
    }

    [Fact]
    public void AFightOfAZoneRunsWithNoEncounterOfAPatrol()
    {
        // D-1266: no patrol takes part, so the map holds no encounter while the fight runs. The
        // tests of the transition in Game read the side of D-1265.
        Simulation run = EncounterMaps.Start(Seed, EncounterMaps.Of(BasisPoints.One));
        _ = EncounterMaps.WalkUntilFight(run, 1);

        Assert.True(BattleRuns.BattleOf(run).FromZone);
        Assert.Null(run.State.Party.Patrols.Encounter);
    }

    [Fact]
    public void AFleeEndsTheFightOfAZoneAndTheMapRunsAgain()
    {
        // D-1266: the party can flee, and the next step of the zone can fight again at once.
        Simulation run = EncounterMaps.Start(Seed, EncounterMaps.Of(BasisPoints.One), TestBattles.SureFlee);
        _ = EncounterMaps.WalkUntilFight(run, 1);
        _ = BattleRuns.Kinds(run);

        run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]);
        Assert.Equal(BattleOutcome.Fled, BattleRuns.BattleOf(run).Outcome);
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Null(run.State.Battle);
        Assert.Equal(1, EncounterMaps.WalkUntilFight(run, 1));
    }

    [Fact]
    public void AWinOfAZoneFightNotesNoWinForTheStory()
    {
        // D-1266: a battle end trigger reads the patrols alone.
        Simulation run = EncounterMaps.Start(Seed, EncounterMaps.Of(BasisPoints.One));
        _ = EncounterMaps.WalkUntilFight(run, 1);
        string storyBefore = StoryOf(RunSnapshotText.Write(run.Snapshot()));

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Null(run.State.Battle);
        Assert.Null(run.State.Story.Values().WonPatrol);
        Assert.Equal(storyBefore, StoryOf(RunSnapshotText.Write(run.Snapshot())));
    }

    [Fact]
    public void TheFightOfAZoneResumesFromItsSnapshot()
    {
        // D-1266, T-2: the snapshot of the fight names the zone, and the resume finds it on the map.
        GameMap map = EncounterMaps.Of(BasisPoints.One);
        Simulation run = EncounterMaps.Start(Seed, map);
        _ = EncounterMaps.WalkUntilFight(run, 1);

        Simulation resumed = EncounterMaps.Resume(Seed, map, run.Snapshot());

        Assert.True(BattleRuns.BattleOf(resumed).FromZone);
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void ASnapshotOfAZoneFightWithADangerCountIsRefused()
    {
        // D-1249, T-2: each fight of a zone sets the count to zero.
        GameMap map = EncounterMaps.Of(BasisPoints.One);
        Simulation run = EncounterMaps.Start(Seed, map);
        _ = EncounterMaps.WalkUntilFight(run, 1);

        ArgumentException error = Assert.Throws<ArgumentException>(() => EncounterMaps.Resume(Seed, map, run.Snapshot() with { Danger = 5 }));

        Assert.Contains("D-1249", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfAZoneFightOfAGroupThatTheZoneLacksIsRefused()
    {
        // D-1266, T-2: the zone of the fight lies on the map and lists the group.
        GameMap map = EncounterMaps.Of(BasisPoints.One);
        Simulation run = EncounterMaps.Start(Seed, map);
        _ = EncounterMaps.WalkUntilFight(run, 1);
        GameMap other = EncounterMaps.Of(BasisPoints.One, """[{ "group": "group.other", "weight": 1 }]""");

        ArgumentException error = Assert.Throws<ArgumentException>(() => EncounterMaps.Resume(Seed, other, run.Snapshot()));

        Assert.Contains("zone.test_wild", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1266", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void ASnapshotWithADangerCountOutsideItsRangeIsRefused(int danger)
    {
        // D-1261, T-2.
        GameMap map = EncounterMaps.Of(1);
        Simulation run = EncounterMaps.Start(Seed, map);

        ArgumentException error = Assert.Throws<ArgumentException>(() => EncounterMaps.Resume(Seed, map, run.Snapshot() with { Danger = danger }));

        Assert.Contains("the danger count", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADungeonStartsNoFightOfAZone()
    {
        // D-1247: every map other than the overworld keeps its visible enemies alone.
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Place);
        StreamPosition before = PositionOf(run, StreamId.Encounter);

        HubWalks.Walk(run, StepDirection.East, 3);

        Assert.Null(run.State.Battle);
        Assert.Equal(0, run.State.Danger);
        Assert.Equal(before, PositionOf(run, StreamId.Encounter));
    }

    /// <summary>Walks one seed to its first fight, and gives the step, the group, and the state hash.</summary>
    private static (int? Steps, string? Group, ulong Hash) FirstFight(ulong seed, GameMap map)
    {
        Simulation run = EncounterMaps.Start(seed, map);
        int? steps = EncounterMaps.WalkUntilFight(run, LongWalk);
        return (steps, run.State.Battle?.Group.Id.Value, run.StateHash());
    }

    private static StreamPosition PositionOf(Simulation run, StreamId stream)
    {
        foreach (StreamPosition position in run.Snapshot().Streams)
        {
            if (position.Stream == stream)
            {
                return position;
            }
        }

        throw new InvalidOperationException($"The run holds no stream {stream}.");
    }

    /// <summary>Gives the story object of a snapshot line.</summary>
    private static string StoryOf(string line)
    {
        int start = line.IndexOf("\"story\":", StringComparison.Ordinal);
        int end = line.IndexOf(",\"stock\":", StringComparison.Ordinal);
        return line[start..end];
    }
}
