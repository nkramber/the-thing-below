using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Identity;

/// <summary>
/// The replay-identity set: a fixed list of runs, each of which gives one state hash. Every
/// CI leg computes each hash and compares it with the identity file in Tests (G-5, D-504).
/// </summary>
/// <remarks>
/// PR-4 filled the set with the vectors of the fixed-point math, the streams, and the state
/// hash. PR-6 added the replay of a run record, and each later Core PR adds a run. A run that
/// changes its hash also bumps <see cref="SimulationVersion"/>, and the review of that PR
/// reads each changed hash (G-17, D-504).
/// </remarks>
public static class IdentitySet
{
    /// <summary>The name of the run that reads the fixed-point math (D-641).</summary>
    public const string BasisPointsRun = "basis-points";

    /// <summary>The name of the run that reads a battle, its snapshot, and its replay (D-531, D-532).</summary>
    public const string BattleRun = "battle";

    /// <summary>The name of the run that fights an enemy record with an ability (exit test 5 of PR-80, D-557, D-787).</summary>
    public const string EnemyRecordRun = "enemy-record";

    /// <summary>The name of the run that reads the stream split (D-643).</summary>
    public const string StreamSplitRun = "stream-split";

    /// <summary>The name of the run that reads the bounded draws of a stream (D-642).</summary>
    public const string RandomDrawsRun = "random-draws";

    /// <summary>The name of the run that reads the state hash (D-644).</summary>
    public const string StateHashRun = "state-hash";

    /// <summary>The name of the run that reads the tick, the record, and the replay (D-650 to D-652).</summary>
    public const string ReplayRun = "replay";

    /// <summary>
    /// The content hash that the record of the replay run names. The run reads no content
    /// file, because Core reads no file, so the value is a fixed text of this set (G-1).
    /// </summary>
    private const string ReplayContentHash = "identity-set-content-hash";

    /// <summary>The count of ticks that the replay run steps.</summary>
    private const int ReplayTickCount = 600;

    /// <summary>The seed of every run of this set. It never changes.</summary>
    private const ulong RunSeed = 20260918;

    /// <summary>
    /// The map of the replay run. The set holds its own map, because Core reads no file and
    /// a content change must never move a hash of this set (G-1, D-495). The map has a
    /// pillar, so the run reads the step rule against a wall as well as open ground.
    /// </summary>
    private const string ReplayMapFile = """
    {
     "comment": "The map of the replay run of the identity set. One enemy walks a route and one paces an area, so the run reads the walk of a patrol and a draw of the exploration stream too (D-504, D-739, D-741). Each sight range is zero, so the party of the script never starts an encounter and every later tick of the run still does work. PR-8 added the enemies, and the map never changes again.",
     "id": "map.identity_run",
     "label": "label.identity_run",
     "time": "day",
     "terrain": [
      "#########",
      "#.......#",
      "#..###..#",
      "#..#....#",
      "#..###..#",
      "#.......#",
      "#########"
     ],
     "things": [
      { "id": "spawn_point.identity_run_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.identity_run_walker",
       "group": "group.identity_run",
       "size": "common",
       "facing": "east",
       "step_ticks": 15,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 4, "y": 3 }, { "x": 5, "y": 3 }] }
       ]
      },
      {
       "id": "patrol.identity_run_pacer",
       "group": "group.identity_run",
       "size": "common",
       "facing": "south",
       "step_ticks": 20,
       "sight_range": 0,
       "areas": [
        { "times": ["dawn", "day", "dusk", "night"], "x": 1, "y": 4, "width": 2, "height": 2 }
       ]
      }
     ]
    }
    """;

    /// <summary>The count of ticks that the battle run steps.</summary>
    private const int BattleTickCount = 240;

    /// <summary>
    /// The map of the battle run. The party stands two tiles west of a guard that never
    /// moves, so two steps east bump into it and start a battle from behind (D-746, D-747).
    /// </summary>
    private const string BattleMapFile = """
    {
     "comment": "The map of the battle run of the identity set. PR-9 added it, and the map never changes again.",
     "id": "map.identity_battle",
     "label": "label.identity_battle",
     "time": "day",
     "terrain": [
      "#########",
      "#.......#",
      "#.......#",
      "#########"
     ],
     "things": [
      { "id": "spawn_point.identity_battle_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.identity_battle_guard",
       "group": "group.identity_battle",
       "size": "common",
       "facing": "east",
       "step_ticks": 15,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 3, "y": 1 }] }
       ]
      }
     ]
    }
    """;

