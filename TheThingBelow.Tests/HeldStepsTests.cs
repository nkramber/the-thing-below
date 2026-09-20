using System;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The step actions that the player holds down (D-716, F-50). The tests read the built Game
/// assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// The state comes from the press events and the release events of the input map, and never
/// from a poll of the input singleton (F-50). The four methods below hold no Godot value.
/// </remarks>
public sealed class HeldStepsTests
{
    private const string TypeName = "TheThingBelow.Game.Ui.HeldSteps";
    private const string North = "step_north";
    private const string South = "step_south";
    private const string East = "step_east";

    [Fact]
    public void NoActionIsHeldAtTheStart()
    {
        Assert.Null(Newest(New()));
    }

    [Fact]
    public void AHeldActionReadsBack()
    {
        object held = New();

        Assert.True(Press(held, North));

        Assert.Equal(North, Newest(held));
    }

    [Fact]
    public void ASecondPressOfOneActionChangesNothing()
    {
        // Godot sends no repeat for a held button, and a key repeat would add the same
        // action twice without this rule (F-50).
        object held = New();
        Press(held, North);

        Assert.False(Press(held, North));

        Assert.Equal(North, Newest(held));
    }

    [Fact]
    public void TheNewestHeldActionWins()
    {
        // A player who presses a second direction turns at once, and never waits for the
        // first key to come up (D-716).
        object held = New();
        Press(held, North);
        Press(held, East);

        Assert.Equal(East, Newest(held));
    }

    [Fact]
    public void TheOlderActionReturnsWhenTheNewerOneComesUp()
    {
        object held = New();
        Press(held, North);
        Press(held, East);

        Assert.True(Release(held, East));

        Assert.Equal(North, Newest(held));
    }

    [Fact]
    public void AReleaseOfAnActionThatNoOneHeldChangesNothing()
    {
        object held = New();
        Press(held, North);

        Assert.False(Release(held, South));

        Assert.Equal(North, Newest(held));
    }

    [Fact]
    public void ANewestActionEndsWhenTheLastKeyComesUp()
    {
        object held = New();
        Press(held, North);
        Release(held, North);

        Assert.Null(Newest(held));
    }

    [Fact]
    public void AClearForgetsEveryHeldAction()
    {
        object held = New();
        Press(held, North);
        Press(held, East);

        Method("Clear").Invoke(held, null);

        Assert.Null(Newest(held));
    }

    [Fact]
    public void AnEmptyActionNameIsAnError()
    {
        object held = New();

        TargetInvocationException error = Assert.Throws<TargetInvocationException>(
            () => Method("Press").Invoke(held, [string.Empty]));

        Assert.IsType<ArgumentException>(error.InnerException);
    }

    private static object New() =>
        Activator.CreateInstance(GameAssemblyFile.Type(TypeName))
        ?? throw new InvalidOperationException("The Game assembly made no held-step set (T-2).");

    private static bool Press(object held, string action) => (bool)Method("Press").Invoke(held, [action])!;

    private static bool Release(object held, string action) => (bool)Method("Release").Invoke(held, [action])!;

    private static string? Newest(object held) =>
        (string?)GameAssemblyFile.Type(TypeName).GetProperty("Newest")!.GetValue(held);

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(TypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Instance)
        ?? throw new InvalidOperationException($"The held-step set holds no method '{name}' (T-2).");
}
