using System;

namespace TheThingBelow.Core.Battles;

/// <summary>The kinds of the events of a battle, which Game queues and plays in order (D-168, D-532).</summary>
public enum BattleEventKind
{
    /// <summary>The encounter became a battle.</summary>
    Started,

    /// <summary>A character takes the turn, and Game takes the next command (D-532).</summary>
    Turn,

    /// <summary>A strike hit. The amount is the damage. The affinity of the target names the rate (D-809).</summary>
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

    /// <summary>A strike of an absorbed element hit. The amount is the health restored (D-795).</summary>
    Absorb,

    /// <summary>A status landed on the combatant, or reset its end (D-800).</summary>
    StatusOn,

    /// <summary>A status ended: its ticks passed, a strike woke a sleeper, or haste and slow met (D-798, D-800, D-802).</summary>
    StatusOff,

    /// <summary>A status landed on an enemy that refuses it, and did nothing (D-805).</summary>
    Immune,

    /// <summary>Poison or bleed took its share at the start of a turn. The amount is the damage (D-799, D-803).</summary>
    StatusHurt,

    /// <summary>Regen healed its share at the start of a turn. The amount is the health restored (D-799).</summary>
    StatusHeal,

    /// <summary>A turn of a sleeper passed with no action (D-802).</summary>
    Asleep,

    /// <summary>An enemy healed an ally with a move. The amount is the health restored (D-955).</summary>
    Heal,
}

/// <summary>One event of a battle (D-168, D-532).</summary>
/// <param name="Kind">What happened.</param>
/// <param name="Actor">The combatant that the event is about.</param>
/// <param name="Target">The target of a strike or an item, and no value for the other kinds.</param>
/// <param name="Amount">The damage or the health, and zero for the other kinds.</param>
/// <param name="Status">The status of a status event, and no value for the other kinds (D-75).</param>
/// <param name="Affinity">The affinity of the target of a hit or an absorb, and `normal` for the other kinds (D-794).</param>
public sealed record BattleEvent(BattleEventKind Kind, BattleTarget Actor, BattleTarget? Target, int Amount, StatusKind? Status = null, Affinity Affinity = Affinity.Normal)
{
    /// <summary>Gives the event as one text, for a log line of Game (D-767).</summary>
    /// <returns>The kind, the actor, the target, and the amount, then the status and an affinity other than `normal`.</returns>
    public string Describe()
    {
        string target = this.Target is BattleTarget aimed ? $" at {aimed.Describe()}" : string.Empty;
        string amount = this.Amount != 0 ? $" for {this.Amount}" : string.Empty;
        string status = this.Status is StatusKind held ? $" ({Statuses.NameOf(held)})" : string.Empty;
        string affinity = this.Affinity != Affinity.Normal ? $" ({Elements.NameOf(this.Affinity)})" : string.Empty;
        return $"{BattleEvents.NameOf(this.Kind)}: {this.Actor.Describe()}{target}{amount}{status}{affinity}";
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
        BattleEventKind.Absorb => "absorb",
        BattleEventKind.StatusOn => "status on",
        BattleEventKind.StatusOff => "status off",
        BattleEventKind.Immune => "immune",
        BattleEventKind.StatusHurt => "status hurt",
        BattleEventKind.StatusHeal => "status heal",
        BattleEventKind.Asleep => "asleep",
        BattleEventKind.Heal => "heal",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no battle event (D-532)"),
    };
}
