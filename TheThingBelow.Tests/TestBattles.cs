using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tests;

/// <summary>
/// The battle content of the tests: the rules of D-777, a fixture, and a group file of the
/// test region that holds every group that a test map names (D-766, D-957). The tests hold their own copy, so a balance change of PR-30
/// in `content/` moves no test (D-757).
/// </summary>
internal static class TestBattles
{
    /// <summary>The rules of the tests, with the numbers of D-777 and D-779.</summary>
    public const string RulesFile = """
    {
     "comment": "The battle rules of the tests.",
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
     "lesson_slots": 2,
     "aptitude_bonus": 2500,
     "level_experience": [0, 20, 60, 120, 200, 300, 420, 560, 720, 900, 1100, 1320, 1560, 1820, 2100, 2400, 2720, 3060, 3420, 3800, 4200, 4620, 5060, 5520, 6000, 6500, 7020, 7560, 8120, 8700, 9300, 9920, 10560, 11220, 11900, 12600, 13320, 14060, 14820, 15600],
     "lesson_slot_levels": [5, 12, 20, 30]
    }
    """;

    /// <summary>
    /// The fixture of the tests. It holds three characters, and the party starts with Marrek
    /// alone. The groups live in <see cref="GroupsFile"/> (D-957).
    /// </summary>
    public static readonly string FixtureFile = $$"""
    {
     "comment": "The battle fixture of the tests.",
     "characters": [
      { "id": "character.marrek", "row": "front", "join_level": 1, "main_aptitude": "blade", "side_aptitude": "guard", "side_flag": "flag.test_marrek_side", "curve": {{MarrekCurve()}} },
      { "id": "character.test_second", "row": "front", "join_level": 1, "main_aptitude": "harm", "side_aptitude": "mend", "side_flag": "flag.test_second_side", "curve": {{StatCurve.FlatText(new StatRow(50, 12, 10, 10, 3, 3, 110))}} },
      { "id": "character.test_third", "row": "back", "join_level": 1, "main_aptitude": "mend", "side_aptitude": "boon", "side_flag": "flag.test_third_side", "curve": {{StatCurve.FlatText(new StatRow(40, 16, 8, 8, 2, 2, 120))}} }
     ],
     "start_party": ["character.marrek"],
     "pack": [{ "item": "item.fixture_draught", "count": 3 }],
     "start_gear": [],
     "start_lessons": [{ "character": "character.marrek", "lessons": ["lesson.fixture_hew", "lesson.fixture_cinder"] }],
     "lesson_pack": ["lesson.fixture_salve", "lesson.fixture_purge", "lesson.fixture_rot", "lesson.fixture_quicken", "lesson.fixture_bolt"]
    }
    """;

    /// <summary>
    /// The group file of the test region (D-957). Each group of a test map is a grunt or two.
    /// The boss group, the wave group, and the full group serve the battle tests. Each enemy
    /// takes the attacker profile, so an enemy of a test attacks on each turn (D-956).
    /// </summary>
    public const string GroupsFile = """
    {
     "comment": "The groups of the tests.",
     "region": "region.test",
     "groups": [
      { "id": "group.one", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" }] },
      { "id": "group.other", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" }] },
      { "id": "group.ring", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" }] },
      {
       "id": "group.test_pair",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" }
       ]
      },
      {
       "id": "group.test_elite",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_brute", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_attacker" }
       ]
      },
      { "id": "group.test_boss", "boss": true, "enemies": [{ "enemy": "enemy.fixture_brute", "row": "front", "waits": false, "profile": "profile.test_attacker" }] },
      {
       "id": "group.test_full",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": true, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true, "profile": "profile.test_attacker" }
       ]
      }
     ]
    }
    """;

    /// <summary>The path of the group file of the test region (D-957).</summary>
    public const string GroupsPath = "rules/groups/test.json";

    /// <summary>
    /// The group file of the fixture region of the tests (D-957). It holds the test copy of the
    /// two groups of the fixture map, so a test of the checkout map finds its region.
    /// </summary>
    public const string FixtureGroupsFile = """
    {
     "comment": "The fixture groups of the tests.",
     "region": "region.fixture",
     "groups": [
      {
       "id": "group.fixture_pair",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" }
       ]
      },
      {
       "id": "group.fixture_elite",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_brute", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true, "profile": "profile.test_attacker" }
       ]
      }
     ]
    }
    """;

    /// <summary>The path of the group file of the fixture region (D-957).</summary>
    public const string FixtureGroupsPath = "rules/groups/fixture.json";

