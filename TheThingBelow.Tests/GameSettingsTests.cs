using System;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The ranges of the settings (D-861, D-867).</summary>
public sealed class GameSettingsTests
{
    [Fact]
    public void TheSliderOfTheDeadZoneHoldsThirteenSteps()
    {
        // D-861: 0.2 to 0.8 in steps of 0.05.
        int steps = ((GameSettings.HighestDeadZone - GameSettings.LowestDeadZone) / GameSettings.DeadZoneStep) + 1;

        Assert.Equal(13, steps);
        Assert.Equal(0, (GameSettings.DefaultDeadZone - GameSettings.LowestDeadZone) % GameSettings.DeadZoneStep);
    }

    [Fact]
    public void AnEnumValueWithNoNameFails()
    {
        GameSettings settings = SettingsFixtures.Defaults() with
        {
            Battle = new BattleSettings((MessageSpeed)9, RememberCursor: false),
        };

        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(settings.Check);

        Assert.Contains("battle.messages", error.Message, StringComparison.Ordinal);
    }
}