    /// <summary>
    /// The map of the enemy-record run: the battle map, with a guard of the record group. The
    /// brute of that group names an ability, so the run fights a record with an ability id,
    /// which no fight reads yet (D-787).
    /// </summary>
    private const string RecordMapFile = """
    {
     "comment": "The map of the enemy-record run of the identity set. PR-80 added it, and the map never changes again.",
     "id": "map.identity_record",
     "label": "label.identity_record",
     "time": "day",
     "terrain": [
      "#########",
      "#.......#",
      "#.......#",
      "#########"
     ],
     "things": [
      { "id": "spawn_point.identity_record_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.identity_record_guard",
       "group": "group.identity_record",
       "size": "common",
       "facing": "east",
       "step_ticks": 15,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 3, "y": 1 }] }
       ]
      }
     ]
    }
    """;

    /// <summary>The grunt record of this set. PR-9 gave the stats, and PR-80 moved them to a record. It never changes.</summary>
    private const string GruntRecordFile = """
    {
     "comment": "The grunt of the identity set. It never changes.",
     "id": "enemy.identity_grunt",
     "health": 20,
     "attack": 6,
     "defense": 2,
     "speed": 90,
     "abilities": []
    }
    """;

    /// <summary>The brute record of this set, which names an ability (D-787). PR-80 added it, and it never changes.</summary>
    private const string BruteRecordFile = """
    {
     "comment": "The brute of the identity set. It never changes.",
     "id": "enemy.identity_brute",
     "health": 45,
     "attack": 11,
     "defense": 5,
     "speed": 80,
     "abilities": ["ability.identity_strike"]
    }
    """;

    /// <summary>The ability file of this set (D-785). PR-80 added it, and it never changes.</summary>
    private const string AbilityFile = """
    {
     "comment": "The ability file of the identity set. It never changes.",
     "abilities": [{ "id": "ability.identity_strike" }]
    }
    """;

    /// <summary>The battle rules of this set, with the numbers of D-777. They never change.</summary>
    private const string BattleRulesFile = """
    {
     "comment": "The battle rules of the identity set. PR-9 added them, and they never change.",
     "attack_delay": 100,
     "attack_power": 10000,
     "defend_delay": 60,
     "step_delay": 60,
     "flee_delay": 100,
     "hit_low": 9000,
     "hit_high": 11000,
     "miss_base": 500,
     "miss_per_speed": 25,
     "miss_floor": 0,
     "miss_ceiling": 1500,
     "defend_cut": 5000,
     "back_row_rate": 5000,
     "haste_rate": 7500,
     "slow_rate": 15000,
     "stun_ticks": 50,
     "flee_base": 5000,
     "flee_per_speed": 100,
     "flee_floor": 1000,
     "flee_ceiling": 9000,
     "item_rate": 5000
    }
    """;

    /// <summary>
    /// The battle fixture of this set. The guard group holds a wave, so the run reads the
    /// step of a waiting enemy too (D-778). It never changes.
    /// </summary>
    private const string BattleFixtureFile = """
    {
     "comment": "The battle fixture of the identity set. PR-9 added it, and PR-80 moved its enemies to the enemy records and added the record group.",
     "characters": [
      { "id": "character.identity_hero", "health": 90, "attack": 14, "defense": 4, "speed": 100, "row": "front" }
     ],
     "groups": [
      {
       "id": "group.identity_run",
       "boss": false,
       "enemies": [{ "enemy": "enemy.identity_grunt", "row": "front", "waits": false }]
      },
      {
       "id": "group.identity_record",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.identity_brute", "row": "front", "waits": false },
        { "enemy": "enemy.identity_grunt", "row": "back", "waits": false }
       ]
      },
      {
       "id": "group.identity_battle",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.identity_grunt", "row": "front", "waits": false },
        { "enemy": "enemy.identity_grunt", "row": "back", "waits": false },
        { "enemy": "enemy.identity_grunt", "row": "front", "waits": true }
       ]
      }
     ],
     "items": [
      { "id": "item.identity_draught", "heal": 30, "delay": 100 }
     ],
     "start_party": ["character.identity_hero"],
     "pack": [{ "item": "item.identity_draught", "count": 9 }]
    }
    """;