    /// <summary>
    /// The profile of each enemy of the tests: damage alone (D-956, D-958). The basic attack
    /// then wins each turn, so a test of a rule reads the same fight as before the evaluator.
    /// </summary>
    public const string AttackerProfileFile = """
    {
     "comment": "The attacker profile of the tests.",
     "id": "profile.test_attacker",
     "weights": { "damage": 100, "kills": 0, "threat": 0, "healing": 0, "timeline": 0, "row": 0 },
     "steal_chance": 3000,
     "steal_gear_chance": 0,
     "steal": [{ "item": "item.fixture_draught" }, { "gold": 5 }],
     "drops": []
    }
    """;

    /// <summary>
    /// The item file of the tests (D-1038, D-1046): one item of each effect and a key item.
    /// The fixture pack holds the draught alone, so a test adds the others with a pick.
    /// </summary>
    public const string ItemsFile = """
    {
     "comment": "The item file of the tests.",
     "items": [
      { "id": "item.fixture_draught", "kind": "heal", "limit": 5, "delay": 100, "amount": 30 },
      { "id": "item.test_tonic", "kind": "restore", "limit": 4, "delay": 100, "amount": 10 },
      { "id": "item.test_salts", "kind": "cure", "limit": 5, "delay": 90, "statuses": ["poison", "silence"] },
      { "id": "item.test_root", "kind": "revive", "limit": 3, "delay": 120, "amount": 25 },
      { "id": "item.test_token", "kind": "key", "limit": 1 },
      { "id": "item.torch", "kind": "key", "limit": 1 }
     ]
    }
    """;

    /// <summary>
    /// The gear file of the tests (D-1036, D-1037): a piece of each slot kind, and three
    /// accessories that resist, absorb, and are weak to fire. Marrek of the tests wears none,
    /// so every number of the battle tests stays.
    /// </summary>
    public const string GearFile = """
    {
     "comment": "The gear file of the tests.",
     "gear": [
      { "id": "gear.test_blade", "slot": "weapon", "limit": 2, "attack": 5, "magic": 0, "defense": 0, "resistance": 0, "speed": -3, "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.test_shield", "slot": "off_hand", "limit": 1, "attack": 0, "magic": 0, "defense": 3, "resistance": 0, "speed": 0, "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.test_helm", "slot": "head", "limit": 1, "attack": 0, "magic": 0, "defense": 1, "resistance": 0, "speed": 0, "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.test_mail", "slot": "body", "limit": 1, "attack": 0, "magic": 0, "defense": 4, "resistance": 0, "speed": -99, "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.test_resist_ring", "slot": "accessory", "limit": 3, "attack": 0, "magic": 0, "defense": 0, "resistance": 0, "speed": 0, "elements": { "fire": "resist", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.test_absorb_ring", "slot": "accessory", "limit": 3, "attack": 0, "magic": 0, "defense": 0, "resistance": 0, "speed": 0, "elements": { "fire": "absorb", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } },
      { "id": "gear.test_weak_charm", "slot": "accessory", "limit": 3, "attack": 1, "magic": 2, "defense": 0, "resistance": -1, "speed": 2, "elements": { "fire": "weak", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" } }
     ]
    }
    """;

    /// <summary>The path of the attacker profile of the tests (D-956).</summary>
    public const string AttackerProfilePath = "rules/profiles/test-attacker.json";

    /// <summary>The grunt record of the tests, with the stats of D-777 and no ability (D-786, D-787).</summary>
    public const string GruntFile = """
    {
     "comment": "The grunt of the tests.",
     "id": "enemy.fixture_grunt",
     "size": "common",
     "level": 1,
     "experience": 6,
     "health": 30,
     "attack": 8,
     "magic": 8,
     "defense": 2,
     "resistance": 2,
     "speed": 90,
     "abilities": [],
     "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" },
     "immune": []
    }
    """;

    /// <summary>The brute record of the tests, with the stats of D-777, the size of D-789, and one ability (D-786, D-787).</summary>
    public const string BruteFile = """
    {
     "comment": "The brute of the tests.",
     "id": "enemy.fixture_brute",
     "size": "elite",
     "level": 3,
     "experience": 20,
     "health": 80,
     "attack": 14,
     "magic": 14,
     "defense": 6,
     "resistance": 6,
     "speed": 80,
     "abilities": ["ability.fixture_bash"],
     "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" },
     "immune": []
    }
    """;

