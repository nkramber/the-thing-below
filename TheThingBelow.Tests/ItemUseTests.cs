using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The use of an item: the four effects, the smaller amount in a fight, and the refusal of the item window (D-382, D-1046, D-1049).</summary>
public sealed class ItemUseTests
{
    private const ulong Seed = 17;

    private static readonly ContentId Draught = Id("item.fixture_draught");
    private static readonly ContentId Tonic = Id("item.test_tonic");
    private static readonly ContentId Salts = Id("item.test_salts");
    private static readonly ContentId Root = Id("item.test_root");
    private static readonly ContentId Token = Id("item.test_token");

    [Fact]
    public void AnItemRestoresLessInAFightThanOutsideOne()
    {
        // Exit test 4 of PR-13: the item rate of 5000 halves the 30 of the draught (D-382).
        Simulation menu = InMenu(marrek => marrek with { Health = 10 });
        menu.Step([Intent.OfMenuItem(Draught, 0)]);
        Assert.Equal(40, menu.State.Characters.Members[0].Health);
        Assert.Equal(2, menu.State.Characters.CountOf(Draught));

        Simulation fight = TestParty.Start(Seed, marrek => marrek with { Health = 10 }, TestBattles.Exact, BattleRuns.Map("group.test_pair"));
        fight.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        _ = fight.TakeBattleEvents();
        fight.Step([Intent.OfPlayer(IntentIds.BattleItem, new BattleTarget(BattleSide.Party, 0), Draught)]);

        BattleEvent used = Assert.Single(fight.TakeBattleEvents(), played => played.Kind == BattleEventKind.Item);
        Assert.Equal(15, used.Amount);
        Assert.Equal(Draught.Value, used.Ability?.Value);
    }

    [Fact]
    public void EachEffectActsInFullFromTheItemWindow()
    {
        // D-1046: a restore of MP, a cure of its statuses, and a revive with a set health.
        Simulation run = InMenu(marrek => marrek with { Growth = marrek.Growth! with { Mp = 1 }, Statuses = [StatusKind.Poison, StatusKind.Blind] });
        Stock(run, Tonic, Salts);

        run.Step([Intent.OfMenuItem(Tonic, 0)]);
        run.Step([Intent.OfMenuItem(Salts, 0)]);

        PartyMember marrek = run.State.Characters.Members[0];
        Assert.Equal(marrek.Stats.Mp, marrek.Mp);
        Assert.Equal([StatusKind.Blind], marrek.Statuses);

        Simulation down = InMenu(stored => stored with { Health = 0, Statuses = [] });
        Stock(down, Root);
        down.Step([Intent.OfMenuItem(Root, 0)]);
        Assert.Equal(25, down.State.Characters.Members[0].Health);
        Assert.Equal(0, down.State.Characters.CountOf(Root));
    }

