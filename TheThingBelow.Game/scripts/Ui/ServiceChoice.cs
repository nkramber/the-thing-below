using System;
using System.Collections.Generic;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>One choice of the window of a service (D-1131, D-1132).</summary>
public enum ServiceOption
{
    /// <summary>The service itself: the rest of a rest service, or the save of a save service.</summary>
    Use,

    /// <summary>The close of the window with no service.</summary>
    Leave,
}

/// <summary>
/// The choices and the cursor of the window of a hub service: the rest or the save, and leave
/// (D-390, D-1131, D-1132). The keyboard, the gamepad, and the mouse drive the one cursor (D-872).
/// </summary>
/// <remarks>
/// A confirm on the service gives its intent, and the window then closes the menu, so the record
/// holds the intent of the service and the close (D-493). A confirm on leave gives no intent and
/// closes the menu. The rules check the open service, so no rule lives here (D-100, D-1141).
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public sealed class ServiceChoice
{
    private static readonly ServiceOption[] AllOptions = [ServiceOption.Use, ServiceOption.Leave];

    /// <summary>Opens the cursor on the service.</summary>
    /// <param name="kind">The kind of the service that a confirm opened.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind of service (T-2).</exception>
    public ServiceChoice(ServiceKind kind)
    {
        if (kind != ServiceKind.Rest && kind != ServiceKind.Save)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The window opens for a rest service or a save service (D-1131, T-2).");
        }

        this.Kind = kind;
    }

    /// <summary>Every choice, in the order of the window: the service, then leave.</summary>
    public static IReadOnlyList<ServiceOption> Options => AllOptions;

    /// <summary>The kind of the service.</summary>
    public ServiceKind Kind { get; }

    /// <summary>The place of the choice under the cursor.</summary>
    public int Cursor { get; private set; }

    /// <summary>The choice under the cursor.</summary>
    public ServiceOption Current => AllOptions[this.Cursor];

    /// <summary>Moves the cursor by one choice, and wraps at each end.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves one choice up or down (T-2).");
        }

        this.Cursor = (this.Cursor + step + AllOptions.Length) % AllOptions.Length;
    }

    /// <summary>Puts the cursor on the choice under the mouse pointer (D-872).</summary>
    /// <param name="place">The place of the choice.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is outside the choices (T-2).</exception>
    public void Point(int place)
    {
        if (place < 0 || place >= AllOptions.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(place), place, $"The window of a service holds {AllOptions.Length} choices (T-2).");
        }

        this.Cursor = place;
    }

    /// <summary>Confirms the choice under the cursor. Either choice closes the window and the menu after it.</summary>
    /// <returns>The rest intent or the save intent of a hub on the service, or no value on leave.</returns>
    public Intent? Confirm()
    {
        if (this.Current == ServiceOption.Leave)
        {
            return null;
        }

        return Intent.OfPlayer(this.Kind == ServiceKind.Rest ? IntentIds.HubRest : IntentIds.HubSave);
    }
}
