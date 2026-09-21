using System;

namespace TheThingBelow.Core.Battles;

/// <summary>The kinds of the events of a battle, which Game queues and plays in order (D-168, D-532).</summary>
public enum BattleEventKind
{
    /// <summary>The encounter became a battle.</summary>
    Started,

    /// <summary>A character takes the turn, and Game takes the next command (D-532).</summary>
    Turn,

    /// <summary>A strike hit. The amount is the damage.</summary>
    Hit,

    /// <summary>A strike missed (D-772).</summary>
    Miss,

    /// <summary>A combatant defends (D-755).</summary>
    Defend,

    /// <summary>A combatant stepped to the other row (D-380).</summary>
    Step,

    /// <summary>A character used an item. The amount is the health restored (D-382).</summary>
    Item,

    /// <summary>A flee failed, and it cost the turn (D-378).</summary>
    FleeFailed,

    /// <summary>A combatant went down (D-36).</summary>
    Down,

    /// <summary>A waiting enemy stepped onto the field (D-778).</summary>
    StepIn,

    /// <summary>The party won. Game sends the wait intent when the screen is done (D-522).</summary>
    Won,

    /// <summary>The party fled. Game sends the wait intent when the screen is done (D-522).</summary>
    Fled,

    /// <summary>Every character who fights is down, and Game reloads (D-397, D-776).</summary>
    Wiped,
}

/// <summary>One event of a battle (D-168, D-532).</summary>
/// <param name="Kind">What happened.</param>
/// <param name="Actor">The combatant that the event is about.</param>
/// <param name="Target">The target of a strike or an item, and no value for the other kinds.</param>
/// <param name="Amount">The damage or the health, and zero for the other kinds.</param>
public sealed record BattleEvent(BattleEventKind Kind, BattleTarget Actor, BattleTarget? Target, int Amount)
{
    /// <summary>Gives the event as one text, for a log line of Game (D-767).</summary>
    /// <returns>The kind, the actor, the target, and the amount.</returns>
    public string Describe()
    {
        string target = this.Target is BattleTarget aimed ? $" at {aimed.Describe()}" : string.Empty;
        string amount = this.Amount != 0 ? $" for {this.Amount}" : string.Empty;
        return $"{BattleEvents.NameOf(this.Kind)}: {this.Actor.Describe()}{target}{amount}";
    }
}

/// <summary>The names of the kinds of event (T-2).</summary>
public static class BattleEvents
{
    /// <summary>Gives the name of one kind.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `hit`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(BattleEventKind kind) => kind switch
    {
        BattleEventKind.Started => "started",
        BattleEventKind.Turn => "turn",
        BattleEventKind.Hit => "hit",
        BattleEventKind.Miss => "miss",
        BattleEventKind.Defend => "defend",
        BattleEventKind.Step => "step",
        BattleEventKind.Item => "item",
        BattleEventKind.FleeFailed => "flee failed",
        BattleEventKind.Down => "down",
        BattleEventKind.StepIn => "step in",
        BattleEventKind.Won => "won",
        BattleEventKind.Fled => "fled",
        BattleEventKind.Wiped => "wiped",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no battle event (D-532)"),
    };
}
