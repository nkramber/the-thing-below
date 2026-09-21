using System;
using TheThingBelow.Core.Battles;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The timings and the motions of the battle screen, in ticks of the fixed-step clock and in
/// art pixels (D-266, D-829). PR-57 moves each number into its effect files.
/// </summary>
/// <remarks>
/// No rule reads a number of this type. The rules resolve each action at once, and the screen
/// then plays the events one after another, so a timing sets the pace of the screen alone and
/// never reaches a replay (D-522, D-532, T-7).
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public static class BattleTimes
{
    /// <summary>The ticks that the first line of a fight stands before the next event.</summary>
    public const int StartTicks = 40;

    /// <summary>The ticks of a strike: the pose, the blow, the flash, and the number (D-96, D-213).</summary>
    public const int StrikeTicks = 44;

    /// <summary>The ticks of an event that shows its line alone, such as a defend or a status.</summary>
    public const int LineTicks = 32;

    /// <summary>The ticks of the last line of a fight, before the map runs again (D-522).</summary>
    public const int EndTicks = 60;

    /// <summary>The ticks that the attack pose or the lunge lasts, from the start of a strike (D-108, D-832).</summary>
    public const int PoseTicks = 16;

    /// <summary>The tick of a strike when the blow lands: the flash and the number start (D-96, D-213).</summary>
    public const int BlowTick = 6;

    /// <summary>The ticks of the flash on a hit, from the blow (D-96).</summary>
    public const int FlashTicks = 8;

    /// <summary>The ticks that the damage number rises, from the blow (D-213).</summary>
    public const int NumberRiseTicks = 6;

    /// <summary>The frame pixels that the damage number rises on each tick of its rise.</summary>
    public const int NumberRisePixels = 3;

    /// <summary>The ticks between two frame pixels of the fall of the damage number.</summary>
    public const int NumberFallTicks = 2;

    /// <summary>The art pixels that an enemy slides toward the party when it acts (D-832).</summary>
    public const int LungePixels = 4;

    /// <summary>The art pixels that the backdrop sways to each side (D-205, D-831).</summary>
    public const int DriftPixels = 2;

    /// <summary>The ticks that the backdrop holds each art pixel of its sway (D-205).</summary>
    public const int DriftStepTicks = 45;

    /// <summary>Gives the ticks that the screen plays one event before the next one (D-532).</summary>
    /// <param name="kind">The kind of the event.</param>
    /// <returns>The ticks. A turn takes none, because it changes only who acts.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The kind has no timing (T-2).</exception>
    public static int TicksOf(BattleEventKind kind) => kind switch
    {
        BattleEventKind.Started => StartTicks,
        BattleEventKind.Turn => 0,
        BattleEventKind.Hit => StrikeTicks,
        BattleEventKind.Miss => StrikeTicks,
        BattleEventKind.Absorb => StrikeTicks,
        BattleEventKind.Defend => LineTicks,
        BattleEventKind.Step => LineTicks,
        BattleEventKind.Item => LineTicks,
        BattleEventKind.FleeFailed => LineTicks,
        BattleEventKind.Down => LineTicks,
        BattleEventKind.StepIn => LineTicks,
        BattleEventKind.StatusOn => LineTicks,
        BattleEventKind.StatusOff => LineTicks,
        BattleEventKind.Immune => LineTicks,
        BattleEventKind.StatusHurt => LineTicks,
        BattleEventKind.StatusHeal => LineTicks,
        BattleEventKind.Asleep => LineTicks,
        BattleEventKind.Won => EndTicks,
        BattleEventKind.Fled => EndTicks,
        BattleEventKind.Wiped => EndTicks,
        _ => throw new ArgumentOutOfRangeException(
            nameof(kind), kind, $"The battle event '{kind}' has no timing on the screen (D-829, T-2)."),
    };

    /// <summary>Tells whether an event of this kind is a strike, which plays the pose and the blow.</summary>
    /// <param name="kind">The kind of the event.</param>
    /// <returns>True for a hit, a miss, and an absorb.</returns>
    public static bool IsStrike(BattleEventKind kind) =>
        kind == BattleEventKind.Hit || kind == BattleEventKind.Miss || kind == BattleEventKind.Absorb;

    /// <summary>Tells whether the flash shows on the target at one tick of a strike (D-96).</summary>
    /// <param name="kind">The kind of the event.</param>
    /// <param name="ticks">The ticks since the event started.</param>
    /// <returns>True from the blow of a hit to the end of the flash. A miss and an absorb never flash.</returns>
    public static bool Flashes(BattleEventKind kind, int ticks) =>
        kind == BattleEventKind.Hit && ticks >= BlowTick && ticks < BlowTick + FlashTicks;

    /// <summary>Tells whether the actor holds its pose or its lunge at one tick of a strike (D-108, D-832).</summary>
    /// <param name="kind">The kind of the event.</param>
    /// <param name="ticks">The ticks since the event started.</param>
    /// <returns>True inside the pose of a strike.</returns>
    public static bool Poses(BattleEventKind kind, int ticks) => IsStrike(kind) && ticks < PoseTicks;

    /// <summary>
    /// Gives the frame pixels that the damage number stands above its start, at one tick of a
    /// strike (D-213). The number pops up, and then it falls away.
    /// </summary>
    /// <param name="ticks">The ticks since the event started.</param>
    /// <returns>The height above the start, or null before the blow, when no number shows.</returns>
    public static int? NumberRise(int ticks)
    {
        int since = ticks - BlowTick;
        if (since < 0)
        {
            return null;
        }

        if (since < NumberRiseTicks)
        {
            return since * NumberRisePixels;
        }

        int top = NumberRiseTicks * NumberRisePixels;
        return top - ((since - NumberRiseTicks) / NumberFallTicks);
    }

    /// <summary>
    /// Gives the sway of the backdrop at one tick, in art pixels (D-205, D-831). The sway runs
    /// from zero to <see cref="DriftPixels"/>, back through zero to the other side, and back.
    /// </summary>
    /// <param name="tick">The tick of the run, which the screen reads and never changes.</param>
    /// <returns>The offset, from minus <see cref="DriftPixels"/> to <see cref="DriftPixels"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public static int DriftAt(long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        int steps = DriftPixels * 4;
        int step = (int)(tick / DriftStepTicks % steps);
        if (step <= DriftPixels)
        {
            return step;
        }

        if (step <= DriftPixels * 3)
        {
            return (DriftPixels * 2) - step;
        }

        return step - (DriftPixels * 4);
    }
}
