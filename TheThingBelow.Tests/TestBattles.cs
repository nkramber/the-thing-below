using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tests;

/// <summary>
/// The battle content of the tests: the rules of D-777 and a fixture that holds every group
/// that a test map names (D-766). The tests hold their own copy, so a balance change of PR-30
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
     "stun_ticks": 50,
     "flee_base": 5000,
     "flee_per_speed": 100,
     "flee_floor": 1000,
     "flee_ceiling": 9000,
     "item_rate": 5000
    }
    """;

    /// <summary>
    /// The fixture of the tests. It holds three characters, and the party starts with Marrek
    /// alone. Each group of a test map is a grunt or two. The boss group, the wave group, and the
    /// full group serve the battle tests.
    /// </summary>
    public const string FixtureFile = """
    {
     "comment": "The battle fixture of the tests.",
     "characters": [
      { "id": "character.marrek", "health": 60, "attack": 12, "defense": 4, "speed": 100, "row": "front" },
      { "id": "character.test_second", "health": 50, "attack": 10, "defense": 3, "speed": 110, "row": "front" },
      { "id": "character.test_third", "health": 40, "attack": 8, "defense": 2, "speed": 120, "row": "back" }
     ],
     "enemies": [
      { "id": "enemy.fixture_grunt", "health": 30, "attack": 8, "defense": 2, "speed": 90 },
      { "id": "enemy.fixture_brute", "health": 80, "attack": 14, "defense": 6, "speed": 80 }
     ],
     "groups": [
      { "id": "group.one", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false }] },
      { "id": "group.other", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false }] },
      { "id": "group.ring", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false }] },
      {
       "id": "group.test_pair",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false }
       ]
      },
      {
       "id": "group.test_elite",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_brute", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false }
       ]
      },
      {
       "id": "group.fixture_pair",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false }
       ]
      },
      {
       "id": "group.fixture_elite",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_brute", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true }
       ]
      },
      { "id": "group.test_boss", "boss": true, "enemies": [{ "enemy": "enemy.fixture_brute", "row": "front", "waits": false }] },
      {
       "id": "group.test_full",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": true },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true }
       ]
      }
     ],
     "items": [
      { "id": "item.fixture_draught", "heal": 30, "delay": 100 }
     ],
     "start_party": ["character.marrek"],
     "pack": [{ "item": "item.fixture_draught", "count": 3 }]
    }
    """;

    /// <summary>Gives the two battle files of a content set, with the text of the tests (D-757, D-766).</summary>
    /// <returns>The rules file and the fixture file.</returns>
    public static IReadOnlyList<ContentFile> Files() =>
    [
        new ContentFile(BattleRules.Path, Encoding.UTF8.GetBytes(RulesFile)),
        new ContentFile(BattleFixture.Path, Encoding.UTF8.GetBytes(FixtureFile)),
    ];

    /// <summary>The battle content of the tests, with Marrek alone in the party (D-336).</summary>
    public static readonly BattleContent Content = Of(FixtureFile);

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

    private static string FixtureWithParty(int size)
    {
        string[] ids = ["\"character.marrek\"", "\"character.test_second\"", "\"character.test_third\""];
        string party = string.Join(", ", ids[..size]);
        return FixtureFile.Replace("\"start_party\": [\"character.marrek\"]", $"\"start_party\": [{party}]", System.StringComparison.Ordinal);
    }

    private static BattleContent Build(string fixture, (string Field, int Value)[] changes)
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
            BattleFixture.Read(Encoding.UTF8.GetBytes(fixture), "tests-fixture.json"));
    }
}
