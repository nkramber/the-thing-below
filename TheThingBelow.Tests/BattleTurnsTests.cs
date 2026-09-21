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
/// The rules of a battle: the start, each action, the enemy turn, the wave, and the end (D-376,
/// D-755 to D-779). The exact rules of the tests take no roll that moves a number, so each test
/// reads the numbers of D-771 and D-777.
/// </summary>
public sealed class BattleTurnsTests
{
    private const ulong Seed = 20260921;

    private static readonly ContentId Draught = ContentId.Parse("item.fixture_draught", "BattleTurnsTests", "item");

    [Fact]
    public void AStepIntoAnEnemyStartsABattleOnTheSameTick()
    {
        // D-531, D-747: the encounter becomes a battle on its own tick, and the map holds still.
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Exact, DebugIntentHandlers.None);

        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal("group.one", battle.Group.Id.Value);
        Assert.Equal(BattleOutcome.Running, battle.Outcome);
        Assert.Equal([BattleEventKind.Started, BattleEventKind.Turn], BattleRuns.Kinds(run));
    }

    [Fact]
    public void TheSideThatCameFromBehindActsFirst()
    {
        // D-770: a step into the guard is a sneak, so the party starts at tick 0 and the
        // enemy one attack push out.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Battle battle = BattleRuns.BattleOf(run);

        Assert.Equal(0, battle.Party[0].ReadyAt);
        Assert.Equal(111, battle.Enemies[0].ReadyAt);
        Assert.Equal(BattleSide.Party, battle.Next()!.Side);
    }

    [Fact]
    public void AnAmbushLetsTheEnemiesActBeforeTheFirstTurnOfACharacter()
    {
        // D-265, D-770: the enemy side came from behind, so each enemy acts before Marrek.
        Simulation run = Encountered("group.test_pair", EncounterSide.Enemy, TestBattles.Exact);

        run.Step([]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(60 - 7 - 7, battle.Party[0].Health);
        Assert.Equal(BattleSide.Party, battle.Next()!.Side);
    }

    [Fact]
    public void WithNoSideBehindSpeedAloneSetsTheFirstTurn()
    {
        // D-770: each combatant starts one attack push out, so the faster one acts first.
        Simulation run = Encountered("group.one", EncounterSide.None, TestBattles.Exact);

        run.Step([]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(100, battle.Party[0].ReadyAt);
        Assert.Equal(111, battle.Enemies[0].ReadyAt);
        Assert.Equal(60, battle.Party[0].Health);
    }

    [Fact]
    public void TheBasicAttackDealsTheDamageOfTheRatioForm()
    {
        // D-771: 12 times 100 over 102 is 11.76, rounded toward zero. The grunt hits back
        // with 8 times 100 over 104, which is 7.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);

        run.Step([BattleRuns.AttackFirst(run)]);
        run.Step([BattleRuns.AttackFirst(run)]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(30 - 11 - 11, battle.Enemies[0].Health);
        Assert.Equal(60 - 7, battle.Party[0].Health);
    }

    [Fact]
    public void AWonBattleWaitsForTheScreenAndThenMarksTheEnemyDead()
    {
        // D-522, D-555: the map holds still until the wait intent, and the won enemy never
        // walks again.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));
        Assert.NotNull(run.State.Party.Patrols.Encounter);
        run.Step([]);
        Assert.NotNull(run.State.Battle);

        IReadOnlyList<LogEntry> log = run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Null(run.State.Battle);
        Assert.Null(run.State.Party.Patrols.Encounter);
        Assert.True(run.State.Party.Patrols.All[0].Dead);
        Assert.Contains(log, entry => entry.Subsystem == LogSubsystems.Battle && entry.Message.Contains("map runs again", StringComparison.Ordinal));
    }

    [Fact]
    public void TheHealthOfACharacterLastsPastTheBattle()
    {
        // D-36, D-389: health is the scarce resource of a dungeon, so a battle keeps each loss.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        BattleRuns.FightToEnd(run, Seed);

        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Equal(53, run.State.Characters.Members[0].Health);
    }

    [Fact]
    public void AWaitIntentWithNoEndedBattleIsAnError()
    {
        // D-522, T-2: the wait ends a win or a flee alone.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]));

        Assert.Contains("running", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADefendHalvesTheDamageUntilTheNextTurnOfTheCharacter()
    {
        // D-755: the grunt hits for 7, and a defend of 5000 basis points makes it 3.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);

        // Marrek acts again at tick 60, before the grunt at 111. His next turn begins before
        // his choice, so the defend ends there (D-799).
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(60, battle.Party[0].ReadyAt);
        Assert.Equal(60, battle.Now);
        Assert.False(battle.Party[0].Defending);
    }

    [Fact]
    public void ADefendCutsAHitThatLandsBeforeTheNextTurn()
    {
        // D-755: with no side behind, the grunt acts after Marrek, and the defend of Marrek
        // halves that blow.
        Simulation run = Encountered("group.one", EncounterSide.None, TestBattles.Exact);
        run.Step([]);

        // Marrek is at 100 and the grunt at 111. A defend pushes Marrek to 160, so the grunt
        // strikes at 111 into the defend.
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);

        Assert.Equal(60 - 3, BattleRuns.BattleOf(run).Party[0].Health);
    }

    [Fact]
    public void AMeleeAttackFromTheBackRowDealsHalfDamage()
    {
        // D-779: 11 from the front row, and half of the 11.76 from the back, which is 5.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);

        run.Step([Intent.OfPlayer(IntentIds.BattleStep)]);
        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(BattleRow.Back, battle.Party[0].Row);
        Assert.Equal(60, battle.Party[0].ReadyAt);

        run.Step([BattleRuns.AttackFirst(run)]);

        Assert.Equal(30 - 5, battle.Enemies[0].Health);
    }

    [Fact]
    public void ARowStepInABattleLeavesTheRowOfThePartyWindow()
    {
        // D-558: the party window sets the start row, and a step inside a battle changes
        // the battle alone.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);

        run.Step([Intent.OfPlayer(IntentIds.BattleStep)]);
        BattleRuns.FightToEnd(run, Seed);
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Equal(BattleRow.Front, run.State.Characters.Members[0].Row);
    }

    [Fact]
    public void MeleeReachesNoBackRowWhileItsFrontRowStands()
    {
        // Exit test 2 of PR-9 (D-377): the brute stands in front, so the grunt behind it is
        // out of reach, and the refusal names the rule.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_elite", TestBattles.Exact);
        BattleChoice behind = new(BattleAction.Attack, new BattleTarget(BattleSide.Enemy, 1), null);

        Assert.Contains("D-377", BattleTurns.RefusalOf(run.State, behind), StringComparison.Ordinal);
        Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.BattleAttack, new BattleTarget(BattleSide.Enemy, 1), null)]));
    }

    [Fact]
    public void MeleeReachesTheBackRowWhenTheFrontRowFalls()
    {
        // D-377: with no enemy in the front row, the back row is the front of the reach.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_elite", TestBattles.ExactWithParty(3));
        Battle battle = BattleRuns.BattleOf(run);

        while (battle.Enemies[0].Place == CombatantPlace.Field && battle.Outcome == BattleOutcome.Running)
        {
            run.Step([Intent.OfPlayer(IntentIds.BattleAttack, new BattleTarget(BattleSide.Enemy, 0), null)]);
        }

        Assume(battle.Outcome == BattleOutcome.Running);
        {
            Assert.Null(BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Attack, new BattleTarget(BattleSide.Enemy, 1), null)));
        }
    }

    [Fact]
    public void AnItemRestoresLessInABattleAndTakesOneFromThePack()
    {
        // D-382, D-775: a draught heals 30, and the battle rate of 5000 makes it 15.
        Simulation run = Encountered("group.one", EncounterSide.Enemy, TestBattles.Exact);
        run.Step([]);
        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(53, battle.Party[0].Health);
        run.TakeBattleEvents();

        run.Step([Intent.OfPlayer(IntentIds.BattleItem, new BattleTarget(BattleSide.Party, 0), Draught)]);

        // The heal stops at the full health of 60, so it restores 7 of the 15. The grunt then
        // strikes at 111, before Marrek at 200.
        BattleEvent used = Assert.Single(run.TakeBattleEvents(), battleEvent => battleEvent.Kind == BattleEventKind.Item);
        Assert.Equal(7, used.Amount);
        Assert.Equal(53, battle.Party[0].Health);
        Assert.Equal(2, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void ASnapshotKeepsTheCountOfThePackOfItsTick()
    {
        // A regression test: the snapshot shared the array of the pack, so a later use of an
        // item changed the start snapshot of a record, and its replay gave another state (G-5).
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        RunSnapshot before = run.Snapshot();

        run.Step([Intent.OfPlayer(IntentIds.BattleItem, new BattleTarget(BattleSide.Party, 0), Draught)]);

        Assert.Equal(3, before.Characters!.Pack[0].Count);
        Assert.Equal(2, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void AnItemWithAnEmptyPackIsRefused()
    {
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_boss", TestBattles.Exact);
        for (int use = 0; use < 3; use += 1)
        {
            run.Step([Intent.OfPlayer(IntentIds.BattleItem, new BattleTarget(BattleSide.Party, 0), Draught)]);
        }

        string? refusal = BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Item, new BattleTarget(BattleSide.Party, 0), Draught));

        Assert.Contains("the pack holds none", refusal, StringComparison.Ordinal);
    }

    [Fact]
    public void NoFleeStartsInABossBattle()
    {
        // Exit test 3 of PR-9 (D-378).
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_boss", TestBattles.SureFlee);

        Assert.Contains("D-378", BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Flee, null, null)), StringComparison.Ordinal);
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]));
        Assert.Contains("group.test_boss", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFailedFleeCostsTheTurn()
    {
        // Exit test 4 of PR-9 (D-378): a flee that fails pushes the character back by the
        // flee delay, and the enemies then act.
        BattleContent never = TestBattles.WithRules(("flee_floor", 0), ("flee_ceiling", 0), ("flee_base", 0));
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", never);
        Battle battle = BattleRuns.BattleOf(run);
        run.TakeBattleEvents();

        run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]);

        Assert.Equal(BattleOutcome.Running, battle.Outcome);
        Assert.Equal(100, battle.Party[0].ReadyAt);
        Assert.Equal(BattleEventKind.FleeFailed, BattleRuns.Kinds(run)[0]);
    }

    [Fact]
    public void AFleeChanceReadsTheSpeedGapOfTheTwoSides()
    {
        // D-763: Marrek at 100 against a grunt at 90 gives 5000 plus 10 times 100. A seed
        // loop reads the rate of the draws.
        int fled = 0;
        const int Tries = 2000;
        for (ulong seed = 0; seed < Tries; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.one", TestBattles.Exact);
            run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]);
            fled += BattleRuns.BattleOf(run).Outcome == BattleOutcome.Fled ? 1 : 0;
        }

        Assert.InRange(fled, (Tries * 55) / 100, (Tries * 65) / 100);
    }

    [Fact]
    public void AWaitingEnemyStepsInWhenAnEnemyFallsOneAttackPushOut()
    {
        // D-761, D-778: the field of the full group holds six, and each fall lets the next
        // waiting enemy step into its row.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_full", TestBattles.ExactWithParty(3));
        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(CombatantPlace.Waiting, battle.Enemies[6].Place);

        while (battle.Enemies[0].Place == CombatantPlace.Field && battle.Outcome == BattleOutcome.Running)
        {
            run.Step([Intent.OfPlayer(IntentIds.BattleAttack, new BattleTarget(BattleSide.Enemy, 0), null)]);
        }

        Assume(battle.Outcome == BattleOutcome.Running);
        Assert.Equal(CombatantPlace.Field, battle.Enemies[6].Place);
        Assert.Equal(BattleRow.Back, battle.Enemies[6].Row);
        Assert.Equal(CombatantPlace.Waiting, battle.Enemies[7].Place);
        Assert.True(battle.Enemies[6].ReadyAt > battle.Now);
    }

    [Fact]
    public void TheWaitingEnemiesStandOffTheTimelineAndOutOfReach()
    {
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_full", TestBattles.Exact);
        Battle battle = BattleRuns.BattleOf(run);

        Assert.DoesNotContain(new BattleTarget(BattleSide.Enemy, 6), battle.Strip(TestBattles.Exact.Rules, run.State.Context("test")));
        Assert.DoesNotContain(battle.Enemies[7], battle.MeleeTargets(BattleSide.Enemy));
    }

    [Fact]
    public void TheStripShowsSixTurns()
    {
        // D-756: the strip reads ahead as though each combatant attacks.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.Exact);

        IReadOnlyList<BattleTarget> strip = BattleRuns.BattleOf(run).Strip(TestBattles.Exact.Rules, run.State.Context("test"));

        // Marrek at 0 and 100, the grunts at 111 in slot order, Marrek at 200, and the first
        // grunt at 222.
        Assert.Equal(
            [
                new BattleTarget(BattleSide.Party, 0),
                new BattleTarget(BattleSide.Party, 0),
                new BattleTarget(BattleSide.Enemy, 0),
                new BattleTarget(BattleSide.Enemy, 1),
                new BattleTarget(BattleSide.Party, 0),
                new BattleTarget(BattleSide.Enemy, 0),
            ],
            strip);
    }

    [Theory]
    [InlineData(100, 100, 10000, 100)]
    [InlineData(100, 90, 10000, 111)]
    [InlineData(100, 100, 7500, 75)]
    [InlineData(100, 100, 15000, 150)]
    [InlineData(160, 100, 10000, 160)]
    [InlineData(1, 100000, 1, 1)]
    public void APushIsTheDelayTimes100OverTheSpeedTimesTheRate(int delay, int speed, int rate, long push)
    {
        // D-768, with the floor of 1.
        Assert.Equal(push, Battle.Push(delay, speed, rate, new RunContext(Seed, 0, "test")));
    }

    [Fact]
    public void HasteShortensAndSlowLengthensEachLaterPush()
    {
        // D-376, D-768, D-800: haste and slow set the pace, each push reads it, and slow on
        // a hasted holder cancels the haste and does not land.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        RunContext context = run.State.Context("test");
        BattleTarget marrek = new(BattleSide.Party, 0);
        List<LogEntry> log = [];

        BattleTurns.GiveStatus(run.State, marrek, StatusKind.Haste, context, log);
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        Assert.Equal(45, BattleRuns.BattleOf(run).Party[0].ReadyAt);

        BattleTurns.GiveStatus(run.State, marrek, StatusKind.Slow, context, log);
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        Assert.Equal(45 + 60, BattleRuns.BattleOf(run).Party[0].ReadyAt);

        BattleTurns.GiveStatus(run.State, marrek, StatusKind.Slow, context, log);
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        Assert.Equal(45 + 60 + 90, BattleRuns.BattleOf(run).Party[0].ReadyAt);
    }

    [Fact]
    public void AHeavyBlowPushesFurtherAndAStunPushesItsTargetBack()
    {
        // D-376, D-802: a heavy move of 160 ticks, and a stun that adds the push of 50 ticks.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.Exact);
        List<LogEntry> log = [];

        BattleTurns.StrikeWith(run.State, new BattleMove(160, 10000, null, new StatusChance(StatusKind.Stun, 10000)), new BattleTarget(BattleSide.Enemy, 0), run.State.Context("test"), log);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(160, battle.Party[0].ReadyAt);
        Assert.Equal(111 + 50, battle.Enemies[0].ReadyAt);

        // The second grunt took no stun, so it acted at 111, before Marrek at 160.
        Assert.Equal(222, battle.Enemies[1].ReadyAt);
    }

    [Fact]
    public void ABattleIntentWhileTheMenuIsOpenIsAnError()
    {
        // D-162, T-2: a menu takes every input of the player.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        Assert.Throws<SimulationException>(() => run.Step([BattleRuns.AttackFirst(run)]));
    }

    [Fact]
    public void ABattleIntentWithNoBattleIsAnError()
    {
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Exact, DebugIntentHandlers.None);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]));

        Assert.Contains("no battle runs", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATargetOnAnIntentOfTheMapIsAnError()
    {
        // D-764, T-2: a value that no rule reads points at a fault of the screen.
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Exact, DebugIntentHandlers.None);

        Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.MoveNorth, new BattleTarget(BattleSide.Enemy, 0), null)]));
    }

    [Fact]
    public void AMapThatNamesAnAbsentGroupFailsTheStartOfTheRun()
    {
        // D-766: the test that each named group exists lands in PR-9.
        ContentException error = Assert.Throws<ContentException>(
            () => Simulation.Start(Seed, BattleRuns.Map("group.absent"), TestBattles.Exact, DebugIntentHandlers.None));

        Assert.Contains("group.absent", error.Message, StringComparison.Ordinal);
        Assert.Contains("patrol.test_guard", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEventsOfAStrikeCarryTheActorTheTargetAndTheDamage()
    {
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        run.TakeBattleEvents();

        run.Step([BattleRuns.AttackFirst(run)]);

        IReadOnlyList<BattleEvent> events = run.TakeBattleEvents();
        BattleEvent hit = events[0];
        Assert.Equal(BattleEventKind.Hit, hit.Kind);
        Assert.Equal(new BattleTarget(BattleSide.Party, 0), hit.Actor);
        Assert.Equal(new BattleTarget(BattleSide.Enemy, 0), hit.Target);
        Assert.Equal(11, hit.Amount);
        Assert.Equal("hit: party 0 at enemy 0 for 11", hit.Describe());
        Assert.Empty(run.TakeBattleEvents());
    }

    /// <summary>
    /// Gives a run whose map holds an encounter with one side behind, as the beat of a sight
    /// would leave it (D-745, D-746). The snapshot of a new run takes the encounter, and the run
    /// resumes from it, so the world step of the next tick starts the battle (D-765).
    /// </summary>
    private static Simulation Encountered(string group, EncounterSide behind, BattleContent content)
    {
        GameMap map = BattleRuns.Map(group);
        RunSnapshot start = Simulation.Start(Seed, map, content, DebugIntentHandlers.None).Snapshot();
        MapEncounter encounter = new(map.Patrols[0].Id, map.Patrols[0].Group, behind);
        RunSnapshot held = start with { Map = start.Map! with { Encounter = encounter } };
        return Simulation.Resume(Seed, held, map, content, DebugIntentHandlers.None);
    }

    private static void Assume(bool holds)
    {
        Assert.True(holds, "The test run ended before the case it reads.");
    }
}
