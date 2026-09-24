using System.Collections.Generic;
using TheThingBelow.Storage;

namespace TheThingBelow.Tests;

/// <summary>The settings that the tests of the settings file share (D-860).</summary>
/// <remarks>
/// Each code is the number of a Godot value, which Storage stores and never reads: 4194309 is
/// `Key.Enter`, 4194305 is `Key.Escape`, 87 is `Key.W`, 0 is `JoyButton.A`, 1 is
/// `JoyButton.B`, and 1 is `JoyAxis.LeftY`.
/// </remarks>
internal static class SettingsFixtures
{
    public const int Enter = 4194309;
    public const int Escape = 4194305;
    public const int W = 87;
    public const int ButtonA = 0;
    public const int ButtonB = 1;
    public const int LeftY = 1;

    /// <summary>Three actions with a key, a button, and a stick direction.</summary>
    /// <returns>The bindings, with no conflict.</returns>
    public static ControlBindings Bindings() => new(new SortedDictionary<string, IReadOnlyList<InputBinding>>
    {
        ["cancel"] = [InputBinding.OfKey(Escape), InputBinding.OfButton(ButtonB)],
        ["confirm"] = [InputBinding.OfKey(Enter), InputBinding.OfButton(ButtonA)],
        ["step_north"] = [InputBinding.OfKey(W), InputBinding.OfStick(LeftY, -1)],
    });

    /// <summary>
    /// The default bindings of each action of the game, with the numbers of the Godot
    /// values: the letter keys, the arrow keys, the pad, and the left stick (D-84).
    /// </summary>
    /// <returns>The bindings that `GameInputMap.DefaultBindings` gives.</returns>
    public static ControlBindings GameBindings() => new(new SortedDictionary<string, IReadOnlyList<InputBinding>>
    {
        ["cancel"] = [InputBinding.OfKey(Escape), InputBinding.OfKey(4194308), InputBinding.OfButton(ButtonB)],
        ["confirm"] = [InputBinding.OfKey(Enter), InputBinding.OfKey(32), InputBinding.OfButton(ButtonA)],
        ["map"] = [InputBinding.OfKey(SettingsMigration.MapKey), InputBinding.OfButton(SettingsMigration.MapButton)],
        ["menu"] = [InputBinding.OfKey(4194306), InputBinding.OfButton(6)],
        ["step_east"] = [InputBinding.OfKey(68), InputBinding.OfKey(4194321), InputBinding.OfButton(14), InputBinding.OfStick(0, 1)],
        ["step_north"] = [InputBinding.OfKey(W), InputBinding.OfKey(4194320), InputBinding.OfButton(11), InputBinding.OfStick(LeftY, -1)],
        ["step_south"] = [InputBinding.OfKey(83), InputBinding.OfKey(4194322), InputBinding.OfButton(12), InputBinding.OfStick(LeftY, 1)],
        ["step_west"] = [InputBinding.OfKey(65), InputBinding.OfKey(4194319), InputBinding.OfButton(13), InputBinding.OfStick(0, -1)],
        ["torch"] = [InputBinding.OfKey(SettingsMigration.TorchKey), InputBinding.OfButton(SettingsMigration.TorchButton)],
    });

    /// <summary>The defaults of a first start with the bindings of each action of the game.</summary>
    /// <returns>The settings.</returns>
    public static GameSettings DefaultsOfTheGame() => GameSettings.Defaults(GameBindings());

    /// <summary>The defaults of a first start (D-861, D-864 to D-868, D-874).</summary>
    /// <returns>The settings.</returns>
    public static GameSettings Defaults() => GameSettings.Defaults(Bindings());
}
