using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cursor of the party window and the row intent of a choice (D-558, D-872). The tests read
/// the built Game assembly (D-614).
/// </summary>
public sealed class PartyListTests
{
    [Fact]
    public void TheCursorWrapsAndTheMouseMovesTheSameCursor()
    {
        GameValue list = GameValue.New("PartyList", 3);

        list.Call("Move", -1);
        Assert.Equal(2, list.Read<int>("Cursor"));
        list.Call("Point", 1);
        list.Call("Move", 1);
        Assert.Equal(2, list.Read<int>("Cursor"));
    }

    [Fact]
    public void AChoiceIsTheRowIntentOfTheCharacterUnderTheCursor()
    {
        // D-558: the intent names the character as a target of the party side, and no item.
        GameValue list = GameValue.New("PartyList", 3);
        list.Call("Move", 1);

        var intent = (Intent)list.Call("Choose")!;

        Assert.Equal(IntentIds.PartyRow.Value, intent.Action.Value);
        Assert.Equal(new BattleTarget(BattleSide.Party, 1), intent.Target);
        Assert.Null(intent.Item);
        Assert.False(intent.IsDebug);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void ACountOutsideTheSizeOfAPartyIsAnError(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("PartyList", count));
    }

    [Fact]
    public void APointOutsideThePartyIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("PartyList", 2).Call("Point", 2));
    }
}
