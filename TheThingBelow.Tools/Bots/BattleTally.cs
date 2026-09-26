using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>One battle of a bot run: its count of turns and its outcome (D-1182).</summary>
/// <param name="Turns">The count of turns: each command of a character, and each action of an enemy.</param>
/// <param name="Outcome">How the battle ended: won, fled, or wiped.</param>
public sealed record BattleCount(int Turns, BattleOutcome Outcome);

/// <summary>
/// Counts the turns and the outcome of each battle of a bot run from the commands of the bot and
/// the battle events of the run (D-1182). A turn is one action of a character or an enemy.
/// </summary>
/// <remarks>
/// Each battle command of the bot is one turn of a character. The events of an enemy action name
/// the enemy as the actor, and the tally counts each run of such events with one actor as one
/// turn. Two turns in a row of one enemy thus count as one, and a sleeper that passes gives no
/// event and no count. No number of the tally fails the job (D-1182).
/// </remarks>
public sealed class BattleTally
{
    private static readonly BattleEventKind[] ActionKinds =
    [
        BattleEventKind.Hit,
        BattleEventKind.Miss,
        BattleEventKind.Defend,
        BattleEventKind.Step,
        BattleEventKind.Item,
        BattleEventKind.FleeFailed,
        BattleEventKind.Absorb,
        BattleEventKind.Immune,
    ];

    private readonly List<BattleCount> battles = [];

    private int turns;

    private BattleTarget? lastEnemy;

    private bool running;

    /// <summary>Each battle that ended, in the order of the run.</summary>
    public IReadOnlyList<BattleCount> Battles => this.battles;

    /// <summary>Reads the command of one tick and the battle events that the tick gave.</summary>
    /// <param name="command">The intent that the bot sent on the tick, or no value.</param>
    /// <param name="events">The battle events that the run gave on the tick.</param>
    public void Read(Intent? command, IReadOnlyList<BattleEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        if (this.running && command is Intent sent && IsCommand(sent))
        {
            this.turns = checked(this.turns + 1);
            this.lastEnemy = null;
        }

        foreach (BattleEvent battleEvent in events)
        {
            this.ReadEvent(battleEvent);
        }
    }

    private void ReadEvent(BattleEvent battleEvent)
    {
        switch (battleEvent.Kind)
        {
            case BattleEventKind.Started:
                this.running = true;
                this.turns = 0;
                this.lastEnemy = null;
                return;
            case BattleEventKind.Won:
                this.End(BattleOutcome.Won);
                return;
            case BattleEventKind.Fled:
                this.End(BattleOutcome.Fled);
                return;
            case BattleEventKind.Wiped:
                this.End(BattleOutcome.Wiped);
                return;
            case BattleEventKind.Turn:
                this.lastEnemy = null;
                return;
            default:
                break;
        }

        bool enemyAction = battleEvent.Actor.Side == BattleSide.Enemy && Array.IndexOf(ActionKinds, battleEvent.Kind) >= 0;
        if (enemyAction && battleEvent.Actor != this.lastEnemy)
        {
            this.turns = checked(this.turns + 1);
            this.lastEnemy = battleEvent.Actor;
        }
    }

    private void End(BattleOutcome outcome)
    {
        if (!this.running)
        {
            throw new InvalidOperationException($"the battle event of the outcome '{Battle.OutcomeName(outcome)}' came with no battle that started (T-2)");
        }

        this.battles.Add(new BattleCount(this.turns, outcome));
        this.running = false;
    }

    private static bool IsCommand(Intent intent)
    {
        foreach (ContentId action in new[] { IntentIds.BattleAttack, IntentIds.BattleDefend, IntentIds.BattleStep, IntentIds.BattleItem, IntentIds.BattleFlee, IntentIds.BattleLesson })
        {
            if (string.CompareOrdinal(intent.Action.Value, action.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }
}
