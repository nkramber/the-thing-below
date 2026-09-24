using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Story;

/// <summary>The kind of one step of a story scene script (D-173, D-563, D-997).</summary>
public enum SceneStepKind
{
    /// <summary>An actor walks a path of tiles, and Game ends the step (D-1000, D-1012).</summary>
    Move,

    /// <summary>An actor turns, and Game ends the step (D-1000).</summary>
    Face,

    /// <summary>A pause of a count of ticks, which Core counts (D-1000).</summary>
    Wait,

    /// <summary>A line of text, and Game ends the step (D-1000).</summary>
    Say,

    /// <summary>A choice of the player, and each option sets its flag (D-1007).</summary>
    Choose,

    /// <summary>A flag turns on (D-542).</summary>
    SetFlag,

    /// <summary>A cast member joins the party (D-563).</summary>
    Join,

    /// <summary>A cast member appears on a marker (D-1006, D-1013).</summary>
    Show,

    /// <summary>A shown cast member leaves the map (D-1006, D-1013).</summary>
    Hide,

    /// <summary>The view moves to a marker, and Game ends the step (D-1013).</summary>
    Camera,

    /// <summary>A battle against an enemy group starts, and a win lets the story scene go on (D-998, D-999).</summary>
    StartBattle,
}

/// <summary>What a step of each kind waits for before the next step runs (D-1000, D-1013).</summary>
public enum SceneStepEnd
{
    /// <summary>The step changes the run at once, and the next step runs in the same tick.</summary>
    AtOnce,

    /// <summary>Game draws the step and sends the wait intent at its end (D-540).</summary>
    WaitIntent,

    /// <summary>Core counts the ticks of the step (D-1000).</summary>
    Ticks,

    /// <summary>The player picks one option, and Game sends the pick intent (D-1007).</summary>
    Pick,

    /// <summary>A battle runs, and its win ends the step (D-999).</summary>
    Battle,
}

/// <summary>The names of the step kinds, and what each kind waits for (D-997).</summary>
public static class SceneStepKinds
{
    /// <summary>Every kind, in one fixed order for a walk of them (G-4).</summary>
    public static readonly SceneStepKind[] All =
    [
        SceneStepKind.Move,
        SceneStepKind.Face,
        SceneStepKind.Wait,
        SceneStepKind.Say,
        SceneStepKind.Choose,
        SceneStepKind.SetFlag,
        SceneStepKind.Join,
        SceneStepKind.Show,
        SceneStepKind.Hide,
        SceneStepKind.Camera,
        SceneStepKind.StartBattle,
    ];

    /// <summary>The names of every kind, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "move, face, wait, say, choose, set_flag, join, show, hide, camera, start_battle";

    /// <summary>Gives the kind of one name.</summary>
    /// <param name="name">The name, such as `set_flag`.</param>
    /// <param name="kind">The kind of that name, when the name names one.</param>
    /// <returns>True when the name names a kind.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out SceneStepKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (SceneStepKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = SceneStepKind.Wait;
        return false;
    }

    /// <summary>Gives the name of one kind, which a story scene file and an error use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `start_battle`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(SceneStepKind kind) => kind switch
    {
        SceneStepKind.Move => "move",
        SceneStepKind.Face => "face",
        SceneStepKind.Wait => "wait",
        SceneStepKind.Say => "say",
        SceneStepKind.Choose => "choose",
        SceneStepKind.SetFlag => "set_flag",
        SceneStepKind.Join => "join",
        SceneStepKind.Show => "show",
        SceneStepKind.Hide => "hide",
        SceneStepKind.Camera => "camera",
        SceneStepKind.StartBattle => "start_battle",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no story scene step kind (D-997)"),
    };

    /// <summary>Gives what a step of one kind waits for (D-1000, D-1013).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The end of the step.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    /// <remarks>
    /// Each step that Game animates waits for the wait intent, and a step that Game does not
    /// animate never waits. A show and a hide change the map at once, so a pause after one
    /// takes a wait step (D-1013).
    /// </remarks>
    public static SceneStepEnd EndOf(SceneStepKind kind) => kind switch
    {
        SceneStepKind.Move => SceneStepEnd.WaitIntent,
        SceneStepKind.Face => SceneStepEnd.WaitIntent,
        SceneStepKind.Wait => SceneStepEnd.Ticks,
        SceneStepKind.Say => SceneStepEnd.WaitIntent,
        SceneStepKind.Choose => SceneStepEnd.Pick,
        SceneStepKind.SetFlag => SceneStepEnd.AtOnce,
        SceneStepKind.Join => SceneStepEnd.AtOnce,
        SceneStepKind.Show => SceneStepEnd.AtOnce,
        SceneStepKind.Hide => SceneStepEnd.AtOnce,
        SceneStepKind.Camera => SceneStepEnd.WaitIntent,
        SceneStepKind.StartBattle => SceneStepEnd.Battle,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no story scene step kind (D-997)"),
    };
}

