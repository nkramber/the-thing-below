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
     "weak_rate": 15000,
     "resist_rate": 5000,
     "absorb_rate": 10000,
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
     "level_experience": [0, 20, 60, 120, 200, 300, 420, 560, 720, 900, 1100, 1320, 1560, 1820, 2100, 2400, 2720, 3060, 3420, 3800, 4200, 4620, 5060, 5520, 6000, 6500, 7020, 7560, 8120, 8700, 9300, 9920, 10560, 11220, 11900, 12600, 13320, 14060, 14820, 15600]
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
      { "id": "character.marrek", "row": "front", "join_level": 1, "curve": {{MarrekCurve()}} },
      { "id": "character.test_second", "row": "front", "join_level": 1, "curve": {{StatCurve.FlatText(new StatRow(50, 12, 10, 3, 110))}} },
      { "id": "character.test_third", "row": "back", "join_level": 1, "curve": {{StatCurve.FlatText(new StatRow(40, 16, 8, 2, 120))}} }
     ],
     "items": [
      { "id": "item.fixture_draught", "heal": 30, "delay": 100 }
     ],
     "start_party": ["character.marrek"],
     "pack": [{ "item": "item.fixture_draught", "count": 3 }]
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
     "steal": [{ "item": "item.fixture_draught" }, { "gold": 5 }]
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
     "defense": 2,
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
     "defense": 6,
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
      { "id": "ability.fixture_bash", "kind": "strike", "delay": 100, "power": 5000, "element": "none", "reach": "melee" },
      { "id": "ability.test_mend", "kind": "heal", "delay": 100, "heal": 20 },
      { "id": "ability.test_shot", "kind": "strike", "delay": 100, "power": 15000, "element": "none", "reach": "any" }
     ]
    }
    """;

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
     "comment": "The flags of the tests. It declares none.",
     "flags": []
    }
    """;

    /// <summary>Gives the battle files of a content set, with the text of the tests (D-757, D-766, D-785, D-786).</summary>
    /// <returns>The rules file, the fixture file, the ability file, the two enemy records, the group file, the profile, the notice file (D-989), the flag file with no flag (D-1003), and the effect files that serve those combatants (D-879).</returns>
    public static IReadOnlyList<ContentFile> Files() =>
    [
        new ContentFile(BattleRules.Path, Encoding.UTF8.GetBytes(RulesFile)),
        new ContentFile(BattleFixture.Path, Encoding.UTF8.GetBytes(FixtureFile)),
        new ContentFile(AbilityList.Path, Encoding.UTF8.GetBytes(AbilitiesFile)),
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

    /// <summary>The battle content of the tests, with a party of the first characters of the fixture.</summary>
    /// <param name="size">The count of characters: 1, 2, or 3 (D-336).</param>
    /// <returns>The battle content.</returns>
    public static BattleContent WithParty(int size) => Of(FixtureWithParty(size));

    /// <summary>
    /// Gives the stats of Marrek at one level, with the numbers of D-977. Level 1 holds the
    /// stats of D-777, so each fight test of level 1 keeps its numbers.
    /// </summary>
    /// <param name="level">The level, from 1 to 40.</param>
    /// <returns>The row.</returns>
    public static StatRow MarrekAt(int level) =>
        new(60 + (6 * (level - 1)), 8 + (2 * (level - 1)), 12 + (level - 1), 4 + (level / 2), 100 + (level / 4));

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
            rows.Add($"{{ \"level\": {level}, \"health\": {row.Health}, \"mp\": {row.Mp}, \"attack\": {row.Attack}, \"defense\": {row.Defense}, \"speed\": {row.Speed} }}");
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

    private static BattleContent Build(string fixture, (string Field, int Value)[] changes, string? grunt = null, string? groups = null, string[]? profiles = null)
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
            AbilityList.Read(Encoding.UTF8.GetBytes(AbilitiesFile), AbilityList.Path),
            [
                GroupFile.Read(Encoding.UTF8.GetBytes(FixtureGroupsFile), FixtureGroupsPath),
                GroupFile.Read(Encoding.UTF8.GetBytes(groups ?? GroupsFile), GroupsPath),
            ],
            ProfilesOf(profiles ?? []));
    }

    private static List<ProfileRecord> ProfilesOf(string[] more)
    {
        List<ProfileRecord> profiles = [ProfileRecord.Read(Encoding.UTF8.GetBytes(AttackerProfileFile), AttackerProfilePath)];
        for (int index = 0; index < more.Length; index += 1)
        {
            profiles.Add(ProfileRecord.Read(Encoding.UTF8.GetBytes(more[index]), $"rules/profiles/test-{index}.json"));
        }

        return profiles;
    }
}
