using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The character, the step rule, and the sight rule of each tile kind (D-528).</summary>
public sealed class TileKindsTests
{
    [Theory]
    [InlineData('.', TileKind.Floor)]
    [InlineData('#', TileKind.Wall)]
    [InlineData('+', TileKind.Doorway)]
    public void ACharacterOfATerrainRowNamesItsKind(char character, TileKind kind)
    {
        Assert.True(TileKinds.TryOf(character, out TileKind found));
        Assert.Equal(kind, found);
        Assert.Equal(character, TileKinds.CharacterOf(kind));
    }

    [Fact]
    public void AnotherCharacterNamesNoKind()
    {
        Assert.False(TileKinds.TryOf('~', out _));
    }

    [Fact]
    public void EveryKindIsInTheCharacterList()
    {
        // The error of an unknown character names this list, so a new kind joins it (T-2).
        foreach (TileKind kind in new[] { TileKind.Floor, TileKind.Wall, TileKind.Doorway })
        {
            Assert.Contains(TileKinds.CharacterOf(kind), TileKinds.EveryCharacter);
        }
    }

    [Theory]
    [InlineData(TileKind.Floor, true, false)]
    [InlineData(TileKind.Wall, false, true)]
    [InlineData(TileKind.Doorway, true, false)]
    public void EachKindGivesItsStepRuleAndItsSightRule(TileKind kind, bool walk, bool stopsSight)
    {
        Assert.Equal(walk, TileKinds.CanWalk(kind));
        Assert.Equal(stopsSight, TileKinds.StopsSight(kind));
    }

    [Fact]
    public void AValueThatNamesNoKindIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TileKinds.CanWalk((TileKind)9));
        Assert.Throws<ArgumentOutOfRangeException>(() => TileKinds.StopsSight((TileKind)9));
        Assert.Throws<ArgumentOutOfRangeException>(() => TileKinds.NameOf((TileKind)9));
        Assert.Throws<ArgumentOutOfRangeException>(() => TileKinds.CharacterOf((TileKind)9));
        Assert.Throws<ArgumentOutOfRangeException>(() => TileIds.Of((TileKind)9));
    }

    [Theory]
    [InlineData(TileKind.Floor, "tile.floor")]
    [InlineData(TileKind.Wall, "tile.wall")]
    [InlineData(TileKind.Doorway, "tile.doorway")]
    public void EachKindNamesTheContentIdOfItsDrawing(TileKind kind, string id)
    {
        // D-519: an art file names the content ids that it draws, and a rule file names no
        // art.
        Assert.Equal(id, TileIds.Of(kind).Value);
    }
}
