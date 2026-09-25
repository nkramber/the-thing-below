using System;
using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// Hides the mouse pointer when the player uses a pad or the keyboard, and shows it again when
/// the player moves the mouse or clicks (D-1078).
/// </summary>
/// <remarks>
/// The Steam Deck in desktop mode sends some buttons as keys, so a key hides the pointer too.
/// The pointer mode hides the picture of the pointer alone, and Godot still sends each mouse
/// event, so a move of the mouse shows the pointer again (D-872).
/// </remarks>
public sealed class MousePointer
{
    /// <summary>True while the pointer shows. A session starts with the pointer shown.</summary>
    public bool Shown { get; private set; } = true;

    /// <summary>Reads one input event, and hides or shows the pointer.</summary>
    /// <param name="signal">The event of this frame.</param>
    /// <returns>True when the event changed the pointer.</returns>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    public bool Read(InputEvent signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        bool? show = ShowAfter(signal);
        if (show is not bool next || next == this.Shown)
        {
            return false;
        }

        this.Shown = next;
        Input.MouseMode = next ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Hidden;
        return true;
    }

    /// <summary>Tells what one event asks of the pointer.</summary>
    /// <param name="signal">The event of this frame.</param>
    /// <returns>
    /// True for a move of the mouse or a click, false for a key press, a pad button press, or a
    /// stick push past half, and null for every other event, such as a release.
    /// </returns>
    /// <remarks>
    /// A stick at rest can drift a little, so a push under half leaves the pointer as it is.
    /// A mouse event with no move, which a system can send when the pointer hides, does the same.
    /// </remarks>
    private static bool? ShowAfter(InputEvent signal) => signal switch
    {
        InputEventMouseMotion motion => motion.Relative != Vector2.Zero ? true : null,
        InputEventMouseButton click => click.Pressed ? true : null,
        InputEventKey key => key.Pressed ? false : null,
        InputEventJoypadButton button => button.Pressed ? false : null,
        InputEventJoypadMotion stick => stick.IsPressed() ? false : null,
        _ => null,
    };
}
