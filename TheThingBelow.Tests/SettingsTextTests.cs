using System;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of the settings file: a round trip, the defaults, and a load that fails with the
/// file and the field (D-570, D-860, T-2).
/// </summary>
public sealed class SettingsTextTests
{
    private const string File = "settings.json";

    [Fact]
    public void TheDefaultsFollowTheDecisionsOfTheOwner()
    {
        GameSettings settings = SettingsFixtures.Defaults();

        // D-865, D-232, and D-874.
        Assert.Equal(WindowSetting.Borderless, settings.Display.Window);
        Assert.Equal(FitSetting.Fill, settings.Display.Fit);
        Assert.Equal(BodySetting.Auto, settings.Display.Body);

        // D-867, D-435, and D-868.
        Assert.Equal(8, settings.Audio.Master);
        Assert.Equal(8, settings.Audio.Music);
        Assert.Equal(8, settings.Audio.Effects);
        Assert.Equal(8, settings.Audio.Ambience);
        Assert.True(settings.Audio.MuteInBackground);
        Assert.False(settings.Audio.Mono);

        // D-861 and D-868.
        Assert.Equal(50, settings.Controls.DeadZone);
        Assert.True(settings.Controls.Vibration);

        // D-866 and D-868.
        Assert.Equal(MessageSpeed.Normal, settings.Battle.Messages);
        Assert.False(settings.Battle.RememberCursor);

        // D-863, D-864, and D-868. D-870 removed the shape icons.
        Assert.Equal(EffectLevel.Full, settings.Access.Effects);
        Assert.Equal(TextSpeed.Normal, settings.Access.Text);
    }

    [Fact]
    public void AReadOfAWriteGivesTheSameSettings()
    {
        // Exit test 1: each setting saves and loads through the settings file.
        GameSettings settings = SettingsFixtures.Defaults() with
        {
            Display = new DisplaySettings(WindowSetting.Window, FitSetting.WholePixels, BodySetting.Small),
            Audio = new AudioSettings(10, 0, 3, 7, MuteInBackground: false, Mono: true),
            Controls = new ControlSettings(
                SettingsFixtures.Bindings().Rebind("confirm", InputBinding.OfKey(SettingsFixtures.Enter), InputBinding.OfKey(SettingsFixtures.W + 1)),
                80,
                Vibration: false),
            Battle = new BattleSettings(MessageSpeed.Fast, RememberCursor: true),
            Access = new AccessSettings(EffectLevel.Reduced, TextSpeed.Slow),
        };

        Assert.Equal(settings, SettingsText.Read(SettingsText.Write(settings), File));
    }

    [Fact]
    public void TheTextIsTheSameOnEverySystem()
    {
        string text = SettingsText.Write(SettingsFixtures.Defaults());

        Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
        Assert.StartsWith("{\n  \"format\": 1,", text, StringComparison.Ordinal);
        Assert.EndsWith("}\n", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFieldFailsWithTheFileAndTheField()
    {
        // Exit test 5: a key that no version declares fails the load (D-570).
        string text = SettingsText.Write(SettingsFixtures.Defaults())
            .Replace("\"mono\": false", "\"mono\": false,\n    \"surround\": true", StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Equal(File, error.Path);
        Assert.Contains("audio.surround", error.Message, StringComparison.Ordinal);
        Assert.Contains("an unknown field", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFieldFailsAndTakesNoDefault()
    {
        string text = SettingsText.Write(SettingsFixtures.Defaults())
            .Replace(",\n    \"vibration\": true", string.Empty, StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Contains("controls.vibration", error.Message, StringComparison.Ordinal);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"master\": 8", "\"master\": 11", "audio.master")]
    [InlineData("\"deadZone\": 50", "\"deadZone\": 15", "controls.deadZone")]
    [InlineData("\"deadZone\": 50", "\"deadZone\": 52", "controls.deadZone")]
    [InlineData("\"body\": \"auto\"", "\"body\": \"huge\"", "display.body")]
    public void AValueOutsideItsRangeFails(string from, string to, string field)
    {
        string text = SettingsText.Write(SettingsFixtures.Defaults()).Replace(from, to, StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Contains(field, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownNameOfAValueFailsWithTheNamesThatExist()
    {
        string text = SettingsText.Write(SettingsFixtures.Defaults())
            .Replace("\"effects\": \"full\"", "\"effects\": \"none\"", StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Contains("'none' is not one of full, reduced, off", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ANewerFormatFailsAndNamesTheNewerBuild()
    {
        string text = SettingsText.Write(SettingsFixtures.Defaults())
            .Replace("\"format\": 1", "\"format\": 2", StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Contains("A newer build wrote it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFormatIsTheFirstField()
    {
        StorageException error = Assert.Throws<StorageException>(
            () => SettingsText.Read("{\"display\": {}, \"format\": 1}", File));

        Assert.Contains("format", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMenuActionInTheFileFails()
    {
        string text = SettingsText.Write(SettingsFixtures.Defaults())
            .Replace("\"cancel\":", "\"ui_cancel\":", StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Contains("D-862", error.Message, StringComparison.Ordinal);
    }
}
