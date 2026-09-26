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
    public void AnEntryThatThePartyCannotBuyGivesItsReasonAndOpensNoCount()
    {
        // D-385, D-1158: 10 gold pays for no draught at 20, and the line of help says why.
        GameValue cursor = Cursor(ShopRulesTests.OpenStore(10));
        cursor.Call("Confirm");

        Assert.Equal("menu.shop_no_gold", ((ContentId?)cursor.Call("Refusal"))?.Value);
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
    public void TheTrialOfAFighterPutsThePieceInTheFirstEmptySlotOfItsKind()
    {
        // D-1159: Marrek wears no gear, so the helm adds its defense and the charm fills the first accessory slot.
        Simulation run = ShopRulesTests.OpenStore(0);
        GameValue cursor = Cursor(run);
        PartyMember marrek = run.State.Characters.Members[0];
        StatRow worn = marrek.StatsWith(run.State.BattleContent.Gear);

        StatRow helm = (StatRow)cursor.Call("TrialOf", marrek, run.State.BattleContent.Gear.Piece(Helm))!;
        StatRow charm = (StatRow)cursor.Call("TrialOf", marrek, run.State.BattleContent.Gear.Piece(Charm))!;

        Assert.Equal(worn.Defense + 1, helm.Defense);
        Assert.Equal((worn.Magic + 2, worn.Speed + 2, worn.Resistance - 1), (charm.Magic, charm.Speed, charm.Resistance));
    }

    private static GameValue Cursor(Simulation run) =>
        GameValue.New("ShopCursor", run.State, run.State.BattleContent.Shops.Shop(Store));

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
