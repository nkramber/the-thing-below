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
    public static GameSettings FromFormatOne(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        ControlBindings bindings = settings.Controls.Bindings;
        var actions = new SortedDictionary<string, IReadOnlyList<InputBinding>>(StringComparer.Ordinal);
        foreach (string action in bindings.Names)
        {
            if (string.CompareOrdinal(action, MapAction) == 0)
            {
                throw new ArgumentException(
                    $"The settings of format 1 hold the action '{MapAction}', and format 2 added it (D-986).", nameof(settings));
            }

            actions.Add(action, bindings.Of(action));
        }

        actions.Add(MapAction, [InputBinding.OfKey(MapKey), InputBinding.OfButton(MapButton)]);
        return settings with { Controls = settings.Controls with { Bindings = new ControlBindings(actions) } };
    }
}
