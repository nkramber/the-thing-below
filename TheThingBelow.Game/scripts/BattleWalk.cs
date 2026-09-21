using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game;

/// <summary>
/// Walks a run of the fixture seed into the hall patrol of the first map, and runs it to a
/// fixed moment of the fight (D-767). The smoke session and the `battle` fixture of the
/// screen-test job both walk this path (D-117, D-172).
/// </summary>
/// <remarks>
/// The walk goes east along the hall, south at its east end, and west along the night route
/// of the patrol, so the party meets it by sight or by a step into it. Each call of the run
/// gets the time of exactly one tick, and never the frame time of the engine, so two walks
/// reach the same tick (T-7).
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public static class BattleWalk
{
    /// <summary>The most ticks that a walk runs before it fails (T-2).</summary>
    public const int TickLimit = 6000;

    /// <summary>The time of one tick, which the walk gives to the run on each call (D-164).</summary>
    public const double OneTickSeconds = 1.0 / FixedStepLoop.TicksPerSecond;

    /// <summary>The column of the east end of the hall of the first map, where the walk turns south.</summary>
    private const int TurnColumn = 26;

    /// <summary>The row of the night route of the hall patrol, where the walk turns west.</summary>
    private const int PatrolRow = 7;

    /// <summary>Gives the step of the walk: east along the hall, then south, then west (D-767).</summary>
    /// <param name="party">The party on the first map.</param>
    /// <returns>The intent id of the step.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public static ContentId StepOf(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        if (party.LeadAt.X < TurnColumn && party.LeadAt.Y < PatrolRow)
        {
            return IntentIds.MoveEast;
        }

        return party.LeadAt.Y < PatrolRow ? IntentIds.MoveSouth : IntentIds.MoveWest;
    }

    /// <summary>Walks into the fight, and runs it until the first command of a character (D-532).</summary>
    /// <param name="run">The run, on the first map at the fixture seed.</param>
    /// <exception cref="ArgumentNullException">The run is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">No command came inside the limit, or a tick wrote an error (T-2).</exception>
    public static void ToFirstCommand(GameRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        for (int tick = 0; tick < TickLimit; tick += 1)
        {
            if (run.TakesBattleCommand)
            {
                return;
            }

            if (!run.InBattle)
            {
                run.Queue(Intent.OfPlayer(StepOf(run.Party)));
            }

            OneTick(run);
        }

        throw new InvalidOperationException(
            $"The walk reached no command of a character in {TickLimit} ticks, and the party is at {run.Party.LeadAt} (D-767, T-2).");
    }

    /// <summary>
    /// Attacks the first enemy that melee reaches on each turn of a character, until a blow
    /// of a character has played the given ticks of its hit (D-96, D-213).
    /// </summary>
    /// <param name="run">The run, at a command of a character.</param>
    /// <param name="ticksIntoHit">The ticks of the hit event that the run plays before it stops.</param>
    /// <exception cref="ArgumentNullException">The run is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The ticks are outside the hit (T-2).</exception>
    /// <exception cref="InvalidOperationException">No such blow came inside the limit (T-2).</exception>
    public static void ToBlowOfCharacter(GameRun run, int ticksIntoHit)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentOutOfRangeException.ThrowIfNegative(ticksIntoHit);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(ticksIntoHit, Ui.BattleTimes.StrikeTicks);

        for (int tick = 0; tick < TickLimit; tick += 1)
        {
            if (run.PlayingEvent is BattleEvent playing
                && playing.Kind == BattleEventKind.Hit
                && playing.Actor.Side == BattleSide.Party
                && run.PlayingTicks == ticksIntoHit)
            {
                return;
            }

            if (run.TakesBattleCommand && run.State.Battle is Battle battle)
            {
                BattleTarget target = battle.MeleeTargets(BattleSide.Enemy)[0].Target;
                run.Queue(Intent.OfPlayer(IntentIds.BattleAttack, target, null));
            }

            OneTick(run);
        }

        throw new InvalidOperationException(
            $"The fight reached no blow of a character in {TickLimit} ticks (D-96, T-2).");
    }

    /// <summary>Gives the run the time of one tick, and fails on a log line of an error (T-2).</summary>
    /// <param name="run">The run.</param>
    /// <exception cref="InvalidOperationException">The loop ran another count of ticks, or a tick wrote an error (T-2).</exception>
    public static void OneTick(GameRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        long before = run.Tick;
        foreach (LogEntry entry in run.Advance(OneTickSeconds))
        {
            if (entry.Level == LogLevel.Error)
            {
                throw new InvalidOperationException(
                    $"The tick {run.Tick} of the battle walk wrote an error: {entry.Message} (T-2).");
            }
        }

        if (run.Tick != before + 1)
        {
            throw new InvalidOperationException(
                $"The battle walk gave the run the time of one tick, and the loop ran {run.Tick - before} ticks (T-2).");
        }
    }
}