    /// <summary>
    /// The ability file of the tests (D-785, D-955). The bash of the brute strikes weaker than
    /// the basic attack, so the attacker profile keeps the basic attack. The mend and the shot
    /// serve the tests of the evaluator.
    /// </summary>
    public const string AbilitiesFile = """
    {
     "comment": "The ability file of the tests.",
     "abilities": [
      { "id": "ability.fixture_bash", "kind": "strike", "delay": 100, "power": 5000, "stat": "attack", "element": "none", "reach": "melee", "status": "none" },
      { "id": "ability.test_mend", "kind": "heal", "delay": 100, "base": 20, "power": 1 },
      { "id": "ability.test_shot", "kind": "strike", "delay": 100, "power": 15000, "stat": "attack", "element": "none", "reach": "any", "status": "none" },
      { "id": "ability.fixture_hew", "kind": "strike", "delay": 110, "power": 15000, "stat": "attack", "element": "none", "reach": "melee", "status": "none" },
      { "id": "ability.fixture_cleave", "kind": "strike", "delay": 120, "power": 20000, "stat": "attack", "element": "none", "reach": "melee", "status": "none" },
      { "id": "ability.fixture_cinder", "kind": "strike", "delay": 120, "power": 14000, "stat": "magic", "element": "fire", "reach": "any", "status": "none" },
      { "id": "ability.fixture_blaze", "kind": "strike", "delay": 140, "power": 22000, "stat": "magic", "element": "fire", "reach": "any", "status": "none" },
      { "id": "ability.fixture_rot", "kind": "strike", "delay": 110, "power": 5000, "stat": "magic", "element": "none", "reach": "any", "status": "poison", "chance": 6000 },
      { "id": "ability.fixture_salve", "kind": "heal", "delay": 120, "base": 30, "power": 1 },
      { "id": "ability.fixture_purge", "kind": "cure", "delay": 100, "statuses": ["poison", "blind", "silence"] },
      { "id": "ability.fixture_quicken", "kind": "boon", "delay": 100, "status": "haste" },
      { "id": "ability.fixture_bolt", "kind": "strike", "delay": 100, "power": 12000, "stat": "magic", "element": "none", "reach": "any", "status": "none" },
      { "id": "ability.test_pilfer", "kind": "steal", "delay": 100 }
     ]
    }
    """;

    /// <summary>
    /// The lesson file of the tests (D-1026). The hew and the cinder open a second form, and
    /// the others cover the heal, the cure, the status strike, the boon, and the shot (D-1029).
    /// The ids match the checkout, so a run of the checkout replays on this content. Marrek
    /// starts with the hew and the cinder, and the rest start in the lesson pack.
    /// </summary>
    public const string LessonsFile = """
    {
     "comment": "The lesson file of the tests.",
     "lessons": [
      { "id": "lesson.fixture_hew", "kind": "blade", "forms": [
       { "ability": "ability.fixture_hew", "points": 0, "mp": 0, "description": "lesson.fixture_hew" },
       { "ability": "ability.fixture_cleave", "points": 60, "mp": 0, "description": "lesson.fixture_cleave" } ] },
      { "id": "lesson.fixture_cinder", "kind": "harm", "forms": [
       { "ability": "ability.fixture_cinder", "points": 0, "mp": 4, "description": "lesson.fixture_cinder" },
       { "ability": "ability.fixture_blaze", "points": 120, "mp": 9, "description": "lesson.fixture_blaze" } ] },
      { "id": "lesson.fixture_salve", "kind": "mend", "forms": [
       { "ability": "ability.fixture_salve", "points": 0, "mp": 3, "description": "lesson.fixture_salve" } ] },
      { "id": "lesson.fixture_purge", "kind": "mend", "forms": [
       { "ability": "ability.fixture_purge", "points": 0, "mp": 2, "description": "lesson.fixture_purge" } ] },
      { "id": "lesson.fixture_rot", "kind": "blight", "forms": [
       { "ability": "ability.fixture_rot", "points": 0, "mp": 3, "description": "lesson.fixture_rot" } ] },
      { "id": "lesson.fixture_quicken", "kind": "boon", "forms": [
       { "ability": "ability.fixture_quicken", "points": 0, "mp": 4, "description": "lesson.fixture_quicken" } ] },
      { "id": "lesson.fixture_bolt", "kind": "shot", "forms": [
       { "ability": "ability.fixture_bolt", "points": 0, "mp": 0, "description": "lesson.fixture_bolt" } ] },
      { "id": "lesson.test_pilfer", "kind": "theft", "forms": [
       { "ability": "ability.test_pilfer", "points": 0, "mp": 0, "description": "lesson.test_pilfer" } ] }
     ]
    }
    """;