/// <summary>
/// The actor of a step: the party lead, or one cast member (D-1006). PR-14 adds the NPC as an
/// actor, with the NPCs themselves (D-1005).
/// </summary>
/// <param name="Character">The cast member, or no value for the party lead.</param>
public sealed record SceneActor(ContentId? Character)
{
    /// <summary>The word that names the party lead in a story scene file (D-1006).</summary>
    public const string LeadName = "lead";

    /// <summary>The party lead, whom the map always follows (D-267).</summary>
    public static readonly SceneActor Lead = new((ContentId?)null);

    /// <summary>True when the actor is the party lead.</summary>
    public bool IsLead => this.Character is null;

    /// <summary>Gives the actor as one word for an error and a log line (T-2).</summary>
    /// <returns>`lead`, or the id of the cast member.</returns>
    public string Describe() => this.Character?.Value ?? LeadName;
}

/// <summary>One step of a story scene script (D-173, D-997).</summary>
/// <param name="Kind">The kind of the step.</param>
public abstract record SceneStep(SceneStepKind Kind);

/// <summary>An actor walks one tile for each direction of the path (D-1012).</summary>
/// <param name="Actor">The actor.</param>
/// <param name="Path">One direction for each tile, one or more.</param>
public sealed record MoveStep(SceneActor Actor, IReadOnlyList<StepDirection> Path) : SceneStep(SceneStepKind.Move);

/// <summary>An actor turns to one direction (D-1000).</summary>
/// <param name="Actor">The actor.</param>
/// <param name="Facing">The direction.</param>
public sealed record FaceStep(SceneActor Actor, StepDirection Facing) : SceneStep(SceneStepKind.Face);

/// <summary>A pause, which Core counts (D-1000).</summary>
/// <param name="Ticks">The count of world ticks, above zero.</param>
public sealed record WaitStep(int Ticks) : SceneStep(SceneStepKind.Wait);

/// <summary>One line of text (D-173, G-7).</summary>
/// <param name="Speaker">The speaker, or no value for a line with no speaker, which the file writes as `none`.</param>
/// <param name="Line">The string id of the line.</param>
public sealed record SayStep(SceneActor? Speaker, ContentId Line) : SceneStep(SceneStepKind.Say);

/// <summary>One option of a choose step (D-1007).</summary>
/// <param name="Line">The string id of the option.</param>
/// <param name="Flag">The flag that a pick of this option turns on.</param>
public sealed record ChooseOption(ContentId Line, ContentId Flag);

/// <summary>A choice of the player (D-1007).</summary>
/// <param name="Options">Two options or more, in the order of the file.</param>
public sealed record ChooseStep(IReadOnlyList<ChooseOption> Options) : SceneStep(SceneStepKind.Choose);

/// <summary>A flag turns on (D-542).</summary>
/// <param name="Flag">The flag.</param>
public sealed record SetFlagStep(ContentId Flag) : SceneStep(SceneStepKind.SetFlag);

/// <summary>A cast member joins the party (D-563).</summary>
/// <param name="Character">The cast member.</param>
public sealed record JoinStep(ContentId Character) : SceneStep(SceneStepKind.Join);

/// <summary>A cast member appears on a marker of the map (D-1006).</summary>
/// <param name="Character">The cast member.</param>
/// <param name="Marker">The marker of the map.</param>
/// <param name="Facing">The direction that the cast member faces.</param>
public sealed record ShowStep(ContentId Character, ContentId Marker, StepDirection Facing) : SceneStep(SceneStepKind.Show);

/// <summary>A shown cast member leaves the map (D-1006).</summary>
/// <param name="Character">The cast member.</param>
public sealed record HideStep(ContentId Character) : SceneStep(SceneStepKind.Hide);

/// <summary>The view moves to a marker of the map (D-1013).</summary>
/// <param name="Marker">The marker of the map.</param>
public sealed record CameraStep(ContentId Marker) : SceneStep(SceneStepKind.Camera);

/// <summary>A battle against one enemy group starts (D-998).</summary>
/// <param name="Group">The enemy group.</param>
public sealed record StartBattleStep(ContentId Group) : SceneStep(SceneStepKind.StartBattle);
