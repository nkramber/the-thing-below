using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The names and the tile counts of the enemy sizes (D-206, D-236).</summary>
public sealed class EnemySizesTests
{
    [Fact]
    public void EveryNameGivesItsSize()
    {
        foreach (EnemySize size in EnemySizes.All)
        {
            Assert.True(EnemySizes.TryOf(EnemySizes.NameOf(size), out EnemySize found));
            Assert.Equal(size, found);
        }
    }

    [Fact]
    public void TheListHoldsEverySizeOfTheEnumeration()
    {
        Assert.Equal(Enum.GetValues<EnemySize>().Length, EnemySizes.All.Length);
    }

    [Fact]
    public void EachNameIsInTheTextOfEveryName()
    {
        // The error of an unknown name prints this text, so a content author reads the set
        // from the message alone (T-2).
        foreach (EnemySize size in EnemySizes.All)
        {
            Assert.Contains(EnemySizes.NameOf(size), EnemySizes.EveryName, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheSideOfEachSizeHoldsTheTileCountsOfD206()
    {
        Assert.Equal(1, EnemySizes.SideOf(EnemySize.Common));
        Assert.Equal(2, EnemySizes.SideOf(EnemySize.Elite));
        Assert.Equal(3, EnemySizes.SideOf(EnemySize.Boss));
    }

    [Fact]
    public void ANameThatNoSizeTakesGivesFalse()
    {
        Assert.False(EnemySizes.TryOf("giant", out EnemySize size));
        Assert.Equal(EnemySize.Common, size);
    }

    [Fact]
    public void AValueThatNamesNoSizeIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemySizes.NameOf((EnemySize)9));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemySizes.SideOf((EnemySize)9));
    }

    [Fact]
    public void ANameThatIsNullIsAnError()
    {
        Assert.Throws<ArgumentNullException>(() => EnemySizes.TryOf(null!, out _));
    }
}
