using System;
using TheThingBelow.Tools.NormalMaps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The integer square root of the normal maps (D-502, F-38). The root uses whole numbers
/// alone, so x86_64 and Apple silicon give the same root, and each CI leg runs these tests.
/// </summary>
public sealed class IntegerRootTests
{
    [Fact]
    public void EachValueUpToTwoHundredThousandGivesTheFloorOfItsRoot()
    {
        for (long value = 0; value <= 200_000; value += 1)
        {
            long root = IntegerRoot.Floor(value);

            Assert.True(
                root * root <= value && (root + 1) * (root + 1) > value,
                $"The value {value} gave the root {root}.");
        }
    }

    /// <summary>A seed loop over large values, near the squares where a root can miss by one.</summary>
    [Fact]
    public void LargeValuesNearASquareGiveTheFloorOfTheirRoot()
    {
        for (int seed = 0; seed < 2_000; seed += 1)
        {
            var random = new Random(seed);
            long root = random.NextInt64(1, 3_037_000_499);
            foreach (long value in new[] { (root * root) - 1, root * root, (root * root) + 1 })
            {
                Assert.True(
                    IntegerRoot.Floor(value) == (value < root * root ? root - 1 : root),
                    $"Seed {seed}: the value {value} gave the root {IntegerRoot.Floor(value)}.");
            }
        }
    }

    [Fact]
    public void TheLargestValueGivesItsRoot()
    {
        Assert.Equal(3_037_000_499, IntegerRoot.Floor(long.MaxValue));
    }

    [Fact]
    public void ANegativeValueFails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IntegerRoot.Floor(-1));
    }
}
