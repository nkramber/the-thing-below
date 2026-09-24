using System;
using TheThingBelow.Core.Content;

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

    /// <summary>A character used a heal item. The ability names the item, and the amount is the health restored (D-382, D-1046).</summary>
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

    /// <summary>An enemy or a lesson healed an ally with a move. The amount is the health restored (D-955, D-1029).</summary>
    Heal,

    /// <summary>A character earned experience from a battle won. The amount is the experience (D-34, D-975).</summary>
    Experience,

    /// <summary>A character reached a new level, which filled its health and its MP. The amount is the new level. PR-70 plays the sting (D-422, D-973).</summary>
    LevelUp,

    /// <summary>A character used a form of a lesson. The ability names the form, the target is the target, and the amount is the MP spent (D-1027, D-1032).</summary>
    Lesson,

    /// <summary>A lesson opened a new form for a character after a battle won. The ability names the form, and the amount is the points of the lesson (D-539, D-1019).</summary>
    FormOpened,

    /// <summary>A character used a restore item. The ability names the item, and the amount is the MP restored (D-1046).</summary>
    ItemMp,

    /// <summary>A character used a cure item. The ability names the item. A status off event follows for each status that ended (D-1046).</summary>
    ItemCure,

    /// <summary>A character used a revive item, and the fallen target stood up. The ability names the item, and the amount is the health (D-36, D-1046).</summary>
    Revive,

    /// <summary>A steal took an item. The target is the enemy, and the ability names the item (D-383, D-1044).</summary>
    StealItem,

    /// <summary>A steal took gold. The target is the enemy, and the amount is the gold (D-1043, D-1044).</summary>
    StealGold,

    /// <summary>A steal failed, and it cost the turn (D-949).</summary>
    StealFailed,

    /// <summary>A steal found no entry left on the enemy, and it cost the turn (D-1044).</summary>
    StealEmpty,

    /// <summary>A steal found an item over its stack limit, which stayed with the enemy. The ability names the item (D-1044).</summary>
    StealFull,

    /// <summary>A fallen enemy dropped an item at a win. The actor is the enemy, and the ability names the item (D-1042).</summary>
    Drop,

    /// <summary>A fallen enemy dropped an item over its stack limit, which left the game. The actor is the enemy, and the ability names the item (D-1042).</summary>
    DropLost,
}

/// <summary>One event of a battle (D-168, D-532).</summary>
/// <param name="Kind">What happened.</param>
/// <param name="Actor">The combatant that the event is about.</param>
/// <param name="Target">The target of a strike or an item, and no value for the other kinds.</param>
/// <param name="Amount">The damage or the health, and zero for the other kinds.</param>
/// <param name="Status">The status of a status event, and no value for the other kinds (D-75).</param>
/// <param name="Affinity">The affinity of the target of a hit or an absorb, and `normal` for the other kinds (D-794).</param>
/// <param name="Ability">The form of a lesson event or a form event, or the item of an item, steal, or drop event, and no value for the other kinds (D-1027, D-1046).</param>
public sealed record BattleEvent(BattleEventKind Kind, BattleTarget Actor, BattleTarget? Target, int Amount, StatusKind? Status = null, Affinity Affinity = Affinity.Normal, ContentId? Ability = null)
{
    /// <summary>Gives the event as one text, for a log line of Game (D-767).</summary>
    /// <returns>The kind, the actor, the target, and the amount, then the status and an affinity other than `normal`.</returns>
    public string Describe()
    {
        string target = this.Target is BattleTarget aimed ? $" at {aimed.Describe()}" : string.Empty;
        string amount = this.Amount != 0 ? $" for {this.Amount}" : string.Empty;
        string status = this.Status is StatusKind held ? $" ({Statuses.NameOf(held)})" : string.Empty;
        string affinity = this.Affinity != Affinity.Normal ? $" ({Elements.NameOf(this.Affinity)})" : string.Empty;
        string ability = this.Ability is ContentId form ? $" [{form.Value}]" : string.Empty;
        return $"{BattleEvents.NameOf(this.Kind)}: {this.Actor.Describe()}{ability}{target}{amount}{status}{affinity}";
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
        BattleEventKind.Experience => "experience",
        BattleEventKind.LevelUp => "level up",
        BattleEventKind.Lesson => "lesson",
        BattleEventKind.FormOpened => "form opened",
        BattleEventKind.ItemMp => "item mp",
        BattleEventKind.ItemCure => "item cure",
        BattleEventKind.Revive => "revive",
        BattleEventKind.StealItem => "steal item",
        BattleEventKind.StealGold => "steal gold",
        BattleEventKind.StealFailed => "steal failed",
        BattleEventKind.StealEmpty => "steal empty",
        BattleEventKind.StealFull => "steal full",
        BattleEventKind.Drop => "drop",
        BattleEventKind.DropLost => "drop lost",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no battle event (D-532)"),
    };
}