    /// <summary>Every run of the set, in the order of the identity file.</summary>
    /// <remarks>The order is the ordinal order of the names, which every machine reads the same (F-39).</remarks>
    public static readonly IReadOnlyList<string> RunNames =
    [
        BasisPointsRun,
        BattleRun,
        EnemyRecordRun,
        RandomDrawsRun,
        ReplayRun,
        StateHashRun,
        StreamSplitRun,
    ];

    /// <summary>Computes the state hash of one run of the set.</summary>
    /// <param name="runName">The name of the run, from <see cref="RunNames"/>.</param>
    /// <returns>The state hash of that run.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The set holds no run with that name (T-2).</exception>
    public static ulong Compute(string runName)
    {
        ArgumentException.ThrowIfNullOrEmpty(runName);

        return runName switch
        {
            BasisPointsRun => ComputeBasisPoints(),
            BattleRun => ComputeBattle(BattleMapFile),
            EnemyRecordRun => ComputeBattle(RecordMapFile),
            RandomDrawsRun => ComputeRandomDraws(),
            ReplayRun => ComputeReplay(),
            StateHashRun => ComputeStateHash(),
            StreamSplitRun => ComputeStreamSplit(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(runName),
                runName,
                $"The identity set holds no run with that name. It holds {string.Join(", ", RunNames)}."),
        };
    }

    private static ulong ComputeBasisPoints()
    {
        RunContext context = new(RunSeed, 0, $"identity/{BasisPointsRun}");
        StateHasher hasher = new();

        // The tables cover a positive value, a negative value, a rate above 100%, a rate of
        // zero, and a product that loses a fraction. Each result rounds toward zero (D-641).
        // No pair below overflows, because an overflow is its own test (T-3).
        int[] values = [0, 1, 7, 100, 9999, -1, -7, -9999, 1_000_000, -1_000_000];
        int[] rates = [0, 1, 3333, 5000, BasisPoints.One, 15000, 99999];
        int[] divisors = [1, 2, 3, 7, 10000, -3, -10000];
        int[] parts = [0, 1, 7, 100, 9999, -1, -9999, 200_000];
        int[] wholes = [1, 2, 3, 100, 9999, 214_748, -7];

        foreach (int value in values)
        {
            foreach (int rate in rates)
            {
                hasher.AddInt32(BasisPoints.Apply(value, rate, context));
            }

            foreach (int divisor in divisors)
            {
                hasher.AddInt32(BasisPoints.Divide(value, divisor, context));
            }
        }

        foreach (int part in parts)
        {
            foreach (int whole in wholes)
            {
                hasher.AddInt32(BasisPoints.RateOf(part, whole, context));
            }
        }

        return hasher.Finish();
    }

    private static ulong ComputeStreamSplit()
    {
        StateHasher hasher = new();

        foreach (StreamId stream in RandomStreams.All)
        {
            hasher.AddInt32((int)stream);
            hasher.AddUInt64(RandomStreams.SeedOf(RunSeed, stream));

            RandomStream opened = RandomStreams.Open(RunSeed, stream);
            for (int draw = 0; draw < 16; draw += 1)
            {
                hasher.AddUInt64(opened.NextUInt64());
            }
        }

        return hasher.Finish();
    }

    private static ulong ComputeRandomDraws()
    {
        RunContext context = new(RunSeed, 0, $"identity/{RandomDrawsRun}");
        StateHasher hasher = new();
        RandomStream stream = RandomStreams.Open(RunSeed, StreamId.Battle);

        // The bounds cover a bound of one, a small bound, a bound that no power of two
        // divides, and the widest bound. The last one exercises the refusal step.
        int[] bounds = [1, 2, 6, 20, 100, 3, int.MaxValue];
        foreach (int bound in bounds)
        {
            for (int draw = 0; draw < 32; draw += 1)
            {
                hasher.AddInt32(stream.NextInt(bound, context));
            }
        }

        for (int draw = 0; draw < 32; draw += 1)
        {
            hasher.AddInt32(stream.NextInt(-50, 50, context));
            hasher.AddBoolean(stream.NextChance(2500, context));
        }

        hasher.AddUInt64(stream.Generator.State);
        hasher.AddUInt64(stream.Generator.Increment);
        return hasher.Finish();
    }

