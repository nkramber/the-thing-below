using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
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
public static partial class IdentitySet
{
    /// <summary>The name of the run that reads the fixed-point math (D-641).</summary>
    public const string BasisPointsRun = "basis-points";

    /// <summary>The name of the run that reads a battle, its snapshot, and its replay (D-531, D-532).</summary>
    public const string BattleRun = "battle";

    /// <summary>The name of the run that fights an enemy record with an ability (exit test 5 of PR-80, D-557, D-787).</summary>
    public const string EnemyRecordRun = "enemy-record";

    /// <summary>The name of the run that fights with every element level and every status (G-17, D-504, D-793).</summary>
    public const string StatusRun = "statuses";

    /// <summary>The name of the run that plays two story scenes with every kind of step, a pause, and a battle (D-540, D-997).</summary>
    public const string StoryRun = "story";

    /// <summary>The name of the run that reads the stream split (D-643).</summary>
    public const string StreamSplitRun = "stream-split";

    /// <summary>The name of the run that holds the torch out on a dark map, where a guard sees it from farther away (D-1062 to D-1064).</summary>
    public const string TorchRun = "torch";

    /// <summary>The name of the run that reads the bounded draws of a stream (D-642).</summary>
    public const string RandomDrawsRun = "random-draws";

    /// <summary>The name of the run that reads the state hash (D-644).</summary>
    public const string StateHashRun = "state-hash";

    /// <summary>The name of the run that reads the tick, the record, and the replay (D-650 to D-652).</summary>
    public const string ReplayRun = "replay";

