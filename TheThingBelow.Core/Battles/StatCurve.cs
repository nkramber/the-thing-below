using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>The five stats of one character level (D-537, D-966).</summary>
/// <param name="Health">The full health.</param>
/// <param name="Mp">The full MP (D-42).</param>
/// <param name="Attack">The attack, which the damage reads (D-771).</param>
/// <param name="Defense">The defense, which the damage reads (D-771).</param>
/// <param name="Speed">The speed, which each push and each tie reads (D-768, D-769).</param>
public sealed record StatRow(int Health, int Mp, int Attack, int Defense, int Speed);

/// <summary>One character: the start row, the join level, and the stat curve (D-363, D-537, D-765, D-966).</summary>
/// <param name="Id">The id, of the kind `character`.</param>
/// <param name="Row">The row at the start of a run (D-558).</param>
/// <param name="JoinLevel">The level at which the character joins the party (D-363).</param>
/// <param name="Curve">One row for each level, from level 1 to <see cref="StatCurve.HighestLevel"/> (D-966, D-972).</param>
public sealed record CharacterRecord(ContentId Id, BattleRow Row, int JoinLevel, IReadOnlyList<StatRow> Curve)
{
    /// <summary>Gives the stats of one level (D-966).</summary>
    /// <param name="level">The level, from 1 to <see cref="StatCurve.HighestLevel"/>.</param>
    /// <returns>The row of the level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The level is outside the curve (T-2).</exception>
    public StatRow At(int level)
    {
        if (level < 1 || level > this.Curve.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                $"the curve of '{this.Id.Value}' holds the levels 1 to {this.Curve.Count} (D-966)");
        }

        return this.Curve[level - 1];
    }
}

/// <summary>The reader of the stat curve of a character in the fixture file (D-966, D-972).</summary>
public static class StatCurve
{
    /// <summary>The highest character level, which is the row count of each curve (D-972).</summary>
    public const int HighestLevel = 40;

    /// <summary>The most health or MP of a level, which the bottom line of a battle can show (D-981).</summary>
    public const int MostPool = 999;

    /// <summary>
    /// Reads a curve: one row for each level, in order, and no stat that falls from one level to
    /// the next (D-966). Each number is a whole number, so a fraction fails the read (G-2, D-169).
    /// </summary>
    /// <param name="reader">The reader, at the array of the curve.</param>
    /// <returns>The rows, from level 1 to <see cref="HighestLevel"/>.</returns>
    /// <exception cref="ContentException">A row is absent, out of order, out of its range, or lower than the row before it (T-2).</exception>
    internal static List<StatRow> Read(ref ContentReader reader)
    {
        List<StatRow> rows = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, rows.Count))
        {
            StatRow row = ReadRow(ref reader, rows.Count + 1);
            if (rows.Count > 0)
            {
                RefuseFall(ref reader, rows[^1], row, rows.Count + 1);
            }

            rows.Add(row);
        }

        if (rows.Count != HighestLevel)
        {
            throw reader.Refuse($"the curve holds {rows.Count} rows, and a curve holds one row for each level from 1 to {HighestLevel} (D-966, D-972)");
        }

        return rows;
    }

    /// <summary>
    /// Gives the JSON text of a flat curve, where every level holds the same row. The inline
    /// fixtures of the identity set, of the cost tool, and of the tests take it, so their fights
    /// keep one set of stats at every level (D-966).
    /// </summary>
    /// <param name="row">The stats of every level.</param>
    /// <returns>The JSON array of the curve.</returns>
    /// <exception cref="ArgumentNullException">The row is null (T-2).</exception>
    public static string FlatText(StatRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var text = new System.Text.StringBuilder("[");
        for (int level = 1; level <= HighestLevel; level += 1)
        {
            text.Append(System.Globalization.CultureInfo.InvariantCulture, $"{(level > 1 ? ", " : string.Empty)}{{ \"level\": {level}, \"health\": {row.Health}, \"mp\": {row.Mp}, \"attack\": {row.Attack}, \"defense\": {row.Defense}, \"speed\": {row.Speed} }}");
        }

        return text.Append(']').ToString();
    }

    private static StatRow ReadRow(ref ContentReader reader, int expectedLevel)
    {
        int? level = null;
        int? health = null;
        int? mp = null;
        int? attack = null;
        int? defense = null;
        int? speed = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "level":
                    level = reader.ReadInt();
                    break;
                case "health":
                    health = ReadPool(ref reader, 1);
                    break;
                case "mp":
                    mp = ReadPool(ref reader, 0);
                    break;
                case "attack":
                    attack = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "defense":
                    defense = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "speed":
                    speed = BattleFixture.ReadStat(ref reader, 1);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        int named = reader.RequireInt(level, depth, "level");
        if (named != expectedLevel)
        {
            throw reader.RefuseField(depth, "level", $"the row names level {named}, and the row in this place is level {expectedLevel}. Each level has one row, in order (D-966)");
        }

        return new StatRow(
            reader.RequireInt(health, depth, "health"),
            reader.RequireInt(mp, depth, "mp"),
            reader.RequireInt(attack, depth, "attack"),
            reader.RequireInt(defense, depth, "defense"),
            reader.RequireInt(speed, depth, "speed"));
    }

    private static int ReadPool(ref ContentReader reader, int lowest)
    {
        int value = reader.ReadInt();
        if (value < lowest || value > MostPool)
        {
            throw reader.Refuse($"the value {value} is outside {lowest} to {MostPool}, which the bottom line of a battle can show (D-981)");
        }

        return value;
    }

    private static void RefuseFall(ref ContentReader reader, StatRow before, StatRow row, int level)
    {
        (string Name, int Before, int Now)[] stats =
        [
            ("health", before.Health, row.Health),
            ("mp", before.Mp, row.Mp),
            ("attack", before.Attack, row.Attack),
            ("defense", before.Defense, row.Defense),
            ("speed", before.Speed, row.Speed),
        ];
        foreach ((string name, int was, int now) in stats)
        {
            if (now < was)
            {
                throw reader.Refuse($"the {name} falls from {was} at level {level - 1} to {now} at level {level}, and no stat falls as a level rises (D-966)");
            }
        }
    }
}
