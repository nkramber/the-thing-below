using System;
using System.Collections.Generic;

namespace TheThingBelow.Storage;

/// <summary>The steps from each older format of the settings file to the format of this build (D-570, D-869).</summary>
/// <remarks>
/// Storage takes no reference to Godot, so a step holds the number of each Godot value that it
/// adds. A test of Tests compares each number with the default bindings of Game (D-614).
/// </remarks>
public static class SettingsMigration
{
    /// <summary>The name of the map action, which format 2 added (D-986).</summary>
    public const string MapAction = "map";

    /// <summary>The number of the Godot `Key.M`, the default key of the map action (D-990).</summary>
    public const int MapKey = 77;

    /// <summary>The number of the Godot `JoyButton.Back`, the default button of the map action (D-990).</summary>
    /// <remarks>The Deck and an Xbox pad name this button View (D-990).</remarks>
    public const int MapButton = 4;

    /// <summary>The name of the torch action, which format 3 added (D-1068).</summary>
    public const string TorchAction = "torch";

    /// <summary>The number of the Godot `Key.T`, the default key of the torch action (D-1068).</summary>
    public const int TorchKey = 84;

    /// <summary>The number of the Godot `JoyButton.Y`, the default button of the torch action (D-1068).</summary>
    public const int TorchButton = 3;

    /// <summary>
    /// Gives the settings of format 1 in format 2: the controls gain the map action with its
    /// default key and button (D-986, D-990).
    /// </summary>
    /// <param name="settings">The settings that a file of format 1 held.</param>
    /// <returns>The settings with the map action. Every other value stays.</returns>
    /// <exception cref="ArgumentNullException">The settings are null (T-2).</exception>
    /// <exception cref="ArgumentException">The settings already hold the map action, which format 1 predates (T-2).</exception>
    /// <remarks>
    /// A player who bound M or the Back button to another action in format 1 meets a conflict
    /// on the settings screen, which names both actions and blocks the close until a remap
    /// ends it (D-862). The step never drops a binding of the player.
    /// </remarks>
    public static GameSettings FromFormatOne(GameSettings settings) =>
        WithAction(settings, MapAction, MapKey, MapButton, "format 1 hold the action 'map', and format 2 added it (D-986)");

    /// <summary>
    /// Gives the settings of format 2 in format 3: the controls gain the torch action with its
    /// default key and button (D-1068).
    /// </summary>
    /// <param name="settings">The settings that a file of format 2 held.</param>
    /// <returns>The settings with the torch action. Every other value stays.</returns>
    /// <exception cref="ArgumentNullException">The settings are null (T-2).</exception>
    /// <exception cref="ArgumentException">The settings already hold the torch action, which format 2 predates (T-2).</exception>
    /// <remarks>
    /// A player who bound T or the Y button to another action meets a conflict on the settings
    /// screen, as the step of format 1 does (D-862). The step never drops a binding of the player.
    /// </remarks>
    public static GameSettings FromFormatTwo(GameSettings settings) =>
        WithAction(settings, TorchAction, TorchKey, TorchButton, "format 2 hold the action 'torch', and format 3 added it (D-1068)");

    /// <summary>Adds one action with its default key and button to the controls, and keeps every other binding.</summary>
    private static GameSettings WithAction(GameSettings settings, string added, int key, int button, string refusal)
    {
        ArgumentNullException.ThrowIfNull(settings);

        ControlBindings bindings = settings.Controls.Bindings;
        var actions = new SortedDictionary<string, IReadOnlyList<InputBinding>>(StringComparer.Ordinal);
        foreach (string action in bindings.Names)
        {
            if (string.CompareOrdinal(action, added) == 0)
            {
                throw new ArgumentException($"The settings of {refusal}.", nameof(settings));
            }

            actions.Add(action, bindings.Of(action));
        }

        actions.Add(added, [InputBinding.OfKey(key), InputBinding.OfButton(button)]);
        return settings with { Controls = settings.Controls with { Bindings = new ControlBindings(actions) } };
    }
}
