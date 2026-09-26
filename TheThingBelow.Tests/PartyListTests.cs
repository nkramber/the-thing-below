using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cursor of the party window: the row intent, the reserve, and the swap intent (D-558, D-872,
/// D-1134, D-1135). The tests read the built Game assembly (D-614). The shipped content keeps Marrek
/// alone, so a party with a reserve comes from the content of the tests (D-1144).
/// </summary>
public sealed class PartyListTests
{
    private const ulong Seed = 20260925;

    [Fact]
    public void TheCursorWrapsAndTheMouseMovesTheSameCursor()
    {
        GameValue list = GameValue.New("PartyList", ThreeWithNoReserve());

        list.Call("Move", -1);
        Assert.Equal(2, list.Read<int>("Cursor"));
        list.Call("Point", 1);
        list.Call("Move", 1);
        Assert.Equal(2, list.Read<int>("Cursor"));
    }

    [Fact]
    public void WithNoReserveAConfirmIsTheRowIntentOfTheCharacterUnderTheCursor()
    {
        // D-558, D-1144: a party with no reserve takes the row at once, as before the reserve.
        GameValue list = GameValue.New("PartyList", ThreeWithNoReserve());
        list.Call("Move", 1);

        var intent = (Intent)list.Call("Confirm")!;

        Assert.False(list.Read<bool>("HasReserve"));
        Assert.Equal("Browse", list.Name("Stage"));
        Assert.Equal(IntentIds.PartyRow.Value, intent.Action.Value);
        Assert.Equal(new BattleTarget(BattleSide.Party, 1), intent.Target);
        Assert.Null(intent.Item);
        Assert.False(intent.IsDebug);
    }

    [Fact]
    public void WithAReserveAConfirmOffersTheRowAndTheSwap()
    {
        GameValue list = GameValue.New("PartyList", ReserveContent.Start(Seed, 2).State);
        list.Call("Move", 1);

        Assert.Null(list.Call("Confirm"));

        Assert.Equal("Action", list.Name("Stage"));
        Assert.Equal(1, list.Read<int>("Chosen"));
        Assert.Equal(2, list.Read<int>("Count"));
        Assert.True((bool)list.Call("AllowsSwap")!);

        // The row stays one confirm away, on the chosen character (D-558).
        var row = (Intent)list.Call("Confirm")!;
        Assert.Equal(IntentIds.PartyRow.Value, row.Action.Value);
        Assert.Equal(new BattleTarget(BattleSide.Party, 1), row.Target);
        Assert.Equal("Browse", list.Name("Stage"));
        Assert.Equal(1, list.Read<int>("Cursor"));
    }

    [Fact]
    public void TheSwapPicksAReserveCharacterAndGivesTheSwapIntent()
    {
        // D-1134: the swap names the party slot that goes out and the reserve index that comes in.
        GameValue list = GameValue.New("PartyList", ReserveContent.Start(Seed, 2).State);
        list.Call("Move", 1);
        list.Call("Move", 1);
        list.Call("Confirm");
        list.Call("Move", 1);
        Assert.Null(list.Call("Confirm"));
        Assert.Equal("Incoming", list.Name("Stage"));
        Assert.Equal(2, list.Read<int>("Count"));
        list.Call("Point", 1);

        var intent = (Intent)list.Call("Confirm")!;

        Assert.Equal(IntentIds.PartySwap.Value, intent.Action.Value);
        Assert.Equal(2, intent.Actor);
        Assert.Equal(1, intent.Option);
        Assert.Null(intent.Target);
        Assert.Equal("Browse", list.Name("Stage"));
        Assert.Equal(2, list.Read<int>("Cursor"));
    }

    [Fact]
    public void ADownedReserveCharacterCannotBePickedToComeIn()
    {
        // D-1135: a downed character never comes into the party, so its line shows dim and a
        // confirm on it does nothing.
        GameValue list = GameValue.New("PartyList", ReserveContent.Start(Seed, 2, 0).State);
        list.Call("Confirm");
        list.Call("Point", 1);
        list.Call("Confirm");

        Assert.Equal("Incoming", list.Name("Stage"));
        Assert.False((bool)list.Call("AllowsIncoming", 0)!);
        Assert.True((bool)list.Call("AllowsIncoming", 1)!);
        Assert.Null(list.Call("Confirm"));
        Assert.Equal("Incoming", list.Name("Stage"));

        list.Call("Move", 1);
        var intent = (Intent)list.Call("Confirm")!;
        Assert.Equal(1, intent.Option);
    }

    [Fact]
    public void TheSwapIsDimWhenEachReserveCharacterIsDown()
    {
        // D-1136: with each reserve character down, no swap can happen.
        GameValue list = GameValue.New("PartyList", ReserveContent.Start(Seed, 2, 0, 1).State);
        list.Call("Confirm");
        list.Call("Point", 1);

        Assert.False((bool)list.Call("AllowsSwap")!);
        Assert.Null(list.Call("Confirm"));
        Assert.Equal("Action", list.Name("Stage"));
    }

    [Fact]
    public void CancelGoesBackOneListAndClosesTheWindowFromTheParty()
    {
        GameValue list = GameValue.New("PartyList", ReserveContent.Start(Seed, 2).State);
        list.Call("Move", 1);
        list.Call("Confirm");
        list.Call("Point", 1);
        list.Call("Confirm");

        Assert.False((bool)list.Call("Cancel")!);
        Assert.Equal("Action", list.Name("Stage"));
        Assert.Equal(1, list.Read<int>("Cursor"));
        Assert.False((bool)list.Call("Cancel")!);
        Assert.Equal("Browse", list.Name("Stage"));
        Assert.Equal(1, list.Read<int>("Cursor"));
        Assert.True((bool)list.Call("Cancel")!);
    }

    [Fact]
    public void TheHelpOfEachStageNamesWhatAConfirmDoes()
    {
        GameValue alone = GameValue.New("PartyList", ThreeWithNoReserve());
        GameValue grouped = GameValue.New("PartyList", ReserveContent.Start(Seed, 2).State);

        Assert.Equal("menu.party_help", HelpOf(alone));
        Assert.Equal("menu.party_reserve_help", HelpOf(grouped));
        grouped.Call("Confirm");
        Assert.Equal("menu.party_reserve_help", HelpOf(grouped));
        grouped.Call("Point", 1);
        grouped.Call("Confirm");
        Assert.Equal("menu.swap_help", HelpOf(grouped));
    }

    [Fact]
    public void ANullRunIsAnError()
    {
        Assert.Throws<ArgumentNullException>(() => GameValue.New("PartyList", [null]));
    }

    [Fact]
    public void APointOutsideTheListIsAnError()
    {
        GameValue list = GameValue.New("PartyList", TestParty.Start(Seed, stored => stored).State);

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Call("Point", 1));
    }

    private static RunState ThreeWithNoReserve() => TestParty.StartEach(Seed, (_, stored) => stored, TestParty.FourContent).State;

    private static string HelpOf(GameValue list) => GameValue.Static("PartyView", "HelpIdOf", list.Value)!.ToString()!;
}
