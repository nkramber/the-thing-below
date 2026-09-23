using System;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The kind of an encounter, in the order boss, wrong thing, ambush, elite, common (D-937).</summary>
public sealed class EncounterKindTests
{
    [Fact]
    public void ABossGroupIsABossWhateverTheSideAndTheSize()
    {
        foreach (EncounterSide side in EncounterSides.All)
        {
            foreach (EnemySize size in EnemySizes.All)
            {
                Assert.Equal(EncounterKind.Boss, EncounterKinds.Of(boss: true, side, size));
            }
        }
    }

    [Fact]
    public void AnAmbushComesBeforeAnElite()
    {
        // D-937: the ambush tells the player what the map did not show.
        foreach (EnemySize size in EnemySizes.All)
        {
            Assert.Equal(EncounterKind.Ambush, EncounterKinds.Of(boss: false, EncounterSide.Enemy, size));
        }
    }

    [Theory]
    [InlineData(EncounterSide.None, EnemySize.Common, EncounterKind.Common)]
    [InlineData(EncounterSide.Party, EnemySize.Common, EncounterKind.Common)]
    [InlineData(EncounterSide.None, EnemySize.Elite, EncounterKind.Elite)]
    [InlineData(EncounterSide.Party, EnemySize.Elite, EncounterKind.Elite)]
    [InlineData(EncounterSide.None, EnemySize.Boss, EncounterKind.Elite)]
    public void WithNoBossAndNoAmbushTheSizeGivesTheKind(EncounterSide side, EnemySize size, EncounterKind kind)
    {
        // D-937: a patrol of elite size or larger gives elite. A sneak of the party is no kind of its own.
        Assert.Equal(kind, EncounterKinds.Of(boss: false, side, size));
    }

    [Fact]
    public void EachFixedKindHasOneNameThatTheTableTakes()
    {
        foreach (EncounterKind kind in EncounterKinds.Fixed)
        {
            Assert.True(EncounterKinds.TryFixedOf(EncounterKinds.NameOf(kind), out EncounterKind read));
            Assert.Equal(kind, read);
            Assert.Contains(EncounterKinds.NameOf(kind), EncounterKinds.FixedNames, StringComparison.Ordinal);
        }

        Assert.False(EncounterKinds.TryFixedOf("common", out _));
        Assert.DoesNotContain(EncounterKind.Common, EncounterKinds.Fixed);
    }
}