    [Fact]
    public void TheItemWindowRefusesAUseThatChangesNothing()
    {
        // D-1049: the window refuses, and the item stays in the pack.
        Simulation run = InMenu(marrek => marrek);
        Stock(run, Tonic, Salts, Root, Token);

        Assert.Contains("at full health", ItemRules.RefusalOfMenuUse(run.State, Draught, 0), StringComparison.Ordinal);
        Assert.Contains("at full MP", ItemRules.RefusalOfMenuUse(run.State, Tonic, 0), StringComparison.Ordinal);
        Assert.Contains("holds none of its statuses", ItemRules.RefusalOfMenuUse(run.State, Salts, 0), StringComparison.Ordinal);
        Assert.Contains("who stands", ItemRules.RefusalOfMenuUse(run.State, Root, 0), StringComparison.Ordinal);
        Assert.Contains("the key item", ItemRules.RefusalOfMenuUse(run.State, Token, 0), StringComparison.Ordinal);
        Assert.Contains("the character slot 1", ItemRules.RefusalOfMenuUse(run.State, Draught, 1), StringComparison.Ordinal);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfMenuItem(Draught, 0)]));
        Assert.Contains("D-1049", error.Message, StringComparison.Ordinal);
        Assert.Equal(3, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void TheItemWindowRefusesAHealOnADownCharacterAndAnItemThePackLacks()
    {
        Simulation run = InMenu(marrek => marrek with { Health = 0, Statuses = [] });

        Assert.Contains("who is down", ItemRules.RefusalOfMenuUse(run.State, Draught, 0), StringComparison.Ordinal);
        Assert.Contains("the pack holds none", ItemRules.RefusalOfMenuUse(run.State, Tonic, 0), StringComparison.Ordinal);
        Assert.Contains("the item file does not hold", ItemRules.RefusalOfMenuUse(run.State, Id("gear.test_blade"), 0), StringComparison.Ordinal);
    }

    [Fact]
    public void AnItemUseNeedsTheOpenMenu()
    {
        Simulation run = TestParty.Start(Seed, marrek => marrek with { Health = 10 });

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfMenuItem(Draught, 0)]));

        Assert.Contains("item and gear windows make it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFightSpendsAnItemThatChangesNothing()
    {
        // D-1049: a turn in a fight is chosen, so the use goes through at full health.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.Exact);
        _ = run.TakeBattleEvents();
        int before = run.State.Characters.CountOf(Draught);
        int health = BattleRuns.BattleOf(run).Party[0].Health;
        Assert.Equal(BattleRuns.BattleOf(run).Party[0].FullHealth, health);

        run.Step([Intent.OfPlayer(IntentIds.BattleItem, new BattleTarget(BattleSide.Party, 0), Draught)]);

        BattleEvent used = Assert.Single(run.TakeBattleEvents(), played => played.Kind == BattleEventKind.Item);
        Assert.Equal(0, used.Amount);
        Assert.Equal(before - 1, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void AFightCutsARestoreAndARevive()
    {
        // D-1046: the rate halves the 10 MP of the tonic and the 25 health of the root, rounded down.
        Simulation run = TestParty.StartEach(
            Seed,
            (slot, stored) => slot == 0 ? stored with { Growth = stored.Growth! with { Mp = 0 } } : stored with { Health = 0 },
            TestBattles.ExactWithParty(2),
            BattleRuns.Map("group.test_pair"));
        Stock(run, Tonic, Root);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Battle battle = BattleRuns.BattleOf(run);
        Combatant second = battle.Party[1];
        Assert.Equal(CombatantPlace.Down, second.Place);
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfPlayer(IntentIds.BattleItem, battle.Next()!.Target, Tonic)]);
        List<BattleEvent> first = [.. run.TakeBattleEvents()];
        run.Step([Intent.OfPlayer(IntentIds.BattleItem, second.Target, Root)]);
        List<BattleEvent> then = [.. run.TakeBattleEvents()];

        Assert.Equal(5, Assert.Single(first, played => played.Kind == BattleEventKind.ItemMp).Amount);
        Assert.Equal(12, Assert.Single(then, played => played.Kind == BattleEventKind.Revive).Amount);
        Assert.Equal(CombatantPlace.Field, second.Place);
    }

    [Fact]
    public void AReviveReachesADownCharacterAloneAndAKeyItemNoneInAFight()
    {
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.Exact);
        Stock(run, Root, Token, Id("gear.test_blade"));
        var marrek = new BattleTarget(BattleSide.Party, 0);

        Assert.Contains("who stands", BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Item, marrek, Root)), StringComparison.Ordinal);
        Assert.Contains("no used-up item", BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Item, marrek, Token)), StringComparison.Ordinal);
        Assert.Contains("no used-up item", BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Item, marrek, Id("gear.test_blade"))), StringComparison.Ordinal);
    }

    [Fact]
    public void ACureInAFightEndsItsStatusesAndNeverTakesTheRate()
    {
        // D-390: poison and silence follow a character into the fight.
        Simulation run = TestParty.Start(Seed, stored => stored with { Statuses = [StatusKind.Poison, StatusKind.Silence] }, TestBattles.Exact, BattleRuns.Map("group.test_pair"));
        Stock(run, Salts);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Combatant marrek = BattleRuns.BattleOf(run).Party[0];
        Assert.True(marrek.Statuses.Holds(StatusKind.Poison));
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfPlayer(IntentIds.BattleItem, marrek.Target, Salts)]);

        List<BattleEvent> events = [.. run.TakeBattleEvents()];
        Assert.Equal(BattleEventKind.ItemCure, events[0].Kind);
        Assert.Equal(2, events.FindAll(played => played.Kind == BattleEventKind.StatusOff).Count);
        Assert.False(marrek.Statuses.Holds(StatusKind.Poison));
        Assert.False(marrek.Statuses.Holds(StatusKind.Silence));
    }

    private static Simulation InMenu(Func<CharacterValues, CharacterValues> change)
    {
        Simulation run = TestParty.Start(Seed, change);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        return run;
    }

    private static void Stock(Simulation run, params ContentId[] items)
    {
        foreach (ContentId item in items)
        {
            Assert.Equal(0, run.State.Characters.Pick(item, 1, run.State.BattleContent));
        }
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
