using System;
using System.Collections.Generic;

namespace TheThingBelow.Game.Ui;

/// <summary>One entry of the main list, in the order of D-992.</summary>
public enum MenuEntry
{
    /// <summary>The party window, which sets the row of each character and swaps the reserve (D-558, D-1134).</summary>
    Party,

    /// <summary>The lessons window, which PR-12 builds (D-525).</summary>
    Lessons,

    /// <summary>The gear window, which changes the gear of each character (D-44, D-1048).</summary>
    Gear,

    /// <summary>The items window, which uses an item outside a fight (D-1046, D-1049).</summary>
    Items,

    /// <summary>The status window (D-569).</summary>
    Status,

    /// <summary>The notice log window (D-987).</summary>
    Log,

    /// <summary>The settings screen of PR-63 (D-871).</summary>
    Settings,
}

/// <summary>
/// The entries and the cursor of the main list (D-211, D-992). The keyboard, the gamepad, and
/// the mouse drive the one cursor (D-219, D-872).
/// </summary>
/// <remarks>
/// Each entry opens its window. The save entry left the list, because a save opens only from the
/// save service of a hub and from a save point (D-1143). Thus no entry shows dim any more
/// (D-988). A move of the cursor makes no intent, so the record holds no cursor move (D-493).
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public sealed class MainList
{
    private static readonly MenuEntry[] AllEntries =
    [
        MenuEntry.Party,
        MenuEntry.Lessons,
        MenuEntry.Gear,
        MenuEntry.Items,
        MenuEntry.Status,
        MenuEntry.Log,
        MenuEntry.Settings,
    ];

    /// <summary>Every entry, in the order of the list (D-992, D-1143).</summary>
    public static IReadOnlyList<MenuEntry> Entries => AllEntries;

    /// <summary>The place of the entry under the cursor.</summary>
    public int Cursor { get; private set; }

    /// <summary>The entry under the cursor.</summary>
    public MenuEntry Current => AllEntries[this.Cursor];

    /// <summary>Gives the window that an entry opens.</summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The kind of the window.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no entry of the list (T-2).</exception>
    public static MenuWindowKind WindowOf(MenuEntry entry) => entry switch
    {
        MenuEntry.Party => MenuWindowKind.Party,
        MenuEntry.Lessons => MenuWindowKind.Lessons,
        MenuEntry.Gear => MenuWindowKind.Gear,
        MenuEntry.Items => MenuWindowKind.Items,
        MenuEntry.Status => MenuWindowKind.Status,
        MenuEntry.Log => MenuWindowKind.Log,
        MenuEntry.Settings => MenuWindowKind.Settings,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, "The main list holds no such entry (T-2)."),
    };

    /// <summary>Moves the cursor by one entry, and wraps at each end.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves one entry up or down (T-2).");
        }

        this.Cursor = (this.Cursor + step + AllEntries.Length) % AllEntries.Length;
    }

    /// <summary>Puts the cursor on the entry under the mouse pointer (D-872).</summary>
    /// <param name="place">The place of the entry in the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is outside the list (T-2).</exception>
    public void Point(int place)
    {
        if (place < 0 || place >= AllEntries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(place), place, $"The main list holds {AllEntries.Length} entries (T-2).");
        }

        this.Cursor = place;
    }
}
