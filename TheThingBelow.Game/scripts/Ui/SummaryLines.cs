using System;
using System.Collections.Generic;
using System.Globalization;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The lines that rise above the head of a character after a won fight: the experience, then
/// "Level up!" and one line for each stat that rose (D-975, D-978, D-979).
/// </summary>
/// <remarks>
/// The stat lines follow the order health, MP, attack, defense, speed, and a stat that did not
/// rise shows no line (D-975). A character with no experience gets no event, so it shows no
/// line. This type holds no Godot value, so a test reads it from the built Game assembly with
/// no engine (D-614).
/// </remarks>
public static class SummaryLines
{
    /// <summary>The place of the name of a stat, such as `ATK`, in a stat line.</summary>
    public const string StatPlace = "stat";

    /// <summary>Gives the lines of one event of the summary, from the top line down, or none for any other event.</summary>
    /// <param name="played">The event.</param>
    /// <param name="shown">The character of the event, after the view applied the event.</param>
    /// <param name="strings">The string table, which gives the name of each stat (D-979).</param>
    /// <returns>The lines. An experience gives one, and a level-up gives the level and each stat that rose.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">A level-up names a combatant that holds no curve (T-2).</exception>
    /// <exception cref="ContentException">The table holds no name of a stat (T-2).</exception>
    public static IReadOnlyList<BattleLine> Of(BattleEvent played, ShownCombatant shown, StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(played);
        ArgumentNullException.ThrowIfNull(shown);
        ArgumentNullException.ThrowIfNull(strings);

        if (played.Kind == BattleEventKind.Experience)
        {
            return [Line("battle.summary_experience", (BattleMessages.AmountPlace, Text(played.Amount)))];
        }

        if (played.Kind != BattleEventKind.LevelUp)
        {
            return [];
        }

        CharacterRecord record = shown.Record ?? throw new InvalidOperationException(
            $"The level-up of {played.Actor.Describe()} names '{shown.Id.Value}', which holds no curve (D-966, T-2).");
        StatRow before = record.At(shown.LevelBefore);
        StatRow after = record.At(shown.Level);
        List<BattleLine> lines = [Line("battle.level_up")];
        AddGain(lines, strings, "battle.stat_hp", after.Health - before.Health);
        AddGain(lines, strings, "battle.stat_mp", after.Mp - before.Mp);
        AddGain(lines, strings, "battle.stat_atk", after.Attack - before.Attack);
        AddGain(lines, strings, "battle.stat_def", after.Defense - before.Defense);
        AddGain(lines, strings, "battle.stat_spd", after.Speed - before.Speed);
        return lines;
    }

    private static void AddGain(List<BattleLine> lines, StringTable strings, string stat, int gain)
    {
        if (gain > 0)
        {
            string name = strings.Text(ContentId.Parse(stat, StringTable.Path, "battle screen"));
            lines.Add(Line("battle.summary_gain", (BattleMessages.AmountPlace, Text(gain)), (StatPlace, name)));
        }
    }

    private static BattleLine Line(string id, params (string Place, string Value)[] values)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string place, string value) in values)
        {
            map.Add(place, value);
        }

        return new BattleLine(ContentId.Parse(id, StringTable.Path, "battle screen"), map);
    }

    private static string Text(int number) => number.ToString(CultureInfo.InvariantCulture);
}
