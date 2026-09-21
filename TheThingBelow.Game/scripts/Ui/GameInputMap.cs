using System;
using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The keys and the buttons of each action of <see cref="InputActions"/> (D-84, D-493).
/// Game builds the input map in code at the start of a session, so the map lives beside the
/// rule that reads it and a review reads both as C# (D-214, D-561).
/// </summary>
/// <remarks>
/// A change to the input map does not last by itself, so PR-63 writes each remap to the
/// settings file and calls <see cref="Build"/> again after it (F-50).
/// <para>
/// The built-in `ui_*` actions drive the focus of a menu, and this class leaves them as they
/// are. A remap of PR-63 changes their events and never removes one (F-50).
/// </para>
/// <para>
/// The keyboard and the gamepad play every screen, and the mouse works on menus alone
/// (D-84, D-219). Thus no action here carries a mouse button.
/// </para>
/// </remarks>
public static class GameInputMap
{
    /// <summary>
    /// The dead zone of a stick, as a part of its full push. The value is the Godot default
    /// until PR-63 sets the one of the game, which D-226 gives to the settings screen (F-50).
    /// </summary>
    public const float StickDeadZone = 0.5f;

    /// <summary>Builds every action of the game in the input map of this session.</summary>
    /// <remarks>
    /// The method removes an action of the game before it adds it again, so a second call
    /// after a remap leaves no old event behind (F-50).
    /// </remarks>
    public static void Build()
    {
        foreach (string action in InputActions.Names)
        {
            if (InputMap.HasAction(action))
            {
                InputMap.EraseAction(action);
            }

            InputMap.AddAction(action, StickDeadZone);
        }

        AddSteps();
        AddChoices();
    }

    /// <summary>The four steps: the letter keys, the arrow keys, and the pad of a gamepad.</summary>
    private static void AddSteps()
    {
        AddKey(InputActions.StepNorth, Key.W);
        AddKey(InputActions.StepNorth, Key.Up);
        AddButton(InputActions.StepNorth, JoyButton.DpadUp);
        AddStick(InputActions.StepNorth, JoyAxis.LeftY, -1);

        AddKey(InputActions.StepSouth, Key.S);
        AddKey(InputActions.StepSouth, Key.Down);
        AddButton(InputActions.StepSouth, JoyButton.DpadDown);
        AddStick(InputActions.StepSouth, JoyAxis.LeftY, 1);

        AddKey(InputActions.StepEast, Key.D);
        AddKey(InputActions.StepEast, Key.Right);
        AddButton(InputActions.StepEast, JoyButton.DpadRight);
        AddStick(InputActions.StepEast, JoyAxis.LeftX, 1);

        AddKey(InputActions.StepWest, Key.A);
        AddKey(InputActions.StepWest, Key.Left);
        AddButton(InputActions.StepWest, JoyButton.DpadLeft);
        AddStick(InputActions.StepWest, JoyAxis.LeftX, -1);
    }

    /// <summary>
    /// The three choices. A button constant of Godot names the place of a button and not its
    /// label, so <see cref="JoyButton.A"/> is Cross on a PlayStation pad (F-50).
    /// </summary>
    private static void AddChoices()
    {
        AddKey(InputActions.Confirm, Key.Enter);
        AddKey(InputActions.Confirm, Key.Space);
        AddButton(InputActions.Confirm, JoyButton.A);

        AddKey(InputActions.Cancel, Key.Escape);
        AddKey(InputActions.Cancel, Key.Backspace);
        AddButton(InputActions.Cancel, JoyButton.B);

        AddKey(InputActions.Menu, Key.Tab);
        AddButton(InputActions.Menu, JoyButton.Start);
    }

    private static void AddKey(string action, Key key)
    {
        var press = new InputEventKey { PhysicalKeycode = key };
        InputMap.ActionAddEvent(action, press);
    }

    private static void AddButton(string action, JoyButton button)
    {
        var press = new InputEventJoypadButton { ButtonIndex = button };
        InputMap.ActionAddEvent(action, press);
    }

    /// <summary>Adds one direction of one stick axis to an action.</summary>
    /// <param name="action">The name of the action.</param>
    /// <param name="axis">The axis of the stick.</param>
    /// <param name="direction">-1 for the low end of the axis, and 1 for the high end.</param>
    private static void AddStick(string action, JoyAxis axis, int direction)
    {
        if (direction is not (-1 or 1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "A direction of a stick axis is -1 or 1 (T-2).");
        }

        var push = new InputEventJoypadMotion { Axis = axis, AxisValue = direction };
        InputMap.ActionAddEvent(action, push);
    }
}
