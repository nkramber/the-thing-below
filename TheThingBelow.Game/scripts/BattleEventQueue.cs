using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;

namespace TheThingBelow.Game;

/// <summary>
/// The events of a battle that the screen has yet to play, in the order of the rules (D-532).
/// Game takes the next command of the player only when this queue is empty, so the queue is
/// the input gate of a battle (D-532, T-2).
/// </summary>
/// <remarks>
/// Core resolves each action at once and emits its events. The run of Game takes the next
/// event when the one before played all its ticks on the battle screen, such as the pose, the
/// flash, and the number of a strike (D-532, D-829). A test proves that the queue always
/// drains (exit test 6 of PR-9).
/// </remarks>
public sealed class BattleEventQueue
{
    private readonly Queue<BattleEvent> waiting = new();

    /// <summary>True when the screen has played every event.</summary>
    public bool Empty => this.waiting.Count == 0;

    /// <summary>The count of events that wait.</summary>
    public int Count => this.waiting.Count;

    /// <summary>Adds the events of one tick, in the order of the rules.</summary>
    /// <param name="events">The events.</param>
    /// <exception cref="ArgumentNullException">The list or one event is null (T-2).</exception>
    public void Add(IReadOnlyList<BattleEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (BattleEvent battleEvent in events)
        {
            ArgumentNullException.ThrowIfNull(battleEvent);
            this.waiting.Enqueue(battleEvent);
        }
    }

    /// <summary>Takes the next event to play.</summary>
    /// <returns>The event.</returns>
    /// <exception cref="InvalidOperationException">The queue is empty (T-2).</exception>
    public BattleEvent PlayNext()
    {
        if (this.waiting.Count == 0)
        {
            throw new InvalidOperationException("The screen asked for a battle event, and the queue holds none (D-532, T-2).");
        }

        return this.waiting.Dequeue();
    }
}
