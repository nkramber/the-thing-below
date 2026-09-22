using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Tests;

/// <summary>
/// The effect files of the tests: a battle file and a hit file (D-879, D-883). The tests hold
/// their own copy, so a tune of `content/effects/` moves no test (T-3).
/// </summary>
internal static class EffectFixtures
{
    /// <summary>The path of the hit file of the tests.</summary>
    public const string HitPath = "effects/hits/test-blood.json";

    /// <summary>The body of the battle file of the tests, with the pace of PR-10 (D-829).</summary>
    public const string BattleBody = """
    {
     "comment": "The battle file of the tests.",
     "start_ticks": 40,
     "strike_ticks": 44,
     "line_ticks": 32,
     "end_ticks": 60,
     "pose_ticks": 16,
     "blow_tick": 6,
     "flash_ticks": 8,
     "number_rise_ticks": 6,
     "number_rise_pixels": 3,
     "number_fall_ticks": 2,
     "lunge_pixels": 4,
     "drift_pixels": 2,
     "drift_step_ticks": 45,
     "hit_stop_ticks": 6,
     "shake": { "ticks": 12, "step_ticks": 2, "full": 4, "reduced": 1, "off": 0 }
    }
    """;

    /// <summary>One emitter of the tests, with the palette key `k` that every palette of the tests holds.</summary>
    public const string Emitter =
        """{ "amount": 10, "lifetime_ticks": 20, "colors": ["k"], "size": 2, "area": 3, "direction": -30, "spread": 40, "slowest_speed": 50, "fastest_speed": 120, "gravity": 400 }""";

    /// <summary>Makes the body of a hit file.</summary>
    /// <param name="serves">The ids that the file serves, each one in quotes.</param>
    /// <param name="id">The id of the effect.</param>
    /// <param name="emitters">The emitters, as JSON objects.</param>
    /// <returns>The body.</returns>
    public static string HitBody(string serves, string id = "effect.test_blood", string emitters = Emitter) =>
        $$"""{ "comment": "a test hit file", "id": "{{id}}", "serves": [{{serves}}], "lit": true, "emitters": [{{emitters}}] }""";

    /// <summary>The ids of every combatant of <see cref="TestBattles"/>, each one in quotes.</summary>
    public const string EveryCombatant =
        "\"character.marrek\", \"character.test_second\", \"character.test_third\", \"enemy.fixture_grunt\", \"enemy.fixture_brute\"";

    /// <summary>Gives the effect files of a content set: the battle file and one hit file that serves every combatant.</summary>
    /// <returns>The files.</returns>
    public static IReadOnlyList<ContentFile> Files() =>
    [
        File(BattleEffects.Path, BattleBody),
        File(HitPath, HitBody(EveryCombatant)),
    ];

    /// <summary>Makes a content file of a text body.</summary>
    /// <param name="path">The path under `content/`.</param>
    /// <param name="body">The text of the file.</param>
    /// <returns>The file.</returns>
    public static ContentFile File(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));
}