    /// <summary>
    /// The name and the line of each item and each piece of gear of the tests. A content set that
    /// holds <see cref="ItemsFile"/> and <see cref="GearFile"/> needs them in its string table (G-7, D-1046).
    /// </summary>
    public const string ItemStrings =
        """{ "id": "item.fixture_draught", "text": "fixture_draught text." }, { "id": "name.fixture_draught", "text": "fixture_draught" }, { "id": "item.test_tonic", "text": "test_tonic text." }, { "id": "name.test_tonic", "text": "test_tonic" }""" + ", " +
        """{ "id": "item.test_salts", "text": "test_salts text." }, { "id": "name.test_salts", "text": "test_salts" }, { "id": "item.test_root", "text": "test_root text." }, { "id": "name.test_root", "text": "test_root" }""" + ", " +
        """{ "id": "item.test_token", "text": "test_token text." }, { "id": "name.test_token", "text": "test_token" }, { "id": "item.torch", "text": "torch text." }, { "id": "name.torch", "text": "torch" }, { "id": "gear.test_blade", "text": "test_blade text." }, { "id": "name.test_blade", "text": "test_blade" }""" + ", " +
        """{ "id": "gear.test_shield", "text": "test_shield text." }, { "id": "name.test_shield", "text": "test_shield" }, { "id": "gear.test_helm", "text": "test_helm text." }, { "id": "name.test_helm", "text": "test_helm" }""" + ", " +
        """{ "id": "gear.test_mail", "text": "test_mail text." }, { "id": "name.test_mail", "text": "test_mail" }, { "id": "gear.test_resist_ring", "text": "test_resist_ring text." }, { "id": "name.test_resist_ring", "text": "test_resist_ring" }""" + ", " +
        """{ "id": "gear.test_absorb_ring", "text": "test_absorb_ring text." }, { "id": "name.test_absorb_ring", "text": "test_absorb_ring" }, { "id": "gear.test_weak_charm", "text": "test_weak_charm text." }, { "id": "name.test_weak_charm", "text": "test_weak_charm" }""";

    /// <summary>
    /// The strings of <see cref="LessonsFile"/>, in ordinal order. A content set that holds the
    /// lesson file needs them in its string table (G-7), before <see cref="NoticeStrings"/>.
    /// </summary>
    public const string LessonStrings =
        """{ "id": "lesson.fixture_blaze", "text": "Blaze text." }, { "id": "lesson.fixture_bolt", "text": "Bolt text." }, { "id": "lesson.fixture_cinder", "text": "Cinder text." }, """ +
        """{ "id": "lesson.fixture_cleave", "text": "Cleave text." }, { "id": "lesson.fixture_hew", "text": "Hew text." }, """ +
        """{ "id": "lesson.fixture_purge", "text": "Purge text." }, { "id": "lesson.fixture_quicken", "text": "Quicken text." }, """ +
        """{ "id": "lesson.fixture_rot", "text": "Rot text." }, { "id": "lesson.fixture_salve", "text": "Salve text." }, """ +
        """{ "id": "lesson.test_pilfer", "text": "Pilfer text." }, { "id": "name.test_pilfer", "text": "Pilfer" }, """ +
        """{ "id": "name.fixture_blaze", "text": "Blaze" }, { "id": "name.fixture_bolt", "text": "Bolt" }, { "id": "name.fixture_cinder", "text": "Cinder" }, """ +
        """{ "id": "name.fixture_cleave", "text": "Cleave" }, { "id": "name.fixture_hew", "text": "Hew" }, """ +
        """{ "id": "name.fixture_purge", "text": "Purge" }, { "id": "name.fixture_quicken", "text": "Quicken" }, """ +
        """{ "id": "name.fixture_rot", "text": "Rot" }, { "id": "name.fixture_salve", "text": "Salve" }""";

    /// <summary>The path of the grunt record in a content set of the tests (D-786).</summary>
    public const string GruntPath = "rules/enemies/fixture-grunt.json";

    /// <summary>The path of the brute record in a content set of the tests (D-786).</summary>
    public const string BrutePath = "rules/enemies/fixture-brute.json";