    /// <summary>
    /// Runs a fixed script of intents, writes the record, reads the text of it again, and
    /// replays it. The hash holds the state of the run, the state of the replay, and the
    /// text of the record, so the run reads the tick, the record, and the replay together
    /// (G-5, D-650, D-651, D-652).
    /// </summary>
    private static ulong ComputeReplay()
    {
        GameMap map = GameMap.Read(Encoding.UTF8.GetBytes(ReplayMapFile), "identity-set-map.json");
        RunHeader header = RunHeader.ForThisBuild(ReplayContentHash, RunSeed);
        Simulation simulation = Simulation.Start(RunSeed, map, ReplayBattleContent(), DebugIntentHandlers.None);
        RunRecorder recorder = new(header, simulation.Snapshot());

        for (int step = 0; step < ReplayTickCount; step += 1)
        {
            IReadOnlyList<Intent> intents = IntentsOfReplayTick(simulation.Tick + 1);
            simulation.Step(intents);
            recorder.Step(simulation.Tick, intents);

            // A save at the middle of the run makes the compaction part of the run too, so a
            // change to the snapshot rule moves this hash (F-10, D-651).
            if (simulation.Tick == ReplayTickCount / 2)
            {
                recorder.Save(simulation.Snapshot());
            }
        }

        string text = RunRecordText.Write(recorder.Build());
        RunState replayed = RunReplay.Play(
            RunRecordText.Read(text), ReplayContentHash, map, ReplayBattleContent(), DebugIntentHandlers.None);

        StateHasher hasher = new();
        hasher.AddUInt64(simulation.StateHash());
        hasher.AddUInt64(replayed.StateHash());
        hasher.AddText(text);
        return hasher.Finish();
    }

    /// <summary>
    /// The script of the replay run. The menu opens and closes four times, so the run reads
    /// a world that runs and a world that a menu pauses (D-162, D-650).
    /// </summary>
    /// <remarks>
    /// The party also walks, so the run reads the step rule, the walked-tile record, and the
    /// wall of the map on every leg (D-567, D-716). Every step intent lies outside the ticks
    /// of the open menu, because a menu pauses the world and a step intent then fails (T-2).
    /// </remarks>
    private static IReadOnlyList<Intent> IntentsOfReplayTick(long tick)
    {
        long inCycle = tick % 150;
        if (inCycle == 37)
        {
            return [Intent.OfPlayer(IntentIds.OpenMenu)];
        }

        if (inCycle == 96)
        {
            return [Intent.OfPlayer(IntentIds.CloseMenu)];
        }

        if (inCycle >= 1 && inCycle <= 30)
        {
            return [Intent.OfPlayer(IntentIds.MoveEast)];
        }

        if (inCycle >= 100 && inCycle <= 130)
        {
            return [Intent.OfPlayer(IntentIds.MoveSouth)];
        }

        return [];
    }

    /// <summary>
    /// The battle content of both runs of this set. The set holds its own copy, because Core
    /// reads no file and a content change must never move a hash of this set (G-1, D-495).
    /// </summary>
    private static BattleContent ReplayBattleContent() =>
        new(
            BattleRules.Read(Encoding.UTF8.GetBytes(BattleRulesFile), "identity-set-battle.json"),
            BattleFixture.Read(Encoding.UTF8.GetBytes(BattleFixtureFile), "identity-set-fixture.json"),
            [
                EnemyRecord.Read(Encoding.UTF8.GetBytes(BruteRecordFile), "identity-set-brute.json"),
                EnemyRecord.Read(Encoding.UTF8.GetBytes(GruntRecordFile), "identity-set-grunt.json"),
            ],
            AbilityList.Read(Encoding.UTF8.GetBytes(AbilityFile), "identity-set-abilities.json"));

