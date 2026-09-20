using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The input map of Game makes one intent from each action, and no intent comes from a poll
/// of the input singleton (D-84, D-493, D-561, F-50).
/// </summary>
/// <remarks>
/// Exit test 6 of PR-61 proves the second rule. A poll of `Input` ignores what a menu already
/// took, so it makes a second intent for one press, and it also reads a key that the player
/// holds from an earlier frame (F-50).
/// </remarks>
public sealed class InputIntentTests
{
    private const string ActionsTypeName = "TheThingBelow.Game.Ui.InputActions";
    private const string GameScripts = "TheThingBelow.Game/scripts";

    /// <summary>
    /// The members of `Input` that read the state of a device. A call of one of them in Game
    /// is a poll, and F-50 refuses it.
    /// </summary>
    private static readonly string[] PollCalls =
    [
        "Input.IsActionPressed",
        "Input.IsActionJustPressed",
        "Input.IsActionJustReleased",
        "Input.GetActionStrength",
        "Input.GetAxis",
        "Input.GetVector",
        "Input.IsKeyPressed",
        "Input.IsPhysicalKeyPressed",
        "Input.IsJoyButtonPressed",
        "Input.GetJoyAxis",
        "Input.IsMouseButtonPressed",
    ];

    [Theory]
    [InlineData("step_north", "intent.move_north")]
    [InlineData("step_south", "intent.move_south")]
    [InlineData("step_east", "intent.move_east")]
    [InlineData("step_west", "intent.move_west")]
    [InlineData("confirm", "intent.confirm")]
    [InlineData("cancel", "intent.cancel")]
    public void EachActionMakesItsIntent(string action, string intent)
    {
        // D-493. Core reads intents alone, so an action name never reaches a run record.
        Assert.Equal(intent, IntentOf(action, false).Value);
        Assert.Equal(intent, IntentOf(action, true).Value);
    }

    [Fact]
    public void TheMenuActionOpensTheMenuAndClosesIt()
    {
        // D-162, D-650. One button opens the menu and closes it, and the menu pauses the world.
        Assert.Equal(IntentIds.OpenMenu.Value, IntentOf("menu", false).Value);
        Assert.Equal(IntentIds.CloseMenu.Value, IntentOf("menu", true).Value);
    }

    [Fact]
    public void AnUnknownActionFails()
    {
        // T-2. A name with no intent is an error, and never a silent step of nothing.
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => IntentOf("fly", false));

        Assert.IsType<ArgumentException>(thrown.InnerException);
        Assert.Contains("step_north", thrown.InnerException!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryActionOfTheMapMakesAnIntent()
    {
        // Each action of the input map reaches a rule of Core, so no action goes nowhere.
        foreach (string action in Names())
        {
            Assert.True(ContentId.IsWellFormed(IntentOf(action, false).Value));
        }
    }

    [Fact]
    public void NoIntentComesFromAPollOfTheInputSingleton()
    {
        // Exit test 6. Game makes each intent from an input event (F-50).
        foreach (string file in GameSourceFiles())
        {
            string text = File.ReadAllText(file);
            foreach (string call in PollCalls)
            {
                Assert.False(
                    text.Contains(call, StringComparison.Ordinal),
                    $"The file '{file}' calls '{call}'. Game makes each intent from an input "
                    + "event, and never from a poll of the input singleton (F-50, D-493).");
            }
        }
    }

    [Fact]
    public void NoCallSiteOfTheInputMapPassesAConstantMenuState()
    {
        // The regression test of P1-1 of `docs/reviews/pr-41.md`. A caller that passed a
        // constant sent `intent.open_menu` for every press of the menu action, so the player
        // could not leave the menu with its own button (D-162, D-650, T-2). The run reads its
        // own menu state, and `GameRunTests` proves the two intents through a real run.
        foreach (string file in GameSourceFiles())
        {
            string text = File.ReadAllText(file);
            foreach (string constant in new[] { "IntentOf(action, false)", "IntentOf(action, true)" })
            {
                Assert.False(
                    text.Contains(constant, StringComparison.Ordinal),
                    $"The file '{file}' calls '{constant}'. The menu state comes from the run, "
                    + "and never from a constant at the call site (D-162, D-650).");
            }
        }
    }

    [Fact]
    public void TheGameSourceFolderHoldsFiles()
    {
        // A scan of an empty list passes every rule, so the scan proves that it read files.
        Assert.NotEmpty(GameSourceFiles());
    }

    private static IReadOnlyList<string> GameSourceFiles() =>
        Directory.GetFiles(RepositoryRoot.PathTo(GameScripts), "*.cs", SearchOption.AllDirectories);

    private static IReadOnlyList<string> Names() =>
        (IReadOnlyList<string>)GameAssemblyFile.Type(ActionsTypeName)
            .GetProperty("Names")!
            .GetValue(null)!;

    private static ContentId IntentOf(string action, bool menuOpen) =>
        (ContentId)GameAssemblyFile.Type(ActionsTypeName)
            .GetMethod("IntentOf")!
            .Invoke(null, [action, menuOpen])!;
}
