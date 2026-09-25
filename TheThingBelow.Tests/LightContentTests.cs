using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The checks of the light files across files: the decor, the light setups, the palette keys,
/// and the effect budget (D-523, D-842 to D-847).
/// </summary>
public sealed class LightContentTests
{
    private static readonly ContentId Map = ContentId.Parse(LightFixtures.MapId, "test", "id");

    [Fact]
    public void EachTorchLightsItselfOverTheTileToItsSouth()
    {
        LightContent light = LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(LightFixtures.Piece("west", 2, 0)),
            LightFixtures.SetupBody()));

        MapLight torch = Assert.Single(light.LightsOf(Map, TimeOfDay.Night));

        // The tile (2, 0) starts at (64, 0), and the kind puts the light at (16, 36) inside it.
        Assert.Equal("piece.west", torch.Id.Value);
        Assert.Equal(80, torch.X);
        Assert.Equal(36, torch.Y);
        Assert.Equal(96, torch.Light.Range);
        Assert.Equal(new LightColor('j', 12000), torch.Light.Color);
    }

    [Fact]
    public void TheChangeOfTheLightSetupWinsOverTheDefaultOfTheKind()
    {
        // D-843: one torch takes its light from two files, and the change wins.
        LightContent light = LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody($"{LightFixtures.Piece("west", 2, 0)}, {LightFixtures.Piece("east", 6, 0)}"),
            LightFixtures.SetupBody(changes: """{ "piece": "piece.east", "color": "k", "strength": 3000, "range": 40, "height": 8 }""")));

        IReadOnlyList<MapLight> lights = light.LightsOf(Map, TimeOfDay.Night);

        Assert.Equal(2, lights.Count);
        Assert.Equal(new PointLightValues(new LightColor('j', 12000), 96, 24), lights[0].Light);
        Assert.Equal(new PointLightValues(new LightColor('k', 3000), 40, 8), lights[1].Light);

        // The change keeps the place that the kind gives the light.
        Assert.Equal((6 * 32) + 16, lights[1].X);
        Assert.Equal(36, lights[1].Y);
    }

    [Fact]
    public void AnAddedLightSitsAtTheCenterOfItsTileAfterEachPiece()
    {
        LightContent light = LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(LightFixtures.Piece("west", 2, 0)),
            LightFixtures.SetupBody(added: LightFixtures.Added("crack", 5, 4))));

        IReadOnlyList<MapLight> lights = light.LightsOf(Map, TimeOfDay.Night);

        Assert.Equal("light.crack", lights[1].Id.Value);
        Assert.Equal((5 * 32) + 16, lights[1].X);
        Assert.Equal((4 * 32) + 16, lights[1].Y);
    }

    [Fact]
    public void AChangeOfAnAbsentPieceFailsWithTheFileAndTheId()
    {
        // D-843: a moved or deleted torch never leaves a change behind in silence.
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(LightFixtures.Piece("west", 2, 0)),
            LightFixtures.SetupBody(changes: """{ "piece": "piece.gone", "color": "j", "strength": 3000, "range": 40, "height": 8 }"""))));

        Assert.Equal(LightFixtures.SetupPath, error.File);
        Assert.Equal("changes[0].piece", error.Field);
        Assert.Contains("piece.gone", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALightSetupOfAnAbsentMapFailsWithTheFileAndTheId()
    {
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files.Add(LightFixtures.File("light/setups/deep-night.json", LightFixtures.SetupBody(map: "map.deep")));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal("light/setups/deep-night.json", error.File);
        Assert.Contains("map.deep", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoLightSetupForItsTimeFails()
    {
        // D-442: the map draws in the light of its own time, so that setup must exist.
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(time: "day"))));

        Assert.Contains("map.lit", error.Message, StringComparison.Ordinal);
        Assert.Contains("night", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoDecorFileFails()
    {
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files.RemoveAll(file => file.Path == LightFixtures.DecorPath);

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal("rules/maps/lit.json", error.File);
        Assert.Contains("each map has one", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADecorFileOfAnAbsentMapFails()
    {
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files.Add(LightFixtures.File("decor/maps/deep.json", LightFixtures.DecorBody(string.Empty, map: "map.deep")));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal("decor/maps/deep.json", error.File);
        Assert.Contains("map.deep", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASecondLightSetupOfOneMapAtOneTimeFails()
    {
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files.Add(LightFixtures.File("light/setups/lit-again.json", LightFixtures.SetupBody()));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal("light/setups/lit-again.json", error.File);
        Assert.Contains("second light setup", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(2, 1, "floor")]
    [InlineData(2, 11, "no floor or doorway to its south")]
    [InlineData(25, 0, "outside the map")]
    public void ATorchOffAWallOrWithNoFloorSouthFailsWithTheFileAndTheId(int x, int y, string reason)
    {
        // D-844: a wall torch hangs on a wall and faces a tile that the party can stand on.
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(LightFixtures.Piece("wrong", x, y)),
            LightFixtures.SetupBody())));

        Assert.Equal(LightFixtures.DecorPath, error.File);
        Assert.Contains("piece.wrong", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APieceOfAnAbsentKindFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody("""{ "id": "piece.odd", "kind": "decor.brazier", "x": 2, "y": 0 }"""),
            LightFixtures.SetupBody())));

        Assert.Equal("pieces[0].kind", error.Field);
        Assert.Contains("decor.brazier", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AKindThatNoDrawingDrawsFails()
    {
        // D-519: Game draws each piece, so a kind with no drawing would draw nothing.
        ContentException error = Assert.Throws<ContentException>(() => LightContent.Load(
            LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody()),
            LightFixtures.Maps(),
            LightFixtures.Palette(),
            LightFixtures.Atlas(draws: "decor.other")));

        Assert.Equal(LightFixtures.KindPath, error.File);
        Assert.Contains("decor.torch", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AColorThatThePaletteLacksFailsWithTheField()
    {
        // D-846: every light names a key of the one palette.
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(added: """{ "id": "light.odd", "x": 3, "y": 3, "color": "Z", "strength": 8000, "range": 32, "height": 16 }"""))));

        Assert.Equal(LightFixtures.SetupPath, error.File);
        Assert.Equal("added[0].color", error.Field);
    }

    [Fact]
    public void AMapOverTheRowOfTheBudgetFailsWithTheFileAndTheCount()
    {
        // D-523, D-842, D-853: five sources and the carried light reach one view as 12 Godot
        // lights, two for each source, and the row allows 10.
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(added: AddedLights(5)),
            LightFixtures.BudgetBody(10))));

        Assert.Equal(LightFixtures.SetupPath, error.File);
        Assert.Contains("12 lights", error.Message, StringComparison.Ordinal);
        Assert.Contains("lights_in_view", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCarriedLightCountsInEachView()
    {
        // D-847, D-853: 11 sources and the carried light fill the row of 24 as pairs, and 12
        // sources pass it.
        LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(added: AddedLights(11)),
            LightFixtures.BudgetBody(24)));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(added: AddedLights(12)),
            LightFixtures.BudgetBody(24))));

        Assert.Contains("26 lights, the carried light included", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MoreThanFifteenLightsOnOneCanvasItemFailUnderAHigherRow()
    {
        // F-46: Godot drops each light past 15 on one canvas item, whatever the budget row.
        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(added: AddedLights(15)),
            LightFixtures.BudgetBody(40))));

        Assert.Contains("canvas item", error.Message, StringComparison.Ordinal);
        Assert.Contains("allows 15", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoTorchesSideBySideCanLightArtToTheThresholdAndFail()
    {
        // D-910, F-47: Godot adds the light of each torch with no clamp. One torch of 12000 lights
        // white art below a threshold of 20000, and a second torch one tile away pushes it past.
        string threshold = UiContentFixtures.GlowBody.Replace("\"threshold\": 70000", "\"threshold\": 20000", StringComparison.Ordinal);
        LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(LightFixtures.Piece("west", 2, 0)),
            LightFixtures.SetupBody(),
            glow: threshold));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody($"{LightFixtures.Piece("west", 2, 0)}, {LightFixtures.Piece("east", 3, 0)}"),
            LightFixtures.SetupBody(),
            glow: threshold)));

        Assert.Equal(LightFixtures.SetupPath, error.File);
        Assert.Contains("of the map 'map.lit'", error.Message, StringComparison.Ordinal);
        Assert.Contains("the threshold of `effects/glow.json` is 20000, so that art would glow", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFightWhoseKeyLightCanLightArtToTheThresholdFails()
    {
        // D-850, D-910: the fight takes the ambient light and the key light of the setup. The
        // key light of 10000 in the flame of 251 of 255 and the ambient light of ink give 10139.
        string threshold = UiContentFixtures.GlowBody.Replace("\"threshold\": 70000", "\"threshold\": 10100", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(LightFixtures.Files(
            LightFixtures.DecorBody(string.Empty),
            LightFixtures.SetupBody(),
            glow: threshold)));

        Assert.Equal(LightFixtures.SetupPath, error.File);
        Assert.Equal("battle", error.Field);
        Assert.Contains("10139 basis points in a fight", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("j", 5000, true)]
    [InlineData("j", 71000, true)]
    [InlineData("j", 72000, false)]
    [InlineData("j", 160000, false)]
    [InlineData("k", 160000, true)]
    public void AGlowHaloThatPassesTheThresholdFails(string key, int strength, bool loads)
    {
        // D-1075, T-2: a halo above the threshold of 70000 clips to full light, and the glow of
        // Godot spreads it into a box. The flame color, whose brightest channel is 251 of 255,
        // stays below at 71000 and passes at 72000. Ink at 16 times stays far below it.
        string body = LightFixtures.KindBody.Replace(
            "\"glow\": { \"color\": \"k\", \"strength\": 0,",
            $"\"glow\": {{ \"color\": \"{key}\", \"strength\": {strength},",
            StringComparison.Ordinal);
        Assert.NotEqual(LightFixtures.KindBody, body);
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files[0] = LightFixtures.File(LightFixtures.KindPath, body);

        if (loads)
        {
            LightContent light = LightFixtures.Load(files);
            Assert.Equal(strength, light.KindOf(ContentId.Parse(LightFixtures.KindId, "test", "id")).Fire.Glow.Strength);
            return;
        }

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));
        Assert.Equal(LightFixtures.KindPath, error.File);
        Assert.Equal("fire.glow.strength", error.Field);
        Assert.Contains("at the top of its pulse", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGlowColorThatThePaletteLacksFailsWithTheField()
    {
        // D-846: the glow of a fire names a key of the one palette, as its light does.
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files[0] = LightFixtures.File(
            LightFixtures.KindPath,
            LightFixtures.KindBody.Replace("\"glow\": { \"color\": \"k\"", "\"glow\": { \"color\": \"Z\"", StringComparison.Ordinal));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal(LightFixtures.KindPath, error.File);
        Assert.Equal("fire.glow.color", error.Field);
    }

    [Fact]
    public void ALightSetWithNoGlowFileFails()
    {
        // T-2: the glow is one file of every build, and its absence is an error.
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        files.RemoveAll(file => file.Path == Glow.Path);

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal(Glow.Path, error.File);
    }

    [Fact]
    public void TheCheckoutFixtureDungeonHoldsItsTorchesAndItsChange()
    {
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        ContentId dungeon = ContentId.Parse("map.fixture_dungeon", "test", "id");

        IReadOnlyList<MapLight> lights = set.Light.LightsOf(dungeon, TimeOfDay.Night);

        Assert.Equal(9, lights.Count);
        MapLight vault = Assert.Single(lights, light => light.Id.Value == "piece.fixture_dungeon_vault");
        Assert.Equal('J', vault.Light.Color.Key);
    }

    /// <summary>Makes one added light on each of the first tiles of row 3, each with a short range.</summary>
    private static string AddedLights(int count)
    {
        var lights = new List<string>();
        for (int index = 0; index < count; index += 1)
        {
            lights.Add(LightFixtures.Added($"lamp_{index}", 1 + index, 3, range: 8));
        }

        return string.Join(", ", lights);
    }
}
