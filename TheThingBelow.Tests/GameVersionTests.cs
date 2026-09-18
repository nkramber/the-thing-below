using System;
using System.Globalization;
using TheThingBelow.Core;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The game version, which one constant in Core holds (D-448, D-653). PR-31 adds the test
/// that the tag of a release matches this constant, and the read of the export preset.
/// </summary>
public sealed class GameVersionTests
{
    [Fact]
    public void TheVersionTakesTheFormOfThreeNumbers()
    {
        string[] parts = GameVersion.Current.Split('.');

        Assert.Equal(3, parts.Length);
        foreach (string part in parts)
        {
            Assert.True(
                int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int number),
                $"The part '{part}' of the version is not a whole number.");
            Assert.True(number >= 0, $"The part '{part}' of the version is below zero.");
        }
    }

    [Fact]
    public void TheVersionStaysBelowOneUntilTheFullGameShips()
    {
        // D-448. The minor number follows the gate: 0.2 at the first playable, and 0.5 at
        // the prologue. The full game ships as 1.0.0.
        Assert.StartsWith("0.", GameVersion.Current, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTagIsTheVersionWithItsPrefix()
    {
        Assert.Equal("v", GameVersion.TagPrefix);
        Assert.Equal("v" + GameVersion.Current, GameVersion.Tag);
    }
}
