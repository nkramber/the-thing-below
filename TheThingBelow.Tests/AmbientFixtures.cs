using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Tests;

/// <summary>
/// The ambient files of the tests: one weather of the test map, with one stream and one layer
/// of fog (D-187, D-889). The tests hold their own copy, so a tune of `content/effects/` moves
/// no test (T-3).
/// </summary>
internal static class AmbientFixtures
{
    /// <summary>The path of the ambient file of the tests.</summary>
    public const string Path = AmbientEffect.Folder + "test-dust.json";

    /// <summary>The path of the capture file of the tests (D-889).</summary>
    public const string CapturePath = AmbientEffect.CaptureFolder + "test-snow.json";

    /// <summary>One stream of the tests, with the palette key `k` that every palette of the tests holds.</summary>
    public const string Stream =
        """{ "amount": 24, "lifetime_ticks": 240, "color": "k", "dark_color": "k", "size": 1, "fall_pixels": 24, "fall_ticks": 120, "drift_pixels": 4, "sway_pixels": 3, "sway_ticks": 60 }""";

    /// <summary>One layer of fog of the tests, in the key `k`, which fades in from 5000 to 6000.</summary>
    public const string Fog =
        """
        {
         "key": "k",
         "from": 5000,
         "to": 6000,
         "strength": 2000,
         "scale": 32,
         "seed": 7,
         "drift_x": 4,
         "drift_y": 0
        }
        """;

    /// <summary>Makes the body of an ambient file.</summary>
    /// <param name="id">The id of the effect.</param>
    /// <param name="maps">The map ids that the file serves, each one in quotes.</param>
    /// <param name="kind">The ambient kind.</param>
    /// <param name="emitters">The streams, as JSON objects.</param>
    /// <param name="fogs">The layers of fog, as JSON objects.</param>
    /// <returns>The body.</returns>
    public static string Body(
        string id = "effect.test_dust",
        string maps = "\"map.lit\"",
        string kind = "dust",
        string emitters = Stream,
        string fogs = "") =>
        $$"""
        {
         "comment": "a test weather",
         "id": "{{id}}",
         "kind": "{{kind}}",
         "maps": [{{maps}}],
         "lit": true,
         "emitters": [{{emitters}}],
         "fogs": [{{fogs}}]
        }
        """;

    /// <summary>Loads the effect files of a test, with the ambient files that the test gives.</summary>
    /// <param name="ambient">The ambient files, as content files.</param>
    /// <param name="terrain">The terrain of the test map, or the room when no value is given.</param>
    /// <param name="liveParticles">The particle row of the effect budget (D-523).</param>
    /// <param name="fullScreenPasses">The pass row of the effect budget (D-523).</param>
    /// <returns>The effect content.</returns>
    /// <exception cref="ContentException">A file breaks a rule, or a check across files fails (T-2).</exception>
    public static EffectContent Load(IReadOnlyList<ContentFile> ambient, string[]? terrain = null, int liveParticles = 8192, int fullScreenPasses = 3)
    {
        var files = new List<ContentFile>(EffectFixtures.Files());
        files.AddRange(ambient);
        return EffectContent.Load(files, World(terrain, liveParticles, fullScreenPasses: fullScreenPasses));
    }

    /// <summary>Makes the world of the checks: the test map, the fight, the light, no drawing, and the test palette.</summary>
    /// <param name="terrain">The terrain of the test map, or the room when no value is given.</param>
    /// <param name="liveParticles">The particle row of the effect budget (D-523).</param>
    /// <param name="drawings">The drawings that the fog test reads, or none.</param>
    /// <param name="fullScreenPasses">The pass row of the effect budget (D-523).</param>
    /// <returns>The world.</returns>
    public static AmbientWorld World(
        string[]? terrain = null,
        int liveParticles = 8192,
        SortedDictionary<string, Drawing>? drawings = null,
        int fullScreenPasses = 3)
    {
        string budget = $$"""{ "comment": "a test budget", "lights_in_view": 24, "live_particles": {{liveParticles}}, "full_screen_passes": {{fullScreenPasses}} }""";
        LightContent light = LightFixtures.Load(
            LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody(), budget),
            terrain);
        return new AmbientWorld(
            LightFixtures.Maps(terrain),
            TestBattles.Content,
            light,
            drawings ?? new SortedDictionary<string, Drawing>(StringComparer.Ordinal),
            LightFixtures.Palette());
    }
}
