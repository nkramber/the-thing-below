using System;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the gear file: the slot kind, the stack limit, the amounts, and the element table (D-44, D-1036, D-1038, D-1047).</summary>
public sealed class GearListTests
{
    [Fact]
    public void TheCheckoutHoldsAPieceOfEachSlotKind()
    {
        // D-44: the weapon, the off-hand, the head, the body, and the accessories.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        GearList gear = content.Battle.Gear;

        foreach (GearSlotKind kind in GearList.AllSlotKinds)
        {
            Assert.Contains(gear.Records, piece => piece.Slot == kind);
        }

        // D-1036: only a special piece holds an element level other than normal.
        GearRecord ring = gear.Piece(ContentId.Parse("gear.fixture_ash_ring", "test", "gear"));
        Assert.Equal(Affinity.Resist, ring.Elements.Of(Element.Fire));
        Assert.Equal(Affinity.Weak, ring.Elements.Of(Element.Ice));
        GearRecord coat = gear.Piece(ContentId.Parse("gear.fixture_coat", "test", "gear"));
        Assert.Equal((0, 3, -2, 1), (coat.Attack, coat.Defense, coat.Speed, coat.Limit));
    }

    [Theory]
    [InlineData("\"slot\": \"weapon\", \"limit\": 2", "\"slot\": \"weapon\", \"limit\": 0", "limit", "outside 1 to 3")]
    [InlineData("\"slot\": \"weapon\", \"limit\": 2", "\"slot\": \"weapon\", \"limit\": 4", "limit", "outside 1 to 3")]
    [InlineData("\"attack\": 5", "\"attack\": 100", "attack", "outside -99 to 99")]
    [InlineData("\"speed\": -99", "\"speed\": -100", "speed", "outside -99 to 99")]
    [InlineData("\"slot\": \"weapon\"", "\"slot\": \"feet\"", "slot", "not one of weapon, off_hand, head, body, accessory")]
    [InlineData("\"attack\": 5, ", "", "attack", "absent")]
    [InlineData("\"fire\": \"normal\", ", "", "fire", "absent")]
    [InlineData("\"defense\": 3, ", "\"defense\": 3, \"health\": 5, ", "health", "unknown field")]
    [InlineData("\"id\": \"gear.test_shield\"", "\"id\": \"gear.test_blade\"", "gear.test_blade", "two times")]
    public void APieceThatBreaksARuleFailsWithTheField(string from, string to, string field, string reason)
    {
        // D-1036: a piece never holds health or MP, so an unknown field fails the load.
        int at = TestBattles.GearFile.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The gear file of the tests holds no '{from}'.");
        string text = string.Concat(TestBattles.GearFile.AsSpan(0, at), to, TestBattles.GearFile.AsSpan(at + from.Length));

        ContentException error = Assert.Throws<ContentException>(() => GearList.Read(Encoding.UTF8.GetBytes(text), GearList.Path));

        Assert.Equal(GearList.Path, error.File);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }
}
