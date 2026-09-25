using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The enemies of one map on the tick: the walk of a route, the pace of an area, the sight,
/// the beat, the encounter, and the grace time (D-208, D-265, D-381, D-531, D-737 to D-751).
/// </summary>
/// <remarks>
/// Each test drives a run through <see cref="Simulation"/>, so it reads the rules by the path
/// of the host and never by a second path (T-1, T-3).
/// </remarks>
public sealed class MapPatrolsTests
{
    private const ulong Seed = 20260920;

    /// <summary>The count of seeds of each property test of this class (T-3).</summary>
    private const int SeedCount = 1000;

    [Fact]
    public void TheTimeOfDayOfTheMapPlacesEachEnemy()
    {
        Simulation run = Start(PatrolMaps.Of(
            $"{PatrolMaps.Enemy()},\n{PatrolMaps.Enemy(id: "patrol.two", stations: NightOnly)}",
            "day"));

        PatrolState placed = Assert.Single(run.State.Party.Patrols.All);
        Assert.Equal("patrol.one", placed.Patrol.Id.Value);
    }

    [Fact]
    public void APatrolWalksItsRouteAndThenWalksBackDownTheList()
    {
        // D-739: the patrol walks to the last tile of the list, and then it walks back down
        // the list.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(stepTicks: 16)));
        List<TilePoint> seen = [];

        for (int tick = 0; tick < 16 * 8; tick += 1)
        {
            run.Step([]);
            seen.Add(Only(run).At);
        }

        // The step starts on the tick that the enemy arrives, so each arrival lands 16 ticks
        // after the one before it (D-742).
        Assert.Equal(new TilePoint(1, 1), seen[15]);
        Assert.Equal(new TilePoint(2, 1), seen[16]);
        Assert.Equal(new TilePoint(3, 1), seen[32]);
        Assert.Equal(new TilePoint(2, 1), seen[48]);
        Assert.Equal(new TilePoint(1, 1), seen[64]);
        Assert.Equal(new TilePoint(2, 1), seen[80]);
    }

    [Fact]
    public void AnEnemyStepsOnTheTickCountOfItsOwnRecord()
    {
        // D-742: each record gives the count of ticks of one step.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(stepTicks: 64)));

        // The enemy takes its first step on the tick that the run opens, so it arrives one
        // tick after its own count (D-742).
        for (int tick = 0; tick < 64; tick += 1)
        {
            run.Step([]);
            Assert.Equal(new TilePoint(1, 1), Only(run).At);
        }

        run.Step([]);
        Assert.Equal(new TilePoint(2, 1), Only(run).At);
    }

    [Fact]
    public void AnEnemyOfARouteOfOneTileStandsStillAndKeepsItsFacing()
    {
        // D-740: one record covers a fixed enemy and a walking one.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "north", stations: OneTile)));

        for (int tick = 0; tick < 100; tick += 1)
        {
            run.Step([]);
        }

        Assert.True(Only(run).Stands);
        Assert.Equal(new TilePoint(1, 1), Only(run).At);
        Assert.Equal(StepDirection.North, Only(run).Facing);
        Assert.Null(Only(run).Stepping);
    }

