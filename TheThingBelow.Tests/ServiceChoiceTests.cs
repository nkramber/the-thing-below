using System;
using System.Collections;
using System.Collections.Generic;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The choices of the window of a hub service: the rest or the save, and leave (D-390, D-1131,
/// D-1132). The tests read the built Game assembly (D-614).
/// </summary>
public sealed class ServiceChoiceTests
{
    [Fact]
    public void TheWindowOffersTheServiceThenLeave()
    {
        Assert.Equal(["Use", "Leave"], Options());

        GameValue choice = GameValue.New("ServiceChoice", ServiceKind.Save, null);
        Assert.Equal("Use", choice.Name("Current"));
        Assert.Equal(ServiceKind.Save, choice.Read<ServiceKind>("Kind"));
    }

    [Theory]
    [InlineData(ServiceKind.Rest, "intent.hub_rest")]
    [InlineData(ServiceKind.Save, "intent.hub_save")]
    public void AConfirmOnTheServiceGivesItsIntentOfThePlayer(ServiceKind kind, string action)
    {
        GameValue choice = GameValue.New("ServiceChoice", kind, kind == ServiceKind.Rest ? (int?)10 : null);

        var intent = (Intent)choice.Call("Confirm")!;

        Assert.Equal(action, intent.Action.Value);
        Assert.False(intent.IsDebug);
        Assert.Null(intent.Target);
        Assert.Null(intent.Option);
    }

    [Fact]
    public void AConfirmOnLeaveGivesNoIntent()
    {
        GameValue choice = GameValue.New("ServiceChoice", ServiceKind.Rest, 10);
        choice.Call("Move", 1);

        Assert.Equal("Leave", choice.Name("Current"));
        Assert.Null(choice.Call("Confirm"));
    }

    [Fact]
    public void TheCursorWrapsAndTheMouseMovesTheSameCursor()
    {
        // D-872: one cursor for the keyboard, the gamepad, and the mouse.
        GameValue choice = GameValue.New("ServiceChoice", ServiceKind.Rest, 10);

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
        GameValue choice = GameValue.New("ServiceChoice", ServiceKind.Rest, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => choice.Call("Move", 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => choice.Call("Point", 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("ServiceChoice", (ServiceKind)9, null));
    }

    [Fact]
    public void ARestPastTheGoldRefusesAndLeaveNeverDoes()
    {
        // Exit test 12 of PR-65 (D-1156): the window refuses a rest that the gold cannot pay.
        GameValue rest = GameValue.New("ServiceChoice", ServiceKind.Rest, 10);
        GameValue save = GameValue.New("ServiceChoice", ServiceKind.Save, null);

        Assert.True((bool)rest.Call("Refuses", 9)!);
        Assert.False((bool)rest.Call("Refuses", 10)!);
        Assert.False((bool)save.Call("Refuses", 0)!);
        rest.Call("Move", 1);
        Assert.False((bool)rest.Call("Refuses", 0)!);
    }

    [Fact]
    public void ARestTakesAPriceAndASaveTakesNone()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("ServiceChoice", ServiceKind.Rest, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("ServiceChoice", ServiceKind.Save, 5));
    }

    [Theory]
    [InlineData(ServiceKind.Rest, "Use", "menu.rest", "menu.rest_help")]
    [InlineData(ServiceKind.Save, "Use", "menu.save", "menu.save_help")]
    [InlineData(ServiceKind.Rest, "Leave", "menu.leave", "menu.leave_help")]
    [InlineData(ServiceKind.Save, "Leave", "menu.leave", "menu.leave_help")]
    public void EachChoiceTakesItsLabelAndItsHelpFromTheStringTable(ServiceKind kind, string option, string label, string help)
    {
        object value = GameValue.Enum("ServiceOption", option);

        Assert.Equal(label, GameValue.Static("ServiceView", "LabelOf", kind, value)!.ToString());
        Assert.Equal(help, GameValue.Static("ServiceView", "HelpOf", kind, value)!.ToString());
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
}