    /// <summary>
    /// The notice file of the tests: one notice that logs and one that does not (D-983, D-989).
    /// A content set that holds it needs <see cref="NoticeStrings"/> in its string table (G-7).
    /// </summary>
    public const string NoticesFile = """
    {
     "comment": "The notices of the tests.",
     "notices": [
      { "id": "notice.test_kept", "log": true },
      { "id": "notice.test_plain", "log": false }
     ]
    }
    """;

    /// <summary>The string entries of the two notices of <see cref="NoticesFile"/>, for the string table of a test content set (G-7).</summary>
    public const string NoticeStrings = """{ "id": "notice.test_kept", "text": "A kept line." }, { "id": "notice.test_plain", "text": "A plain line." }""";

    /// <summary>The notice file of the tests, as a run reads it (D-989).</summary>
    public static readonly NoticeList Notices = NoticeList.Read(Encoding.UTF8.GetBytes(NoticesFile), NoticeList.Path);

    /// <summary>The id of the notice of the tests that logs (D-983).</summary>
    public static readonly ContentId KeptNotice = ContentId.Parse("notice.test_kept", "test", "notice");

    /// <summary>The id of the notice of the tests that does not log (D-983).</summary>
    public static readonly ContentId PlainNotice = ContentId.Parse("notice.test_plain", "test", "notice");

    /// <summary>The flag file of a test content set with no story scene: it declares no flag (D-1003).</summary>
    public const string NoFlagsFile = """
    {
     "comment": "The flags of the tests. It declares the flags of the side aptitudes alone.",
     "flags": [
      { "id": "flag.test_marrek_side", "note": "The side aptitude of Marrek is open." },
      { "id": "flag.test_second_side", "note": "The side aptitude of the second character is open." },
      { "id": "flag.test_third_side", "note": "The side aptitude of the third character is open." }
     ]
    }
    """;

    /// <summary>Gives the battle files of a content set, with the text of the tests (D-757, D-766, D-785, D-786).</summary>
    /// <returns>The rules file, the fixture file, the ability file, the two enemy records, the group file, the profile, the notice file (D-989), the flag file with no flag (D-1003), and the effect files that serve those combatants (D-879).</returns>
    public static IReadOnlyList<ContentFile> Files() =>
    [
        new ContentFile(BattleRules.Path, Encoding.UTF8.GetBytes(RulesFile)),
        new ContentFile(BattleFixture.Path, Encoding.UTF8.GetBytes(FixtureFile)),
        new ContentFile(AbilityList.Path, Encoding.UTF8.GetBytes(AbilitiesFile)),
        new ContentFile(LessonList.Path, Encoding.UTF8.GetBytes(LessonsFile)),
        new ContentFile(ItemList.Path, Encoding.UTF8.GetBytes(ItemsFile)),
        new ContentFile(GearList.Path, Encoding.UTF8.GetBytes(GearFile)),
        new ContentFile(BrutePath, Encoding.UTF8.GetBytes(BruteFile)),
        new ContentFile(GruntPath, Encoding.UTF8.GetBytes(GruntFile)),
        new ContentFile(FixtureGroupsPath, Encoding.UTF8.GetBytes(FixtureGroupsFile)),
        new ContentFile(GroupsPath, Encoding.UTF8.GetBytes(GroupsFile)),
        new ContentFile(AttackerProfilePath, Encoding.UTF8.GetBytes(AttackerProfileFile)),
        new ContentFile(NoticeList.Path, Encoding.UTF8.GetBytes(NoticesFile)),
        new ContentFile(FlagList.Path, Encoding.UTF8.GetBytes(NoFlagsFile)),
        .. EffectFixtures.Files(),
    ];

    /// <summary>The battle content of the tests, with Marrek alone in the party (D-336).</summary>
    public static readonly BattleContent Content = Of(FixtureFile);

    /// <summary>The story content of a run of the tests with no story scene: no flag and no trigger (D-540).</summary>
    public static readonly StoryContent Story = StoryContent.Load(FlagList.Read(Encoding.UTF8.GetBytes(NoFlagsFile), FlagList.Path), [], Content);

    /// <summary>The battle content of the tests, with a flee chance of 100%, so a test of the map never meets a failed flee.</summary>
    public static readonly BattleContent SureFlee = WithRules(("flee_floor", 10000), ("flee_ceiling", 10000));

    /// <summary>
    /// The battle content of the tests with no roll that moves a number: every hit takes the
    /// factor 10000, and no attack misses. A test then reads the exact damage of D-771.
    /// </summary>
    public static readonly BattleContent Exact = ExactWithParty(1);

