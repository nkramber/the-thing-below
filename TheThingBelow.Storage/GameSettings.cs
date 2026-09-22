using System;

namespace TheThingBelow.Storage;

/// <summary>The two window modes of the display group (D-865).</summary>
public enum WindowSetting
{
    /// <summary>The default. The window covers the screen and has no border (D-227, D-865).</summary>
    Borderless,

    /// <summary>A window with a border, which the player can move.</summary>
    Window,
}

/// <summary>
/// The two fits of the frame on the screen (D-232). Game holds the same two values in its
/// own `FitMode`, because Storage takes no reference to Game (D-494).
/// </summary>
public enum FitSetting
{
    /// <summary>The default. The frame fills the height or the width of the screen (D-232, D-573).</summary>
    Fill,

    /// <summary>The frame grows by a whole number alone, and the bars grow (D-232).</summary>
    WholePixels,
}

/// <summary>The three values of the body size setting (D-707, D-874).</summary>
public enum BodySetting
{
    /// <summary>The default. The rule of D-707 on each screen: the larger size at a fit of 1x, and the smaller size above it.</summary>
    Auto,

    /// <summary>The smaller body size of the style file, 24 frame pixels.</summary>
    Small,

    /// <summary>The larger body size of the style file, 32 frame pixels.</summary>
    Large,
}

/// <summary>The three speeds of the type-out of a prose box (D-864).</summary>
public enum TextSpeed
{
    /// <summary>30 characters a second.</summary>
    Slow,

    /// <summary>The default. 60 characters a second.</summary>
    Normal,

    /// <summary>120 characters a second.</summary>
    Fast,
}

/// <summary>The three speeds of the events of the battle screen (D-866, D-873).</summary>
public enum MessageSpeed
{
    /// <summary>Each event holds 1.5 times as long as at normal.</summary>
    Slow,

    /// <summary>The default. Each event holds the ticks of PR-10: 32 for a line, and 44 for a strike.</summary>
    Normal,

    /// <summary>Each event holds half as long as at normal.</summary>
    Fast,
}

/// <summary>The three levels of the flash and shake reduction (D-863).</summary>
public enum EffectLevel
{
    /// <summary>The default. Each flash and each shake plays as its effect file sets (D-868).</summary>
    Full,

    /// <summary>Each shake and each flash plays at a quarter, and a fade replaces the color split.</summary>
    Reduced,

    /// <summary>No shake, no spell flash, and no color split. The hit-stop stays.</summary>
    Off,
}

/// <summary>The display group: the window mode, the fit, and the body size (D-226, D-618).</summary>
/// <param name="Window">The window mode (D-865).</param>
/// <param name="Fit">The fit of the frame (D-232).</param>
/// <param name="Body">The body size of the text (D-707, D-874).</param>
public sealed record DisplaySettings(WindowSetting Window, FitSetting Fit, BodySetting Body);

/// <summary>The audio group (D-435).</summary>
/// <param name="Master">The master volume, 0 to 10 (D-867).</param>
/// <param name="Music">The music volume, 0 to 10.</param>
/// <param name="Effects">The volume of the sound effects, 0 to 10.</param>
/// <param name="Ambience">The ambience volume, 0 to 10.</param>
/// <param name="MuteInBackground">True when the game goes silent while its window has no focus.</param>
/// <param name="Mono">True when the game mixes both channels into one (D-868).</param>
public sealed record AudioSettings(int Master, int Music, int Effects, int Ambience, bool MuteInBackground, bool Mono);

/// <summary>The controls group (D-226, D-434).</summary>
/// <param name="Bindings">The buttons of each action of the game (D-862).</param>
/// <param name="DeadZone">The dead zone of a stick, in hundredths of its full push (D-861).</param>
/// <param name="Vibration">True when the gamepad vibrates at the heavy moments of D-434.</param>
public sealed record ControlSettings(ControlBindings Bindings, int DeadZone, bool Vibration);

/// <summary>The battle group (D-226).</summary>
/// <param name="Messages">How long each event of the battle screen holds (D-866, D-873).</param>
/// <param name="RememberCursor">True when the command menu opens on the last command of each member.</param>
public sealed record BattleSettings(MessageSpeed Messages, bool RememberCursor);

/// <summary>The accessibility settings, apart from the remap of the controls group (D-214, D-870).</summary>
/// <param name="Effects">The level of the flash and shake reduction (D-863).</param>
/// <param name="Text">The speed of the type-out (D-864).</param>
public sealed record AccessSettings(EffectLevel Effects, TextSpeed Text);

