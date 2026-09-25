using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The gate that passes the first press of each hold of an action (D-1077, F-107). The tests
/// read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// The four methods below hold no Godot value. The smoke session reads the gate with the pad
/// events of the engine.
/// </remarks>
public sealed class PressGateTests
{
    private const string TypeName = "TheThingBelow.Game.Ui.PressGate";
    private const string Menu = "menu";
    private const string Down = "ui_down";
    private const string Start = "pad 0 button 6";
    private const string MirrorStart = "pad 1 button 6";
    private const string Tab = "key 4194306";

    [Fact]
    public void TheFirstPressOfAHoldPasses()
    {
        Assert.True(Press(New(), Menu, Start));
    }

    [Fact]
    public void ASecondPressOfAHeldSourceStops()
    {
        // A stick past the dead zone gives a press on each motion event, so one push moved the
        // cursor of a menu many lines (F-107).
        object gate = New();
        Press(gate, Menu, Start);

        Assert.False(Press(gate, Menu, Start));
    }

    [Fact]
    public void AMirrorOfAHeldButtonOnASecondDeviceStops()
    {
        // A pad that the system shows as two devices gives two presses of one button, and the
        // menu opened and closed on one press (F-107).
        object gate = New();
        Press(gate, Menu, Start);

        Assert.False(Press(gate, Menu, MirrorStart));
    }

    [Fact]
    public void APressPassesAgainOnlyAfterEverySourceComesUp()
    {
        object gate = New();
        Press(gate, Menu, Start);
        Press(gate, Menu, MirrorStart);

        Assert.True(Release(gate, Menu, Start));
        Assert.False(Press(gate, Menu, Start));
        Release(gate, Menu, Start);
        Release(gate, Menu, MirrorStart);

        Assert.True(Press(gate, Menu, Start));
    }

    [Fact]
    public void TwoActionsHoldTheirSourcesApart()
    {
        object gate = New();
        Press(gate, Menu, Start);

        Assert.True(Press(gate, Down, Start));
    }

    [Fact]
    public void AReleaseOfASourceThatHeldNothingChangesNothing()
    {
        object gate = New();
        Press(gate, Menu, Start);

        Assert.False(Release(gate, Menu, Tab));
        Assert.False(Press(gate, Menu, Tab));
    }

    [Fact]
    public void AClearForgetsEveryHeldSource()
    {
        // The system sends no release to a window that lost the focus (T-2).
        object gate = New();
        Press(gate, Menu, Start);
        Press(gate, Down, Tab);

        Method("Clear").Invoke(gate, null);

        Assert.True(Press(gate, Menu, Start));
        Assert.True(Press(gate, Down, Tab));
    }

    [Fact]
    public void APadThatDisconnectsLetsGoOfItsSourcesAlone()
    {
        object gate = New();
        Press(gate, Menu, Start);
        Press(gate, Menu, Tab);
        Press(gate, Down, "pad 10 axis 1");

        Assert.Equal(1, ForgetPad(gate, 0));

        Assert.False(Press(gate, Menu, MirrorStart));
        Assert.False(Press(gate, Down, "pad 10 axis 1"));
        Release(gate, Menu, Tab);
        Release(gate, Menu, MirrorStart);
        Assert.True(Press(gate, Menu, Start));
    }

    [Fact]
    public void AnEmptyNameFails()
    {
        object gate = New();

        Assert.IsType<ArgumentException>(Assert.Throws<TargetInvocationException>(
            () => Method("Press").Invoke(gate, [string.Empty, Start])).InnerException);
        Assert.IsType<ArgumentException>(Assert.Throws<TargetInvocationException>(
            () => Method("Release").Invoke(gate, [Menu, string.Empty])).InnerException);
    }

    [Fact]
    public void AnActionStaysHeldUntilItsLastSourceComesUp()
    {
        // The held steps read this answer, so a release of one of two sources leaves the step
        // held (D-1084).
        object gate = New();
        Assert.False(Holds(gate, Menu));

        Press(gate, Menu, Start);
        Press(gate, Menu, Tab);
        Release(gate, Menu, Start);
        Assert.True(Holds(gate, Menu));

        Release(gate, Menu, Tab);
        Assert.False(Holds(gate, Menu));
    }

    [Fact]
    public void AClearLeavesNoActionHeld()
    {
        // A loss of the focus clears the gate, because the system sends no release (D-1084).
        object gate = New();
        Press(gate, Menu, Start);

        Method("Clear").Invoke(gate, null);

        Assert.False(Holds(gate, Menu));
    }

    [Fact]
    public void TheGateReadsEachMenuActionThatTheMenusRead()
    {
        IReadOnlyList<string> names = (IReadOnlyList<string>)GameAssemblyFile.Type(TypeName)
            .GetField("MenuNames", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

        Assert.Equal(["ui_accept", "ui_cancel", "ui_up", "ui_down", "ui_left", "ui_right"], names);
    }

    private static object New() =>
        Activator.CreateInstance(GameAssemblyFile.Type(TypeName))
        ?? throw new InvalidOperationException($"The type {TypeName} made no object.");

    private static bool Press(object gate, string action, string source) =>
        (bool)Method("Press").Invoke(gate, [action, source])!;

    private static bool Release(object gate, string action, string source) =>
        (bool)Method("Release").Invoke(gate, [action, source])!;

    private static bool Holds(object gate, string action) =>
        (bool)Method("Holds").Invoke(gate, [action])!;

    private static int ForgetPad(object gate, int device) =>
        (int)Method("ForgetPad").Invoke(gate, [device])!;

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(TypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Instance)
        ?? throw new InvalidOperationException($"The type {TypeName} has no method {name}.");
}
