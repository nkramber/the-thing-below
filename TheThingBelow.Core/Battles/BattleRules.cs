using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The numbers of the battle rules: the delay of each action, the rolls, and the rates
/// (D-757, D-777). The file is `content/rules/battle.json`.
/// </summary>
/// <remarks>
/// Every delay counts ticks at speed 100, and every rate is in basis points (D-169, D-768).
/// PR-30 tunes each number with no code change (D-757, M-4). The delay of an item lives with
/// the item in the fixture file, because each item holds its own delay (D-757).
/// </remarks>
public sealed class BattleRules
{
    /// <summary>The path of the file under the content folder (D-757).</summary>
    public const string Path = "rules/battle.json";

    /// <summary>The largest delay, push, or stun, which keeps every sum of the timeline inside a `long` (T-2).</summary>
    public const int MostTicks = 100_000;

    /// <summary>The largest power or multiplier in basis points (D-169).</summary>
    public const int MostRate = 100_000;

    // The names of every field, in the order of the file. One list serves the read and the
    // error of an unknown field, so the two never differ (T-1).
    private static readonly string[] Fields =
    [
        "attack_delay",
        "attack_power",
        "defend_delay",
        "step_delay",
        "flee_delay",
        "hit_low",
        "hit_high",
        "miss_base",
        "miss_per_speed",
        "miss_floor",
        "miss_ceiling",
        "defend_cut",
        "back_row_rate",
        "haste_rate",
        "slow_rate",
        "stun_ticks",
        "flee_base",
        "flee_per_speed",
        "flee_floor",
        "flee_ceiling",
        "item_rate",
    ];

    private BattleRules(SortedDictionary<string, int> numbers)
    {
        this.AttackDelay = numbers["attack_delay"];
        this.AttackPower = numbers["attack_power"];
        this.DefendDelay = numbers["defend_delay"];
        this.StepDelay = numbers["step_delay"];
        this.FleeDelay = numbers["flee_delay"];
        this.HitLow = numbers["hit_low"];
        this.HitHigh = numbers["hit_high"];
        this.MissBase = numbers["miss_base"];
        this.MissPerSpeed = numbers["miss_per_speed"];
        this.MissFloor = numbers["miss_floor"];
        this.MissCeiling = numbers["miss_ceiling"];
        this.DefendCut = numbers["defend_cut"];
        this.BackRowRate = numbers["back_row_rate"];
        this.HasteRate = numbers["haste_rate"];
        this.SlowRate = numbers["slow_rate"];
        this.StunTicks = numbers["stun_ticks"];
        this.FleeBase = numbers["flee_base"];
        this.FleePerSpeed = numbers["flee_per_speed"];
        this.FleeFloor = numbers["flee_floor"];
        this.FleeCeiling = numbers["flee_ceiling"];
        this.ItemRate = numbers["item_rate"];
    }

    /// <summary>The delay of the basic attack, in ticks at speed 100 (D-359, D-757).</summary>
    public int AttackDelay { get; }

    /// <summary>The power of the basic attack, in basis points (D-771).</summary>
    public int AttackPower { get; }

    /// <summary>The delay of a defend (D-755).</summary>
    public int DefendDelay { get; }

    /// <summary>The delay of a row step, a light action (D-380).</summary>
    public int StepDelay { get; }

    /// <summary>The delay of a flee, which a failed flee costs (D-378).</summary>
    public int FleeDelay { get; }

    /// <summary>The lowest factor of a hit, in basis points (D-772).</summary>
    public int HitLow { get; }

    /// <summary>The highest factor of a hit, in basis points (D-772).</summary>
    public int HitHigh { get; }

    /// <summary>The miss chance of two combatants of one speed (D-773).</summary>
    public int MissBase { get; }

    /// <summary>The miss chance that each point of the speed gap adds or takes away (D-773).</summary>
    public int MissPerSpeed { get; }

    /// <summary>The lowest miss chance (D-773).</summary>
    public int MissFloor { get; }

    /// <summary>The highest miss chance (D-773).</summary>
    public int MissCeiling { get; }

