using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The names of the side that reached the other from behind (D-265, D-746).</summary>
public sealed class EncounterSidesTests
{
    [Fact]
    public void EveryNameGivesItsSide()
    {
        foreach (EncounterSide side in EncounterSides.All)
        {
            Assert.True(EncounterSides.TryOf(EncounterSides.NameOf(side), out EncounterSide found));
            Assert.Equal(side, found);
        }
    }

    [Fact]
    public void TheListHoldsEverySideOfTheEnumeration()
    {
        Assert.Equal(Enum.GetValues<EncounterSide>().Length, EncounterSides.All.Length);
    }

    [Fact]
    public void EachNameIsInTheTextOfEveryName()
    {
        foreach (EncounterSide side in EncounterSides.All)
        {
            Assert.Contains(EncounterSides.NameOf(side), EncounterSides.EveryName, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ANameThatNoSideTakesGivesFalse()
    {
        Assert.False(EncounterSides.TryOf("both", out EncounterSide side));
        Assert.Equal(EncounterSide.None, side);
    }

    [Fact]
    public void AValueThatNamesNoSideIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EncounterSides.NameOf((EncounterSide)9));
    }

    [Fact]
    public void ANameThatIsNullIsAnError()
    {
        Assert.Throws<ArgumentNullException>(() => EncounterSides.TryOf(null!, out _));
    }
}
