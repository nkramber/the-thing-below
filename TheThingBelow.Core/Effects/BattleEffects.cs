using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// The shake of a heavy blow at each level of the flash and shake reduction, in art pixels
/// (D-863, D-876).
/// </summary>
/// <param name="Ticks">The ticks that the shake lasts, from the blow.</param>
/// <param name="StepTicks">The ticks that the picture holds each side before it moves to the other side.</param>
/// <param name="Full">The distance at the full level.</param>
/// <param name="Reduced">The distance at the reduced level: a quarter of the full distance (D-863).</param>
/// <param name="Off">The distance at the off level: zero, because the off level plays no shake (D-863).</param>
/// <remarks>
/// The world draws at 2x, so each art pixel moves the picture by two whole pixels of the frame
/// (D-634, D-876). Core holds no settings type, so Game picks the distance of its level (G-1).
/// </remarks>
public sealed record ShakeValues(int Ticks, int StepTicks, int Full, int Reduced, int Off);

/// <summary>The pace of the summary after a won fight: the text above each head and the fill of the bars (D-975, D-976).</summary>
/// <param name="ExperienceTicks">The ticks that the experience of one character holds.</param>
/// <param name="LevelUpTicks">The ticks that the level-up of one character holds.</param>
/// <param name="LineTicks">The ticks between the start of one line of a level-up and the next.</param>
/// <param name="RiseTicks">The ticks that a line takes to slide up and settle.</param>
/// <param name="RisePixels">The art pixels that a line slides up.</param>
/// <param name="BouncePixels">The art pixels that a line passes its place before it settles back, the bounce.</param>
/// <param name="FillTicks">The ticks that the health bar and the MP bar take to fill at a level-up (D-973).</param>
public sealed record SummaryValues(int ExperienceTicks, int LevelUpTicks, int LineTicks, int RiseTicks, int RisePixels, int BouncePixels, int FillTicks);