    /// <summary>
    /// Runs a scripted battle, writes the record, reads the text of it again, and replays it
    /// (exit test 7 of PR-9). The party steps into the guard, fights with every action, saves
    /// in the middle of the battle, and walks on after the wait intent (D-522, D-531, D-532).
    /// The battle run and the enemy-record run each give one map (exit test 5 of PR-80).
    /// </summary>
    private static ulong ComputeBattle(string mapFile)
    {
        GameMap map = GameMap.Read(Encoding.UTF8.GetBytes(mapFile), "identity-set-battle-map.json");
        BattleContent content = ReplayBattleContent();
        RunHeader header = RunHeader.ForThisBuild(ReplayContentHash, RunSeed);
        Simulation simulation = Simulation.Start(RunSeed, map, content, DebugIntentHandlers.None);
        RunRecorder recorder = new(header, simulation.Snapshot());
        StateHasher hasher = new();
        int turns = 0;
        bool saved = false;

        for (int step = 0; step < BattleTickCount; step += 1)
        {
            IReadOnlyList<Intent> intents = IntentsOfBattleTick(simulation.State, turns);
            if (intents.Count > 0 && simulation.State.Battle is not null)
            {
                turns += 1;
            }

            simulation.Step(intents);
            recorder.Step(simulation.Tick, intents);
            foreach (BattleEvent battleEvent in simulation.TakeBattleEvents())
            {
                hasher.AddText(battleEvent.Describe());
            }

            // A save inside the battle makes the battle part of the snapshot too (D-531).
            if (!saved && turns == 3)
            {
                recorder.Save(simulation.Snapshot());
                saved = true;
            }
        }

        string text = RunRecordText.Write(recorder.Build());
        RunState replayed = RunReplay.Play(
            RunRecordText.Read(text), ReplayContentHash, map, content, DebugIntentHandlers.None);

        hasher.AddUInt64(simulation.StateHash());
        hasher.AddUInt64(replayed.StateHash());
        hasher.AddText(text);
        return hasher.Finish();
    }

    /// <summary>
    /// The script of the battle run. It reads the state, and the record holds each intent,
    /// so the replay needs no script (D-493). Each turn of a character takes the next action
    /// of a fixed cycle, so the run reads every action of a character.
    /// </summary>
    private static IReadOnlyList<Intent> IntentsOfBattleTick(RunState state, int turns)
    {
        if (state.Battle is not Battle battle)
        {
            return state.Party.Patrols.Encounter is null ? [Intent.OfPlayer(IntentIds.MoveEast)] : [];
        }

        if (battle.Outcome == BattleOutcome.Won || battle.Outcome == BattleOutcome.Fled)
        {
            return [Intent.OfPlayer(IntentIds.WaitBattleEnd)];
        }

        if (battle.Outcome != BattleOutcome.Running || battle.Next() is not Combatant next || next.Side != BattleSide.Party)
        {
            return [];
        }

        BattleTarget self = next.Target;
        return (turns % 6) switch
        {
            1 => [Intent.OfPlayer(IntentIds.BattleDefend)],
            2 => [Intent.OfPlayer(IntentIds.BattleStep)],
            4 => [Intent.OfPlayer(IntentIds.BattleStep)],
            5 => [Intent.OfPlayer(IntentIds.BattleItem, self, ContentId.Parse("item.identity_draught", "identity", "item"))],
            _ => [Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null)],
        };
    }

    private static ulong ComputeStateHash()
    {
        StateHasher hasher = new();

        hasher.AddInt32(SimulationVersion.Current);
        hasher.AddInt32(int.MinValue);
        hasher.AddInt32(int.MaxValue);
        hasher.AddInt64(long.MinValue);
        hasher.AddInt64(long.MaxValue);
        hasher.AddUInt64(ulong.MaxValue);
        hasher.AddBoolean(true);
        hasher.AddBoolean(false);
        hasher.AddText(string.Empty);
        hasher.AddText("the thing below");

        // The text below holds characters outside the ASCII range, so the run proves that
        // the UTF-8 bytes are the same on every leg (T-7, F-39).
        hasher.AddText("a lantern, a knife, and a door é中🔑");
        return hasher.Finish();
    }
}