    [Fact]
    public void AnEnemyOfAnAreaMovesInsideIt()
    {
        // D-209: a large enemy does not stand perfectly still, and it keeps its place inside
        // its own area (D-741).
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Pacer));
        TilePoint start = Only(run).At;
        bool moved = false;

        for (int tick = 0; tick < 300; tick += 1)
        {
            run.Step([]);
            moved = moved || Only(run).At != start;
        }

        Assert.True(moved, "The enemy of an area never moved, and D-209 asks it to move.");
    }

    [Fact]
    public void TheEnemyOfAnAreaDrawsFromTheExplorationStream()
    {
        // G-4: one seeded stream for each subsystem, and the map draws from the exploration
        // stream (D-741).
        Simulation one = Start(PatrolMaps.Of(PatrolMaps.Pacer));
        Simulation other = Simulation.Start(Seed + 1, PatrolMaps.Of(PatrolMaps.Pacer), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        List<TilePoint> first = [];
        List<TilePoint> second = [];

        for (int tick = 0; tick < 300; tick += 1)
        {
            one.Step([]);
            other.Step([]);
            first.Add(Only(one).At);
            second.Add(Only(other).At);
        }

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void AnEnemyBlocksAStepOfThePartyAndStartsTheEncounter()
    {
        // D-747: every tile of the body refuses a step, and the step starts the encounter at
        // once, with the party as the side that reached the other.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "north", stations: NextToSpawn)));
        TilePoint spawn = run.State.Party.LeadAt;

        run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);

        Assert.Equal(spawn, run.State.Party.LeadAt);
        Assert.Null(run.State.Party.Stepping);
        Assert.Equal(StepDirection.North, run.State.Party.Facing);
        MapEncounter encounter = Assert.IsType<MapEncounter>(run.State.Party.Patrols.Encounter);
        Assert.Equal("patrol.one", encounter.Enemy.Value);
        Assert.Equal("group.one", encounter.Group.Value);
        Assert.Equal(EncounterSide.Party, encounter.Behind);
        Assert.Null(run.State.Party.Patrols.Mark);
    }

    [Fact]
    public void AStepIntoTheEnemyWhoseMarkRunsTakesTheSideOfTheBeat()
    {
        // D-1104, P3-20: the patrol saw the party first. A step into it during the beat starts
        // the encounter at once, and the facings give the side: each faces the other, so no
        // side acts first. The old rule gave the party a sneak.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: NextToSpawn)));
        run.Step([]);
        Assert.NotNull(run.State.Party.Patrols.Mark);

        run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);

        MapEncounter encounter = Assert.IsType<MapEncounter>(run.State.Party.Patrols.Encounter);
        Assert.Equal(EncounterSide.None, encounter.Behind);
        Assert.Null(run.State.Party.Patrols.Mark);
    }

    [Fact]
    public void AStepIntoABodyTakesNoBeat()
    {
        // The beat of D-208 is the telegraph of the patrol, so the approach of the party
        // needs none (D-747).
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "north", stations: NextToSpawn)));

        IReadOnlyList<LogEntry> log = run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);

        Assert.NotNull(run.State.Party.Patrols.Encounter);
        Assert.Contains(log, entry => entry.Message.Contains("stepped into an enemy", StringComparison.Ordinal));
    }

    [Fact]
    public void ASightStartsAMarkAndTheEncounterWhenTheBeatEnds()
    {
        // D-208 and D-745: a patrol that sees the party shows a mark for a beat, and then the
        // encounter starts. Nothing cancels the beat.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher)));

        run.Step([]);

        SightMark mark = Assert.IsType<SightMark>(run.State.Party.Patrols.Mark);
        Assert.Equal("patrol.one", mark.Enemy.Value);
        Assert.Equal(MapRules.BeatTicks, mark.TicksLeft);

        for (int tick = 1; tick < MapRules.BeatTicks; tick += 1)
        {
            run.Step([]);
            Assert.NotNull(run.State.Party.Patrols.Mark);
            Assert.Null(run.State.Party.Patrols.Encounter);
        }

        run.Step([]);

        Assert.Null(run.State.Party.Patrols.Mark);
        Assert.NotNull(run.State.Party.Patrols.Encounter);
    }

    [Fact]
    public void NoEnemyWalksAndNoEnemySeesWhileAnEncounterRuns()
    {
        // D-531: while a battle runs, no map system ticks, so the patrols and the grace time
        // all stand still.
        Simulation run = Start(PatrolMaps.Of($"{PatrolMaps.Enemy(facing: "north", stations: NextToSpawn)},\n{PatrolMaps.Enemy(id: "patrol.two", stations: Southwest)}"));

        run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);
        Assert.NotNull(run.State.Party.Patrols.Encounter);

        TilePoint walker = run.State.Party.Patrols.All[1].At;
        TilePoint lead = run.State.Party.LeadAt;
        ulong hash = run.StateHash();

        for (int tick = 0; tick < 200; tick += 1)
        {
            run.Step([Intent.OfPlayer(IntentIds.MoveWest)]);
        }

        Assert.Equal(walker, run.State.Party.Patrols.All[1].At);
        Assert.Equal(lead, run.State.Party.LeadAt);
        Assert.NotEqual(hash, run.StateHash());
    }

    [Fact]
    public void AFledBattleEndsTheEncounterAndStartsTheGraceTime()
    {
        // D-767: the flee of the battle replaced the flee command of PR-8, and the wait intent
        // ends the encounter (D-522).
        Simulation run = Fought();

        IReadOnlyList<LogEntry> log = FleeAndWait(run);

        Assert.Null(run.State.Party.Patrols.Encounter);
        Assert.Null(run.State.Battle);

        // The wait intent runs inside the tick, and the world step of the same tick counts one
        // tick of the grace time (D-748).
        Assert.Equal(MapRules.GraceTicks - 1, Only(run).GraceTicks);
        Assert.Contains(log, entry => entry.Level == LogLevel.Info && entry.Message.Contains("battle ended", StringComparison.Ordinal));
    }

    [Fact]
    public void TheFleeCommandWithNoBattleChangesNothingAndWarns()
    {
        // The command changes nothing in silence, and the warning names the reason and the
        // context of the tick (D-179, T-2).
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest)));

        IReadOnlyList<LogEntry> log = run.Step([Intent.OfDebugConsole(FleeAction)]);

        Assert.Null(run.State.Party.Patrols.Encounter);
        Assert.Equal(0, Only(run).GraceTicks);
        LogEntry warning = Assert.Single(log, entry => entry.Level == LogLevel.Warning);
        Assert.Contains("no battle runs", warning.Message, StringComparison.Ordinal);
        Assert.Contains(warning.Fields, field => field.Name == "context");
    }

    [Fact]
    public void NoBattleStartsInsideTheGraceTimeAndOneStartsAfterIt()
    {
        // Exit test 5 of section 7.6 of `phase-2-first-playable.md` (D-381, D-748).
        Simulation run = Fought();
        FleeAndWait(run);
        int ticks = 0;

        while (Only(run).GraceTicks > 0)
        {
            run.Step([]);
            ticks += 1;
            Assert.Null(run.State.Party.Patrols.Encounter);
            if (Only(run).GraceTicks > 0)
            {
                Assert.Null(run.State.Party.Patrols.Mark);
            }
        }

        // The wait tick counted one tick of the grace time itself, so the count ends one
        // tick before the number of D-748.
        Assert.Equal(MapRules.GraceTicks - 1, ticks);
        Assert.NotNull(run.State.Party.Patrols.Mark);
    }

    [Fact]
    public void APatrolNeverLeavesItsRouteOverOneThousandSeeds()
    {
        // Exit test 1 of section 7.6 of `phase-2-first-playable.md` (D-739).
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = Simulation.Start(seed, TwoRooms(12), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            IReadOnlyList<TilePoint> route = Only(run).Station.Tiles;

            for (int tick = 0; tick < 200; tick += 1)
            {
                run.Step([Intent.OfPlayer(WalkOf(seed, tick))]);
                PatrolState patrol = Only(run);
                Assert.True(
                    OnRoute(route, patrol.At),
                    $"The seed {seed} put the enemy at {patrol.At}, which lies on no leg of its route (D-739).");
            }
        }
    }

    [Fact]
    public void APatrolNeverSeesThroughAWallOverOneThousandSeeds()
    {
        // Exit test 2 of section 7.6 of `phase-2-first-playable.md` (D-718).
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = Simulation.Start(seed, TwoRooms(12), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

            for (int tick = 0; tick < 200; tick += 1)
            {
                run.Step([Intent.OfPlayer(WalkOf(seed, tick))]);
                MapPatrols patrols = run.State.Party.Patrols;
                Assert.True(
                    patrols.Mark is null && patrols.Encounter is null,
                    $"The seed {seed} let the enemy see the party through the wall at the tick {tick} (D-718).");
            }
        }
    }

    [Fact]
    public void ALargeEnemyNeverLeavesItsAreaOverOneThousandSeeds()
    {
        // Exit test 4 of section 7.6 of `phase-2-first-playable.md` (D-209, D-741).
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = Simulation.Start(seed, TestMaps.Patrolled, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            PatrolState elite = run.State.Party.Patrols.All[2];
            TileArea area = elite.Station.Area!.Value;

            for (int tick = 0; tick < 120; tick += 1)
            {
                run.Step([Intent.OfPlayer(WalkOf(seed, tick))]);
                Assert.True(
                    area.HoldsBody(elite.Body),
                    $"The seed {seed} put the body {elite.Body} outside the area {area} (D-209).");
            }
        }
    }

    [Fact]
    public void NoBattleStartsInsideTheGraceTimeOverOneThousandSeeds()
    {
        // Exit test 5 of section 7.6, with a party that walks during the grace time (D-381).
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = Fought(seed);
            FleeAndWait(run);

            for (int tick = 0; tick < MapRules.GraceTicks; tick += 1)
            {
                run.Step([Intent.OfPlayer(WalkOf(seed, tick))]);
                Assert.True(
                    run.State.Party.Patrols.Encounter is null,
                    $"The seed {seed} started a battle at the tick {tick} of the grace time (D-381).");
            }
        }
    }

    [Fact]
    public void AStepIntoAGroupInsideItsGraceTimeStartsNoEncounterAndOneAfterIt()
    {
        // Finding P2-4 of the repository review: a step into the fled group started a fight with
        // the party first on the next tick. During the grace time, the body still blocks the
        // step, and no encounter starts (D-381, D-1085). After it, a step starts one (D-747).
        Simulation run = Bumped();
        FleeAndWait(run);
        TilePoint lead = run.State.Party.LeadAt;
        int ticks = 0;

        while (Only(run).GraceTicks > 0)
        {
            run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);
            ticks += 1;
            Assert.Null(run.State.Party.Patrols.Encounter);
            Assert.Null(run.State.Party.Stepping);
            Assert.Equal(lead, run.State.Party.LeadAt);
        }

        Assert.Equal(MapRules.GraceTicks - 1, ticks);

        run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);

        MapEncounter encounter = Assert.IsType<MapEncounter>(run.State.Party.Patrols.Encounter);
        Assert.Equal(EncounterSide.Party, encounter.Behind);
    }

    [Fact]
    public void AStepIntoAGroupOnTheLastTickOfItsGraceTimeStartsNoEncounter()
    {
        // The boundary of D-1085: the grace time counts down in the world step, after the step
        // of the party, so a step on the tick that ends the count still meets the grace.
        Simulation run = Bumped();
        FleeAndWait(run);
        while (Only(run).GraceTicks > 1)
        {
            run.Step([]);
        }

        run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);

        Assert.Equal(0, Only(run).GraceTicks);
        Assert.Null(run.State.Party.Patrols.Encounter);
    }

    [Fact]
    public void AMoveIntentDuringABattleStartsNoStepWhenTheBattleEnds()
    {
        // Finding P3-2 of the repository review: the wanted direction outlived its tick while a
        // battle held the world, and the tick of the wait intent started a step from it. The
        // snapshot leaves the direction out, so a resumed run went another way (T-7, G-5).
        Simulation run = Fought();
        run.Step([Intent.OfPlayer(IntentIds.MoveWest)]);

        FleeAndWait(run);

        Assert.Null(run.State.Party.Stepping);
    }

    [Fact]
    public void AMoveIntentBeforeTheMenuOpensStartsNoStepWhenTheMenuCloses()
    {
        // The second path of finding P3-2: a move and the open of the menu in one tick.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest)));
        run.Step([Intent.OfPlayer(IntentIds.MoveNorth), Intent.OfPlayer(IntentIds.OpenMenu)]);

        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);

        Assert.Null(run.State.Party.Stepping);
    }

    [Fact]
    public void ARunResumedFromASnapshotInsideABattleMatchesTheLiveRun()
    {
        // G-5 with a move intent during the battle: the live run and a copy that a snapshot
        // resumed reach one state hash after the battle ends.
        Simulation live = Fought();
        live.Step([Intent.OfPlayer(IntentIds.MoveWest)]);
        Simulation copy = Simulation.Resume(
            Seed,
            live.Snapshot(),
            live.State.Party.Map,
            TestBattles.SureFlee,
            TestBattles.Notices,
            TestBattles.Story,
            DebugAssemblyFile.Handlers());

        FleeAndWait(live);
        FleeAndWait(copy);
        for (int tick = 0; tick < 20; tick += 1)
        {
            live.Step([]);
            copy.Step([]);
        }

        Assert.Equal(live.StateHash(), copy.StateHash());
    }

    [Fact]
    public void AnEnemyInsideItsGraceTimeSeesNothing()
    {
        // D-381: no battle with that group starts for the grace time, from either side.
        Simulation run = Fought();
        FleeAndWait(run);

        run.Step([]);

        Assert.True(Only(run).GraceTicks > 0);
        Assert.Null(run.State.Party.Patrols.Mark);
    }

    [Fact]
    public void ThePartyBlocksAStepOfAnEnemy()
    {
        // No state of a run holds the party and an enemy on one tile (D-747, T-2).
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(stepTicks: 16, stations: TowardSpawn)));

        for (int tick = 0; tick < 200; tick += 1)
        {
            run.Step([]);
            Assert.NotEqual(run.State.Party.LeadAt, Only(run).At);
            Assert.False(Only(run).Body.Holds(run.State.Party.LeadAt));
        }
    }

    [Fact]
    public void TwoEnemiesNeverHoldOneTile()
    {
        // Each body blocks every other body, and a step that runs counts as the tile of its
        // end too (D-206, T-2).
        GameMap map = PatrolMaps.Of($"{PatrolMaps.Enemy(id: "patrol.one", stepTicks: 16, stations: RowSix)},\n{PatrolMaps.Enemy(id: "patrol.two", stepTicks: 32, stations: RowSixBack)}");
        Simulation run = Start(map);

        for (int tick = 0; tick < 400; tick += 1)
        {
            run.Step([]);
            PatrolState one = run.State.Party.Patrols.All[0];
            PatrolState other = run.State.Party.Patrols.All[1];
            Assert.False(
                MapRules.BodiesOverlap(one.Body, other.Body),
                $"The two enemies hold one tile at the tick {tick}: {one.Body} and {other.Body}.");
        }
    }

    [Fact]
    public void ThePartySneaksAnEnemyThatItStandsBehind()
    {
        // D-746: the party sneaks a patrol when it stands in the quarter behind the facing
        // of that patrol, and the patrol does not stand behind the party.
        Assert.Equal(EncounterSide.Party, SideOf(new TilePoint(8, 6), StepDirection.North, 8, 5, StepDirection.North));
    }

    [Fact]
    public void AnEnemyAmbushesThePartyThatItStandsBehind()
    {
        Assert.Equal(EncounterSide.Enemy, SideOf(new TilePoint(8, 6), StepDirection.South, 8, 5, StepDirection.South));
    }

    [Fact]
    public void NoSideActsFirstWhenBothStandBehindTheOther()
    {
        // D-746: when both hold, or neither holds, no side acts first.
        Assert.Equal(EncounterSide.None, SideOf(new TilePoint(8, 6), StepDirection.South, 8, 5, StepDirection.North));
    }

    [Fact]
    public void NoSideActsFirstWhenNeitherStandsBehindTheOther()
    {
        Assert.Equal(EncounterSide.None, SideOf(new TilePoint(8, 6), StepDirection.North, 8, 5, StepDirection.South));
    }

    [Fact]
    public void TheSideOfAnEncounterComesFromTheFacingsWhenTheBeatEnds()
    {
        // The party still walks and turns during the beat, so the side comes from the
        // facings at the moment that the encounter starts (D-745, D-746).
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher)));

        for (int tick = 0; tick <= MapRules.BeatTicks; tick += 1)
        {
            run.Step([]);
        }

        MapEncounter encounter = Assert.IsType<MapEncounter>(run.State.Party.Patrols.Encounter);
        Assert.Equal(EncounterSide.Enemy, encounter.Behind);
    }

    [Fact]
    public void ThePartyThatTurnsToTheEnemyDuringTheBeatDeniesTheAmbush()
    {
        // Nothing cancels the beat, and the party can still turn to face the patrol (D-745,
        // D-746).
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher)));

        run.Step([]);
        Assert.NotNull(run.State.Party.Patrols.Mark);

        // The party steps north through the beat, so the encounter waits for the end of that step
        // (D-1094).
        for (int tick = 1; tick <= MapRules.BeatTicks + MapRules.TicksPerStep && run.State.Party.Patrols.Encounter is null; tick += 1)
        {
            run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);
        }

        MapEncounter encounter = Assert.IsType<MapEncounter>(run.State.Party.Patrols.Encounter);
        Assert.Null(run.State.Party.Stepping);
        Assert.Equal(StepDirection.North, run.State.Party.Facing);
        Assert.Equal(EncounterSide.None, encounter.Behind);
    }

    [Fact]
    public void AnEncounterWaitsUntilTheLeadStopsMoving()
    {
        // D-1094: the beat ends while the lead walks, and the fight waits for the end of that
        // step. The lead then starts no new step, although the player holds the direction.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher)));
        run.Step([]);
        Assert.NotNull(run.State.Party.Patrols.Mark);
        while (run.State.Party.Patrols.Mark is SightMark { TicksLeft: > 2 })
        {
            run.Step([]);
        }

        // One step starts one tick before the beat ends, so the beat ends in the middle of it.
        run.Step([Intent.OfPlayer(IntentIds.MoveWest)]);
        Assert.NotNull(run.State.Party.Stepping);
        run.Step([Intent.OfPlayer(IntentIds.MoveWest)]);
        Assert.NotNull(run.State.Party.Stepping);
        Assert.Null(run.State.Party.Patrols.Encounter);
        Assert.Equal(0, run.State.Party.Patrols.Mark!.TicksLeft);

        int ticks = 0;
        while (run.State.Party.Patrols.Encounter is null)
        {
            Assert.True(ticks <= MapRules.TicksPerStep, $"No encounter started {ticks} ticks after the beat ended (D-1094).");
            run.Step([Intent.OfPlayer(IntentIds.MoveWest)]);
            ticks += 1;
            if (run.State.Party.Patrols.Encounter is null && run.State.Party.Stepping is null)
            {
                // The lead reached its tile, and it starts no new step while the fight is due.
                Assert.Equal(0, run.State.Party.StepTicks);
            }
        }

        Assert.Null(run.State.Party.Stepping);
        Assert.Equal(0, run.State.Party.StepTicks);
    }

    [Fact]
    public void AnEncounterOfALeadThatStandsStillStartsWhenTheBeatEnds()
    {
        // The boundary of D-1094: a lead that stands still meets the fight on the last tick of
        // the beat, as before.
        Simulation run = Start(PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher)));
        run.Step([]);

        for (int tick = 1; tick < MapRules.BeatTicks; tick += 1)
        {
            run.Step([]);
            Assert.Null(run.State.Party.Patrols.Encounter);
        }

        run.Step([]);

        Assert.NotNull(run.State.Party.Patrols.Encounter);
    }

    [Fact]
    public void TheEnemiesOfASnapshotComeBackInTheOrderOfTheMap()
    {
        // D-750: a load puts each patrol back where it stood.
        GameMap map = TestMaps.Patrolled;
        Simulation run = Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        for (int tick = 0; tick < 90; tick += 1)
        {
            run.Step([]);
        }

        Simulation again = Simulation.Resume(Seed, run.Snapshot(), map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), again.StateHash());
        for (int index = 0; index < map.Patrols.Count; index += 1)
        {
            Assert.Equal(run.State.Party.Patrols.All[index].At, again.State.Party.Patrols.All[index].At);
            Assert.Equal(run.State.Party.Patrols.All[index].Facing, again.State.Party.Patrols.All[index].Facing);
        }
    }

    [Fact]
    public void ASnapshotWithAnotherEnemyCountFailsTheResume()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [],
            null,
            null,
            "the save"));

        Assert.Contains("holds 0 enemies", error.Message, StringComparison.Ordinal);
        Assert.Contains("the save", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAnotherEnemyIdFailsTheResume()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));
        ContentId other = ContentId.Parse("patrol.other", "the save", "id");

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [new PatrolValues(other, 1, 5, StepDirection.South, null, 0, 1, true, 0, false)],
            null,
            null,
            "the save"));

        Assert.Contains("patrol.other", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAMarkOfNoEnemyOfTheMapFailsTheResume()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));
        ContentId other = ContentId.Parse("patrol.other", "the save", "id");

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [Standing(map)],
            new SightMark(other, 4),
            null,
            "the save"));

        Assert.Contains("the mark names the enemy 'patrol.other'", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ASnapshotWithAMarkOrAnEncounterOfADeadEnemyFailsTheResume(bool mark)
    {
        // P3-18: a dead enemy sees nothing, and a win ends its encounter (D-531, D-555).
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));
        ContentId enemy = map.Patrols[0].Id;

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [Standing(map) with { Dead = true }],
            mark ? new SightMark(enemy, 4) : null,
            mark ? null : new MapEncounter(enemy, map.Patrols[0].Group, EncounterSide.None),
            "the save"));

        Assert.Contains($"'{enemy.Value}', which is dead", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAnEncounterOfAnotherGroupFailsTheResume()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));
        ContentId group = ContentId.Parse("group.other", "the save", "group");

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [Standing(map)],
            null,
            new MapEncounter(map.Patrols[0].Id, group, EncounterSide.None),
            "the save"));

        Assert.Contains("group.other", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAMarkAndAnEncounterFailsTheResume()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [Standing(map)],
            new SightMark(map.Patrols[0].Id, 4),
            new MapEncounter(map.Patrols[0].Id, map.Patrols[0].Group, EncounterSide.None),
            "the save"));

        Assert.Contains("a mark and an encounter", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithThePartyOnABodyFailsTheResume()
    {
        // No state of a run holds the party and an enemy on one tile (D-747, T-2).
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            map,
            new TilePoint(1, 5),
            StepDirection.South,
            null,
            0,
            WalkedAt(map, new TilePoint(1, 5)),
            [Standing(map)],
            null,
            null,
            "the save"));

        Assert.Contains("holds the body", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfFormatTwoPutsEachEnemyOnTheStartOfItsStation()
    {
        // D-654 and D-750: save format 2 predates the enemies, and its migration puts each
        // enemy on the start tile of its station.
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Southwest));

        MapState party = MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            null,
            null,
            null,
            "the save");

        PatrolState patrol = Assert.Single(party.Patrols.All);
        Assert.Equal(new TilePoint(1, 5), patrol.At);
        Assert.Equal(StepDirection.East, patrol.Facing);
        Assert.Equal(0, patrol.GraceTicks);
    }

    [Fact]
    public void ADeadEnemyNeverWalksAndNeverSeesTheParty()
    {
        // D-555: a killed enemy stays dead. PR-9 kills an enemy, and a snapshot of a later
        // build can hold one.
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher));
        MapState party = MapState.Resume(
            map,
            map.Spawn,
            StepDirection.South,
            null,
            0,
            WalkedOf(map),
            [new PatrolValues(map.Patrols[0].Id, 8, 3, StepDirection.South, null, 0, 0, true, 0, true)],
            null,
            null,
            "the test");

        Assert.True(party.Patrols.All[0].Dead);
        Assert.False(party.Patrols.TryEnemyAt(new TilePoint(8, 3), out _));
        Assert.False(party.Patrols.TrySight(map, party, torchHeld: false, out _));
    }

    private static readonly ContentId FleeAction = ContentId.Parse(
        "debug.battle_flee",
        "TheThingBelow.Tests/MapPatrolsTests.cs",
        nameof(FleeAction));

    private const string OneTile = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 1 }] }
       ]
      """;

    private const string NightOnly = """
       "routes": [
        { "times": ["night"], "tiles": [{ "x": 1, "y": 1 }] }
       ]
      """;

    private const string Southwest = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 5 }, { "x": 1, "y": 6 }] }
       ]
      """;

    private const string NextToSpawn = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 8, "y": 5 }] }
       ]
      """;

    private const string TowardSpawn = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 8, "y": 1 }, { "x": 8, "y": 6 }] }
       ]
      """;

    /// <summary>A route down the east column of the room, which passes the spawn tile.</summary>
    private const string Column = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 8, "y": 1 }, { "x": 8, "y": 6 }] }
       ]
      """;

    private const string Watcher = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 8, "y": 3 }] }
       ]
      """;

    private const string RowSix = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 6 }, { "x": 6, "y": 6 }] }
       ]
      """;

    private const string RowSixBack = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 7, "y": 6 }, { "x": 2, "y": 6 }] }
       ]
      """;

    /// <summary>
    /// A map of two rooms with a solid wall between them. The party never reaches the enemy,
    /// and no line of sight crosses the wall, so the sight test reads the wall alone (D-718).
    /// </summary>
    private static GameMap TwoRooms(int sightRange) => TestMaps.Of(
        "two-rooms.json",
        $$"""
        {
         "comment": "two rooms with a solid wall between them, for the property tests of PR-8",
         "id": "map.two_rooms",
         "region": "region.test",
         "label": "label.two_rooms",
         "time": "day",
         "dark": false,
         "terrain": [
          "############",
          "#....#.....#",
          "#....#.....#",
          "#....#.....#",
          "#....#.....#",
          "#....#.....#",
          "#....#.....#",
          "############"
         ],
         "things": [
          { "id": "spawn_point.two_rooms_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [
          {
           "id": "patrol.ring",
           "group": "group.ring",
           "size": "common",
           "facing": "east",
           "step_ticks": 16,
           "sight_range": {{sightRange}},
           "routes": [
            {
             "times": ["dawn", "day", "dusk", "night"],
             "tiles": [{ "x": 6, "y": 1 }, { "x": 10, "y": 1 }, { "x": 10, "y": 6 }, { "x": 6, "y": 6 }]
            }
           ]
          }
         ], "triggers": []
        }
        """);

    /// <summary>Starts a run on one map, with the handlers of the debug console (D-749).</summary>
    private static Simulation Start(GameMap map) =>
        Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugAssemblyFile.Handlers());

    /// <summary>
    /// Runs a map to the start of an encounter and its battle, so a test can flee it (D-767).
    /// The rules of this run let every flee work, so no seed ends in a wipe.
    /// </summary>
    private static Simulation Fought(ulong seed = Seed)
    {
        Simulation run = Simulation.Start(
            seed,
            PatrolMaps.Of(PatrolMaps.Enemy(facing: "south", stations: Watcher)),
            TestBattles.SureFlee,
            TestBattles.Notices,
            TestBattles.Story,
            DebugAssemblyFile.Handlers());

        while (run.State.Party.Patrols.Encounter is null)
        {
            run.Step([]);
            Assert.True(run.Tick <= MapRules.BeatTicks + 2, "The encounter never started (D-745).");
        }

        return run;
    }

    /// <summary>
    /// Runs a map to the encounter of a step of the party into a body (D-747), with the rules
    /// that let every flee work (D-767). The enemy faces away, so it sees no approach.
    /// </summary>
    private static Simulation Bumped()
    {
        Simulation run = Simulation.Start(
            Seed,
            PatrolMaps.Of(PatrolMaps.Enemy(facing: "north", stations: NextToSpawn)),
            TestBattles.SureFlee,
            TestBattles.Notices,
            TestBattles.Story,
            DebugAssemblyFile.Handlers());

        run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]);
        MapEncounter encounter = Assert.IsType<MapEncounter>(run.State.Party.Patrols.Encounter);
        Assert.Equal(EncounterSide.Party, encounter.Behind);
        return run;
    }

    /// <summary>Flees the battle of the run, and sends the wait intent that ends it (D-378, D-522).</summary>
    /// <returns>The log of the tick of the wait intent.</returns>
    private static IReadOnlyList<LogEntry> FleeAndWait(Simulation run)
    {
        run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]);
        Assert.Equal(BattleOutcome.Fled, BattleRuns.BattleOf(run).Outcome);
        return run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);
    }

    /// <summary>Gives the direction of the party on one tick of a property test (T-7).</summary>
    private static ContentId WalkOf(ulong seed, int tick)
    {
        long turn = (long)(seed % 4) + (tick / 13);
        return (turn % 4) switch
        {
            0 => IntentIds.MoveNorth,
            1 => IntentIds.MoveEast,
            2 => IntentIds.MoveSouth,
            _ => IntentIds.MoveWest,
        };
    }

    private static bool OnRoute(IReadOnlyList<TilePoint> route, TilePoint at)
    {
        for (int index = 0; index + 1 < route.Count; index += 1)
        {
            if (!PatrolStation.TryLeg(route[index], route[index + 1], out StepDirection direction, out int length))
            {
                return false;
            }

            TilePoint step = route[index];
            for (int count = 0; count <= length; count += 1)
            {
                if (step == at)
                {
                    return true;
                }

                step = step.Step(direction);
            }
        }

        return route.Count == 1 && route[0] == at;
    }

    /// <summary>
    /// Gives the side that reached the other from behind, by the sight of one enemy and the
    /// beat that follows it (D-745, D-746).
    /// </summary>
    private static EncounterSide SideOf(
        TilePoint leadAt,
        StepDirection leadFacing,
        int enemyX,
        int enemyY,
        StepDirection enemyFacing)
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stations: Column));
        MapState party = MapState.Resume(
            map,
            leadAt,
            leadFacing,
            null,
            0,
            WalkedAt(map, leadAt),
            [new PatrolValues(map.Patrols[0].Id, enemyX, enemyY, enemyFacing, null, 0, 1, true, 0, false)],
            null,
            null,
            "the test");

        Assert.True(party.Patrols.TrySight(map, party, torchHeld: false, out PatrolState? seen));
        party.Patrols.StartMark(seen!);
        for (int tick = 0; tick < MapRules.BeatTicks; tick += 1)
        {
            party.Patrols.CountBeat(party);
        }

        return Assert.IsType<MapEncounter>(party.Patrols.Encounter).Behind;
    }

    /// <summary>The stored values of one enemy on the start tile of its station (D-750).</summary>
    private static PatrolValues Standing(GameMap map) =>
        new(map.Patrols[0].Id, 1, 5, StepDirection.East, null, 0, 1, true, 0, false);

    private static WalkedTiles WalkedAt(GameMap map, TilePoint at)
    {
        WalkedTiles walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(at);
        return walked;
    }

    private static WalkedTiles WalkedOf(GameMap map)
    {
        WalkedTiles walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(map.Spawn);
        return walked;
    }

    private static PatrolState Only(Simulation run) => run.State.Party.Patrols.All[0];
}