    /// <summary>The part of the damage that a defend cuts (D-755).</summary>
    public int DefendCut { get; }

    /// <summary>The rate of a melee attack from the back row (D-779).</summary>
    public int BackRowRate { get; }

    /// <summary>The rate that haste puts on each push (D-768).</summary>
    public int HasteRate { get; }

    /// <summary>The rate that slow puts on each push (D-768).</summary>
    public int SlowRate { get; }

    /// <summary>The ticks that a stun adds to the next turn of its target (D-376).</summary>
    public int StunTicks { get; }

    /// <summary>The flee chance of two sides of one average speed (D-763).</summary>
    public int FleeBase { get; }

    /// <summary>The flee chance that each point of the speed gap adds or takes away (D-763).</summary>
    public int FleePerSpeed { get; }

    /// <summary>The lowest flee chance (D-763).</summary>
    public int FleeFloor { get; }

    /// <summary>The highest flee chance (D-763).</summary>
    public int FleeCeiling { get; }

    /// <summary>The rate of the effect of an item in battle (D-382).</summary>
    public int ItemRate { get; }

    /// <summary>Reads the rules file, and checks every number (T-2).</summary>
    /// <param name="bytes">The bytes of the file.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The rules.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, repeated, or out of its range (G-6).</exception>
    public static BattleRules Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        var numbers = new SortedDictionary<string, int>(StringComparer.Ordinal);
        string? comment = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (string.CompareOrdinal(field, "comment") == 0)
            {
                comment = reader.ReadString();
            }
            else if (Array.IndexOf(Fields, field) >= 0)
            {
                numbers.Add(field, reader.ReadInt());
            }
            else
            {
                throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        foreach (string field in Fields)
        {
            if (!numbers.ContainsKey(field))
            {
                _ = reader.RequireInt(null, depth, field);
            }
        }

        reader.ReadFileEnd();
        CheckRanges(numbers, file);
        return new BattleRules(numbers);
    }

    private static void CheckRanges(SortedDictionary<string, int> numbers, string file)
    {
        CheckRange(numbers, file, "attack_delay", 1, MostTicks);
        CheckRange(numbers, file, "attack_power", 1, MostRate);
        CheckRange(numbers, file, "defend_delay", 1, MostTicks);
        CheckRange(numbers, file, "step_delay", 1, MostTicks);
        CheckRange(numbers, file, "flee_delay", 1, MostTicks);
        CheckRange(numbers, file, "hit_low", 1, MostRate);
        CheckRange(numbers, file, "hit_high", numbers["hit_low"], MostRate);
        CheckRange(numbers, file, "miss_base", 0, BasisPoints.One);
        CheckRange(numbers, file, "miss_per_speed", 0, BasisPoints.One);
        CheckRange(numbers, file, "miss_floor", 0, BasisPoints.One);
        CheckRange(numbers, file, "miss_ceiling", numbers["miss_floor"], BasisPoints.One);
        CheckRange(numbers, file, "defend_cut", 0, BasisPoints.One);
        CheckRange(numbers, file, "back_row_rate", 0, BasisPoints.One);
        CheckRange(numbers, file, "haste_rate", 1, MostRate);
        CheckRange(numbers, file, "slow_rate", 1, MostRate);
        CheckRange(numbers, file, "stun_ticks", 0, MostTicks);
        CheckRange(numbers, file, "flee_base", 0, BasisPoints.One);
        CheckRange(numbers, file, "flee_per_speed", 0, BasisPoints.One);
        CheckRange(numbers, file, "flee_floor", 0, BasisPoints.One);
        CheckRange(numbers, file, "flee_ceiling", numbers["flee_floor"], BasisPoints.One);
        CheckRange(numbers, file, "item_rate", 0, BasisPoints.One);
    }

    private static void CheckRange(SortedDictionary<string, int> numbers, string file, string field, int lowest, int highest)
    {
        int value = numbers[field];
        if (value < lowest || value > highest)
        {
            throw ContentException.ForField(
                file,
                field,
                $"the value {value} is outside {lowest} to {highest} (D-169, D-777)");
        }
    }
}
