using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The checks that span the ambient files: the maps, the budget, and the fog (D-202, D-523, D-885).</summary>
public sealed class AmbientContentTests
{
    [Fact]
    public void TheWeatherOfAMapComesFromItsAmbientFile()
    {
        EffectContent effects = AmbientFixtures.Load([File(AmbientFixtures.Path, AmbientFixtures.Body())]);

        AmbientEffect weather = Assert.IsType<AmbientEffect>(effects.Ambient.WeatherOf(MapId()));
        Assert.Equal("effect.test_dust", weather.Id.Value);
        Assert.Equal(AmbientKind.Dust, weather.Kind);
    }

    [Fact]
    public void AMapWithNoAmbientFileTakesNoWeather()
    {
        // D-202: each map has its own weather, and a map with no file shows none.
        EffectContent effects = AmbientFixtures.Load([]);

        Assert.Null(effects.Ambient.WeatherOf(MapId()));
    }

    [Fact]
    public void AnAmbientFileThatNamesAnAbsentMapFailsWithTheFileAndTheId()
    {
        // Exit test 4 of PR-58.
        ContentException error = Assert.Throws<ContentException>(() => AmbientFixtures.Load(
            [File(AmbientFixtures.Path, AmbientFixtures.Body(maps: "\"map.absent\""))]));

        Assert.Equal(AmbientFixtures.Path, error.File);
        Assert.Equal("maps[0]", error.Field);
        Assert.Contains("map.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoWeathersOfOneMapFail()
    {
        // D-202: a map takes one weather, and its time of day never changes it.
        ContentException error = Assert.Throws<ContentException>(() => AmbientFixtures.Load(
        [
            File(AmbientFixtures.Path, AmbientFixtures.Body()),
            File(AmbientEffect.Folder + "second.json", AmbientFixtures.Body(id: "effect.test_second")),
        ]));

        Assert.Contains("a map takes one weather", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACaptureFileOfAMapWithItsOwnWeatherPasses()
    {
        // D-889: a capture loads its file in place of the weather, and the shipped build never does.
        EffectContent effects = AmbientFixtures.Load(
        [
            File(AmbientFixtures.Path, AmbientFixtures.Body()),
            File(AmbientFixtures.CapturePath, AmbientFixtures.Body(id: "effect.test_snow", kind: "snow")),
        ]);

        Assert.Equal("effect.test_dust", effects.Ambient.WeatherOf(MapId())!.Id.Value);
        AmbientEffect capture = effects.Ambient.Effect(ContentId.Parse("effect.test_snow", AmbientFixtures.CapturePath, "id"));
        Assert.Equal(AmbientKind.Snow, capture.Kind);
    }

    [Fact]
    public void AWeatherThatPassesTheParticleRowFails()
    {
        // D-523: the map shows its weather, its torches, and the carried light at once.
        ContentException error = Assert.Throws<ContentException>(() => AmbientFixtures.Load(
            [File(AmbientFixtures.Path, AmbientFixtures.Body())],
            liveParticles: 20));

        Assert.Equal("emitters", error.Field);
        Assert.Contains("live_particles", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFogOfThreeLayersAndTheGlowKeepInsideABudgetOfTwoPasses()
    {
        // D-898: the shader draws the three layers of the fog capture in one pass, so the budget
        // counts one. The count of one pass for each layer refused this checkout. The glow takes
        // one more pass on every map (D-910).
        var files = new List<ContentFile>(ContentFolder.Read(RepositoryRoot.Find()));
        int index = files.FindIndex(file => file.Path == EffectBudget.Path);
        string budget = Encoding.UTF8.GetString(files[index].Bytes).Replace("\"full_screen_passes\": 3", "\"full_screen_passes\": 2", StringComparison.Ordinal);
        files[index] = new ContentFile(files[index].Path, Encoding.UTF8.GetBytes(budget));

        ContentSet set = ContentSet.Load(files);

        Assert.Equal(2, set.Light.Budget.FullScreenPasses);
        AmbientEffect fog = Assert.Single(set.Effects.Ambient.All, effect => effect.Kind == AmbientKind.Fog);
        Assert.Equal(3, fog.Fogs.Count);
        Assert.Equal(1, fog.FullScreenPasses);
    }

    [Fact]
    public void TheBudgetCountsTheGlowPassWithTheFog()
    {
        // D-523, D-910: the glow is one full-screen pass on every map, so a budget of one pass
        // holds no fog. The count with no glow pass took this budget.
        var files = new List<ContentFile>(ContentFolder.Read(RepositoryRoot.Find()));
        int index = files.FindIndex(file => file.Path == EffectBudget.Path);
        string budget = Encoding.UTF8.GetString(files[index].Bytes).Replace("\"full_screen_passes\": 3", "\"full_screen_passes\": 1", StringComparison.Ordinal);
        files[index] = new ContentFile(files[index].Path, Encoding.UTF8.GetBytes(budget));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal("fogs", error.Field);
        Assert.Contains("the weather and the glow draw 2 full-screen passes", error.Message, StringComparison.Ordinal);
        Assert.Contains("allows 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFogThatHidesAnEnemyOfItsMapFails()
    {
        // D-885, D-886, D-892: the fog test reads the checkout, where the map foe stands on the
        // floor of the dungeon. A layer of 8000 basis points pulls each gap below the floor of 17 (D-906).
        var files = new List<ContentFile>(ContentFolder.Read(RepositoryRoot.Find()));
        int index = files.FindIndex(file => file.Path.Contains("fog-fixture-dungeon", StringComparison.Ordinal));
        string thick = Encoding.UTF8.GetString(files[index].Bytes).Replace("\"strength\": 4500", "\"strength\": 8000", StringComparison.Ordinal);
        files[index] = new ContentFile(files[index].Path, Encoding.UTF8.GetBytes(thick));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Contains("luma gap", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-886", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutGivesTheFixtureDungeonItsDustAndThreeCaptureFiles()
    {
        // D-889: one file for each kind of D-187, and the shipped dungeon takes the dust.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        ContentId dungeon = ContentId.Parse("map.fixture_dungeon", "test", "id");

        AmbientEffect weather = Assert.IsType<AmbientEffect>(set.Effects.Ambient.WeatherOf(dungeon));
        Assert.Equal(AmbientKind.Dust, weather.Kind);

        var kinds = new SortedSet<string>(StringComparer.Ordinal);
        foreach (AmbientEffect effect in set.Effects.Ambient.All)
        {
            kinds.Add(AmbientEffect.NameOf(effect.Kind));
            Assert.Equal("map.fixture_dungeon", Assert.Single(effect.Maps).Value);
        }

        Assert.Equal(["dust", "fire", "fog", "snow"], kinds);
    }

    private static ContentId MapId() => ContentId.Parse(LightFixtures.MapId, AmbientFixtures.Path, "maps");

    private static ContentFile File(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));
}
