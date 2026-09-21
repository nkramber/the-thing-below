using System;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The route of each key event before any node of the game reads it (D-725, D-813). The tests
/// read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// The console draws in the frame viewport, which the engine gives no event. Before the fix of
/// PR-55, the host read the console key alone and let every other key go on, so no typed key
/// reached the entry. The smoke session types a line through the engine, and these tests pin
/// each route with no engine (T-3).
/// </remarks>
public sealed class KeyRoutesTests
{
    private const string TypeName = "TheThingBelow.Game.Ui.KeyRoutes";

    /// <summary>The value of the Godot key of the letter H, which types one character.</summary>
    private const long LetterKey = 'H';

    [Fact]
    public void EachKeyOfAnOpenConsoleGoesToTheConsole()
    {
        Assert.Equal("Console", Route(LetterKey, pressed: true, echo: false, consoleOpen: true, quitAllowed: true));
    }

    [Fact]
    public void TheReleaseAndTheRepeatOfAKeyOfAnOpenConsoleGoToTheConsole()
    {
        // A held Backspace repeats in the entry, so the repeat must reach it as well.
        Assert.Equal("Console", Route(LetterKey, pressed: false, echo: false, consoleOpen: true, quitAllowed: false));
        Assert.Equal("Console", Route(LetterKey, pressed: true, echo: true, consoleOpen: true, quitAllowed: false));
    }

    [Fact]
    public void TheQuitKeyClosesAnOpenConsoleAndNeverTheGame()
    {
        Assert.Equal("CloseConsole", Route(QuitKey(), pressed: true, echo: false, consoleOpen: true, quitAllowed: true));
    }

    [Fact]
    public void TheConsoleKeyOpensAndClosesTheConsole()
    {
        Assert.Equal("ToggleConsole", Route(ConsoleKey(), pressed: true, echo: false, consoleOpen: false, quitAllowed: true));
        Assert.Equal("ToggleConsole", Route(ConsoleKey(), pressed: true, echo: false, consoleOpen: true, quitAllowed: true));
    }

    [Fact]
    public void TheQuitKeyEndsTheSessionWhenTheHostAllowsIt()
    {
        Assert.Equal("Quit", Route(QuitKey(), pressed: true, echo: false, consoleOpen: false, quitAllowed: true));
    }

    [Fact]
    public void TheQuitKeyGoesToTheGameWhenTheHostRefusesTheQuit()
    {
        // A release build and an open menu read the key as cancel (D-813).
        Assert.Equal("Game", Route(QuitKey(), pressed: true, echo: false, consoleOpen: false, quitAllowed: false));
    }

    [Fact]
    public void TheReleaseAndTheRepeatOfTheQuitKeyEndNoSession()
    {
        Assert.Equal("Game", Route(QuitKey(), pressed: false, echo: false, consoleOpen: false, quitAllowed: true));
        Assert.Equal("Game", Route(QuitKey(), pressed: true, echo: true, consoleOpen: false, quitAllowed: true));
    }

    [Fact]
    public void AKeyOfAClosedConsoleGoesToTheGame()
    {
        Assert.Equal("Game", Route(LetterKey, pressed: true, echo: false, consoleOpen: false, quitAllowed: true));
    }

    [Fact]
    public void TheConsoleKeyAndTheQuitKeyAreTheBacktickAndEscape()
    {
        // The values of the Godot keys: the backtick is its character, and Escape sits in the
        // special range of the enum (the Godot docs, "Key").
        Assert.Equal((long)'`', ConsoleKey());
        Assert.Equal(4194305L, QuitKey());
    }

    private static string Route(long keycode, bool pressed, bool echo, bool consoleOpen, bool quitAllowed)
    {
        MethodInfo found = GameAssemblyFile.Type(TypeName).GetMethod("Of", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The key routes hold no method 'Of' (T-2).");

        object route = found.Invoke(null, [keycode, pressed, echo, consoleOpen, quitAllowed])
            ?? throw new InvalidOperationException("The key routes gave no route (T-2).");

        return route.ToString() ?? throw new InvalidOperationException("The route has no name (T-2).");
    }

    private static long ConsoleKey() => Constant("ConsoleKey");

    private static long QuitKey() => Constant("QuitKey");

    private static long Constant(string name) =>
        (long)(GameAssemblyFile.Type(TypeName).GetField(name, BindingFlags.Public | BindingFlags.Static)?.GetRawConstantValue()
            ?? throw new InvalidOperationException($"The key routes hold no constant '{name}' (T-2)."));
}