/// <summary>
/// Every choice of the settings screen, which the settings file holds (D-226, D-860).
/// </summary>
/// <remarks>
/// No setting reaches Core or a run record. Core takes no reference to Storage, and a test
/// asserts the reference list (T-7, G-1). A setting changes the look, the sound, and the
/// input of the game, and never a rule.
/// <para>
/// Each value passes the checks of <see cref="Check"/> at the read and before the write, so
/// no value outside its range reaches Game (T-2).
/// </para>
/// </remarks>
/// <param name="Display">The display group.</param>
/// <param name="Audio">The audio group.</param>
/// <param name="Controls">The controls group.</param>
/// <param name="Battle">The battle group.</param>
/// <param name="Access">The accessibility settings.</param>
public sealed record GameSettings(
    DisplaySettings Display,
    AudioSettings Audio,
    ControlSettings Controls,
    BattleSettings Battle,
    AccessSettings Access)
{
    /// <summary>The lowest step of a volume, which is silent (D-867).</summary>
    public const int LowestVolume = 0;

    /// <summary>The highest step of a volume, which is full (D-867).</summary>
    public const int HighestVolume = 10;

    /// <summary>The step that each volume starts at (D-867).</summary>
    public const int DefaultVolume = 8;

    /// <summary>The lowest dead zone of the slider, in hundredths (D-861).</summary>
    public const int LowestDeadZone = 20;

    /// <summary>The highest dead zone of the slider, in hundredths (D-861).</summary>
    public const int HighestDeadZone = 80;

    /// <summary>The step of the slider of the dead zone, in hundredths (D-861).</summary>
    public const int DeadZoneStep = 5;

    /// <summary>The dead zone that every action starts with, in hundredths (D-861).</summary>
    public const int DefaultDeadZone = 50;

    /// <summary>Makes the settings of a first start, with no settings file (D-861, D-864 to D-868).</summary>
    /// <param name="bindings">The default buttons of each action, which Game holds.</param>
    /// <returns>The settings, which pass <see cref="Check"/>.</returns>
    /// <exception cref="ArgumentNullException">The bindings are null (T-2).</exception>
    public static GameSettings Defaults(ControlBindings bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        GameSettings settings = new(
            new DisplaySettings(WindowSetting.Borderless, FitSetting.Fill, BodySetting.Auto),
            new AudioSettings(DefaultVolume, DefaultVolume, DefaultVolume, DefaultVolume, MuteInBackground: true, Mono: false),
            new ControlSettings(bindings, DefaultDeadZone, Vibration: true),
            new BattleSettings(MessageSpeed.Normal, RememberCursor: false),
            new AccessSettings(EffectLevel.Full, TextSpeed.Normal));
        settings.Check();
        return settings;
    }

    /// <summary>Fails when a value is outside its range, or when a group is absent (T-2).</summary>
    /// <exception cref="ArgumentNullException">A group or the bindings are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A value is outside its range, or an enum holds no named value.
    /// </exception>
    public void Check()
    {
        ArgumentNullException.ThrowIfNull(this.Display, nameof(this.Display));
        ArgumentNullException.ThrowIfNull(this.Audio, nameof(this.Audio));
        ArgumentNullException.ThrowIfNull(this.Controls, nameof(this.Controls));
        ArgumentNullException.ThrowIfNull(this.Battle, nameof(this.Battle));
        ArgumentNullException.ThrowIfNull(this.Access, nameof(this.Access));
        ArgumentNullException.ThrowIfNull(this.Controls.Bindings, nameof(this.Controls.Bindings));

        CheckNamed(this.Display.Window, "display.window");
        CheckNamed(this.Display.Fit, "display.fit");
        CheckNamed(this.Display.Body, "display.body");

        CheckVolume(this.Audio.Master, "audio.master");
        CheckVolume(this.Audio.Music, "audio.music");
        CheckVolume(this.Audio.Effects, "audio.effects");
        CheckVolume(this.Audio.Ambience, "audio.ambience");
        CheckDeadZone(this.Controls.DeadZone);
        CheckNamed(this.Battle.Messages, "battle.messages");
        CheckNamed(this.Access.Effects, "access.effects");
        CheckNamed(this.Access.Text, "access.text");
    }

    private static void CheckVolume(int volume, string field)
    {
        if (volume < LowestVolume || volume > HighestVolume)
        {
            throw new ArgumentOutOfRangeException(
                field, volume, $"A volume takes a step from {LowestVolume} to {HighestVolume} (D-867).");
        }
    }

    private static void CheckDeadZone(int deadZone)
    {
        if (deadZone < LowestDeadZone || deadZone > HighestDeadZone || deadZone % DeadZoneStep != 0)
        {
            throw new ArgumentOutOfRangeException(
                "controls.deadZone",
                deadZone,
                $"The dead zone takes a value from {LowestDeadZone} to {HighestDeadZone} hundredths, in steps of {DeadZoneStep} (D-861).");
        }
    }

    private static void CheckNamed<T>(T value, string field)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                field, value, $"The value is not one of {string.Join(", ", Enum.GetNames<T>())} (T-2).");
        }
    }
}