    /// <summary>The name of the run that casts from the menu and uses the forms of lessons in a fight (D-391, D-1027, D-1028).</summary>
    public const string LessonRun = "lessons";

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
     "region": "region.identity",
     "label": "label.identity_run",
     "time": "day",
     "dark": false,
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
       "step_ticks": 16,
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
       "step_ticks": 32,
       "sight_range": 0,
       "areas": [
        { "times": ["dawn", "day", "dusk", "night"], "x": 1, "y": 4, "width": 2, "height": 2 }
       ]
      }
     ],
     "triggers": []
    }
    """;

    /// <summary>The run of the evaluator: a fight against a brute and a mender (D-504, D-955).</summary>
    public const string EvaluatorRun = "evaluator";

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
     "region": "region.identity",
     "label": "label.identity_battle",
     "time": "day",
     "dark": false,
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
       "step_ticks": 16,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 3, "y": 1 }] }
       ]
      }
     ],
     "triggers": []
    }
    """;

    /// <summary>
    /// The map of the torch run: a dark hall with a guard that faces the party and sees 2 tiles
    /// (D-1062, D-1063). The party holds the torch out, so the guard sees it 6 tiles away and the
    /// fight starts 4 tiles before it would in the dark.
    /// </summary>
    private const string TorchMapFile = """
    {
     "comment": "The map of the torch run of the identity set. PR-91 added it, and the map never changes again.",
     "id": "map.identity_torch",
     "region": "region.identity",
     "label": "label.identity_torch",
     "time": "night",
     "dark": true,
     "terrain": [
      "################",
      "#..............#",
      "#..............#",
      "################"
     ],
     "things": [
      { "id": "spawn_point.identity_torch_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.identity_torch_guard",
       "group": "group.identity_battle",
       "size": "common",
       "facing": "west",
       "step_ticks": 16,
       "sight_range": 2,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 12, "y": 1 }] }
       ]
      }
     ],
     "triggers": []
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
     "region": "region.identity",
     "label": "label.identity_record",
     "time": "day",
     "dark": false,
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
       "step_ticks": 16,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 3, "y": 1 }] }
       ]
      }
     ],
     "triggers": []
    }
    """;

    /// <summary>
    /// The map of the statuses run: the battle map, with a guard of the status group. The
    /// brute of that group is weak to fire, resists ice, absorbs dark, and refuses sleep, so
    /// the run reads each level of the element table and the immune list (D-794, D-805).
    /// </summary>
    private const string StatusMapFile = """
    {
     "comment": "The map of the statuses run of the identity set. PR-66 added it, and the map never changes again.",
     "id": "map.identity_status",
     "region": "region.identity",
     "label": "label.identity_status",
     "time": "day",
     "dark": false,
     "terrain": [
      "#########",
      "#.......#",
      "#.......#",
      "#########"
     ],
     "things": [
      { "id": "spawn_point.identity_status_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.identity_status_guard",
       "group": "group.identity_status",
       "size": "common",
       "facing": "east",
       "step_ticks": 16,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 3, "y": 1 }] }
       ]
      }
     ],
     "triggers": []
    }
    """;

    /// <summary>
    /// The map of the evaluator run: the battle map, with a guard of the evaluator group. The
    /// mender of that group heals, and the brute strikes with its ability, so the run reads each
    /// kind of move and each term of a score (D-955, D-959).
    /// </summary>
    private const string EvaluatorMapFile = """
    {
     "comment": "The map of the evaluator run of the identity set. PR-11 added it, and the map never changes again.",
     "id": "map.identity_evaluator",
     "region": "region.identity",
     "label": "label.identity_evaluator",
     "time": "day",
     "dark": false,
     "terrain": [
      "#########",
      "#.......#",
      "#.......#",
      "#########"
     ],
     "things": [
      { "id": "spawn_point.identity_evaluator_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.identity_evaluator_guard",
       "group": "group.identity_evaluator",
       "size": "common",
       "facing": "east",
       "step_ticks": 16,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 3, "y": 1 }] }
       ]
      }
     ],
     "triggers": []
    }
    """;

    /// <summary>The mender record of this set, which heals (D-955). PR-11 added it, and it never changes.</summary>
    private const string MenderRecordFile = """
    {
     "comment": "The mender of the identity set. It never changes.",
     "id": "enemy.identity_mender",
     "size": "common",
     "level": 1,
     "experience": 8,
     "health": 30,
     "attack": 5,
     "magic": 8,
     "defense": 3,
     "resistance": 4,
     "speed": 95,
     "abilities": ["ability.identity_mend"],
     "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" },
     "immune": []
    }
    """;

    /// <summary>
    /// The group file of this set (D-957). The guard group holds a wave, so the run reads the
    /// step of a waiting enemy too (D-778). PR-11 moved the groups here from the fixture, and
    /// the file never changes again.
    /// </summary>
    private const string GroupFileText = """
    {
     "comment": "The groups of the identity set. PR-9 added them to the fixture, and PR-11 moved them to this file with a profile for each enemy.",
     "region": "region.identity",
     "groups": [
      {
       "id": "group.identity_run",
       "boss": false,
       "enemies": [{ "enemy": "enemy.identity_grunt", "row": "front", "waits": false, "profile": "profile.identity_brute" }]
      },
      {
       "id": "group.identity_record",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.identity_brute", "row": "front", "waits": false, "profile": "profile.identity_brute" },
        { "enemy": "enemy.identity_grunt", "row": "back", "waits": false, "profile": "profile.identity_brute" }
       ]
      },
      {
       "id": "group.identity_status",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.identity_brute", "row": "front", "waits": false, "profile": "profile.identity_brute" },
        { "enemy": "enemy.identity_grunt", "row": "front", "waits": false, "profile": "profile.identity_brute" }
       ]
      },
      {
       "id": "group.identity_battle",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.identity_grunt", "row": "front", "waits": false, "profile": "profile.identity_brute" },
        { "enemy": "enemy.identity_grunt", "row": "back", "waits": false, "profile": "profile.identity_brute" },
        { "enemy": "enemy.identity_grunt", "row": "front", "waits": true, "profile": "profile.identity_brute" }
       ]
      },
      {
       "id": "group.identity_evaluator",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.identity_brute", "row": "front", "waits": false, "profile": "profile.identity_brute" },
        { "enemy": "enemy.identity_mender", "row": "back", "waits": false, "profile": "profile.identity_mender" }
       ]
      }
     ]
    }
    """;

    /// <summary>The profile of each brute and grunt of this set (D-956). PR-11 added it, and PR-13 added the drop list and the charm of the steal list (D-1042, D-1051).</summary>
    private const string BruteProfileFile = """
    {
     "comment": "The profile of the brutes of the identity set. It never changes.",
     "id": "profile.identity_brute",
     "weights": { "damage": 100, "kills": 3, "threat": 60, "healing": 50, "timeline": 1, "row": 200 },
     "steal_chance": 2500,
     "steal_gear_chance": 2500,
     "steal": [{ "item": "item.identity_draught" }, { "gold": 12 }, { "gear": "gear.identity_charm" }],
     "drops": [{ "item": "item.identity_draught", "chance": 3000 }]
    }
    """;

    /// <summary>The profile of the mender of this set (D-956). PR-11 added it, and PR-13 added the empty drop list (D-1042).</summary>
    private const string MenderProfileFile = """
    {
     "comment": "The profile of the mender of the identity set. It never changes.",
     "id": "profile.identity_mender",
     "weights": { "damage": 40, "kills": 1, "threat": 80, "healing": 300, "timeline": 1, "row": 400 },
     "steal_chance": 0,
     "steal_gear_chance": 0,
     "steal": [],
     "drops": []
    }
    """;

    /// <summary>The grunt record of this set. PR-9 gave the stats, and PR-80 moved them to a record. It never changes.</summary>
    private const string GruntRecordFile = """
    {
     "comment": "The grunt of the identity set. It never changes.",
     "id": "enemy.identity_grunt",
     "size": "common",
     "level": 1,
     "experience": 6,
     "health": 20,
     "attack": 6,
     "magic": 2,
     "defense": 2,
     "resistance": 1,
     "speed": 90,
     "abilities": [],
     "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" },
     "immune": []
    }
    """;

    /// <summary>The brute record of this set, which names an ability (D-787). PR-80 added it, and it never changes.</summary>
    private const string BruteRecordFile = """
    {
     "comment": "The brute of the identity set. It never changes.",
     "id": "enemy.identity_brute",
     "size": "common",
     "level": 2,
     "experience": 15,
     "health": 45,
     "attack": 11,
     "magic": 4,
     "defense": 5,
     "resistance": 2,
     "speed": 80,
     "abilities": ["ability.identity_strike"],
     "elements": { "fire": "weak", "ice": "resist", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "absorb" },
     "immune": ["sleep"]
    }
    """;

    /// <summary>The item file of this set (D-1038, D-1046). PR-13 added it with the draught of PR-9.</summary>
    private const string ItemFile = """
    {
     "comment": "The item file of the identity set. PR-13 added it, and PR-91 added the torch.",
     "items": [
      { "id": "item.identity_draught", "kind": "heal", "limit": 10, "delay": 100, "amount": 30 },
      { "id": "item.torch", "kind": "key", "limit": 1 }
     ]
    }
    """;

    /// <summary>The gear file of this set (D-1036). PR-13 added it: a weapon, a ring that resists fire, and a charm that is weak to fire.</summary>
    private const string GearFile = """
    {
     "comment": "The gear file of the identity set. PR-13 added it.",
     "gear": [
      { "id": "gear.identity_blade", "slot": "weapon", "limit": 1, "attack": 3, "magic": 0, "defense": 0, "resistance": 0, "speed": -2, "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.identity_ring", "slot": "accessory", "limit": 1, "attack": 0, "magic": 0, "defense": 1, "resistance": 2, "speed": 0, "elements": { "fire": "resist", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.identity_charm", "slot": "accessory", "limit": 1, "attack": 0, "magic": 2, "defense": 0, "resistance": 0, "speed": 4, "elements": { "fire": "weak", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } }
     ]
    }
    """;

    /// <summary>The notice file of this set (D-989). PR-62 added it, and it never changes.</summary>
    private const string NoticeFile = """
    {
     "comment": "The notices of the identity set.",
     "notices": [
      { "id": "notice.identity_kept", "log": true },
      { "id": "notice.identity_plain", "log": false }
     ]
    }
    """;

    /// <summary>The ability file of this set (D-785). PR-80 added it, and it never changes.</summary>
    private const string AbilityFile = """
    {
     "comment": "The ability file of the identity set. PR-11 gave each ability its effect and added the mend. PR-12 gave the strike its status field and added the moves of the lessons, PR-13 added the steal, and PR-99 added the stat of a strike and the base and the power of a heal.",
     "abilities": [
      { "id": "ability.identity_strike", "kind": "strike", "delay": 120, "power": 14000, "stat": "magic", "element": "fire", "reach": "any", "status": "none" },
      { "id": "ability.identity_mend", "kind": "heal", "delay": 100, "base": 12, "power": 5000 },
      { "id": "ability.identity_blast", "kind": "strike", "delay": 130, "power": 20000, "stat": "magic", "element": "fire", "reach": "any", "status": "poison", "chance": 5000 },
      { "id": "ability.identity_purge", "kind": "cure", "delay": 90, "statuses": ["poison", "blind", "silence"] },
      { "id": "ability.identity_haste", "kind": "boon", "delay": 90, "status": "haste" },
      { "id": "ability.identity_pilfer", "kind": "steal", "delay": 100 }
     ]
    }
    """;

    /// <summary>The lesson file of this set (D-1026). PR-12 added it.</summary>
    private const string LessonFile = """
    {
     "comment": "The lesson file of the identity set. PR-12 added it, and PR-13 added the steal drill.",
     "lessons": [
      { "id": "lesson.identity_blast", "kind": "harm", "forms": [
       { "ability": "ability.identity_strike", "points": 0, "mp": 3, "description": "lesson.identity_strike" },
       { "ability": "ability.identity_blast", "points": 6, "mp": 5, "description": "lesson.identity_blast" } ] },
      { "id": "lesson.identity_mend", "kind": "mend", "forms": [
       { "ability": "ability.identity_mend", "points": 0, "mp": 2, "description": "lesson.identity_mend" } ] },
      { "id": "lesson.identity_purge", "kind": "mend", "forms": [
       { "ability": "ability.identity_purge", "points": 0, "mp": 1, "description": "lesson.identity_purge" } ] },
      { "id": "lesson.identity_haste", "kind": "boon", "forms": [
       { "ability": "ability.identity_haste", "points": 0, "mp": 2, "description": "lesson.identity_haste" } ] },
      { "id": "lesson.identity_pilfer", "kind": "theft", "forms": [
       { "ability": "ability.identity_pilfer", "points": 0, "mp": 0, "description": "lesson.identity_pilfer" } ] }
     ]
    }
    """;

    /// <summary>The battle rules of this set, with the numbers of D-777. They never change.</summary>
    private const string BattleRulesFile = """
    {
     "comment": "The battle rules of the identity set. PR-9 added them, PR-12 added the lesson slots and the aptitude bonus, and PR-13 added the steal numbers and a third slot for the steal drill.",
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
     "stun_push": 50,
     "flee_base": 5000,
     "flee_per_speed": 100,
     "flee_floor": 1000,
     "flee_ceiling": 9000,
     "item_rate": 5000,
     "steal_rate": 5000,
     "steal_floor": 0,
     "steal_ceiling": 9000,
     "steal_gear_first": 500,
     "steal_gear_second": 1500,
     "steal_gear_third": 2500,
     "weak_rate": 15000,
     "resist_rate": 5000,
     "absorb_rate": 2500,
     "poison_share": 500,
     "bleed_share": 1000,
     "bleed_ticks": 300,
     "regen_share": 1000,
     "regen_ticks": 400,
     "sleep_ticks": 300,
     "haste_ticks": 400,
     "slow_ticks": 400,
     "stun_ticks": 50,
     "shell_cut": 5000,
     "shell_ticks": 400,
     "blind_miss": 3000,
     "experience_cut": 1500,
     "experience_gap": 4,
     "lesson_slots": 3,
     "aptitude_bonus": 2500,
     "level_experience": [0, 20, 60, 120, 200, 300, 420, 560, 720, 900, 1100, 1320, 1560, 1820, 2100, 2400, 2720, 3060, 3420, 3800, 4200, 4620, 5060, 5520, 6000, 6500, 7020, 7560, 8120, 8700, 9300, 9920, 10560, 11220, 11900, 12600, 13320, 14060, 14820, 15600],
     "lesson_slot_levels": [5, 12, 20, 30]
    }
    """;

    /// <summary>
    /// The battle fixture of this set: the character, the item, and the start of a run. PR-11
    /// moved its groups to the group file of this set (D-957).
    /// </summary>
    private static readonly string BattleFixtureFile = $$"""
    {
     "comment": "The battle fixture of the identity set. PR-9 added it, PR-80 moved its enemies to the enemy records, PR-11 moved its groups to the group file, PR-68 added the friend who joins in the story run, PR-12 added the aptitudes, the start lessons, and the lesson pack, PR-13 moved the item to the item file and added the gear, PR-99 added the magic and the resistance, and PR-91 added the torch.",
     "characters": [
      { "id": "character.identity_hero", "row": "front", "join_level": 1, "main_aptitude": "blade", "side_aptitude": "guard", "side_flag": "flag.identity_side", "curve": {{StatCurve.FlatText(new StatRow(90, 20, 14, 6, 4, 3, 100))}} },
      { "id": "character.identity_friend", "row": "back", "join_level": 1, "main_aptitude": "mend", "side_aptitude": "harm", "side_flag": "flag.identity_side", "curve": {{StatCurve.FlatText(new StatRow(70, 30, 10, 14, 3, 6, 110))}} }
     ],
     "start_party": ["character.identity_hero"],
     "pack": [{ "item": "item.identity_draught", "count": 9 }, { "item": "item.torch", "count": 1 }, { "gear": "gear.identity_charm", "count": 1 }],
     "start_gear": [{ "character": "character.identity_hero", "gear": ["gear.identity_blade", "gear.identity_ring"] }],
     "start_lessons": [{ "character": "character.identity_hero", "lessons": ["lesson.identity_blast", "lesson.identity_mend", "lesson.identity_pilfer"] }],
     "lesson_pack": ["lesson.identity_purge", "lesson.identity_haste"]
    }
    """;

    /// <summary>Every run of the set, in the order of the identity file.</summary>
    /// <remarks>The order is the ordinal order of the names, which every machine reads the same (F-39).</remarks>
    public static readonly IReadOnlyList<string> RunNames =
    [
        BasisPointsRun,
        BattleRun,
        EnemyRecordRun,
        EvaluatorRun,
        LessonRun,
        RandomDrawsRun,
        ReplayRun,
        StateHashRun,
        StatusRun,
        StoryRun,
        StreamSplitRun,
        TorchRun,
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
            BattleRun => ComputeBattle(BattleMapFile, IntentsOfBattleTick),
            EnemyRecordRun => ComputeBattle(RecordMapFile, IntentsOfBattleTick),
            EvaluatorRun => ComputeBattle(EvaluatorMapFile, IntentsOfBattleTick),
            LessonRun => ComputeBattle(BattleMapFile, IntentsOfLessonTick),
            RandomDrawsRun => ComputeRandomDraws(),
            ReplayRun => ComputeReplay(),
            StateHashRun => ComputeStateHash(),
            StatusRun => ComputeStatuses(),
            StoryRun => ComputeStory(),
            StreamSplitRun => ComputeStreamSplit(),
            TorchRun => ComputeBattle(TorchMapFile, IntentsOfTorchTick),
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
        Simulation simulation = Simulation.Start(RunSeed, map, ReplayBattleContent(), ReplayNotices(), NoStory(), DebugIntentHandlers.None);
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
            RunRecordText.Read(text), ReplayContentHash, map, ReplayBattleContent(), ReplayNotices(), NoStory(), DebugIntentHandlers.None);

        StateHasher hasher = new();
        hasher.AddUInt64(simulation.StateHash());
        hasher.AddUInt64(replayed.StateHash());
        hasher.AddText(text);
        return hasher.Finish();
    }

    /// <summary>
    /// The script of the replay run. The menu opens and closes four times, so the run reads
    /// a world that runs and a world that a menu pauses (D-162, D-650). The party window moves
    /// the lead to the other row inside each open menu (D-558).
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

        if (inCycle == 50)
        {
            return [Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(BattleSide.Party, 0), null)];
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
                EnemyRecord.Read(Encoding.UTF8.GetBytes(MenderRecordFile), "identity-set-mender.json"),
            ],
            AbilityList.Read(Encoding.UTF8.GetBytes(AbilityFile), "identity-set-abilities.json"),
            LessonList.Read(Encoding.UTF8.GetBytes(LessonFile), "identity-set-lessons.json"),
            ItemList.Read(Encoding.UTF8.GetBytes(ItemFile), "identity-set-items.json"),
            GearList.Read(Encoding.UTF8.GetBytes(GearFile), "identity-set-gear.json"),
            [GroupFile.Read(Encoding.UTF8.GetBytes(GroupFileText), $"{GroupFile.Folder}identity.json")],
            [
                ProfileRecord.Read(Encoding.UTF8.GetBytes(BruteProfileFile), "identity-set-brute-profile.json"),
                ProfileRecord.Read(Encoding.UTF8.GetBytes(MenderProfileFile), "identity-set-mender-profile.json"),
            ]);

    /// <summary>The notice file of every run of this set (D-989), from its own copy for the reason of <see cref="ReplayBattleContent"/>.</summary>
    private static NoticeList ReplayNotices() => NoticeList.Read(Encoding.UTF8.GetBytes(NoticeFile), "identity-set-notices.json");

    /// <summary>
    /// Runs a scripted battle, writes the record, reads the text of it again, and replays it
    /// (exit test 7 of PR-9). The party steps into the guard, fights with every action, saves
    /// in the middle of the battle, and walks on after the wait intent (D-522, D-531, D-532).
    /// The battle run, the enemy-record run, and the evaluator run each give one map (exit
    /// test 5 of PR-80, D-504).
    /// </summary>
    private static ulong ComputeBattle(string mapFile, Func<RunState, int, int, IReadOnlyList<Intent>> script)
    {
        GameMap map = GameMap.Read(Encoding.UTF8.GetBytes(mapFile), "identity-set-battle-map.json");
        BattleContent content = ReplayBattleContent();
        RunHeader header = RunHeader.ForThisBuild(ReplayContentHash, RunSeed);
        Simulation simulation = Simulation.Start(RunSeed, map, content, ReplayNotices(), NoStory(), DebugIntentHandlers.None);
        RunRecorder recorder = new(header, simulation.Snapshot());
        StateHasher hasher = new();
        int turns = 0;
        bool saved = false;

        for (int step = 0; step < BattleTickCount; step += 1)
        {
            IReadOnlyList<Intent> intents = script(simulation.State, turns, step);
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
            RunRecordText.Read(text), ReplayContentHash, map, content, ReplayNotices(), NoStory(), DebugIntentHandlers.None);

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
    private static IReadOnlyList<Intent> IntentsOfBattleTick(RunState state, int turns, int step)
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

    /// <summary>
    /// The script of the torch run (D-1064, D-1071). The party holds the torch out on the first
    /// tick and walks at the guard, so the bonus of the torch starts the fight (D-1063). After
    /// the fight, the party puts the torch away and walks on.
    /// </summary>
    private static IReadOnlyList<Intent> IntentsOfTorchTick(RunState state, int turns, int step)
    {
        if (step == 0)
        {
            return [Intent.OfPlayer(IntentIds.HoldTorch)];
        }

        bool walks = state.Battle is null && state.Party.Patrols.Encounter is null;
        if (walks && turns > 0 && state.Characters.TorchHeld)
        {
            return [Intent.OfPlayer(IntentIds.PutTorchAway)];
        }

        return IntentsOfBattleTick(state, turns, step);
    }

    /// <summary>
    /// The script of the lesson run (D-391, D-1027, D-1048). The hero casts the mend from the
    /// menu and puts the charm of the pack on, before the fight, then walks into the guard. In
    /// the fight each turn takes the next step of a fixed cycle: the first form of the blast,
    /// the mend on the hero, the second form of the blast once the hero opened it, a steal, and
    /// an attack. A form that the rules refuse, such as a rite with too little MP or a fourth
    /// steal, gives its place to the attack, so the run reads the refusal too.
    /// </summary>
    private static IReadOnlyList<Intent> IntentsOfLessonTick(RunState state, int turns, int step)
    {
        ContentId blast = ContentId.Parse("lesson.identity_blast", "identity", "lesson");
        ContentId mend = ContentId.Parse("lesson.identity_mend", "identity", "lesson");
        ContentId pilfer = ContentId.Parse("lesson.identity_pilfer", "identity", "lesson");
        switch (step)
        {
            case 0:
                return [Intent.OfPlayer(IntentIds.OpenMenu)];
            case 1:
                return [Intent.OfMenuCast(0, mend, 0, 0)];
            case 2:
                return [Intent.OfGearWear(0, 5, ContentId.Parse("gear.identity_charm", "identity", "gear"))];
            case 3:
                return [Intent.OfPlayer(IntentIds.CloseMenu)];
            default:
                break;
        }

        if (state.Battle is not Battle battle || battle.Outcome != BattleOutcome.Running)
        {
            return IntentsOfBattleTick(state, turns, step);
        }

        if (battle.Next() is not Combatant next || next.Side != BattleSide.Party)
        {
            return [];
        }

        Intent attack = Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null);
        Intent chosen = (turns % 5) switch
        {
            0 => Intent.OfBattleLesson(blast, 0, battle.MeleeTargets(BattleSide.Enemy)[0].Target),
            1 => Intent.OfBattleLesson(mend, 0, next.Target),
            2 => Intent.OfBattleLesson(blast, 1, battle.MeleeTargets(BattleSide.Enemy)[0].Target),
            3 => Intent.OfBattleLesson(pilfer, 0, battle.MeleeTargets(BattleSide.Enemy)[0].Target),
            _ => attack,
        };
        BattleChoice choice = new(BattleAction.Lesson, chosen.Target, null, chosen.Lesson, chosen.Option);
        return [chosen.Lesson is null || BattleTurns.RefusalOf(state, choice) is null ? chosen : attack];
    }

    /// <summary>
    /// Runs a scripted fight that reaches each level of the element table and each of the ten
    /// statuses, through the move fields and the status call of D-793. The party steps into
    /// the guard, and each turn of the character takes the next step of a fixed script.
    /// </summary>
    /// <remarks>
    /// No intent carries a move with an element before PR-12, so this run keeps no record. It
    /// writes the snapshot in the middle of the fight, reads it again, and fights on from both
    /// copies. The hash holds every event and both state hashes, so a change of a rule, of the
    /// snapshot, or of the order of the rolls moves it (G-5, T-7).
    /// </remarks>
    private static ulong ComputeStatuses()
    {
        GameMap map = GameMap.Read(Encoding.UTF8.GetBytes(StatusMapFile), "identity-set-status-map.json");
        BattleContent content = ReplayBattleContent();
        Simulation simulation = Simulation.Start(RunSeed, map, content, ReplayNotices(), NoStory(), DebugIntentHandlers.None);
        for (int step = 0; step < BattleTickCount && simulation.State.Battle is null; step += 1)
        {
            _ = simulation.Step(simulation.State.Party.Patrols.Encounter is null ? [Intent.OfPlayer(IntentIds.MoveEast)] : []);
        }

        if (simulation.State.Battle is null)
        {
            throw new SimulationException($"the statuses run, and no battle started in {BattleTickCount} ticks", simulation.State.Context("identity"));
        }

        StateHasher hasher = new();
        Simulation? copy = null;
        for (int turn = 0; turn < StatusTurnLimit && BattleRuns(simulation); turn += 1)
        {
            StatusTurn(simulation.State, turn);
            copy = copy is null ? null : StatusTurnOf(copy, turn);
            if (turn == StatusTurnLimit / 4)
            {
                string text = RunSnapshotText.Write(simulation.Snapshot());
                var reader = new ContentReader(Encoding.UTF8.GetBytes(text), "identity-set-status-snapshot");
                copy = Simulation.Resume(RunSeed, RunSnapshotText.Read(ref reader), map, content, ReplayNotices(), NoStory(), DebugIntentHandlers.None);
                hasher.AddText(text);
            }

            foreach (BattleEvent battleEvent in simulation.TakeBattleEvents())
            {
                hasher.AddText(battleEvent.Describe());
            }
        }

        hasher.AddUInt64(simulation.StateHash());
        hasher.AddUInt64(copy?.StateHash() ?? 0);
        return hasher.Finish();
    }

    /// <summary>The most turns of a character that the statuses run takes.</summary>
    private const int StatusTurnLimit = 40;

    private static bool BattleRuns(Simulation simulation) =>
        simulation.State.Battle is Battle battle && battle.Outcome == BattleOutcome.Running;

    private static Simulation? StatusTurnOf(Simulation copy, int turn)
    {
        if (BattleRuns(copy))
        {
            StatusTurn(copy.State, turn);
        }

        return copy;
    }

    /// <summary>
    /// One turn of the character in the statuses run. The script gives a status to each side
    /// and strikes with each element level in a fixed cycle, so the run reads every rule of
    /// D-794 to D-810 on every leg.
    /// </summary>
    private static void StatusTurn(RunState state, int turn)
    {
        Battle battle = state.Battle ?? throw new SimulationException("a turn of the statuses run, and no battle runs", state.Context("identity"));
        RunContext context = state.Context($"identity/{StatusRun} {turn}");
        List<LogEntry> log = [];
        BattleTarget self = new(BattleSide.Party, 0);
        BattleTarget aimed = battle.MeleeTargets(BattleSide.Enemy)[0].Target;
        int attack = state.BattleContent.Rules.AttackDelay;
        int power = state.BattleContent.Rules.AttackPower;
        switch (turn % 8)
        {
            case 0:
                BattleTurns.GiveStatus(state, self, StatusKind.Regen, context);
                BattleTurns.StrikeWith(state, new BattleMove(attack, power, StrikeStat.Magic, Element.Fire, new StatusChance(StatusKind.Poison, 10000)), aimed, context, log);
                break;
            case 1:
                BattleTurns.StrikeWith(state, new BattleMove(attack, power, StrikeStat.Magic, Element.Ice, new StatusChance(StatusKind.Sleep, 6000)), aimed, context, log);
                break;
            case 2:
                BattleTurns.GiveStatus(state, self, StatusKind.Haste, context);
                BattleTurns.StrikeWith(state, new BattleMove(attack, power, StrikeStat.Magic, Element.Dark, new StatusChance(StatusKind.Bleed, 8000)), aimed, context, log);
                break;
            case 3:
                BattleTurns.GiveStatus(state, self, StatusKind.Shell, context);
                BattleTurns.GiveStatus(state, aimed, StatusKind.Slow, context);
                BattleTurns.StrikeWith(state, new BattleMove(160, power, StrikeStat.Attack, null, new StatusChance(StatusKind.Stun, 10000)), aimed, context, log);
                break;
            case 4:
                BattleTurns.GiveStatus(state, self, StatusKind.Blind, context);
                BattleTurns.GiveStatus(state, aimed, StatusKind.Haste, context);
                BattleTurns.StrikeWith(state, new BattleMove(attack, power, StrikeStat.Magic, Element.Holy, new StatusChance(StatusKind.Stun, 10000)), aimed, context, log);
                break;
            case 5:
                BattleTurns.GiveStatus(state, self, StatusKind.Silence, context);
                BattleTurns.StrikeWith(state, new BattleMove(attack, power, StrikeStat.Magic, Element.Lightning, null), aimed, context, log);
                break;
            case 6:
                BattleTurns.GiveStatus(state, self, StatusKind.Poison, context);
                BattleTurns.StrikeWith(state, BattleMove.BasicAttack(state.BattleContent.Rules), aimed, context, log);
                break;
            default:
                BattleTurns.StrikeWith(state, new BattleMove(attack, power, StrikeStat.Magic, Element.Water, new StatusChance(StatusKind.Sleep, 10000)), aimed, context, log);
                break;
        }
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
