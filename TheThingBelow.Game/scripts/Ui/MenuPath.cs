using System;
using System.Collections.Generic;

namespace TheThingBelow.Game.Ui;

/// <summary>The kind of one window of the menu stack (D-211, D-986, D-987).</summary>
public enum MenuWindowKind
{
    /// <summary>The main list, which opens one window for each task (D-211, D-992).</summary>
    MainList,

    /// <summary>The party window, which sets the row of each character (D-558).</summary>
    Party,

    /// <summary>The status window, which shows the full sheet of each character (D-569, D-991).</summary>
    Status,

    /// <summary>The notice log window (D-987).</summary>
    Log,

    /// <summary>The settings screen of PR-63 (D-871).</summary>
    Settings,

    /// <summary>The dungeon map screen, which the map action opens from the walk (D-986).</summary>
    DungeonMap,

    /// <summary>The lesson window, which swaps a lesson and casts a heal or a cure (D-391, D-1030).</summary>
    Lessons,

    /// <summary>The gear window, which changes the gear of each character (D-44, D-1048).</summary>
    Gear,

    /// <summary>The item window, which uses an item outside a fight (D-1046, D-1049).</summary>
    Items,
}

/// <summary>
/// The windows of the menu stack, from the bottom to the top (D-211). Back closes the top
/// window, and the menu closes when the last one goes (D-162).
/// </summary>
/// <remarks>
/// The main list and the dungeon map screen each open a menu from the walk. A task window
/// opens over the main list alone, and the dungeon map screen opens alone, because the map
/// action works on the walk (D-986). Any other order points at a fault in the host (T-2).
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public sealed class MenuPath
{
    private readonly List<MenuWindowKind> windows = [];

    /// <summary>The open windows, from the bottom to the top.</summary>
    public IReadOnlyList<MenuWindowKind> Windows => this.windows;

    /// <summary>True while a window is open, so the menu pauses the world (D-162).</summary>
    public bool IsOpen => this.windows.Count > 0;

    /// <summary>The window on top, which takes the input.</summary>
    /// <exception cref="InvalidOperationException">No window is open (T-2).</exception>
    public MenuWindowKind Top => this.windows.Count > 0
        ? this.windows[^1]
        : throw new InvalidOperationException("The menu stack holds no window, so no window is on top (T-2).");

    /// <summary>Opens one window over the others.</summary>
    /// <param name="kind">The window.</param>
    /// <exception cref="InvalidOperationException">The window cannot open in this place of the stack (T-2).</exception>
    public void Open(MenuWindowKind kind)
    {
        bool first = kind is MenuWindowKind.MainList or MenuWindowKind.DungeonMap;
        if (first && this.windows.Count > 0)
        {
            throw new InvalidOperationException(
                $"The window '{kind}' opens a menu from the walk, and the stack already holds {Describe(this.windows)} (D-211, D-986, T-2).");
        }

        if (!first && (this.windows.Count == 0 || this.Top != MenuWindowKind.MainList))
        {
            throw new InvalidOperationException(
                $"The window '{kind}' opens over the main list, and the stack holds {Describe(this.windows)} (D-211, T-2).");
        }

        this.windows.Add(kind);
    }

    /// <summary>Closes the window on top (D-211).</summary>
    /// <returns>True when the stack is empty after the close, so the menu closes (D-162).</returns>
    /// <exception cref="InvalidOperationException">No window is open (T-2).</exception>
    public bool Back()
    {
        if (this.windows.Count == 0)
        {
            throw new InvalidOperationException("A back with no window open, and back closes the window on top (T-2).");
        }

        this.windows.RemoveAt(this.windows.Count - 1);
        return this.windows.Count == 0;
    }

    /// <summary>Closes every window, as the menu action and the map action do (D-162, D-986).</summary>
    public void CloseAll() => this.windows.Clear();

    private static string Describe(IReadOnlyList<MenuWindowKind> windows) =>
        windows.Count == 0 ? "no window" : string.Join(", ", windows);
}
