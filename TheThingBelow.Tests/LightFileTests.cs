using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict readers of the light files (D-843 to D-847, F-46).</summary>
public sealed class LightFileTests
{
    [Fact]
    public void AKindFileReadsItsLightAndItsPlace()
    {
        DecorKind kind = DecorKind.Read(Bytes(LightFixtures.KindBody), LightFixtures.KindPath);

        Assert.Equal("decor.torch", kind.Id.Value);
        Assert.Equal(new PointLightValues(new LightColor('j', 12000), 96, 24), kind.Light);
        Assert.Equal(16, kind.LightX);
        Assert.Equal(36, kind.LightY);
    }

    [Theory]
    [InlineData("\"height\": 24", "\"height\": 0", "light.height", "height 0")]
    [InlineData("\"color\": \"j\"", "\"color\": \"jj\"", "light.color", "one palette key")]
    [InlineData("\"strength\": 12000", "\"strength\": 40001", "light.strength", "0 to 40000")]
    [InlineData("\"range\": 96", "\"range\": 0", "light.range", "1 to 640")]
    [InlineData("\"y\": 36", "\"y\": 64", "light.y", "0 to 63")]
    [InlineData("\"x\": 16", "\"x\": 32", "light.x", "0 to 31")]
    public void AKindValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // F-46: a light at height 0 gives no light to a flat pixel of a normal map.
        ContentException error = Assert.Throws<ContentException>(
            () => DecorKind.Read(Bytes(LightFixtures.KindBody.Replace(from, to, StringComparison.Ordinal)), LightFixtures.KindPath));

        Assert.Equal(LightFixtures.KindPath, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AKindWithNoHeightFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => DecorKind.Read(
            Bytes(LightFixtures.KindBody.Replace("\"height\": 24, ", string.Empty, StringComparison.Ordinal)),
            LightFixtures.KindPath));

        Assert.Equal("light.height", error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AKindWithAnUnknownLightFieldFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => DecorKind.Read(
            Bytes(LightFixtures.KindBody.Replace("\"x\": 16", "\"flicker\": 2, \"x\": 16", StringComparison.Ordinal)),
            LightFixtures.KindPath));

        Assert.Equal("light.flicker", error.Field);
    }

    [Fact]
    public void ADecorFileWithTwoPiecesOfOneIdFails()
    {
        string body = LightFixtures.DecorBody($"{LightFixtures.Piece("west", 2, 0)}, {LightFixtures.Piece("west", 4, 0)}");

        ContentException error = Assert.Throws<ContentException>(() => DecorFile.Read(Bytes(body), LightFixtures.DecorPath));

        Assert.Equal("pieces[1].id", error.Field);
        Assert.Contains("piece.west", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALightSetupReadsEachPart()
    {
        string body = LightFixtures.SetupBody(
            changes: """{ "piece": "piece.west", "color": "k", "strength": 3000, "range": 40, "height": 8 }""",
            added: LightFixtures.Added("crack", 5, 4));

        LightSetup setup = LightSetup.Read(Bytes(body), LightFixtures.SetupPath);

        Assert.Equal("map.lit", setup.Map.Value);
        Assert.Equal(Core.Maps.TimeOfDay.Night, setup.Time);
        Assert.Equal(new LightColor('k', 5000), setup.Ambient);
        Assert.Equal(new PointLightValues(new LightColor('j', 10000), 480, 96), setup.Battle);
        Assert.Equal("piece.west", Assert.Single(setup.Changes).Piece.Value);
        Assert.Equal(new Core.Maps.TilePoint(5, 4), Assert.Single(setup.Added).Tile);
    }

    [Fact]
    public void AnAmbientLightOverFullStrengthFails()
    {
        string body = LightFixtures.SetupBody().Replace(
            "\"ambient\": { \"color\": \"k\", \"strength\": 5000 }",
            "\"ambient\": { \"color\": \"k\", \"strength\": 10001 }",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => LightSetup.Read(Bytes(body), LightFixtures.SetupPath));

        Assert.Equal("ambient.strength", error.Field);
    }

    [Fact]
    public void ALightSetupOfAnUnknownTimeFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => LightSetup.Read(Bytes(LightFixtures.SetupBody(time: "noon")), LightFixtures.SetupPath));

        Assert.Equal("time", error.Field);
        Assert.Contains("noon", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoChangesOfOnePieceFail()
    {
        // The second change would win in silence (T-2).
        const string Change = """{ "piece": "piece.west", "color": "k", "strength": 3000, "range": 40, "height": 8 }""";

        ContentException error = Assert.Throws<ContentException>(
            () => LightSetup.Read(Bytes(LightFixtures.SetupBody(changes: $"{Change}, {Change}")), LightFixtures.SetupPath));

        Assert.Equal("changes[1].piece", error.Field);
    }

    [Fact]
    public void TwoAddedLightsOfOneIdFail()
    {
        string added = $"{LightFixtures.Added("crack", 5, 4)}, {LightFixtures.Added("crack", 6, 4)}";

        ContentException error = Assert.Throws<ContentException>(
            () => LightSetup.Read(Bytes(LightFixtures.SetupBody(added: added)), LightFixtures.SetupPath));

        Assert.Equal("added[1].id", error.Field);
    }

    [Fact]
    public void ALightSetupWithNoBattleLightFails()
    {
        // D-850: a battle that begins on the map takes its key light from the setup.
        string body = LightFixtures.SetupBody().Replace(
            " \"battle\": { \"color\": \"j\", \"strength\": 10000, \"range\": 480, \"height\": 96 },\n",
            string.Empty,
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => LightSetup.Read(Bytes(body), LightFixtures.SetupPath));

        Assert.Equal("battle", error.Field);
    }

    [Fact]
    public void TheCarriedLightReadsItsPlaceInTheHandOfTheLead()
    {
        CarriedLight carried = CarriedLight.Read(Bytes(UiContentFixtures.CarriedBody), CarriedLight.Path);

        Assert.Equal(16, carried.X);
        Assert.Equal(-16, carried.Y);
        Assert.Equal(64, carried.Light.Range);
    }

    [Fact]
    public void ACarriedLightUnderTheFeetOfTheLeadFails()
    {
        string body = UiContentFixtures.CarriedBody.Replace("\"y\": -16", "\"y\": 4", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => CarriedLight.Read(Bytes(body), CarriedLight.Path));

        Assert.Equal("y", error.Field);
    }

    [Fact]
    public void ABudgetRowOfZeroFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => EffectBudget.Read(Bytes(LightFixtures.BudgetBody(0)), EffectBudget.Path));

        Assert.Equal("lights_in_view", error.Field);
    }

    [Theory]
    [InlineData("decor/kinds/torch.json", true)]
    [InlineData("decor/maps/lit.json", true)]
    [InlineData("light/setups/lit-night.json", true)]
    [InlineData("light/carried.json", true)]
    [InlineData("effects/budget.json", true)]
    [InlineData("rules/maps/lit.json", false)]
    [InlineData("light/notes.json", false)]
    public void TheLightReaderTakesTheLightFilesAlone(string path, bool light)
    {
        Assert.Equal(light, LightContent.IsLightFile(path));
    }

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
}