/// <summary>
/// The pace of every fight on screen: the timings and the motions of PR-10, the shake, and the
/// hit-stop, in ticks and art pixels (D-266, D-829, D-883). No rule reads this file, so it lies
/// outside the rule folder (D-495, D-522).
/// </summary>
/// <remarks>
/// The rules resolve each action at once, and the screen then plays the events one after
/// another, so a timing sets the pace of the screen alone and never reaches a replay (D-522,
/// D-532, T-7). The file names no content id, because it serves every fight (D-883).
/// <para>
/// A strike holds half its ticks at the fast message speed (D-873). The blow, the hit-stop,
/// the flash, and the shake keep their own ticks at each speed, so the load checks that each
/// one ends inside that half.
/// </para>
/// </remarks>
public sealed class BattleEffects
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "effects/battle.json";

    /// <summary>The longest timing of the file, in ticks: five seconds.</summary>
    public const int MostTicks = 300;

    /// <summary>The longest motion of the file, in art pixels.</summary>
    public const int MostPixels = 16;

    /// <summary>The share of the full distance that the reduced level plays: a quarter (D-863).</summary>
    public const int ReducedShare = 4;

    /// <summary>The most lines above a head at a level-up: the level, and one line for each of the five stats (D-975, D-979).</summary>
    public const int SummaryLines = 6;

    private BattleEffects(ReadValues values)
    {
        this.StartTicks = values.Start;
        this.StrikeTicks = values.Strike;
        this.LineTicks = values.Line;
        this.EndTicks = values.End;
        this.PoseTicks = values.Pose;
        this.BlowTick = values.Blow;
        this.FlashTicks = values.Flash;
        this.NumberRiseTicks = values.RiseTicks;
        this.NumberRisePixels = values.RisePixels;
        this.NumberFallTicks = values.FallTicks;
        this.LungePixels = values.Lunge;
        this.DriftPixels = values.Drift;
        this.DriftStepTicks = values.DriftStep;
        this.HitStopTicks = values.HitStop;
        this.Shake = values.Shake;
        this.Summary = values.Summary;
    }

    /// <summary>The ticks that the first line of a fight stands before the next event.</summary>
    public int StartTicks { get; }

    /// <summary>The ticks of a strike: the pose, the blow, the flash, and the number (D-96, D-213).</summary>
    public int StrikeTicks { get; }

    /// <summary>The ticks of an event that shows its line alone, such as a defend or a status.</summary>
    public int LineTicks { get; }

    /// <summary>The ticks of the last line of a fight, before the map runs again (D-522).</summary>
    public int EndTicks { get; }

    /// <summary>The ticks that the attack pose or the lunge lasts, from the start of a strike (D-108, D-832).</summary>
    public int PoseTicks { get; }

    /// <summary>The tick of a strike when the blow lands: the flash, the number, and the burst start (D-96, D-213).</summary>
    public int BlowTick { get; }

    /// <summary>The ticks of the flash on a hit, from the blow (D-96).</summary>
    public int FlashTicks { get; }

    /// <summary>The ticks that the damage number rises, from the blow (D-213).</summary>
    public int NumberRiseTicks { get; }

    /// <summary>The frame pixels that the damage number rises on each tick of its rise.</summary>
    public int NumberRisePixels { get; }

    /// <summary>The ticks between two frame pixels of the fall of the damage number.</summary>
    public int NumberFallTicks { get; }

    /// <summary>The art pixels that an enemy slides toward the party when it acts (D-832).</summary>
    public int LungePixels { get; }

    /// <summary>The art pixels that the backdrop sways to each side (D-205, D-831).</summary>
    public int DriftPixels { get; }

    /// <summary>The ticks that the backdrop holds each art pixel of its sway (D-205).</summary>
    public int DriftStepTicks { get; }

    /// <summary>The ticks that a heavy blow freezes the picture at the blow, at each level (D-863, D-880).</summary>
    public int HitStopTicks { get; }

    /// <summary>The shake of a heavy blow (D-876, D-877).</summary>
    public ShakeValues Shake { get; }

    /// <summary>The pace of the summary after a won fight (D-975).</summary>
    public SummaryValues Summary { get; }

    /// <summary>Reads the battle file from its bytes.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The pace of every fight.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static BattleEffects Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        BattleEffects effects = Read(ref reader);
        reader.ReadFileEnd();
        return effects;
    }

    private static BattleEffects Read(ref ContentReader reader)
    {
        var fields = new ReadFields();
        string? comment = null;
        ShakeValues? shake = null;
        SummaryValues? summary = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "summary":
                    summary = ReadSummary(ref reader);
                    break;
                case "shake":
                    shake = ReadShake(ref reader);
                    break;
                default:
                    if (!fields.TryRead(ref reader, field))
                    {
                        throw reader.UnknownField(field);
                    }

                    break;
            }
        }

        _ = reader.Require(comment, depth, "comment");
        ReadValues values = fields.Build(ref reader, depth, reader.Require(shake, depth, "shake"), reader.Require(summary, depth, "summary"));
        RefuseLateMoment(ref reader, depth, values);
        return new BattleEffects(values);
    }

    /// <summary>
    /// Refuses a moment inside a strike that ends after the hold of a strike at the fast
    /// message speed, half the strike (D-873, T-2). The pose, the blow, and the flash keep
    /// their own ticks at each speed, so a late one would never show.
    /// </summary>
    private static void RefuseLateMoment(ref ContentReader reader, int depth, ReadValues values)
    {
        int fastHold = values.Strike / 2;
        RefuseAfter(ref reader, depth, "pose_ticks", values.Pose, fastHold);
        RefuseAfter(ref reader, depth, "flash_ticks", checked(values.Blow + values.HitStop + values.Flash), fastHold);
        RefuseAfter(ref reader, depth, "shake", checked(values.Blow + values.Shake.Ticks), fastHold);
    }

    private static void RefuseAfter(ref ContentReader reader, int depth, string field, int end, int fastHold)
    {
        if (end > fastHold)
        {
            throw reader.RefuseField(
                depth,
                field,
                $"the moment ends at tick {end} of its event, after the {fastHold} ticks that the event holds at the fast message speed (D-873)");
        }
    }

    private static ShakeValues ReadShake(ref ContentReader reader)
    {
        int? ticks = null;
        int? step = null;
        int? full = null;
        int? reduced = null;
        int? off = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "ticks":
                    ticks = reader.ReadInt();
                    break;
                case "step_ticks":
                    step = reader.ReadInt();
                    break;
                case "full":
                    full = reader.ReadInt();
                    break;
                case "reduced":
                    reduced = reader.ReadInt();
                    break;
                case "off":
                    off = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        int length = InRange(ref reader, depth, "ticks", ticks, 1, MostTicks);
        int hold = InRange(ref reader, depth, "step_ticks", step, 1, length);
        int fullPixels = InRange(ref reader, depth, "full", full, 1, MostPixels);
        int reducedPixels = reader.RequireInt(reduced, depth, "reduced");
        if (checked(reducedPixels * ReducedShare) != fullPixels)
        {
            throw reader.RefuseField(
                depth,
                "reduced",
                $"the reduced distance is {reducedPixels}, and it is a quarter of the full distance {fullPixels} (D-863)");
        }

        int offPixels = reader.RequireInt(off, depth, "off");
        if (offPixels != 0)
        {
            throw reader.RefuseField(depth, "off", $"the distance is {offPixels}, and the off level plays no shake (D-863)");
        }

        return new ShakeValues(length, hold, fullPixels, reducedPixels, offPixels);
    }

    /// <summary>
    /// Reads the summary, and refuses a motion that ends after the hold of its event at the fast
    /// message speed, half the hold (D-873, D-975). A level-up shows six lines at most: the
    /// level and one line for each of the five stats (D-979).
    /// </summary>
    private static SummaryValues ReadSummary(ref ContentReader reader)
    {
        int? experience = null;
        int? levelUp = null;
        int? line = null;
        int? rise = null;
        int? risePixels = null;
        int? bounce = null;
        int? fill = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "experience_ticks":
                    experience = reader.ReadInt();
                    break;
                case "level_up_ticks":
                    levelUp = reader.ReadInt();
                    break;
                case "line_ticks":
                    line = reader.ReadInt();
                    break;
                case "rise_ticks":
                    rise = reader.ReadInt();
                    break;
                case "rise_pixels":
                    risePixels = reader.ReadInt();
                    break;
                case "bounce_pixels":
                    bounce = reader.ReadInt();
                    break;
                case "fill_ticks":
                    fill = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var values = new SummaryValues(
            InRange(ref reader, depth, "experience_ticks", experience, 2, MostTicks),
            InRange(ref reader, depth, "level_up_ticks", levelUp, 2, MostTicks),
            InRange(ref reader, depth, "line_ticks", line, 1, MostTicks),
            InRange(ref reader, depth, "rise_ticks", rise, 2, MostTicks),
            InRange(ref reader, depth, "rise_pixels", risePixels, 1, MostPixels),
            InRange(ref reader, depth, "bounce_pixels", bounce, 0, MostPixels),
            InRange(ref reader, depth, "fill_ticks", fill, 1, MostTicks));
        RefuseAfter(ref reader, depth, "rise_ticks", values.RiseTicks, values.ExperienceTicks / 2);
        RefuseAfter(ref reader, depth, "line_ticks", checked((values.LineTicks * (SummaryLines - 1)) + values.RiseTicks), values.LevelUpTicks / 2);
        RefuseAfter(ref reader, depth, "fill_ticks", values.FillTicks, values.LevelUpTicks / 2);
        return values;
    }

    private static int InRange(ref ContentReader reader, int depth, string field, int? value, int least, int most)
    {
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most}");
        }

        return read;
    }

    /// <summary>The checked values of the file, before the checks that span fields.</summary>
    private sealed record ReadValues(
        int Start,
        int Strike,
        int Line,
        int End,
        int Pose,
        int Blow,
        int Flash,
        int RiseTicks,
        int RisePixels,
        int FallTicks,
        int Lunge,
        int Drift,
        int DriftStep,
        int HitStop,
        ShakeValues Shake,
        SummaryValues Summary);

    /// <summary>The whole-number fields of the file, which the read fills one at a time.</summary>
    private sealed class ReadFields
    {
        private int? start;
        private int? strike;
        private int? line;
        private int? end;
        private int? pose;
        private int? blow;
        private int? flash;
        private int? riseTicks;
        private int? risePixels;
        private int? fallTicks;
        private int? lunge;
        private int? drift;
        private int? driftStep;
        private int? hitStop;

        public bool TryRead(ref ContentReader reader, string field)
        {
            switch (field)
            {
                case "start_ticks":
                    this.start = reader.ReadInt();
                    return true;
                case "strike_ticks":
                    this.strike = reader.ReadInt();
                    return true;
                case "line_ticks":
                    this.line = reader.ReadInt();
                    return true;
                case "end_ticks":
                    this.end = reader.ReadInt();
                    return true;
                case "pose_ticks":
                    this.pose = reader.ReadInt();
                    return true;
                case "blow_tick":
                    this.blow = reader.ReadInt();
                    return true;
                case "flash_ticks":
                    this.flash = reader.ReadInt();
                    return true;
                case "number_rise_ticks":
                    this.riseTicks = reader.ReadInt();
                    return true;
                case "number_rise_pixels":
                    this.risePixels = reader.ReadInt();
                    return true;
                case "number_fall_ticks":
                    this.fallTicks = reader.ReadInt();
                    return true;
                case "lunge_pixels":
                    this.lunge = reader.ReadInt();
                    return true;
                case "drift_pixels":
                    this.drift = reader.ReadInt();
                    return true;
                case "drift_step_ticks":
                    this.driftStep = reader.ReadInt();
                    return true;
                case "hit_stop_ticks":
                    this.hitStop = reader.ReadInt();
                    return true;
                default:
                    return false;
            }
        }

        public ReadValues Build(ref ContentReader reader, int depth, ShakeValues shake, SummaryValues summary) => new(
            InRange(ref reader, depth, "start_ticks", this.start, 1, MostTicks),
            InRange(ref reader, depth, "strike_ticks", this.strike, 2, MostTicks),
            InRange(ref reader, depth, "line_ticks", this.line, 1, MostTicks),
            InRange(ref reader, depth, "end_ticks", this.end, 1, MostTicks),
            InRange(ref reader, depth, "pose_ticks", this.pose, 1, MostTicks),
            InRange(ref reader, depth, "blow_tick", this.blow, 0, MostTicks),
            InRange(ref reader, depth, "flash_ticks", this.flash, 1, MostTicks),
            InRange(ref reader, depth, "number_rise_ticks", this.riseTicks, 1, MostTicks),
            InRange(ref reader, depth, "number_rise_pixels", this.risePixels, 1, MostPixels),
            InRange(ref reader, depth, "number_fall_ticks", this.fallTicks, 1, MostTicks),
            InRange(ref reader, depth, "lunge_pixels", this.lunge, 0, MostPixels),
            InRange(ref reader, depth, "drift_pixels", this.drift, 1, MostPixels),
            InRange(ref reader, depth, "drift_step_ticks", this.driftStep, 1, MostTicks),
            InRange(ref reader, depth, "hit_stop_ticks", this.hitStop, 0, MostTicks),
            shake,
            summary);
    }
}
