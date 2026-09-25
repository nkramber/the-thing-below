using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The keys and the buttons of each action of <see cref="InputActions"/> (D-84, D-493), and
/// the dead zone of every action (D-861). Game builds the input map in code from the
/// settings, so the map lives beside the rule that reads it and a review reads both as C#
/// (D-214, D-561).
/// </summary>
/// <remarks>
/// A change to the input map does not last by itself, so the settings file holds each remap,
/// and a remap calls <see cref="Build"/> again (F-50, D-860).
/// <para>
/// The built-in `ui_*` actions drive the focus of a menu, and they keep their default
/// buttons, so every menu answers whatever the remap does (D-862, F-50). They take the dead
/// zone of the settings, and the pad buttons of <see cref="MenuPadButtons"/>, alone (D-861,
/// D-1077).
/// </para>
/// <para>
/// Godot gives a new event the device 0, and the input map then matches the first pad alone.
/// Every event of this map takes <see cref="AllDevices"/>, so each pad plays the game, the
/// Steam Deck and a USB or Bluetooth pad alike (D-1077, F-107).
/// </para>
/// <para>
/// The keyboard and the gamepad play every screen, and the mouse works on menus alone
/// (D-84, D-219). Thus no action here carries a mouse button.
/// </para>
/// </remarks>
public static class GameInputMap
{
    /// <summary>The count of hundredths in the full push of a stick (D-861).</summary>
    private const float HundredthsOfFullPush = 100f;

    /// <summary>The device number of an event that matches every device, which is `InputMap.ALL_DEVICES` of Godot (F-107).</summary>
    public const int AllDevices = -1;

    /// <summary>
    /// The pad button of each menu action that lacks one in Godot 4.7.2: the A button chooses
    /// and the B button goes back, as the confirm action and the cancel action do (D-1077, F-107).
    /// </summary>
    /// <remarks>
    /// The default `ui_accept` of Godot holds Enter and Space alone, and `ui_cancel` holds
    /// Escape alone. Each menu thus read no pad button but the D-pad and the stick.
    /// </remarks>
    public static readonly IReadOnlyList<(string Action, JoyButton Button)> MenuPadButtons =
    [
        ("ui_accept", JoyButton.A),
        ("ui_cancel", JoyButton.B),
    ];

    /// <summary>Gives the default bindings of each action of the game (D-84, D-862).</summary>
    /// <returns>The bindings of a first start, with no conflict.</returns>
    /// <remarks>
    /// A button constant of Godot names the place of a button and not its label, so
    /// <see cref="JoyButton.A"/> is Cross on a PlayStation pad (F-50).
    /// </remarks>
    public static ControlBindings DefaultBindings() => new(new SortedDictionary<string, IReadOnlyList<InputBinding>>(StringComparer.Ordinal)
    {
        [InputActions.StepNorth] = [Key(Godot.Key.W), Key(Godot.Key.Up), Button(JoyButton.DpadUp), Stick(JoyAxis.LeftY, -1)],
        [InputActions.StepSouth] = [Key(Godot.Key.S), Key(Godot.Key.Down), Button(JoyButton.DpadDown), Stick(JoyAxis.LeftY, 1)],
        [InputActions.StepEast] = [Key(Godot.Key.D), Key(Godot.Key.Right), Button(JoyButton.DpadRight), Stick(JoyAxis.LeftX, 1)],
        [InputActions.StepWest] = [Key(Godot.Key.A), Key(Godot.Key.Left), Button(JoyButton.DpadLeft), Stick(JoyAxis.LeftX, -1)],
        [InputActions.Confirm] = [Key(Godot.Key.Enter), Key(Godot.Key.Space), Button(JoyButton.A)],
        [InputActions.Cancel] = [Key(Godot.Key.Escape), Key(Godot.Key.Backspace), Button(JoyButton.B)],
        [InputActions.Menu] = [Key(Godot.Key.Tab), Button(JoyButton.Start)],

        // D-990: the Deck and an Xbox pad name the Back button View. The step of the settings file
        // from format 1 holds the same two numbers, and a test compares them (D-570).
        [InputActions.Map] = [Key(Godot.Key.M), Button(JoyButton.Back)],

        // D-1068: the step of the settings file from format 2 holds the same two numbers, and a test
        // compares them (D-570).
        [InputActions.Torch] = [Key(Godot.Key.T), Button(JoyButton.Y)],
    });

