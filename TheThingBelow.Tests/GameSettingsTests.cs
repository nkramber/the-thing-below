using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The ranges of the settings, and the two body sizes of the style file (D-707, D-861, D-867).</summary>
public sealed class GameSettingsTests
{
    [Fact]
    public void TheTwoBodySizesAreTheSizesOfTheStyleFile()
    {
        // Storage cannot read content, so it holds the two sizes of D-707 as constants.
        UiStyle style = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())).Style;

        Assert.Equal(GameSettings.SmallBody, style.SmallBody);
        Assert.Equal(GameSettings.LargeBody, style.LargeBody);
    }

    [Fact]
    public void TheSliderOfTheDeadZoneHoldsThirteenSteps()
    {
        // D-861: 0.2 to 0.8 in steps of 0.05.
        int steps = ((GameSettings.HighestDeadZone - GameSettings.LowestDeadZone) / GameSettings.DeadZoneStep) + 1;

        Assert.Equal(13, steps);
        Assert.Equal(0, (GameSettings.DefaultDeadZone - GameSettings.LowestDeadZone) % GameSettings.DeadZoneStep);
    }

    [Fact]
    public void DefaultsWithABodyOfNoSizeFail()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameSettings.Defaults(SettingsFixtures.Bindings(), 28));
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
