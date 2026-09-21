using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>Where one key event of the keyboard goes (D-725, D-813).</summary>
public enum KeyRoute
{
    /// <summary>The event goes on to the nodes of the game and to the input map.</summary>
    Game,

    /// <summary>The event opens the debug console, or closes it.</summary>
    ToggleConsole,

    /// <summary>The event closes the open debug console.</summary>
    CloseConsole,

    /// <summary>The event goes to the entry of the open debug console, which draws in the frame.</summary>
    Console,

    /// <summary>The event ends the session.</summary>
    Quit,
}

/// <summary>
/// Picks the route of one key event, before any node of the game reads it (D-725, D-813).
/// </summary>
/// <remarks>
/// The console draws in the frame viewport, and the screen shows that viewport through a
/// texture, so no event reaches the entry unless the host pushes it there. Thus every key of
/// an open console takes the <see cref="KeyRoute.Console"/> route, the release and the repeat
/// included, and the game makes no intent (D-725).
/// <para>
/// <see cref="Of"/> holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public static class KeyRoutes
{
    /// <summary>
    /// The key that opens the debug console and closes it, in a development build alone
    /// (D-171, D-725). The key sits outside the input map, so it makes no intent and the
    /// remap of PR-63 never reaches it (D-214, F-50).
    /// </summary>
    public const long ConsoleKey = (long)Key.Quoteleft;

    /// <summary>
    /// The key that closes the open console, and that ends a session of a development build
    /// on the map (D-813). The key stays in the cancel action of the input map, so a release
    /// build reads it as cancel alone.
    /// </summary>
    public const long QuitKey = (long)Key.Escape;

    /// <summary>Gives the route of one key event.</summary>
    /// <param name="keycode">The key of the event, as the value of the Godot key.</param>
    /// <param name="pressed">True for a press, and false for a release.</param>
    /// <param name="echo">True for the repeat of a held key.</param>
    /// <param name="consoleOpen">True while the debug console shows.</param>
    /// <param name="quitAllowed">
    /// True in a development build with a run and no menu, where the quit key ends the
    /// session (D-813).
    /// </param>
    /// <returns>The route of the event.</returns>
    public static KeyRoute Of(long keycode, bool pressed, bool echo, bool consoleOpen, bool quitAllowed)
    {
        bool firstPress = pressed && !echo;

        if (firstPress && keycode == ConsoleKey)
        {
            return KeyRoute.ToggleConsole;
        }

        if (consoleOpen)
        {
            return firstPress && keycode == QuitKey ? KeyRoute.CloseConsole : KeyRoute.Console;
        }

        if (firstPress && keycode == QuitKey && quitAllowed)
        {
            return KeyRoute.Quit;
        }

        return KeyRoute.Game;
    }
}