    /// <summary>Builds every action of the game in the input map of this session.</summary>
    /// <param name="controls">The controls group of the settings (D-861, D-862).</param>
    /// <exception cref="ArgumentNullException">The controls are null (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// The bindings hold an action that the game lacks, or lack an action of the game (T-2).
    /// </exception>
    /// <remarks>
    /// The method removes an action of the game before it adds it again, so a second call
    /// after a remap leaves no old event behind (F-50).
    /// </remarks>
    public static void Build(ControlSettings controls)
    {
        ArgumentNullException.ThrowIfNull(controls);
        CheckNames(controls.Bindings);

        float deadZone = controls.DeadZone / HundredthsOfFullPush;
        foreach (string action in InputActions.Names)
        {
            if (InputMap.HasAction(action))
            {
                InputMap.EraseAction(action);
            }

            InputMap.AddAction(action, deadZone);
            foreach (InputBinding binding in controls.Bindings.Of(action))
            {
                InputMap.ActionAddEvent(action, EventOf(binding));
            }
        }

        foreach (StringName action in InputMap.GetActions())
        {
            if (action.ToString().StartsWith(ControlBindings.MenuPrefix, StringComparison.Ordinal))
            {
                InputMap.ActionSetDeadzone(action, deadZone);
            }
        }

        AddMenuPadButtons();
    }

    /// <summary>Adds each pad button of <see cref="MenuPadButtons"/> to its menu action (D-1077).</summary>
    /// <exception cref="InvalidOperationException">The input map lacks a menu action (T-2).</exception>
    /// <remarks>
    /// <see cref="Build"/> runs again after each remap, and the menu actions stay in the map, so
    /// the method adds a button only when the action lacks it.
    /// </remarks>
    private static void AddMenuPadButtons()
    {
        foreach ((string action, JoyButton button) in MenuPadButtons)
        {
            if (!InputMap.HasAction(action))
            {
                throw new InvalidOperationException(
                    $"The input map lacks the menu action '{action}', which takes the pad button {button} (D-1077, T-2).");
            }

            InputEvent made = EventOf(InputBinding.OfButton((int)button));
            if (!InputMap.ActionHasEvent(action, made))
            {
                InputMap.ActionAddEvent(action, made);
            }
        }
    }

    /// <summary>Makes the Godot event that one binding stands for.</summary>
    /// <param name="binding">The binding of the settings file.</param>
    /// <returns>A key, a button, or a stick event of <see cref="AllDevices"/>, so any pad matches (D-1077).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The binding has no kind (T-2).</exception>
    public static InputEvent EventOf(InputBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        return binding.Kind switch
        {
            BindingKind.Key => new InputEventKey { PhysicalKeycode = (Godot.Key)binding.Code, Device = AllDevices },
            BindingKind.Button => new InputEventJoypadButton { ButtonIndex = (JoyButton)binding.Code, Device = AllDevices },
            BindingKind.Stick => new InputEventJoypadMotion { Axis = (JoyAxis)binding.Code, AxisValue = binding.Direction, Device = AllDevices },
            _ => throw new ArgumentOutOfRangeException(
                nameof(binding), binding.Kind, $"The binding {binding} has no kind of input (T-2)."),
        };
    }

    /// <summary>Gives the binding of one input event, for the remap (D-862).</summary>
    /// <param name="signal">The event that the player made.</param>
    /// <param name="deadZone">The dead zone of the settings, in hundredths (D-861).</param>
    /// <returns>
    /// The binding of a key press, a button press, or a stick push past the dead zone, or null
    /// for every other event, such as a release or a mouse event.
    /// </returns>
    public static InputBinding? BindingOf(InputEvent signal, int deadZone)
    {
        ArgumentNullException.ThrowIfNull(signal);

        switch (signal)
        {
            case InputEventKey key when key.Pressed && !key.Echo:
                return InputBinding.OfKey((int)(key.PhysicalKeycode != Godot.Key.None ? key.PhysicalKeycode : key.Keycode));
            case InputEventJoypadButton button when button.Pressed:
                return InputBinding.OfButton((int)button.ButtonIndex);
            case InputEventJoypadMotion motion when Math.Abs(motion.AxisValue) * HundredthsOfFullPush > deadZone:
                return InputBinding.OfStick((int)motion.Axis, motion.AxisValue < 0 ? -1 : 1);
            default:
                return null;
        }
    }

    private static void CheckNames(ControlBindings bindings)
    {
        List<string> absent = [];
        foreach (string action in InputActions.Names)
        {
            if (!ContainsName(bindings.Names, action))
            {
                absent.Add(action);
            }
        }

        List<string> unknown = [];
        foreach (string action in bindings.Names)
        {
            if (!ContainsName(InputActions.Names, action))
            {
                unknown.Add(action);
            }
        }

        if (absent.Count > 0 || unknown.Count > 0)
        {
            throw new InvalidOperationException(
                $"The bindings of the settings lack [{string.Join(", ", absent)}] and hold the unknown actions " +
                $"[{string.Join(", ", unknown)}]. The actions of the game are {InputActions.Describe()}. A new action " +
                "ships with a migration step of the settings file (D-570, T-2).");
        }
    }

    private static bool ContainsName(IEnumerable<string> names, string action)
    {
        foreach (string name in names)
        {
            if (string.CompareOrdinal(name, action) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static InputBinding Key(Godot.Key key) => InputBinding.OfKey((int)key);

    private static InputBinding Button(JoyButton button) => InputBinding.OfButton((int)button);

    private static InputBinding Stick(JoyAxis axis, int direction) => InputBinding.OfStick((int)axis, direction);
}
