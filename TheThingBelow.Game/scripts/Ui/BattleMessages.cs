using System;
using System.Collections.Generic;
using System.Globalization;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>One line of the battle screen: a string id and the value of each place in it (G-7).</summary>
/// <param name="Id">The string id, such as `battle.hit`.</param>
/// <param name="Values">The value of each place, by its name, such as `target`.</param>
public sealed record BattleLine(ContentId Id, IReadOnlyDictionary<string, string> Values);

/// <summary>
/// The message line of each battle event, in the game voice, from the string table (D-213,
/// G-7, G-20). The text helper puts each line on screen (D-499).
/// </summary>
/// <remarks>
/// A name comes from the string table under the id `name.` and the name part of the content
/// id, so `enemy.fixture_grunt` reads `name.fixture_grunt`. A status reads `status.` and the
/// name of the status. A test proves that every id that this type gives exists, and that
/// each line holds the limit of 40 characters with the longest names (D-241).
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public static class BattleMessages
{
    /// <summary>The kind of the string id of a name, such as `name.marrek`.</summary>
    public const string NameKind = "name";

    /// <summary>The kind of the string id of the name of a status, such as `status.poison`.</summary>
    public const string StatusNameKind = "status";

    /// <summary>The place of the combatant that acts, in a message.</summary>
    public const string ActorPlace = "actor";

    /// <summary>The place of the target, in a message.</summary>
    public const string TargetPlace = "target";

    /// <summary>The place of the amount of damage or of health, in a message.</summary>
    public const string AmountPlace = "amount";

    /// <summary>The place of the name of a status, in a message.</summary>
    public const string StatusPlace = "status";

    /// <summary>
    /// Gives the line of one event, or no value for a turn, a win, or the summary. A turn changes only who
    /// acts, a win keeps the line of the last event on screen, and the summary shows above each head (D-835, D-975).
    /// </summary>
    /// <param name="played">The event.</param>
    /// <param name="view">The view, which gives the row of a step after the event.</param>
    /// <param name="strings">The string table, which gives each name.</param>
    /// <returns>The line, or no value for a turn, a win, or the summary.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The table holds no name of a combatant or of a status (T-2).</exception>
    public static BattleLine? Of(BattleEvent played, BattleView view, StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(played);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(strings);

        return played.Kind switch
        {
            BattleEventKind.Turn => null,

            // A win shows no line of its own. The last line of the fight stands, and the text of
            // the summary rises above each head. PR-13 and PR-65 add the loot as lines (D-835, D-975).
            BattleEventKind.Won => null,
            BattleEventKind.Experience => null,
            BattleEventKind.LevelUp => null,
            BattleEventKind.Started => Line("battle.started"),
            BattleEventKind.Hit => Line(HitIdOf(played.Affinity), Target(played, view, strings), Amount(played)),
            BattleEventKind.Miss => Line("battle.miss", Actor(played, view, strings)),
            BattleEventKind.Absorb => Line("battle.absorb", Target(played, view, strings), Amount(played)),
            BattleEventKind.Defend => Line("battle.defend", Actor(played, view, strings)),
            BattleEventKind.Step => Line(StepIdOf(view.At(played.Actor).Row), Actor(played, view, strings)),
            BattleEventKind.Item => Line("battle.item", Target(played, view, strings), Amount(played)),
            BattleEventKind.Heal => Line("battle.heal", Actor(played, view, strings), Target(played, view, strings), Amount(played)),
            BattleEventKind.FleeFailed => Line("battle.flee_failed"),
            BattleEventKind.Down => Line(DownIdOf(played.Actor.Side), Actor(played, view, strings)),
            BattleEventKind.StepIn => Line("battle.step_in", Actor(played, view, strings)),
            BattleEventKind.Fled => Line("battle.fled"),
            BattleEventKind.Wiped => Line("battle.wiped"),
            BattleEventKind.StatusOn => Line(StatusOnIdOf(StatusOf(played)), Actor(played, view, strings)),
            BattleEventKind.StatusOff => Line("battle.status_off", Actor(played, view, strings), Status(played, strings)),
            BattleEventKind.Immune => Line("battle.immune", Actor(played, view, strings), Status(played, strings)),
            BattleEventKind.StatusHurt => Line("battle.status_hurt", Actor(played, view, strings), Amount(played), Status(played, strings)),
            BattleEventKind.StatusHeal => Line("battle.status_heal", Actor(played, view, strings), Amount(played)),
            BattleEventKind.Asleep => Line("battle.asleep", Actor(played, view, strings)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(played), played.Kind, $"The battle event '{played.Kind}' has no message line (G-20, T-2)."),
        };
    }

    /// <summary>Gives the string id of the name of a combatant or of an item, such as `name.marrek`.</summary>
    /// <param name="id">The content id, such as `character.marrek`.</param>
    /// <returns>The string id.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public static ContentId NameIdOf(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return ContentId.Parse($"{NameKind}.{id.Name}", StringTable.Path, id.Value);
    }

    /// <summary>Gives the string id of the name of a status, such as `status.poison`.</summary>
    /// <param name="status">The status.</param>
    /// <returns>The string id.</returns>
    public static ContentId StatusIdOf(StatusKind status) =>
        ContentId.Parse($"{StatusNameKind}.{Statuses.NameOf(status)}", StringTable.Path, "status");

    /// <summary>Gives the string id of the line of a status that lands, such as `battle.on_poison` (G-20).</summary>
    /// <param name="status">The status.</param>
    /// <returns>The string id.</returns>
    public static ContentId StatusOnIdOf(StatusKind status) =>
        ContentId.Parse($"battle.on_{Statuses.NameOf(status)}", StringTable.Path, "status");

    /// <summary>Gives the string id of the line of a hit, which names a weak spot or a resist (D-794).</summary>
    /// <param name="affinity">The affinity of the target to the element of the blow.</param>
    /// <returns>The string id.</returns>
    public static ContentId HitIdOf(Affinity affinity) => IdOf(affinity switch
    {
        Affinity.Weak => "battle.hit_weak",
        Affinity.Resist => "battle.hit_resist",
        _ => "battle.hit",
    });

    /// <summary>Gives the string id of the line of a step, by the row that the actor reached (D-380).</summary>
    /// <param name="row">The row after the step.</param>
    /// <returns>The string id.</returns>
    public static ContentId StepIdOf(BattleRow row) =>
        IdOf(row == BattleRow.Back ? "battle.step_back" : "battle.step_front");

    /// <summary>Gives the string id of the line of a fall, which reads another way for a character (D-36).</summary>
    /// <param name="side">The side of the combatant that went down.</param>
    /// <returns>The string id.</returns>
    public static ContentId DownIdOf(BattleSide side) =>
        IdOf(side == BattleSide.Party ? "battle.down_party" : "battle.down_enemy");

    private static ContentId IdOf(string id) => ContentId.Parse(id, StringTable.Path, "battle line");

    private static BattleLine Line(string id, params KeyValuePair<string, string>[] values) => Line(IdOf(id), values);

    private static BattleLine Line(ContentId id, params KeyValuePair<string, string>[] values)
    {
        var filled = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> value in values)
        {
            filled.Add(value.Key, value.Value);
        }

        return new BattleLine(id, filled);
    }

    private static KeyValuePair<string, string> Actor(BattleEvent played, BattleView view, StringTable strings) =>
        new(ActorPlace, strings.Text(NameIdOf(view.At(played.Actor).Id)));

    private static KeyValuePair<string, string> Target(BattleEvent played, BattleView view, StringTable strings)
    {
        BattleTarget target = played.Target ?? throw new ArgumentException(
            $"The battle event '{BattleEvents.NameOf(played.Kind)}' of {played.Actor.Describe()} holds no target (T-2).",
            nameof(played));

        return new(TargetPlace, strings.Text(NameIdOf(view.At(target).Id)));
    }

    private static KeyValuePair<string, string> Amount(BattleEvent played) =>
        new(AmountPlace, played.Amount.ToString(CultureInfo.InvariantCulture));

    private static KeyValuePair<string, string> Status(BattleEvent played, StringTable strings) =>
        new(StatusPlace, strings.Text(StatusIdOf(StatusOf(played))));

    private static StatusKind StatusOf(BattleEvent played) =>
        played.Status ?? throw new ArgumentException(
            $"The battle event '{BattleEvents.NameOf(played.Kind)}' of {played.Actor.Describe()} holds no status (T-2).",
            nameof(played));
}
