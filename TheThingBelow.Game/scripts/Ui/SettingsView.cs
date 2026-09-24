using System;
using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The settings screen of PR-63 as a window of the menu stack: the settings entry of the main
/// list opens it (D-871).
/// </summary>
/// <remarks>
/// The screen keeps its own rules: a remap row takes the next input first, and a conflict
/// blocks the close (D-862). Back returns to the main list, and the menu action closes the whole
/// menu, each one only when no conflict stays. The host writes and applies the settings when
/// the screen closes (D-860).
/// </remarks>
public sealed class SettingsView : IMenuView
{
    private readonly string menuAction;

    /// <summary>Wraps one settings screen.</summary>
    /// <param name="screen">The screen, which this window frees when it closes.</param>
    /// <param name="menuAction">The name of the menu action of the game, which the screen also reads.</param>
    /// <exception cref="ArgumentNullException">The screen is null (T-2).</exception>
    /// <exception cref="ArgumentException">The action name is empty (T-2).</exception>
    public SettingsView(SettingsScreen screen, string menuAction)
    {
        ArgumentNullException.ThrowIfNull(screen);
        ArgumentException.ThrowIfNullOrEmpty(menuAction);

        this.Screen = screen;
        this.menuAction = menuAction;
    }

    /// <summary>The settings screen, whose menu holds the changed settings.</summary>
    public SettingsScreen Screen { get; }

    /// <inheritdoc/>
    public ViewOutcome Read(InputEvent signal, ScreenFit fit)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);

        return this.Screen.Read(signal, fit, this.menuAction) == SettingsOutcome.Close ? ViewOutcome.Back : ViewOutcome.Stay;
    }

    /// <inheritdoc/>
    public void Show() => this.Screen.Show();

    /// <inheritdoc/>
    public void Free() => this.Screen.Free();
}
