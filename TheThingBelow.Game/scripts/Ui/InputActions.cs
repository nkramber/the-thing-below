using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The actions of the input map, and the intent that each one makes (D-84, D-493, D-561).
/// Core reads intents alone, so an action name never reaches a run record (D-214).
/// </summary>
/// <remarks>
/// Game makes each intent from an input event, and never from a poll of the input singleton.
/// A poll ignores what a menu already took, and it would make a second intent for one press
/// (F-50). <see cref="GameInputMap"/> holds the keys and the buttons of each action.
/// <para>
/// The menu action and the map action each make two intents, because one button opens the
/// menu or the map screen and closes it (D-162, D-650, D-986). Every other action makes one.
/// </para>
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public static class InputActions
{
    /// <summary>The action of one step to the north.</summary>
    public const string StepNorth = "step_north";

    /// <summary>The action of one step to the south.</summary>
    public const string StepSouth = "step_south";

    /// <summary>The action of one step to the east.</summary>
    public const string StepEast = "step_east";

    /// <summary>The action of one step to the west.</summary>
    public const string StepWest = "step_west";

    /// <summary>The action that chooses the thing under the cursor.</summary>
    public const string Confirm = "confirm";

    /// <summary>The action that goes back one step.</summary>
    public const string Cancel = "cancel";

    /// <summary>The action that opens the menu, and that closes it (D-162).</summary>
    public const string Menu = "menu";

    /// <summary>The action that opens the dungeon map screen from the walk, and that closes it (D-986).</summary>
    public const string Map = "map";

    /// <summary>The action that holds the torch out on the walk, and that puts it away (D-1064, D-1068).</summary>
    public const string Torch = "torch";

    private static readonly string[] AllStepNames = [StepNorth, StepSouth, StepEast, StepWest];

    private static readonly string[] AllNames =
    [
        StepNorth,
        StepSouth,
        StepEast,
        StepWest,
        Confirm,
        Cancel,
        Menu,
        Map,
        Torch,
    ];

    /// <summary>Every action of the input map, in a fixed order.</summary>
    public static IReadOnlyList<string> Names => AllNames;

    /// <summary>The four actions that move the party, in a fixed order (D-716).</summary>
    /// <remarks>
    /// The party walks while a direction is held, so <see cref="HeldSteps"/> reads the press
    /// and the release of each one (F-50).
    /// </remarks>
    public static IReadOnlyList<string> StepNames => AllStepNames;

    /// <summary>Tells whether one action moves the party (D-716).</summary>
    /// <param name="action">The name of the action, such as `step_north`.</param>
    /// <returns>True when the action is one of the four steps.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public static bool IsStep(string action)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        foreach (string name in AllStepNames)
        {
            if (string.CompareOrdinal(name, action) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the intent that one action makes.</summary>
    /// <param name="action">The name of the action, such as `confirm`.</param>
    /// <param name="menuOpen">True while a menu is open, which the menu action reads.</param>
    /// <param name="torchHeld">True while the party holds the torch out, which the torch action reads (D-1064).</param>
    /// <returns>The id of the intent that Core reads.</returns>
    /// <exception cref="ArgumentException">The name is not an action of the input map (T-2).</exception>
    public static ContentId IntentOf(string action, bool menuOpen, bool torchHeld)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        return action switch
        {
            StepNorth => IntentIds.MoveNorth,
            StepSouth => IntentIds.MoveSouth,
            StepEast => IntentIds.MoveEast,
            StepWest => IntentIds.MoveWest,
            Confirm => IntentIds.Confirm,
            Cancel => IntentIds.Cancel,
            Menu or Map => menuOpen ? IntentIds.CloseMenu : IntentIds.OpenMenu,
            Torch => torchHeld ? IntentIds.PutTorchAway : IntentIds.HoldTorch,
            _ => throw new ArgumentException(
                $"The name '{action}' is not an action of the input map. The actions are {Describe()} (T-2).",
                nameof(action)),
        };
    }

    /// <summary>Gives every action as one line, for an error message (T-2).</summary>
    /// <returns>The names, separated by a comma and a space.</returns>
    public static string Describe() => string.Join(", ", AllNames);
}