    /// <summary>The exact rules of <see cref="Exact"/>, with a party of the first characters of the fixture.</summary>
    /// <param name="size">The count of characters: 1, 2, or 3 (D-336).</param>
    /// <returns>The battle content.</returns>
    public static BattleContent ExactWithParty(int size) =>
        Build(FixtureWithParty(size), [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0)]);

    /// <summary>
    /// The exact content of <see cref="Exact"/>, with a grunt that holds one affinity to one
    /// element and an immune list (D-794, D-805). Each other element stays normal.
    /// </summary>
    /// <param name="element">The element whose affinity changes.</param>
    /// <param name="affinity">The affinity of the grunt to that element.</param>
    /// <param name="immune">The statuses that the grunt refuses.</param>
    /// <param name="exact">True for the rolls of <see cref="Exact"/>, and false for the rolls of the rules of the tests.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithGrunt(Element element, Affinity affinity, StatusKind[] immune, bool exact)
    {
        string elements = $"\"{Elements.NameOf(element)}\": \"normal\"";
        string grunt = GruntFile.Replace(elements, $"\"{Elements.NameOf(element)}\": \"{Elements.NameOf(affinity)}\"", StringComparison.Ordinal);
        List<string> names = [];
        foreach (StatusKind status in immune)
        {
            names.Add($"\"{Statuses.NameOf(status)}\"");
        }

        grunt = grunt.Replace("\"immune\": []", $"\"immune\": [{string.Join(", ", names)}]", StringComparison.Ordinal);
        (string Field, int Value)[] changes = exact
            ? [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0)]
            : [];
        return Build(FixtureFile, changes, grunt);
    }

    /// <summary>Reads the rules of the tests with some numbers changed, and the fixture of the tests.</summary>
    /// <param name="changes">Each field and its new value.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithRules(params (string Field, int Value)[] changes) => Build(FixtureFile, changes);

    /// <summary>Reads the rules of the tests with one fixture file.</summary>
    /// <param name="fixture">The text of the fixture file.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent Of(string fixture) => Build(fixture, []);

    /// <summary>Reads the content of the tests with other text for the fixture, the lesson file, the ability file, or the grunt (D-1026).</summary>
    /// <param name="fixture">The text of the fixture file, or no value for <see cref="FixtureFile"/>.</param>
    /// <param name="lessons">The text of the lesson file, or no value for <see cref="LessonsFile"/>.</param>
    /// <param name="abilities">The text of the ability file, or no value for <see cref="AbilitiesFile"/>.</param>
    /// <param name="grunt">The text of the grunt record, or no value for <see cref="GruntFile"/>.</param>
    /// <param name="exact">True for the rolls of <see cref="Exact"/>: no miss and a hit factor of 10000.</param>
    /// <param name="rules">Other values of the battle rules, after the values of <paramref name="exact"/>.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithLessonFiles(string? fixture = null, string? lessons = null, string? abilities = null, string? grunt = null, bool exact = false, (string Field, int Value)[]? rules = null) =>
        Build(
            fixture ?? FixtureFile,
            [.. (exact ? [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0)] : Array.Empty<(string, int)>()), .. rules ?? []],
            grunt,
            null,
            null,
            lessons,
            abilities);

    /// <summary>The battle content of the tests, with a party of the first characters of the fixture.</summary>
    /// <param name="size">The count of characters: 1, 2, or 3 (D-336).</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithParty(int size) => Of(FixtureWithParty(size));

    /// <summary>
    /// Gives the battle content of the tests with a thief: Marrek carries the hew and the steal
    /// drill of the tests, and every hit takes the factor 10000 with no miss (D-950, D-1045).
    /// </summary>
    /// <param name="stealChance">The base chance of a steal of the attacker profile, in basis points.</param>
    /// <param name="rules">More changes of the rules, such as the steal clamp.</param>
    /// <returns>The battle content.</returns>
    /// <summary>Gives the battle content of the tests with a drop list on the attacker profile, and every hit at the factor 10000 with no miss (D-1042).</summary>
    /// <param name="drops">The text of the drop list, such as `[{ "item": "item.fixture_draught", "chance": 10000 }]`.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithDrops(string drops)
    {
        (string Field, int Value)[] exact = [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0)];
        string profile = AttackerProfileFile.Replace("\"drops\": []", $"\"drops\": {drops}", System.StringComparison.Ordinal);
        return Build(FixtureFile, exact, attacker: profile);
    }

    /// <summary>Gives the battle content of the tests with a thief, sure steals, and a steal list with a gear chance (D-1051).</summary>
    /// <param name="gearChance">The gear chance of the attacker profile, in basis points.</param>
    /// <param name="steal">The text of the steal list.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithGearThief(int gearChance, string steal)
    {
        string fixture = FixtureFile.Replace(
            "\"lessons\": [\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]",
            "\"lessons\": [\"lesson.fixture_hew\", \"lesson.test_pilfer\"]",
            System.StringComparison.Ordinal);
        (string Field, int Value)[] rules = [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0), ("steal_ceiling", 10000), ("steal_rate", 10000)];
        string profile = AttackerProfileFile
            .Replace("\"steal_chance\": 3000", "\"steal_chance\": 10000", System.StringComparison.Ordinal)
            .Replace("\"steal_gear_chance\": 0", $"\"steal_gear_chance\": {gearChance}", System.StringComparison.Ordinal)
            .Replace("[{ \"item\": \"item.fixture_draught\" }, { \"gold\": 5 }]", steal, System.StringComparison.Ordinal);
        return Build(fixture, rules, attacker: profile);
    }

    public static BattleContent WithThief(int stealChance, params (string Field, int Value)[] rules)
    {
        string fixture = FixtureFile.Replace(
            "\"lessons\": [\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]",
            "\"lessons\": [\"lesson.fixture_hew\", \"lesson.test_pilfer\"]",
            System.StringComparison.Ordinal);
        (string Field, int Value)[] exact = [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0), .. rules];
        string profile = AttackerProfileFile.Replace("\"steal_chance\": 3000", $"\"steal_chance\": {stealChance}", System.StringComparison.Ordinal);
        return Build(fixture, exact, attacker: profile);
    }

    /// <summary>
    /// Gives stored lessons with the slot count of another level, in the rules of the tests: the
    /// lessons keep their slots, and each new slot is empty (D-1018). A test that raises a stored
    /// level calls it, so the resume reads a state that a run can make.
    /// </summary>
    /// <param name="lessons">The stored lessons.</param>
    /// <param name="level">The new level.</param>
    /// <returns>The lessons with the slots of that level.</returns>
    public static LessonValues LessonsAtLevel(LessonValues? lessons, int level)
    {
        ArgumentNullException.ThrowIfNull(lessons);

        var slots = new ContentId?[Content.Rules.SlotsAt(level)];
        for (int index = 0; index < lessons.Slots.Count; index += 1)
        {
            slots[index] = lessons.Slots[index];
        }

        return lessons with { Slots = slots };
    }

    /// <summary>
    /// Gives the stats of Marrek at one level, with the numbers of D-977. Level 1 holds the
    /// stats of D-777, so each fight test of level 1 keeps its numbers. The magic equals the
    /// attack and the resistance equals the defense, so a magic strike of the tests deals the
    /// damage of a physical strike. `StatSetTests` sets the two apart (D-1052, D-1053).
    /// </summary>
    /// <param name="level">The level, from 1 to 40.</param>
    /// <returns>The row.</returns>
    public static StatRow MarrekAt(int level) =>
        new(60 + (6 * (level - 1)), 8 + (2 * (level - 1)), 12 + (level - 1), 12 + (level - 1), 4 + (level / 2), 4 + (level / 2), 100 + (level / 4));

    /// <summary>Gives the total experience of one level in the rules of the tests: 10 x n x (n - 1) (D-977).</summary>
    /// <param name="level">The level, from 1 to 40.</param>
    /// <returns>The total.</returns>
    public static int TotalOf(int level) => 10 * level * (level - 1);

    private static string MarrekCurve()
    {
        List<string> rows = [];
        for (int level = 1; level <= StatCurve.HighestLevel; level += 1)
        {
            StatRow row = MarrekAt(level);
            rows.Add($"{{ \"level\": {level}, \"health\": {row.Health}, \"mp\": {row.Mp}, \"attack\": {row.Attack}, \"magic\": {row.Magic}, \"defense\": {row.Defense}, \"resistance\": {row.Resistance}, \"speed\": {row.Speed} }}");
        }

        return $"[{string.Join(", ", rows)}]";
    }

    private static string FixtureWithParty(int size)
    {
        string[] ids = ["\"character.marrek\"", "\"character.test_second\"", "\"character.test_third\""];
        string party = string.Join(", ", ids[..size]);
        return FixtureFile.Replace("\"start_party\": [\"character.marrek\"]", $"\"start_party\": [{party}]", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Gives a group file of the test region with one group, `group.wave`: a grunt on the field,
    /// then each waiting enemy in the back row, in the order of the list (D-778, D-963).
    /// </summary>
    /// <param name="waiting">The enemy id of each waiting entry.</param>
    /// <returns>The text of the group file.</returns>
    public static string WaveGroupsFile(IReadOnlyList<string> waiting)
    {
        List<string> entries = ["{ \"enemy\": \"enemy.fixture_grunt\", \"row\": \"front\", \"waits\": false, \"profile\": \"profile.test_attacker\" }"];
        foreach (string enemy in waiting)
        {
            entries.Add($"{{ \"enemy\": \"{enemy}\", \"row\": \"back\", \"waits\": true, \"profile\": \"profile.test_attacker\" }}");
        }

        return $$"""
            {
             "comment": "A group file of the test region with one wave.",
             "region": "region.test",
             "groups": [{ "id": "group.wave", "boss": false, "enemies": [{{string.Join(", ", entries)}}] }]
            }
            """;
    }

    /// <summary>Reads the content of the tests with one group file of the test region in place of <see cref="GroupsFile"/>.</summary>
    /// <param name="groups">The text of the group file of the test region.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent OfGroups(string groups) => Build(FixtureFile, [], null, groups);

    /// <summary>
    /// Reads the content of the tests for a test of the evaluator: one grunt, one group file of
    /// the test region, and more profiles beside the attacker (D-955, D-956).
    /// </summary>
    /// <param name="grunt">The text of the grunt record.</param>
    /// <param name="groups">The text of the group file of the test region.</param>
    /// <param name="exact">True for the rolls of <see cref="Exact"/>.</param>
    /// <param name="profiles">The text of each more profile.</param>
    /// <returns>The battle content.</returns>
    public static BattleContent ForEvaluator(string grunt, string groups, bool exact, params string[] profiles)
    {
        (string Field, int Value)[] changes = exact
            ? [("hit_low", 10000), ("hit_high", 10000), ("miss_base", 0), ("miss_ceiling", 0)]
            : [];
        return Build(FixtureWithParty(3), changes, grunt, groups, profiles);
    }

    private static BattleContent Build(string fixture, (string Field, int Value)[] changes, string? grunt = null, string? groups = null, string[]? profiles = null, string? lessons = null, string? abilities = null, string? attacker = null)
    {
        string rules = RulesFile;
        foreach ((string field, int value) in changes)
        {
            string pattern = $"\"{field}\": ";
            int start = rules.IndexOf(pattern, System.StringComparison.Ordinal) + pattern.Length;
            int end = rules.IndexOfAny([',', '\n'], start);
            rules = string.Concat(rules.AsSpan(0, start), value.ToString(System.Globalization.CultureInfo.InvariantCulture), rules.AsSpan(end));
        }

        return new(
            BattleRules.Read(Encoding.UTF8.GetBytes(rules), "tests-battle.json"),
            BattleFixture.Read(Encoding.UTF8.GetBytes(fixture), "tests-fixture.json"),
            [
                EnemyRecord.Read(Encoding.UTF8.GetBytes(BruteFile), BrutePath),
                EnemyRecord.Read(Encoding.UTF8.GetBytes(grunt ?? GruntFile), GruntPath),
            ],
            AbilityList.Read(Encoding.UTF8.GetBytes(abilities ?? AbilitiesFile), AbilityList.Path),
            LessonList.Read(Encoding.UTF8.GetBytes(lessons ?? LessonsFile), LessonList.Path),
            ItemList.Read(Encoding.UTF8.GetBytes(ItemsFile), ItemList.Path),
            GearList.Read(Encoding.UTF8.GetBytes(GearFile), GearList.Path),
            [
                GroupFile.Read(Encoding.UTF8.GetBytes(FixtureGroupsFile), FixtureGroupsPath),
                GroupFile.Read(Encoding.UTF8.GetBytes(groups ?? GroupsFile), GroupsPath),
            ],
            ProfilesOf(profiles ?? [], attacker ?? AttackerProfileFile));
    }

    private static List<ProfileRecord> ProfilesOf(string[] more, string attacker)
    {
        List<ProfileRecord> profiles = [ProfileRecord.Read(Encoding.UTF8.GetBytes(attacker), AttackerProfilePath)];
        for (int index = 0; index < more.Length; index += 1)
        {
            profiles.Add(ProfileRecord.Read(Encoding.UTF8.GetBytes(more[index]), $"rules/profiles/test-{index}.json"));
        }

        return profiles;
    }
}
