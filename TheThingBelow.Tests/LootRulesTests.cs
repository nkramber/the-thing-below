using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The steal of a Theft drill and the drops of a win (D-383, D-949, D-1042 to D-1045).</summary>
public sealed class LootRulesTests
{
    private const int Seeds = 1000;

    private static readonly ContentId Pilfer = ContentId.Parse("lesson.test_pilfer", "test", "lesson");
    private static readonly ContentId Draught = ContentId.Parse("item.fixture_draught", "test", "item");

    [Theory]
    [InlineData(3000, 0, 0, 3000)]
    [InlineData(3000, 2500, 0, 5500)]
    [InlineData(3000, 2500, 1, 2750)]
    [InlineData(3000, 2500, 2, 1375)]
    [InlineData(10000, 0, 0, 9000)]
    [InlineData(10000, 0, 1, 5000)]
    public void EachEarlierSuccessHalvesTheChanceBeforeTheClamp(int baseChance, int term, int successes, int expected)
    {
        // D-949: the base chance and the Theft term, in the clamp. D-1045: each success halves it.
        Assert.Equal(expected, LootRules.StealChance(baseChance, term, successes, TestBattles.Content.Rules));
    }

    [Fact]
    public void AFightAllowsThreeStealTriesAndEachSuccessMakesTheNextHarder()
    {
        // Exit test 6 of PR-13: three tries of the whole party, and a failed try counts (D-1045).
        BattleContent content = TestBattles.WithThief(6000);
        int firstTries = 0;
        int firstWins = 0;
        int afterWinTries = 0;
        int afterWinWins = 0;
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            Battle battle = BattleRuns.BattleOf(run);
            for (int attempt = 0; attempt < LootRules.MostStealTries; attempt += 1)
            {
                int before = battle.Stolen.Count;
                Steal(run, battle.Enemies[0].Target);
                Assert.True(battle.StealTries == attempt + 1, $"Seed {seed}: try {attempt + 1} counted {battle.StealTries} tries.");
                bool won = battle.Stolen.Count > before;
                if (attempt == 0)
                {
                    firstTries += 1;
                    firstWins += won ? 1 : 0;
                }
                else if (before > 0)
                {
                    afterWinTries += 1;
                    afterWinWins += won ? 1 : 0;
                }
            }

            string? refusal = BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Lesson, battle.Enemies[0].Target, null, Pilfer, 0));
            Assert.True(refusal?.Contains("a fight allows 3", StringComparison.Ordinal) == true, $"Seed {seed}: a fourth try reads '{refusal}'.");
        }

        // The rolls follow the seeds, so the rates are fixed. The first try takes 6000, and a try
        // after one success takes 3000 or less.
        Assert.InRange(firstWins * 100 / firstTries, 55, 65);
        Assert.True(afterWinWins * 100 / afterWinTries < 35, $"{afterWinWins} of {afterWinTries} tries after a success took an entry.");
    }

    [Fact]
    public void EachEntryLeavesTheEnemyAndAnEmptyListCostsTheTurn()
    {
        // D-1044: a taken entry leaves the enemy for the fight. The clamp of 10000 makes the first
        // two tries sure, and the rate of 10000 keeps the second one sure.
        BattleContent content = TestBattles.WithThief(10000, ("steal_ceiling", 10000), ("steal_rate", 10000));
        for (ulong seed = 1; seed <= 20; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            Battle battle = BattleRuns.BattleOf(run);
            BattleTarget enemy = battle.Enemies[0].Target;

            List<BattleEventKind> kinds = [.. Steal(run, enemy), .. Steal(run, enemy), .. Steal(run, enemy)];

            Assert.True(kinds.Contains(BattleEventKind.StealItem), $"Seed {seed}: no item.");
            Assert.True(kinds.Contains(BattleEventKind.StealGold), $"Seed {seed}: no gold.");
            Assert.True(kinds.Contains(BattleEventKind.StealEmpty), $"Seed {seed}: no empty list.");
            Assert.Equal(5, run.State.Characters.Gold);
            Assert.Equal(4, run.State.Characters.CountOf(Draught));
            Assert.Equal(2, battle.Stolen.Count);
        }
    }

    [Fact]
    public void AStolenItemOverTheStackLimitStaysWithTheEnemy()
    {
        // D-1044: the pack holds 5 draughts of 5, so the draught stays and a later steal can take it.
        BattleContent content = TestBattles.WithThief(10000, ("steal_ceiling", 10000), ("steal_rate", 10000));
        int full = 0;
        for (ulong seed = 1; seed <= 40; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            Assert.Equal(0, run.State.Characters.Pick(Draught, 2, content));
            Battle battle = BattleRuns.BattleOf(run);

            List<BattleEventKind> kinds = Steal(run, battle.Enemies[0].Target);

            if (kinds.Contains(BattleEventKind.StealFull))
            {
                full += 1;
                Assert.Empty(battle.Stolen);
                Assert.Equal(5, run.State.Characters.CountOf(Draught));
            }
        }

        Assert.True(full > 0, "No seed picked the draught.");
    }

    [Fact]
    public void AWinRollsTheDropsOfEachFallenEnemy()
    {
        // D-1042: a sure drop of each of the two grunts. The pack holds 3 of a limit of 5, so
        // both drops fit.
        Simulation run = BattleRuns.IntoBattle(1, "group.test_pair", TestBattles.WithDrops("[{ \"item\": \"item.fixture_draught\", \"chance\": 10000 }]"));
        _ = run.TakeBattleEvents();

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 1));

        List<BattleEvent> drops = [];
        foreach (BattleEvent played in run.TakeBattleEvents())
        {
            if (played.Kind is BattleEventKind.Drop or BattleEventKind.DropLost)
            {
                drops.Add(played);
            }
        }

        Assert.Equal([BattleEventKind.Drop, BattleEventKind.Drop], [drops[0].Kind, drops[1].Kind]);
        Assert.Equal([0, 1], [drops[0].Actor.Slot, drops[1].Actor.Slot]);
        Assert.Equal(Draught.Value, drops[0].Ability?.Value);
        Assert.Equal(5, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void ADropOverTheStackLimitLeavesTheGame()
    {
        Simulation run = BattleRuns.IntoBattle(2, "group.test_pair", TestBattles.WithDrops("[{ \"item\": \"item.fixture_draught\", \"chance\": 10000 }]"));
        Assert.Equal(0, run.State.Characters.Pick(Draught, 2, run.State.BattleContent));
        _ = run.TakeBattleEvents();

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 2));

        List<BattleEventKind> kinds = BattleRuns.Kinds(run);
        Assert.Equal(2, kinds.FindAll(kind => kind == BattleEventKind.DropLost).Count);
        Assert.Equal(5, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void ADropFollowsItsChance()
    {
        // D-1042: a drop is rare. A chance of 1000 drops about one grunt in ten over the seeds.
        BattleContent content = TestBattles.WithDrops("[{ \"item\": \"item.fixture_draught\", \"chance\": 1000 }]");
        int drops = 0;
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            Assert.True(BattleRuns.FightToEnd(run, seed) == BattleOutcome.Won, $"Seed {seed}: the fight did not end in a win.");
            drops += BattleRuns.Kinds(run).FindAll(kind => kind is BattleEventKind.Drop or BattleEventKind.DropLost).Count;
        }

        Assert.InRange(drops * 1000 / (2 * Seeds), 70, 130);
    }

    [Theory]
    [InlineData(10000, 0, 500)]
    [InlineData(10000, 1, 1500)]
    [InlineData(10000, 2, 2500)]
    [InlineData(1000, 2, 1000)]
    [InlineData(1000, 0, 500)]
    public void TheGearChanceIsTheLowerOfTheProfileAndTheCapOfTheSuccess(int profileChance, int successes, int expected)
    {
        // D-1051: 5%, 15%, and 25% for the first, the second, and the third success, and a profile can lower each.
        Assert.Equal(expected, LootRules.GearChance(profileChance, successes, TestBattles.Content.Rules));
    }

    [Fact]
    public void EachSuccessTakesGearAtTheCapOfItsPlace()
    {
        // D-1051: three golds keep a gold entry left for each success, so the gear rate of each
        // success reads its cap. The rolls follow the seeds, so the rates are fixed.
        BattleContent content = TestBattles.WithGearThief(10000, "[{ \"gold\": 5 }, { \"gold\": 6 }, { \"gold\": 7 }, { \"gear\": \"gear.test_blade\" }]");
        int[] tries = new int[3];
        int[] gear = new int[3];
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            BattleTarget enemy = BattleRuns.BattleOf(run).Enemies[0].Target;
            for (int success = 0; success < 3; success += 1)
            {
                List<BattleEventKind> kinds = Steal(run, enemy);
                tries[success] += 1;
                if (kinds.Contains(BattleEventKind.StealGear))
                {
                    gear[success] += 1;
                    break;
                }
            }
        }

        Assert.InRange(gear[0] * 1000 / tries[0], 20, 80);
        Assert.InRange(gear[1] * 1000 / tries[1], 100, 200);
        Assert.InRange(gear[2] * 1000 / tries[2], 190, 310);
    }

    [Fact]
    public void AFirstSuccessAfterTwoFailedTriesTakesTheCapOfAFirstSuccess()
    {
        // D-1051: the count reads the successes and never the tries.
        BattleContent content = TestBattles.WithGearThief(10000, "[{ \"gold\": 5 }, { \"gear\": \"gear.test_blade\" }]");
        int gear = 0;
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            Simulation fresh = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            RunSnapshot snapshot = fresh.Snapshot();
            RunSnapshot twoFailures = snapshot with { Battle = snapshot.Battle! with { Steals = new StealValues(2, []) } };
            Simulation run = Simulation.Resume(seed, twoFailures, BattleRuns.Map("group.test_pair"), content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            Assert.Equal(2, BattleRuns.BattleOf(run).StealTries);

            List<BattleEventKind> kinds = Steal(run, BattleRuns.BattleOf(run).Enemies[0].Target);

            Assert.True(kinds.Contains(BattleEventKind.StealGear) || kinds.Contains(BattleEventKind.StealGold), $"Seed {seed}: the sure steal took nothing.");
            gear += kinds.Contains(BattleEventKind.StealGear) ? 1 : 0;
        }

        Assert.InRange(gear * 1000 / Seeds, 20, 80);
    }

    [Fact]
    public void AStealNeverPicksGearThatThePackHasNoRoomFor()
    {
        // D-1051: the blade has a limit of 2, and the pack holds 2, so the pick checks the room
        // first. With the gold taken, only the blade is left, and the gold entry comes back.
        BattleContent content = TestBattles.WithGearThief(10000, "[{ \"gold\": 5 }, { \"gear\": \"gear.test_blade\" }]");
        ContentId blade = ContentId.Parse("gear.test_blade", "test", "gear");
        for (ulong seed = 1; seed <= 200; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_pair", content);
            Assert.Equal(0, run.State.Characters.Pick(blade, 2, content));
            Battle battle = BattleRuns.BattleOf(run);
            BattleTarget enemy = battle.Enemies[0].Target;

            List<BattleEventKind> kinds = [.. Steal(run, enemy), .. Steal(run, enemy), .. Steal(run, enemy)];

            Assert.DoesNotContain(BattleEventKind.StealGear, kinds);
            Assert.Equal(3, kinds.FindAll(kind => kind == BattleEventKind.StealGold).Count);
            Assert.Equal(15, run.State.Characters.Gold);
            Assert.Equal([0, 0, 0], [battle.Stolen[0].Entry, battle.Stolen[1].Entry, battle.Stolen[2].Entry]);
        }
    }

    [Fact]
    public void AResumeTakesARepeatedGoldEntryAndRefusesARepeatedItem()
    {
        // D-1051: the gold entry comes back when only gear is left, and no other entry repeats.
        Simulation run = BattleRuns.IntoBattle(4, "group.test_pair");
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot gold = snapshot with { Battle = snapshot.Battle! with { Steals = new StealValues(2, [new StolenEntry(0, 1), new StolenEntry(0, 1)]) } };
        RunSnapshot item = snapshot with { Battle = snapshot.Battle! with { Steals = new StealValues(2, [new StolenEntry(0, 0), new StolenEntry(0, 0)]) } };

        Simulation resumed = Simulation.Resume(4, gold, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            Simulation.Resume(4, item, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Equal(2, BattleRuns.BattleOf(resumed).Stolen.Count);
        Assert.Contains("a gold entry alone comes back", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFailedStealCostsTheTurnOfTheThief()
    {
        // D-949: a failure costs the turn, so the thief waits one push of the steal.
        BattleContent content = TestBattles.WithThief(1, ("steal_ceiling", 0));
        Simulation run = BattleRuns.IntoBattle(3, "group.test_pair", content);
        Battle battle = BattleRuns.BattleOf(run);

        List<BattleEventKind> kinds = Steal(run, battle.Enemies[0].Target);

        Assert.Contains(BattleEventKind.StealFailed, kinds);
        Assert.Equal(1, battle.StealTries);
        Assert.Empty(battle.Stolen);
    }

    /// <summary>Steals from one enemy on the turn of Marrek, and gives the kinds of the events of that step.</summary>
    private static List<BattleEventKind> Steal(Simulation run, BattleTarget enemy)
    {
        _ = run.TakeBattleEvents();
        run.Step([Intent.OfBattleLesson(Pilfer, 0, enemy)]);
        return BattleRuns.Kinds(run);
    }
}
