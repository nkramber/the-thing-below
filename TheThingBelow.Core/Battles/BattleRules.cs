using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The numbers of the battle rules: the delay of each action, the rolls, and the rates
/// (D-757, D-777, D-808). The file is `content/rules/battle.json`.
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
        "stun_push",
        "flee_base",
        "flee_per_speed",
        "flee_floor",
        "flee_ceiling",
        "item_rate",
        "weak_rate",
        "resist_rate",
        "absorb_rate",
        "poison_share",
        "bleed_share",
        "bleed_ticks",
        "regen_share",
        "regen_ticks",
        "sleep_ticks",
        "haste_ticks",
        "slow_ticks",
        "stun_ticks",
        "shell_cut",
        "shell_ticks",
        "blind_miss",
        "experience_cut",
        "experience_gap",
    ];

    private BattleRules(SortedDictionary<string, int> numbers, IReadOnlyList<int> levelExperience)
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
        this.StunPush = numbers["stun_push"];
        this.FleeBase = numbers["flee_base"];
        this.FleePerSpeed = numbers["flee_per_speed"];
        this.FleeFloor = numbers["flee_floor"];
        this.FleeCeiling = numbers["flee_ceiling"];
        this.ItemRate = numbers["item_rate"];
        this.WeakRate = numbers["weak_rate"];
        this.ResistRate = numbers["resist_rate"];
        this.AbsorbRate = numbers["absorb_rate"];
        this.PoisonShare = numbers["poison_share"];
        this.BleedShare = numbers["bleed_share"];
        this.BleedTicks = numbers["bleed_ticks"];
        this.RegenShare = numbers["regen_share"];
        this.RegenTicks = numbers["regen_ticks"];
        this.SleepTicks = numbers["sleep_ticks"];
        this.HasteTicks = numbers["haste_ticks"];
        this.SlowTicks = numbers["slow_ticks"];
        this.StunTicks = numbers["stun_ticks"];
        this.ShellCut = numbers["shell_cut"];
        this.ShellTicks = numbers["shell_ticks"];
        this.BlindMiss = numbers["blind_miss"];
        this.ExperienceCut = numbers["experience_cut"];
        this.ExperienceGap = numbers["experience_gap"];
        this.LevelExperience = levelExperience;
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

    /// <summary>The ticks that a stun adds to the next turn of a target that holds no stun (D-376, D-810).</summary>
    public int StunPush { get; }

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

    /// <summary>The rate of a hit of an element that the target is weak to (D-794).</summary>
    public int WeakRate { get; }

    /// <summary>The rate of a hit of an element that the target resists (D-794).</summary>
    public int ResistRate { get; }

    /// <summary>The rate of the heal of a hit of an element that the target absorbs (D-795).</summary>
    public int AbsorbRate { get; }

    /// <summary>The share of full health that poison takes at each turn of the holder (D-803).</summary>
    public int PoisonShare { get; }

    /// <summary>The share of full health that bleed takes at each turn of the holder (D-803).</summary>
    public int BleedShare { get; }

    /// <summary>The ticks that bleed lasts (D-798).</summary>
    public int BleedTicks { get; }

    /// <summary>The share of full health that regen heals at each turn of the holder (D-799).</summary>
    public int RegenShare { get; }

    /// <summary>The ticks that regen lasts (D-798).</summary>
    public int RegenTicks { get; }

    /// <summary>The ticks that sleep lasts, unless a strike wakes the holder (D-802).</summary>
    public int SleepTicks { get; }

    /// <summary>The ticks that haste lasts (D-798).</summary>
    public int HasteTicks { get; }

    /// <summary>The ticks that slow lasts (D-798).</summary>
    public int SlowTicks { get; }

    /// <summary>The ticks that a stun lasts (D-802).</summary>
    public int StunTicks { get; }

    /// <summary>The part of the damage of a move with an element that shell cuts (D-804).</summary>
    public int ShellCut { get; }

    /// <summary>The ticks that shell lasts (D-798).</summary>
    public int ShellTicks { get; }

    /// <summary>The miss chance that blind adds after the clamp (D-806).</summary>
    public int BlindMiss { get; }

    /// <summary>The part of the experience of an enemy that each level of a character above the enemy cuts (D-968).</summary>
    public int ExperienceCut { get; }

    /// <summary>The most levels that a character can stand above an enemy and still earn from it (D-968).</summary>
    public int ExperienceGap { get; }

    /// <summary>
    /// The total experience of each character level, from level 1 to <see cref="StatCurve.HighestLevel"/>
    /// (D-971, D-972). Level 1 takes zero, and each level takes more than the level before it.
    /// </summary>
    public IReadOnlyList<int> LevelExperience { get; }

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
        List<int>? levelExperience = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (string.CompareOrdinal(field, "comment") == 0)
            {
                comment = reader.ReadString();
            }
            else if (string.CompareOrdinal(field, "level_experience") == 0)
            {
                levelExperience = ReadLevelExperience(ref reader);
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

        List<int> table = reader.Require(levelExperience, depth, "level_experience");
        reader.ReadFileEnd();
        CheckRanges(numbers, file);
        return new BattleRules(numbers, table);
    }

    /// <summary>
    /// Reads the experience table: one total for each level, from zero at level 1, each total
    /// above the one before it (D-971, D-972).
    /// </summary>
    private static List<int> ReadLevelExperience(ref ContentReader reader)
    {
        List<int> totals = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, totals.Count))
        {
            int total = reader.ReadInt();
            int level = totals.Count + 1;
            if (level == 1 && total != 0)
            {
                throw reader.Refuse($"level 1 takes the total {total}, and a character at level 1 holds no experience (D-971)");
            }

            if (level > 1 && (total <= totals[^1] || total > BattleFixture.MostStat))
            {
                throw reader.Refuse($"level {level} takes the total {total}, and each total is above the total of the level before it ({totals[^1]}) and at most {BattleFixture.MostStat} (D-971)");
            }

            totals.Add(total);
        }

        if (totals.Count != StatCurve.HighestLevel)
        {
            throw reader.Refuse($"the table holds {totals.Count} totals, and it holds one total for each level from 1 to {StatCurve.HighestLevel} (D-971, D-972)");
        }

        return totals;
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
        CheckRange(numbers, file, "stun_push", 0, MostTicks);
        CheckRange(numbers, file, "flee_base", 0, BasisPoints.One);
        CheckRange(numbers, file, "flee_per_speed", 0, BasisPoints.One);
        CheckRange(numbers, file, "flee_floor", 0, BasisPoints.One);
        CheckRange(numbers, file, "flee_ceiling", numbers["flee_floor"], BasisPoints.One);
        CheckRange(numbers, file, "item_rate", 0, BasisPoints.One);
        CheckRange(numbers, file, "weak_rate", 0, MostRate);
        CheckRange(numbers, file, "resist_rate", 0, MostRate);
        CheckRange(numbers, file, "absorb_rate", 0, MostRate);
        CheckRange(numbers, file, "poison_share", 1, BasisPoints.One);
        CheckRange(numbers, file, "bleed_share", 1, BasisPoints.One);
        CheckRange(numbers, file, "bleed_ticks", 1, MostTicks);
        CheckRange(numbers, file, "regen_share", 1, BasisPoints.One);
        CheckRange(numbers, file, "regen_ticks", 1, MostTicks);
        CheckRange(numbers, file, "sleep_ticks", 1, MostTicks);
        CheckRange(numbers, file, "haste_ticks", 1, MostTicks);
        CheckRange(numbers, file, "slow_ticks", 1, MostTicks);
        CheckRange(numbers, file, "stun_ticks", 1, MostTicks);
        CheckRange(numbers, file, "shell_cut", 0, BasisPoints.One);
        CheckRange(numbers, file, "shell_ticks", 1, MostTicks);
        CheckRange(numbers, file, "blind_miss", 0, BasisPoints.One);
        CheckRange(numbers, file, "experience_cut", 0, BasisPoints.One);
        CheckRange(numbers, file, "experience_gap", 0, StatCurve.HighestLevel - 1);
    }

    private static void CheckRange(SortedDictionary<string, int> numbers, string file, string field, int lowest, int highest)
    {
        int value = numbers[field];
        if (value < lowest || value > highest)
        {
            throw ContentException.ForField(
                file,
                field,
                $"the value {value} is outside {lowest} to {highest} (D-169, D-777, D-808)");
        }
    }
}
