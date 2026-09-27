using System;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>One choice of the window of a service (D-1131, D-1132).</summary>
public enum ServiceOption
{
    /// <summary>The service itself: the rest of a rest service, or the save of a save point.</summary>
    Use,

    /// <summary>The close of the window with no service.</summary>
    Leave,
}

/// <summary>
/// The choices and the cursor of the rest window of a hub service or of the save window of a save
/// point: the rest or the save, and leave (D-390, D-1131, D-1132, D-1221). The keyboard, the
/// gamepad, and the mouse drive the one cursor (D-872).
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
    /// <param name="window">The rest window or the save window that a confirm opened.</param>
    /// <param name="price">The gold of one rest for a rest, and no value for a save (D-1156).</param>
    /// <exception cref="ArgumentOutOfRangeException">The window is not the rest window or the save window, or a rest holds no price or a save holds one (T-2).</exception>
    public ServiceChoice(MenuWindowKind window, int? price)
    {
        if (window != MenuWindowKind.Rest && window != MenuWindowKind.Save)
        {
            throw new ArgumentOutOfRangeException(nameof(window), window, "The choice serves the rest window and the save window (D-1131, D-1221, T-2).");
        }

        if ((window == MenuWindowKind.Rest) != (price is not null))
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "A rest takes a price, and a save takes none (D-1156, T-2).");
        }

        this.Window = window;
        this.Price = price;
    }

    /// <summary>The gold of one rest, or no value for a save (D-1156).</summary>
    public int? Price { get; }

    /// <summary>Tells whether the choice under the cursor is a rest that the gold cannot pay (D-1156).</summary>
    /// <param name="gold">The gold of the party.</param>
    /// <returns>True when a confirm takes no rest, and the line of help says why.</returns>
    public bool Refuses(int gold) => this.Current == ServiceOption.Use && this.Price is int price && gold < price;

    /// <summary>Every choice, in the order of the window: the service, then leave.</summary>
    public static IReadOnlyList<ServiceOption> Options => AllOptions;

    /// <summary>The window: the rest window or the save window.</summary>
    public MenuWindowKind Window { get; }

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
    /// <returns>The rest intent or the save intent on the service, or no value on leave.</returns>
    public Intent? Confirm()
    {
        if (this.Current == ServiceOption.Leave)
        {
            return null;
        }

        return Intent.OfPlayer(this.Window == MenuWindowKind.Rest ? IntentIds.HubRest : IntentIds.Save);
    }
}
