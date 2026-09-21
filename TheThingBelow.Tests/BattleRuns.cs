using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Runs that reach a battle. The party spawns one tile west of a guard that never moves and
/// faces away, so one step east bumps into it and starts a battle with the party behind
/// (D-746, D-747, D-770).
/// </summary>
internal static class BattleRuns
{
    /// <summary>The most ticks that a run of these tests steps before it fails (T-2).</summary>
    public const int TickLimit = 20_000;

    /// <summary>Gives the map of one guard, whose group is the named one.</summary>
    /// <param name="group">The id of the group, which the test fixture holds.</param>
    /// <returns>The map.</returns>
    public static GameMap Map(string group)
    {
        string text = $$"""
        {
         "comment": "A room with one guard beside the spawn point.",
         "id": "map.test_guarded",
         "label": "label.test_guarded",
         "time": "day",
         "terrain": [
          "######",
          "#....#",
          "#....#",
          "######"
         ],
         "things": [
          { "id": "spawn_point.test_guarded_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [
          {
           "id": "patrol.test_guard",
           "group": "{{group}}",
           "size": "common",
           "facing": "east",
           "step_ticks": 30,
           "sight_range": 0,
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 2, "y": 1 }] }
           ]
          }
         ]
        }
        """;
        return GameMap.Read(Encoding.UTF8.GetBytes(text), "tests-guarded.json");
    }

    /// <summary>
    /// Gives the map of one guard and one walker. The walker paces the south row, far from the
    /// party, and it never sees, so a test reads whether a map system moves during a battle.
    /// </summary>
    /// <param name="group">The id of the group of both enemies.</param>
    /// <returns>The map.</returns>
    public static GameMap MapWithWalker(string group)
    {
        string text = $$"""
        {
         "comment": "A room with one guard beside the spawn point and one walker on the south row.",
         "id": "map.test_guarded_walker",
         "label": "label.test_guarded_walker",
         "time": "day",
         "terrain": [
          "########",
          "#......#",
          "#......#",
          "#......#",
          "########"
         ],
         "things": [
          { "id": "spawn_point.test_guarded_walker_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [
          {
           "id": "patrol.test_guard",
           "group": "{{group}}",
           "size": "common",
           "facing": "east",
           "step_ticks": 30,
           "sight_range": 0,
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 2, "y": 1 }] }
           ]
          },
          {
           "id": "patrol.test_walker",
           "group": "{{group}}",
           "size": "common",
           "facing": "east",
           "step_ticks": 20,
           "sight_range": 0,
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 3 }, { "x": 6, "y": 3 }] }
           ]
          }
         ]
        }
        """;
        return GameMap.Read(Encoding.UTF8.GetBytes(text), "tests-guarded-walker.json");
    }

    /// <summary>Starts a run and steps it into a battle with the named group.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="group">The id of the group.</param>
    /// <param name="content">The battle content, or the content of the tests.</param>
    /// <returns>The run, in a battle.</returns>
    public static Simulation IntoBattle(ulong seed, string group, BattleContent? content = null)
    {
        Simulation run = Simulation.Start(seed, Map(group), content ?? TestBattles.Content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);
        return run;
    }

    /// <summary>Gives the battle of a run, and fails when none runs.</summary>
    /// <param name="run">The run.</param>
    /// <returns>The battle.</returns>
    public static Battle BattleOf(Simulation run) =>
        run.State.Battle ?? throw new InvalidOperationException("The run holds no battle.");

    /// <summary>Gives the attack of a player at the first enemy that melee reaches (D-377).</summary>
    /// <param name="run">The run, in a battle.</param>
    /// <returns>The intent.</returns>
    public static Intent AttackFirst(Simulation run) =>
        Intent.OfPlayer(IntentIds.BattleAttack, BattleOf(run).MeleeTargets(BattleSide.Enemy)[0].Target, null);

    /// <summary>
    /// Attacks the first reachable enemy on each turn until the battle ends, and gives the
    /// outcome. The run steps one tick for each turn of a character.
    /// </summary>
    /// <param name="run">The run, in a battle.</param>
    /// <param name="seed">The seed, for the message of a failure.</param>
    /// <returns>The outcome.</returns>
    public static BattleOutcome FightToEnd(Simulation run, ulong seed)
    {
        for (int tick = 0; tick < TickLimit; tick += 1)
        {
            Battle battle = BattleOf(run);
            if (battle.Outcome != BattleOutcome.Running)
            {
                return battle.Outcome;
            }

            run.Step([AttackFirst(run)]);
        }

        throw new InvalidOperationException($"Seed {seed}: the battle ran past {TickLimit} turns with no end.");
    }

    /// <summary>Gives the events of a run since the last take, as their kinds.</summary>
    /// <param name="run">The run.</param>
    /// <returns>The kinds, in order.</returns>
    public static List<BattleEventKind> Kinds(Simulation run)
    {
        List<BattleEventKind> kinds = [];
        foreach (BattleEvent battleEvent in run.TakeBattleEvents())
        {
            kinds.Add(battleEvent.Kind);
        }

        return kinds;
    }
}
