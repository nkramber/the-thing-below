using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Effects;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The moments and the motions of the battle screen at each tick of an event: the pose, the
/// flash, the number, the sway, the hit-stop, the shake, and the burst (D-186, D-829). The
/// battle file holds each number (D-883).
/// </summary>
/// <remarks>
/// No rule reads a number of this type. The rules resolve each action at once, and the screen
/// then plays the events one after another, so a timing sets the pace of the screen alone and
/// never reaches a replay (D-522, D-532, T-7).
/// <para>
/// A heavy blow freezes the picture at the blow for the ticks of the hit-stop (D-880). The
/// pose, the flash, the number, and the burst then read the ticks of the picture, which stand
/// still during the freeze. The event holds the same ticks, so the freeze costs no time
/// (D-873).
/// </para>
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public static class BattleTimes
{
    /// <summary>Gives the ticks that the screen plays one event before the next one (D-532).</summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="kind">The kind of the event.</param>
    /// <returns>The ticks. A turn takes none, because it changes only who acts.</returns>
    /// <exception cref="ArgumentNullException">The battle file is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The kind has no timing (T-2).</exception>
    public static int TicksOf(BattleEffects pace, BattleEventKind kind)
    {
        ArgumentNullException.ThrowIfNull(pace);

        return kind switch
        {
            BattleEventKind.Started => pace.StartTicks,
            BattleEventKind.Turn => 0,
            BattleEventKind.Hit => pace.StrikeTicks,
            BattleEventKind.Miss => pace.StrikeTicks,
            BattleEventKind.Absorb => pace.StrikeTicks,
            BattleEventKind.Defend => pace.LineTicks,
            BattleEventKind.Step => pace.LineTicks,
            BattleEventKind.Item => pace.LineTicks,
            BattleEventKind.Heal => pace.LineTicks,
            BattleEventKind.FleeFailed => pace.LineTicks,
            BattleEventKind.Down => pace.LineTicks,
            BattleEventKind.StepIn => pace.LineTicks,
            BattleEventKind.StatusOn => pace.LineTicks,
            BattleEventKind.StatusOff => pace.LineTicks,
            BattleEventKind.Immune => pace.LineTicks,
            BattleEventKind.StatusHurt => pace.LineTicks,
            BattleEventKind.StatusHeal => pace.LineTicks,
            BattleEventKind.Asleep => pace.LineTicks,
            BattleEventKind.Won => pace.EndTicks,
            BattleEventKind.Fled => pace.EndTicks,
            BattleEventKind.Wiped => pace.EndTicks,
            BattleEventKind.Experience => pace.Summary.ExperienceTicks,
            BattleEventKind.LevelUp => pace.Summary.LevelUpTicks,

            // A new form rises as one line of the summary, as the experience does (D-975, D-1027).
            BattleEventKind.FormOpened => pace.Summary.ExperienceTicks,

            // The line of a lesson holds while the flash of a spell plays (D-1032).
            BattleEventKind.Lesson => pace.LineTicks,
            BattleEventKind.ItemMp => pace.LineTicks,
            BattleEventKind.ItemCure => pace.LineTicks,
            BattleEventKind.Revive => pace.LineTicks,
            BattleEventKind.StealItem => pace.LineTicks,
            BattleEventKind.StealGear => pace.LineTicks,
            BattleEventKind.StealGold => pace.LineTicks,
            BattleEventKind.StealFailed => pace.LineTicks,
            BattleEventKind.StealEmpty => pace.LineTicks,
            BattleEventKind.StealFull => pace.LineTicks,

            // A drop is a line of the loot after the summary (D-975, D-1042).
            BattleEventKind.Drop => pace.LineTicks,
            BattleEventKind.DropLost => pace.LineTicks,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind), kind, $"The battle event '{kind}' has no timing on the screen (D-829, T-2)."),
        };
    }

    /// <summary>Gives the ticks that the screen holds one event at a message speed (D-866, D-873).</summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="kind">The kind of the event.</param>
    /// <param name="speed">The message speed of the battle group.</param>
    /// <returns>
    /// The ticks of <see cref="TicksOf"/> at normal, 1.5 times as many at slow, and half as many
    /// at fast. The pose, the blow, and the flash inside a strike keep their own ticks.
    /// </returns>
    /// <exception cref="ArgumentNullException">The battle file is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The kind has no timing, or the speed has no name (T-2).</exception>
    public static int HoldTicksOf(BattleEffects pace, BattleEventKind kind, MessageSpeed speed)
    {
        int normal = TicksOf(pace, kind);
        return speed switch
        {
            MessageSpeed.Slow => normal * 3 / 2,
            MessageSpeed.Normal => normal,
            MessageSpeed.Fast => normal / 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(speed), speed, $"The message speed '{speed}' has no timing (D-866, T-2)."),
        };
    }

    /// <summary>Tells whether an event of this kind is a strike, which plays the pose and the blow.</summary>
    /// <param name="kind">The kind of the event.</param>
    /// <returns>True for a hit, a miss, and an absorb.</returns>
    public static bool IsStrike(BattleEventKind kind) =>
        kind == BattleEventKind.Hit || kind == BattleEventKind.Miss || kind == BattleEventKind.Absorb;

    /// <summary>Tells whether an event is a heavy blow: a hit on an element that the target is weak to (D-877).</summary>
    /// <param name="played">The event.</param>
    /// <returns>True for a hit with the weak affinity.</returns>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    public static bool IsHeavy(BattleEvent played)
    {
        ArgumentNullException.ThrowIfNull(played);

        return played.Kind == BattleEventKind.Hit && played.Affinity == Affinity.Weak;
    }

    /// <summary>
    /// Gives the ticks of the picture at one tick of an event. A heavy blow holds the picture at
    /// the blow for the ticks of the hit-stop, and every other event shows its own ticks (D-880).
    /// </summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="played">The event.</param>
    /// <param name="ticks">The ticks since the event started.</param>
    /// <returns>The ticks that the pose, the flash, the number, and the burst read.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static int PictureTicks(BattleEffects pace, BattleEvent played, int ticks)
    {
        ArgumentNullException.ThrowIfNull(pace);

        if (!IsHeavy(played) || ticks <= pace.BlowTick)
        {
            return ticks;
        }

        int stopEnd = pace.BlowTick + pace.HitStopTicks;
        return ticks < stopEnd ? pace.BlowTick : ticks - pace.HitStopTicks;
    }

    /// <summary>Tells whether the flash shows on the target at one tick of the picture (D-96).</summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="kind">The kind of the event.</param>
    /// <param name="ticks">The ticks of the picture, from <see cref="PictureTicks"/>.</param>
    /// <returns>True from the blow of a hit to the end of the flash. A miss and an absorb never flash.</returns>
    /// <exception cref="ArgumentNullException">The battle file is null (T-2).</exception>
    /// <remarks>The flash shows at each level of the flash and shake reduction (D-881).</remarks>
    public static bool Flashes(BattleEffects pace, BattleEventKind kind, int ticks)
    {
        ArgumentNullException.ThrowIfNull(pace);

        return kind == BattleEventKind.Hit && ticks >= pace.BlowTick && ticks < pace.BlowTick + pace.FlashTicks;
    }

    /// <summary>Tells whether the actor holds its pose or its lunge at one tick of the picture (D-108, D-832).</summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="kind">The kind of the event.</param>
    /// <param name="ticks">The ticks of the picture, from <see cref="PictureTicks"/>.</param>
    /// <returns>True inside the pose of a strike.</returns>
    /// <exception cref="ArgumentNullException">The battle file is null (T-2).</exception>
    public static bool Poses(BattleEffects pace, BattleEventKind kind, int ticks)
    {
        ArgumentNullException.ThrowIfNull(pace);

        return IsStrike(kind) && ticks < pace.PoseTicks;
    }

    /// <summary>
    /// Gives the frame pixels that the damage number stands above its start, at one tick of the
    /// picture of a strike (D-213). The number pops up, and then it falls away.
    /// </summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="ticks">The ticks of the picture, from <see cref="PictureTicks"/>.</param>
    /// <returns>The height above the start, or null before the blow, when no number shows.</returns>
    /// <exception cref="ArgumentNullException">The battle file is null (T-2).</exception>
    public static int? NumberRise(BattleEffects pace, int ticks)
    {
        ArgumentNullException.ThrowIfNull(pace);

        int since = ticks - pace.BlowTick;
        if (since < 0)
        {
            return null;
        }

        if (since < pace.NumberRiseTicks)
        {
            return since * pace.NumberRisePixels;
        }

        int top = pace.NumberRiseTicks * pace.NumberRisePixels;
        return top - ((since - pace.NumberRiseTicks) / pace.NumberFallTicks);
    }

    /// <summary>
    /// Gives the art pixels that a line of the summary stands above its start, at one tick of
    /// the line (D-975). The line slides up past its place by the bounce, and then it settles
    /// back to its place.
    /// </summary>
    /// <param name="summary">The pace of the summary.</param>
    /// <param name="ticks">The ticks since the line started, which is below zero before its start.</param>
    /// <returns>The height above the start, or null before the line starts, when it shows nothing.</returns>
    /// <exception cref="ArgumentNullException">The pace is null (T-2).</exception>
    public static int? SummaryRise(SummaryValues summary, int ticks)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (ticks < 0)
        {
            return null;
        }

        int top = summary.RisePixels + summary.BouncePixels;
        int up = Math.Max(1, summary.RiseTicks * 2 / 3);
        if (ticks < up)
        {
            return ticks * top / up;
        }

        if (ticks < summary.RiseTicks)
        {
            return top - ((ticks - up) * summary.BouncePixels / (summary.RiseTicks - up));
        }

        return summary.RisePixels;
    }

    /// <summary>
    /// Gives the value that a bar shows at one tick of a level-up, as it fills from its value
    /// before the level-up to the full value (D-975).
    /// </summary>
    /// <param name="summary">The pace of the summary.</param>
    /// <param name="ticks">The ticks since the level-up started.</param>
    /// <param name="before">The value before the level-up.</param>
    /// <param name="full">The full value of the new level.</param>
    /// <returns>The value, from the value before to the full value.</returns>
    /// <exception cref="ArgumentNullException">The pace is null (T-2).</exception>
    public static int FillAt(SummaryValues summary, int ticks, int before, int full)
    {
        ArgumentNullException.ThrowIfNull(summary);

        int passed = Math.Clamp(ticks, 0, summary.FillTicks);
        return before + ((full - before) * passed / summary.FillTicks);
    }

    /// <summary>
    /// Gives the ticks since the burst of a hit started, at one tick of the picture (D-879).
    /// </summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="played">The event.</param>
    /// <param name="ticks">The ticks of the picture, from <see cref="PictureTicks"/>.</param>
    /// <returns>The age of the burst, or null when no burst plays: before the blow, or on an event that is no hit.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <remarks>A miss hits nothing, and an absorb heals, so neither one bleeds or sparks.</remarks>
    public static int? BurstAge(BattleEffects pace, BattleEvent played, int ticks)
    {
        ArgumentNullException.ThrowIfNull(pace);
        ArgumentNullException.ThrowIfNull(played);

        if (played.Kind != BattleEventKind.Hit || ticks < pace.BlowTick)
        {
            return null;
        }

        return ticks - pace.BlowTick;
    }

    /// <summary>Gives the distance of the shake at one level of the flash and shake reduction (D-863).</summary>
    /// <param name="shake">The shake of the battle file.</param>
    /// <param name="level">The level of the settings.</param>
    /// <returns>The distance, in art pixels.</returns>
    /// <exception cref="ArgumentNullException">The shake is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The level has no name (T-2).</exception>
    public static int ShakeDistance(ShakeValues shake, EffectLevel level)
    {
        ArgumentNullException.ThrowIfNull(shake);

        return level switch
        {
            EffectLevel.Full => shake.Full,
            EffectLevel.Reduced => shake.Reduced,
            EffectLevel.Off => shake.Off,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, $"The level '{level}' has no shake (D-863, T-2)."),
        };
    }

    /// <summary>
    /// Gives the offset of the battle picture at one tick of an event, in art pixels (D-876).
    /// A heavy blow moves the picture to one side and the other, from the blow to the end of the
    /// shake. The UI stays still.
    /// </summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="played">The event.</param>
    /// <param name="ticks">The ticks since the event started, and not the ticks of the picture, because the shake plays through the freeze.</param>
    /// <param name="level">The level of the flash and shake reduction (D-863).</param>
    /// <returns>The offset across the screen, or zero outside a shake.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static int ShakeAt(BattleEffects pace, BattleEvent played, int ticks, EffectLevel level)
    {
        ArgumentNullException.ThrowIfNull(pace);

        int since = ticks - pace.BlowTick;
        if (!IsHeavy(played) || since < 0 || since >= pace.Shake.Ticks)
        {
            return 0;
        }

        int distance = ShakeDistance(pace.Shake, level);
        bool firstSide = since / pace.Shake.StepTicks % 2 == 0;
        return firstSide ? distance : -distance;
    }

    /// <summary>
    /// Gives the sway of the backdrop at one tick, in art pixels (D-205, D-831). The sway runs
    /// from zero to the drift of the battle file, back through zero to the other side, and back.
    /// </summary>
    /// <param name="pace">The battle file.</param>
    /// <param name="tick">The tick of the run, which the screen reads and never changes.</param>
    /// <returns>The offset, from minus the drift to the drift.</returns>
    /// <exception cref="ArgumentNullException">The battle file is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public static int DriftAt(BattleEffects pace, long tick)
    {
        ArgumentNullException.ThrowIfNull(pace);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        int drift = pace.DriftPixels;
        int steps = drift * 4;
        int step = (int)(tick / pace.DriftStepTicks % steps);
        if (step <= drift)
        {
            return step;
        }

        if (step <= drift * 3)
        {
            return (drift * 2) - step;
        }

        return step - (drift * 4);
    }
}
