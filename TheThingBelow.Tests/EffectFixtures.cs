using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The effect files of the tests: a battle file, a hit file, the ten transitions, and a transition table (D-195, D-879, D-883). The tests hold
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

    /// <summary>
    /// Gives the effect files of a content set: the battle file, one hit file that serves every
    /// combatant, the ten transitions, and a table of one region that holds the maps (D-195, D-936).
    /// </summary>
    /// <param name="maps">The ids of the maps of the set, each one in quotes. The region then holds them.</param>
    /// <returns>The files.</returns>
    public static IReadOnlyList<ContentFile> Files(string maps = "")
    {
        List<ContentFile> files =
        [
            File(BattleEffects.Path, BattleBody),
            File(HitPath, HitBody(EveryCombatant)),
            File(TransitionTable.Path, TableBody(maps: maps)),
        ];

        files.AddRange(TransitionFiles());
        return files;
    }

    /// <summary>
    /// Gives the files with the transition table replaced by one whose region holds each map of the
    /// files, so a set of the tests with its own maps passes the check of D-936.
    /// </summary>
    /// <param name="files">The files of a set, with one transition table.</param>
    /// <returns>The same files, with the new table in the place of the old one.</returns>
    /// <remarks>The map id comes from the text of each map file, so a map file that a test breaks still reaches the load.</remarks>
    public static List<ContentFile> WithMapsOf(IEnumerable<ContentFile> files)
    {
        var result = new List<ContentFile>(files);
        var maps = new List<string>();
        foreach (ContentFile file in result)
        {
            if (GameMap.IsMapFile(file.Path)
                && Regex.Match(Encoding.UTF8.GetString(file.Bytes), "\"id\": \"(map\\.[a-z0-9_]+)\"") is { Success: true } found)
            {
                maps.Add($"\"{found.Groups[1].Value}\"");
            }
        }

        int index = result.FindIndex(file => string.CompareOrdinal(file.Path, TransitionTable.Path) == 0);
        Assert.True(index >= 0, "the files hold no transition table to replace");
        result[index] = File(TransitionTable.Path, TableBody(maps: string.Join(", ", maps)));
        return result;
    }

    /// <summary>The ten transition files of the tests, one for each look, each with the cover key `k` (D-195).</summary>
    /// <param name="ticks">The length of each transition.</param>
    /// <returns>The files.</returns>
    public static IReadOnlyList<ContentFile> TransitionFiles(int ticks = 60)
    {
        var files = new List<ContentFile>(Transition.AllLooks.Length);
        foreach (TransitionLook look in Transition.AllLooks)
        {
            string name = Transition.NameOf(look);
            files.Add(File(TransitionPath(name), TransitionBody(name, ticks: ticks)));
        }

        return files;
    }

    /// <summary>Gives the path of the transition file of one look.</summary>
    /// <param name="name">The name of the look, such as `color_split`.</param>
    /// <returns>The path under `content/`.</returns>
    public static string TransitionPath(string name) => $"{Transition.Folder}test-{name.Replace('_', '-')}.json";

    /// <summary>Makes the body of a transition file.</summary>
    /// <param name="name">The name of the look, which the id takes too.</param>
    /// <param name="ticks">The length.</param>
    /// <param name="cover">The palette key of the cover.</param>
    /// <returns>The body.</returns>
    public static string TransitionBody(string name, int ticks = 60, string cover = "k") =>
        $$"""{ "comment": "a test transition", "id": "transition.{{name}}", "look": "{{name}}", "ticks": {{ticks}}, "cover": "{{cover}}" }""";

    /// <summary>The kinds of the table of the tests, with the picks of D-940.</summary>
    public const string Kinds =
        """{ "ambush": "transition.color_split", "elite": "transition.swirl", "boss": "transition.shatter", "wrong_thing": "transition.ripple" }""";

    /// <summary>The pool of region one of the tests, with the six transitions of D-940.</summary>
    public const string Pool =
        "\"transition.pixel_dissolve\", \"transition.mosaic\", \"transition.crt_power_off\", \"transition.snow_whiteout\", \"transition.blinds\", \"transition.scanline_sweep\"";

    /// <summary>Makes the body of a transition table.</summary>
    /// <param name="kinds">The kinds, as a JSON object.</param>
    /// <param name="maps">The ids of the maps of the one region, each one in quotes.</param>
    /// <param name="pool">The ids of its pool, each one in quotes.</param>
    /// <param name="regions">Other regions, as JSON objects with a comma before each one.</param>
    /// <returns>The body.</returns>
    public static string TableBody(string kinds = Kinds, string maps = "", string pool = Pool, string regions = "") =>
        $$"""{ "comment": "a test table", "fade_ticks": 20, "back_cover": "k", "kinds": {{kinds}}, "regions": [{ "region": "region.one", "maps": [{{maps}}], "pool": [{{pool}}] }{{regions}}] }""";

    /// <summary>Makes a content file of a text body.</summary>
    /// <param name="path">The path under `content/`.</param>
    /// <param name="body">The text of the file.</param>
    /// <returns>The file.</returns>
    public static ContentFile File(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));
}
