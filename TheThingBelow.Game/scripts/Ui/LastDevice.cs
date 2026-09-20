using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The device that the player touched last, and the glyph set that a prompt draws for it
/// (D-222, D-711). A key event or a mouse event gives the keyboard, and a joypad event gives
/// the set of that pad.
/// </summary>
/// <remarks>
/// Godot reports no controller type, so the name of the pad picks the set through the table
/// of `content/ui/devices.json` (F-50). Under Steam, PR-78 asks Steamworks for the type and
/// overrides the pick (D-460, D-553).
/// <para>
/// The tracker reads input events alone, and it never polls the input singleton (F-50). It
/// reads the name of a pad through the engine, which is a lookup of a device and not a read
/// of the state of that device.
/// </para>
/// </remarks>
public sealed class LastDevice
{
    private readonly DeviceNames names;

    /// <summary>Starts the tracker on the keyboard, which every screen accepts (D-84).</summary>
    /// <param name="names">The device table of the content set (D-711).</param>
    /// <exception cref="ArgumentNullException">The table is null (T-2).</exception>
    public LastDevice(DeviceNames names)
    {
        ArgumentNullException.ThrowIfNull(names);

        this.names = names;
        this.GlyphSet = names.KeyboardSet;
    }

    /// <summary>The name of the glyph set of the last device, such as `xbox`.</summary>
    public string GlyphSet { get; private set; }

    /// <summary>
    /// Gives the glyph set of one device. The method reads no engine value, so a test drives
    /// it with a pad name and no engine (D-614).
    /// </summary>
    /// <param name="names">The device table of the content set.</param>
    /// <param name="fromPad">True when a joypad made the event, and false for a key or a mouse.</param>
    /// <param name="padName">The name that the engine reports for the pad, or an empty text.</param>
    /// <returns>The name of the glyph set.</returns>
    /// <exception cref="ArgumentNullException">The table or the name is null (T-2).</exception>
    public static string SetFor(DeviceNames names, bool fromPad, string padName)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(padName);

        return fromPad ? names.SetOfPad(padName) : names.KeyboardSet;
    }

    /// <summary>Reads one input event, and changes the glyph set when the device changed.</summary>
    /// <param name="signal">The event that the engine sent to this frame.</param>
    /// <returns>True when the glyph set changed, so the prompts draw again.</returns>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    /// <remarks>
    /// An event of another kind, such as a window event, leaves the glyph set as it is. The
    /// prompts then keep the device of the last press.
    /// </remarks>
    public bool Read(InputEvent signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        if (!TryDeviceOf(signal, out bool fromPad))
        {
            return false;
        }

        string padName = fromPad ? Input.GetJoyName(signal.Device) : string.Empty;
        string picked = SetFor(this.names, fromPad, padName);
        if (string.CompareOrdinal(picked, this.GlyphSet) == 0)
        {
            return false;
        }

        this.GlyphSet = picked;
        return true;
    }

    /// <summary>Tells whether an event names a device, and which kind it is.</summary>
    /// <param name="signal">The event of the frame.</param>
    /// <param name="fromPad">True when a joypad made it, and false for a key or a mouse.</param>
    /// <returns>False for an event that names no device of the player.</returns>
    private static bool TryDeviceOf(InputEvent signal, out bool fromPad)
    {
        switch (signal)
        {
            case InputEventJoypadButton:
            case InputEventJoypadMotion:
                fromPad = true;
                return true;
            case InputEventKey:
            case InputEventMouseButton:
            case InputEventMouseMotion:
                fromPad = false;
                return true;
            default:
                fromPad = false;
                return false;
        }
    }
}
