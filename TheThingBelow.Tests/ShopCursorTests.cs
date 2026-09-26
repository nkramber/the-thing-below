using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Shops;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cursor of the shop window: the shop menu, the list, the count, the reason of a refusal, and
/// the change of a fighter for a piece of gear (D-1149 to D-1159). The tests read the built Game
/// assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class ShopCursorTests
{
    private static readonly ContentId Store = Id("shop.test_store");
    private static readonly ContentId Draught = Id("item.fixture_draught");
    private static readonly ContentId Salts = Id("item.test_salts");
    private static readonly ContentId Helm = Id("gear.test_helm");
    private static readonly ContentId Charm = Id("gear.test_weak_charm");
    private static readonly ContentId Blade = Id("gear.test_blade");

    [Fact]
    public void TheMenuOffersBuySellAndLeaveAndLeaveClosesTheWindow()
    {
        GameValue cursor = Cursor(ShopRulesTests.OpenStore(100));

        Assert.Equal("Mode", cursor.Name("Stage"));
        Assert.Equal("Buy", cursor.Name("Mode"));
        cursor.Call("Move", -1);
        Assert.Equal("Leave", cursor.Name("Mode"));
        Assert.True(cursor.Read<bool>("LeaveChosen"));
        Assert.Throws<InvalidOperationException>(() => cursor.Call("Confirm"));
        Assert.True((bool)cursor.Call("Cancel")!);
    }

    [Fact]
    public void ACountOfABuyRunsFromOneToTheMostAndGivesTheBuyIntent()
    {
        // D-1158: 3 draughts of a limit of 5 leave room for 2, and a confirm on the count buys them.
        Simulation run = ShopRulesTests.OpenStore(100);
        GameValue cursor = Cursor(run);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal(("List", 5), (cursor.Name("Stage"), cursor.Read<int>("ListCount")));

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal(("Count", 1, 2), (cursor.Name("Stage"), cursor.Read<int>("Count"), cursor.Read<int>("Most")));
        cursor.Call("Step", 1);
        cursor.Call("Step", 1);
        Assert.Equal(2, cursor.Read<int>("Count"));
        cursor.Call("Step", -1);
        cursor.Call("Step", -1);
        Assert.Equal(1, cursor.Read<int>("Count"));
        cursor.Call("Step", 1);

        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The count sent no intent.");

        Assert.Equal((IntentIds.ShopBuy.Value, Draught.Value, 2), (made.Action.Value, made.Item?.Value, made.Option));
        Assert.Equal("List", cursor.Name("Stage"));
        run.Step([made]);
        Assert.Equal(60, run.State.Characters.Gold);
    }

    [Fact]
    public void AnEntryThatTheGoldCannotPayGivesNoReasonAndOpensNoCount()
    {
        // D-1166: 10 gold pays for no draught at 20, and the confirm does nothing, with no line of help.
        GameValue cursor = Cursor(ShopRulesTests.OpenStore(10));
        cursor.Call("Confirm");

        Assert.Null(cursor.Call("Refusal"));
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("List", cursor.Name("Stage"));
    }

    [Fact]
    public void TheSaleListHoldsThePackAndACategoryThatTheTypeRefusesGivesItsReason()
    {
        // D-1151, D-1154: the trader buys the draughts, and no cure.
        Simulation run = ShopRulesTests.OpenStore(0, party => party with { Pack = [.. party.Pack, new PackValues(Salts, 1)] });
        GameValue cursor = Cursor(run);
        cursor.Call("Move", 1);
        cursor.Call("Confirm");

        Assert.Equal(("Sell", 2), (cursor.Name("Mode"), cursor.Read<int>("ListCount")));
        Assert.Equal(Draught.Value, cursor.Read<ContentId>("Thing").Value);
        Assert.Null(cursor.Call("Refusal"));
        cursor.Call("Move", 1);
        Assert.Equal(Salts.Value, cursor.Read<ContentId>("Thing").Value);
        Assert.Equal("menu.shop_not_bought", ((ContentId?)cursor.Call("Refusal"))?.Value);
        Assert.Equal(0, cursor.Read<int>("Most"));
        cursor.Call("Move", -1);
        cursor.Call("Confirm");
        cursor.Call("Step", 1);

        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The count sent no intent.");

        Assert.Equal((IntentIds.ShopSell.Value, Draught.Value, 2), (made.Action.Value, made.Item?.Value, made.Option));
    }

    [Fact]
    public void CancelWalksBackFromTheCountToTheListToTheMenu()
    {
        GameValue cursor = Cursor(ShopRulesTests.OpenStore(100));
        cursor.Call("Confirm");
        cursor.Call("Confirm");

        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal("List", cursor.Name("Stage"));
        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal("Mode", cursor.Name("Stage"));
    }

    [Fact]
    public void TheCursorOfTheListStaysOnALineAfterTheListShortens()
    {
        // D-1153: the bought pilfer leaves the list, and the cursor moves up to the new last line.
        Simulation run = ShopRulesTests.OpenStore(1000);
        GameValue cursor = Cursor(run);
        cursor.Call("Confirm");
        cursor.Call("Point", 4);
        run.Step([Intent.OfShopBuy(Id("lesson.test_pilfer"), 1)]);

        Assert.Equal(4, cursor.Read<int>("ListCount"));
        Assert.Equal(Id("gear.test_blade").Value, cursor.Read<ContentId>("Thing").Value);
    }

    [Fact]
    public void ABuyOfGearAsksToEquipEachCopyAndNoLeavesEachInThePack()
    {
        // D-1167: two blades take two questions, and no or back leaves each copy in the pack.
        Simulation run = ShopRulesTests.OpenStore(1000);
        GameValue cursor = Cursor(run);
        run.Step([BuyThrough(cursor, Blade, 2)]);
        Assert.Equal(("EquipAsk", Blade.Value), (cursor.Name("Stage"), cursor.Read<ContentId>("EquipPiece").Value));

        cursor.Call("Move", 1);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal(("EquipAsk", 0), (cursor.Name("Stage"), cursor.Read<int>("AskCursor")));
        Assert.False((bool)cursor.Call("Cancel")!);

        Assert.Equal("List", cursor.Name("Stage"));
        Assert.Null(cursor.Read<object?>("EquipPiece"));
        Assert.Equal(2, run.State.Characters.CountOf(Blade));
    }

    [Fact]
    public void YesEquipsTheCopyOnTheChosenCharacterAndTheChangeReadsItsSlot()
    {
        // D-1167: Marrek wears no helm, so the helm goes in the head slot and adds its defense.
        Simulation run = ShopRulesTests.OpenStore(1000);
        GameValue cursor = Cursor(run);
        run.Step([BuyThrough(cursor, Helm, 1)]);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("EquipWho", cursor.Name("Stage"));

        PartyMember marrek = run.State.Characters.Members[0];
        int head = Assert.Single((System.Collections.Generic.IReadOnlyList<int>)cursor.Call("SlotsFor", marrek)!);
        StatRow trial = (StatRow)cursor.Call("TrialOf", marrek, head)!;
        Assert.Equal(marrek.StatsWith(run.State.BattleContent.Gear).Defense + 1, trial.Defense);
        Intent worn = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The equip step sent no intent.");

        Assert.Equal((IntentIds.GearWear.Value, 0, head, Helm.Value), (worn.Action.Value, worn.Actor, worn.Option, worn.Item?.Value));
        Assert.Equal("List", cursor.Name("Stage"));
        run.Step([worn]);
        Assert.Equal(Helm.Value, run.State.Characters.Members[0].Gear[head]?.Value);
        Assert.Equal(0, run.State.Characters.CountOf(Helm));
    }

    [Fact]
    public void BothFullAccessorySlotsAskWhichOneToReplaceAndAnEmptyOneTakesThePiece()
    {
        // D-1167: with both accessory slots full, a confirm on the character asks for the slot, and
        // the replaced ring goes to the pack. With one slot empty, the piece goes there.
        ContentId resist = Id("gear.test_resist_ring");
        ContentId absorb = Id("gear.test_absorb_ring");
        Simulation full = CharmRun([resist, absorb]);
        GameValue cursor = Cursor(full);
        full.Step([BuyThrough(cursor, Charm, 1)]);
        cursor.Call("Confirm");
        Assert.Equal([4, 5], (System.Collections.Generic.IReadOnlyList<int>)cursor.Call("SlotsFor", full.State.Characters.Members[0])!);

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("EquipSlot", cursor.Name("Stage"));
        cursor.Call("Move", 1);
        Intent worn = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The equip step sent no intent.");
        Assert.Equal((5, Charm.Value), (worn.Option, worn.Item?.Value));
        full.Step([worn]);
        Assert.Equal(1, full.State.Characters.CountOf(absorb));

        Simulation half = CharmRun([resist, null]);
        GameValue other = Cursor(half);
        half.Step([BuyThrough(other, Charm, 1)]);
        Assert.Equal([5], (System.Collections.Generic.IReadOnlyList<int>)other.Call("SlotsFor", half.State.Characters.Members[0])!);
    }

    /// <summary>Opens the buy list, picks a thing, sets a count, and gives the buy intent of the count.</summary>
    private static Intent BuyThrough(GameValue cursor, ContentId thing, int count)
    {
        cursor.Call("Confirm");
        System.Collections.IList entries = (System.Collections.IList)cursor.Read<object>("BuyEntries");
        for (int place = 0; place < entries.Count; place += 1)
        {
            if (string.Equals(((StockEntry)entries[place]!).Thing.Value, thing.Value, StringComparison.Ordinal))
            {
                cursor.Call("Point", place);
            }
        }

        cursor.Call("Confirm");
        for (int step = 1; step < count; step += 1)
        {
            cursor.Call("Step", 1);
        }

        return (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException($"The count of '{thing.Value}' sent no buy intent.");
    }

    /// <summary>Opens a store that sells the charm, with Marrek wearing two accessories or one.</summary>
    private static Simulation CharmRun(ContentId?[] accessories)
    {
        string shops = TestBattles.ShopsFile.Replace(
            "{ \"gear\": \"gear.test_blade\", \"price\": 80, \"count\": 3 },",
            "{ \"gear\": \"gear.test_blade\", \"price\": 80, \"count\": 3 },\n        { \"gear\": \"gear.test_weak_charm\", \"price\": 40, \"count\": 1 },",
            StringComparison.Ordinal);
        var gear = new ContentId?[GearRules.SlotCount];
        gear[4] = accessories[0];
        gear[5] = accessories[1];
        return ShopRulesTests.OpenStore(1000, party => party with { Characters = [party.Characters[0] with { Gear = gear }] }, TestBattles.WithShops(shops));
    }

    private static GameValue Cursor(Simulation run) =>
        GameValue.New("ShopCursor", run.State, run.State.BattleContent.Shops.Shop(Store));

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
