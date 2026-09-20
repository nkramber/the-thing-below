using System;
using TheThingBelow.Core;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The fixed-point math of Core (D-169, D-641). Every result rounds toward zero, and every
/// error carries the run context (T-2).
/// </summary>
public sealed class BasisPointsTests
{
    private static readonly RunContext Context = new(seed: 4242, tick: 7, subject: "battle/actor-3");

    [Fact]
    public void OneIsTenThousand()
    {
        Assert.Equal(10000, BasisPoints.One);
    }

    [Theory]
    [InlineData(100, 10000, 100)]
    [InlineData(100, 0, 0)]
    [InlineData(100, 15000, 150)]
    [InlineData(100, 5000, 50)]
    [InlineData(7, 5000, 3)]
    [InlineData(-7, 5000, -3)]
    [InlineData(1, 9999, 0)]
    [InlineData(-1, 9999, 0)]
    [InlineData(3, 3333, 0)]
    [InlineData(1000, 250, 25)]
    public void ARateRoundsTowardZero(int value, int rate, int expected)
    {
        Assert.Equal(expected, BasisPoints.Apply(value, rate, Context));
    }

    [Theory]
    [InlineData(7, 2, 3)]
    [InlineData(-7, 2, -3)]
    [InlineData(7, -2, -3)]
    [InlineData(-7, -2, 3)]
    [InlineData(0, 5, 0)]
    public void ADivisionRoundsTowardZero(int value, int divisor, int expected)
    {
        Assert.Equal(expected, BasisPoints.Divide(value, divisor, Context));
    }

    [Theory]
    [InlineData(1, 2, 5000)]
    [InlineData(1, 3, 3333)]
    [InlineData(-1, 3, -3333)]
    [InlineData(100, 100, 10000)]
    [InlineData(0, 7, 0)]
    public void ARateOfAPartRoundsTowardZero(int part, int whole, int expected)
    {
        Assert.Equal(expected, BasisPoints.RateOf(part, whole, Context));
    }

    [Fact]
    public void ADivisionByZeroThrowsWithTheContext()
    {
        SimulationException error = Assert.Throws<SimulationException>(
            () => BasisPoints.Divide(10, 0, Context));

        AssertCarriesTheContext(error);
        Assert.Contains("division of 10 by zero", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARateAgainstAWholeOfZeroThrowsWithTheContext()
    {
        SimulationException error = Assert.Throws<SimulationException>(
            () => BasisPoints.RateOf(10, 0, Context));

        AssertCarriesTheContext(error);
    }

    [Fact]
    public void AnOverflowOfARateThrowsWithTheContext()
    {
        // The product is above 4 thousand million, which no `int` holds. C# integer math
        // wraps in silence by default, and Core never does (T-2, G-18).
        SimulationException error = Assert.Throws<SimulationException>(
            () => BasisPoints.Apply(int.MaxValue, 20000, Context));

        AssertCarriesTheContext(error);
        Assert.IsType<OverflowException>(error.InnerException);
    }

    [Fact]
    public void AnOverflowOfADivisionThrowsWithTheContext()
    {
        // The one quotient of two `int` values that no `int` holds.
        SimulationException error = Assert.Throws<SimulationException>(
            () => BasisPoints.Divide(int.MinValue, -1, Context));

        AssertCarriesTheContext(error);
        Assert.IsType<OverflowException>(error.InnerException);
    }

    [Fact]
    public void AnOverflowOfARateOfAPartThrowsWithTheContext()
    {
        SimulationException error = Assert.Throws<SimulationException>(
            () => BasisPoints.RateOf(int.MaxValue, 1, Context));

        AssertCarriesTheContext(error);
    }

    [Fact]
    public void ARateOfOneGivesTheValueBackForEverySeedOfARange()
    {
        // A seed loop over the values (T-3). Each failure names the value that found it.
        for (int value = -5000; value <= 5000; value += 1)
        {
            Assert.True(
                BasisPoints.Apply(value, BasisPoints.One, Context) == value,
                $"A rate of 100% changed the value {value}.");
        }
    }

    [Fact]
    public void ANullContextIsAnError()
    {
        Assert.Throws<ArgumentNullException>(() => BasisPoints.Apply(1, 1, null!));
        Assert.Throws<ArgumentNullException>(() => BasisPoints.Divide(1, 1, null!));
        Assert.Throws<ArgumentNullException>(() => BasisPoints.RateOf(1, 1, null!));
    }

    private static void AssertCarriesTheContext(SimulationException error)
    {
        Assert.Equal(Context, error.Context);
        Assert.Contains("seed 4242", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 7", error.Message, StringComparison.Ordinal);
        Assert.Contains("battle/actor-3", error.Message, StringComparison.Ordinal);
    }
}
