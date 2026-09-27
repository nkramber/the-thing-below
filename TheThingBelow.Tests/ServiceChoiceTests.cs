using System;
using System.Collections;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The choices of the rest window of a hub service and of the save window of a save point: the
/// rest or the save, and leave (D-390, D-1131, D-1132, D-1221). The tests read the built Game
/// assembly (D-614).
/// </summary>
public sealed class ServiceChoiceTests
{
    [Fact]
    public void TheWindowOffersTheServiceThenLeave()
    {
        Assert.Equal(["Use", "Leave"], Options());

        GameValue choice = GameValue.New("ServiceChoice", Window("Save"), null);
        Assert.Equal("Use", choice.Name("Current"));
        Assert.Equal("Save", choice.Name("Window"));
    }

    [Theory]
    [InlineData("Rest", "intent.hub_rest")]
    [InlineData("Save", "intent.save")]
    public void AConfirmOnTheServiceGivesItsIntentOfThePlayer(string window, string action)
    {
        GameValue choice = GameValue.New("ServiceChoice", Window(window), window == "Rest" ? (int?)10 : null);

        var intent = (Intent)choice.Call("Confirm")!;

        Assert.Equal(action, intent.Action.Value);
        Assert.False(intent.IsDebug);
        Assert.Null(intent.Target);
        Assert.Null(intent.Option);
    }

    [Fact]
    public void AConfirmOnLeaveGivesNoIntent()
    {
        GameValue choice = GameValue.New("ServiceChoice", Window("Rest"), 10);
        choice.Call("Move", 1);

        Assert.Equal("Leave", choice.Name("Current"));
        Assert.Null(choice.Call("Confirm"));
    }

    [Fact]
    public void TheCursorWrapsAndTheMouseMovesTheSameCursor()
    {
        // D-872: one cursor for the keyboard, the gamepad, and the mouse.
        GameValue choice = GameValue.New("ServiceChoice", Window("Rest"), 10);

        choice.Call("Move", -1);
        Assert.Equal(1, choice.Read<int>("Cursor"));
        choice.Call("Point", 0);
        choice.Call("Move", 1);
        Assert.Equal("Leave", choice.Name("Current"));
        choice.Call("Move", 1);
        Assert.Equal("Use", choice.Name("Current"));
    }

    [Fact]
    public void AStepOtherThanOneAPointOutsideTheChoicesAndAnUnknownKindAreErrors()
    {
        GameValue choice = GameValue.New("ServiceChoice", Window("Rest"), 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => choice.Call("Move", 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => choice.Call("Point", 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("ServiceChoice", Window("Shop"), null));
    }

    [Fact]
    public void ARestPastTheGoldRefusesAndLeaveNeverDoes()
    {
        // Exit test 12 of PR-65 (D-1156): the window refuses a rest that the gold cannot pay.
        GameValue rest = GameValue.New("ServiceChoice", Window("Rest"), 10);
        GameValue save = GameValue.New("ServiceChoice", Window("Save"), null);

        Assert.True((bool)rest.Call("Refuses", 9)!);
        Assert.False((bool)rest.Call("Refuses", 10)!);
        Assert.False((bool)save.Call("Refuses", 0)!);
        rest.Call("Move", 1);
        Assert.False((bool)rest.Call("Refuses", 0)!);
    }

    [Fact]
    public void ARestTakesAPriceAndASaveTakesNone()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("ServiceChoice", Window("Rest"), null));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("ServiceChoice", Window("Save"), 5));
    }

    [Theory]
    [InlineData("Rest", "Use", "menu.rest", "menu.rest_help")]
    [InlineData("Save", "Use", "menu.save", "menu.save_help")]
    [InlineData("Rest", "Leave", "menu.leave", "menu.leave_help")]
    [InlineData("Save", "Leave", "menu.leave", "menu.leave_help")]
    public void EachChoiceTakesItsLabelAndItsHelpFromTheStringTable(string window, string option, string label, string help)
    {
        object value = GameValue.Enum("ServiceOption", option);

        Assert.Equal(label, GameValue.Static("ServiceView", "LabelOf", Window(window), value)!.ToString());
        Assert.Equal(help, GameValue.Static("ServiceView", "HelpOf", Window(window), value)!.ToString());
    }

    private static List<string> Options()
    {
        List<string> names = [];
        foreach (object option in (IEnumerable)GameValue.StaticProperty("ServiceChoice", "Options")!)
        {
            names.Add(option.ToString()!);
        }

        return names;
    }

    /// <summary>Gives one value of the enum of the menu windows of Game, by its name.</summary>
    private static object Window(string name) => GameValue.Enum("MenuWindowKind", name);
}
