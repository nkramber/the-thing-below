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

    /// <summary>The defaults of a first start on a screen at a fit of 1x (D-707).</summary>
    /// <returns>The settings.</returns>
    public static GameSettings Defaults() => GameSettings.Defaults(Bindings(), GameSettings.LargeBody);
}
