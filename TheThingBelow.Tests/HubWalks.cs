using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tests;

/// <summary>
/// Walks the lead of a run of the tests through the intents of the player, as Game makes them:
/// one move intent starts a step, and the lead reaches the next tile after its step ticks (D-203,
/// D-716). The tests of the confirm rule and of the services of a hub read it (D-1131).
/// </summary>
internal static class HubWalks
{
    /// <summary>The most ticks that one step of the lead waits for its end, far above <see cref="MapRules.TicksPerStep"/>.</summary>
    private const int MostTicksOfAStep = 200;

    /// <summary>Gives the move intent of one direction (D-716).</summary>
    /// <param name="direction">The direction.</param>
    /// <returns>The intent of the player.</returns>
    public static Intent Move(StepDirection direction) => Intent.OfPlayer(direction switch
    {
        StepDirection.North => IntentIds.MoveNorth,
        StepDirection.South => IntentIds.MoveSouth,
        StepDirection.East => IntentIds.MoveEast,
        StepDirection.West => IntentIds.MoveWest,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "the value names no step direction"),
    });

    /// <summary>Walks the lead a count of tiles in one direction, and waits for the end of each step.</summary>
    /// <param name="run">The run, with the lead standing.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="tiles">The count of tiles.</param>
    /// <exception cref="InvalidOperationException">A step does not start or does not end (T-2).</exception>
    public static void Walk(Simulation run, StepDirection direction, int tiles)
    {
        for (int tile = 0; tile < tiles; tile += 1)
        {
            TilePoint from = run.State.Party.LeadAt;
            run.Step([Move(direction)]);
            if (run.State.Party.Stepping is null)
            {
                throw new InvalidOperationException($"The lead at {from} started no step to the {StepDirections.NameOf(direction)} at tick {run.Tick}.");
            }

            for (int tick = 0; run.State.Party.Stepping is not null; tick += 1)
            {
                if (tick == MostTicksOfAStep)
                {
                    throw new InvalidOperationException($"The step of the lead from {from} did not end in {MostTicksOfAStep} ticks.");
                }

                run.Step([]);
            }
        }
    }

    /// <summary>Turns the standing lead to a blocked tile, such as an NPC or a service point, with no step (D-207, D-1139).</summary>
    /// <param name="run">The run, with the lead standing.</param>
    /// <param name="direction">The direction of the blocked tile.</param>
    /// <exception cref="InvalidOperationException">The lead started a step, so the tile is not blocked (T-2).</exception>
    public static void Face(Simulation run, StepDirection direction)
    {
        run.Step([Move(direction)]);
        if (run.State.Party.Stepping is not null || run.State.Party.Facing != direction)
        {
            throw new InvalidOperationException($"The lead at {run.State.Party.LeadAt} did not turn to the {StepDirections.NameOf(direction)} with no step.");
        }
    }

    /// <summary>Runs one tick with the confirm intent of the player (D-1131).</summary>
    /// <param name="run">The run.</param>
    /// <returns>The log entries of the tick.</returns>
    public static IReadOnlyList<LogEntry> Confirm(Simulation run) => run.Step([Intent.OfPlayer(IntentIds.Confirm)]);

    /// <summary>Gives the messages of log entries, in order, for a test that reads the log.</summary>
    /// <param name="log">The log entries.</param>
    /// <returns>The messages.</returns>
    public static List<string> Messages(IReadOnlyList<LogEntry> log)
    {
        List<string> messages = [];
        foreach (LogEntry entry in log)
        {
            messages.Add(entry.Message);
        }

        return messages;
    }

    /// <summary>Gives the ids of notices or services, in order.</summary>
    /// <param name="ids">The ids.</param>
    /// <returns>The values of the ids.</returns>
    public static List<string> Values(IEnumerable<ContentId> ids)
    {
        List<string> values = [];
        foreach (ContentId id in ids)
        {
            values.Add(id.Value);
        }

        return values;
    }
}
