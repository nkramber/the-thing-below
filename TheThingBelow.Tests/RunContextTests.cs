using System;
using TheThingBelow.Core;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The run context and the assertion helper of Core (T-2, G-18).
/// </summary>
public sealed class RunContextTests
{
    [Fact]
    public void TheDescriptionNamesTheSeedTheTickAndTheSubject()
    {
        RunContext context = new(seed: 99, tick: 12, subject: "map/chest-2");

        Assert.Equal("seed 99, tick 12, subject map/chest-2", context.Describe());
    }

    [Fact]
    public void AnEmptySubjectIsAnError()
    {
        Assert.Throws<ArgumentException>(() => new RunContext(1, 0, string.Empty));
        Assert.Throws<ArgumentNullException>(() => new RunContext(1, 0, null!));
    }

    [Fact]
    public void ATickBelowZeroIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunContext(1, -1, "map"));
    }

    [Fact]
    public void AnAssertionThatHoldsThrowsNothing()
    {
        RunContext context = new(1, 0, "party");

        CoreAssert.That(true, "the party holds one member", context);
    }

    [Fact]
    public void AFailedAssertionThrowsWithTheContext()
    {
        RunContext context = new(seed: 5, tick: 3, subject: "party");

        SimulationException error = Assert.Throws<SimulationException>(
            () => CoreAssert.That(false, "the party holds one member", context));

        Assert.Contains("assertion failed", error.Message, StringComparison.Ordinal);
        Assert.Contains("the party holds one member", error.Message, StringComparison.Ordinal);
        Assert.Contains("seed 5, tick 3, subject party", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAssertionHelperStaysOnInThisBuild()
    {
        // Exit test 8 of section 7.11 of `phase-1-foundations.md`. The CI legs build Tests
        // in Release, and a release export drops every `Debug.Assert`. The helper of Core
        // holds no `Conditional` attribute, so the call below throws in every build (G-18).
        RunContext context = new(1, 0, "release");

        Assert.Throws<SimulationException>(() => CoreAssert.That(false, "the guard holds", context));
    }
}
