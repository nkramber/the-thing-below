using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict reader of a shaft kind, and the light shafts of a decor file (D-918, D-921).</summary>
public sealed class ShaftKindTests
{
    [Fact]
    public void AShaftKindReadsEachValue()
    {
        ShaftKind kind = Read(LightFixtures.ShaftKindBody);

        Assert.Equal(LightFixtures.ShaftKindId, kind.Id.Value);
        Assert.Equal('j', kind.Key);
        Assert.Equal(3000, kind.Strength);
        Assert.Equal(12, kind.Width);
        Assert.Equal(96, kind.Length);
        Assert.Equal(16, kind.Slant);
        Assert.Equal(16, kind.X);
        Assert.Equal(8, kind.Y);
        Assert.Equal(240, kind.ShimmerTicks);
        Assert.Equal(0, kind.ShimmerDepth);
    }

    [Theory]
    [InlineData("\"color\": \"j\"", "\"color\": \"jj\"", "color", "one palette key")]
    [InlineData("\"strength\": 3000", "\"strength\": 0", "strength", "draws nothing")]
    [InlineData("\"strength\": 3000", "\"strength\": 10001", "strength", "1 to 10000")]
    [InlineData("\"width\": 12", "\"width\": 0", "width", "1 to 32")]
    [InlineData("\"width\": 12", "\"width\": 33", "width", "1 to 32")]
    [InlineData("\"length\": 96", "\"length\": 0", "length", "1 to 360")]
    [InlineData("\"length\": 96", "\"length\": 361", "length", "1 to 360")]
    [InlineData("\"slant\": 16", "\"slant\": 97", "slant", "-96 to 96")]
    [InlineData("\"slant\": 16", "\"slant\": -97", "slant", "-96 to 96")]
    [InlineData("\"x\": 16", "\"x\": 32", "x", "0 to 31")]
    [InlineData("\"y\": 8", "\"y\": 64", "y", "0 to 63")]
    [InlineData("\"shimmer_ticks\": 240", "\"shimmer_ticks\": 1", "shimmer_ticks", "2 to 600")]
    [InlineData("\"shimmer_depth\": 0", "\"shimmer_depth\": 5001", "shimmer_depth", "0 to 5000")]
    public void AValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule. A beam of no light would
        // draw nothing, in silence.
        ContentException error = Assert.Throws<ContentException>(() => Read(Replaced(from, to)));

        Assert.Equal(LightFixtures.ShaftKindPath, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"color\": \"j\", ")]
    [InlineData("\"length\": 96, ")]
    [InlineData(", \"shimmer_depth\": 0")]
    public void AShaftKindWithAnAbsentFieldFails(string removed)
    {
        // T-2: an absent value is an error, never a default.
        ContentException error = Assert.Throws<ContentException>(() => Read(Replaced(removed, string.Empty)));

        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutHoldsAStillShaftAndAShaftThatShimmers()
    {
        // D-921: the owner compares a still beam with a beam that shimmers, on the fixture dungeon.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        DecorFile decor = set.Light.DecorOf(ContentId.Parse("map.fixture_dungeon", "test", "map"));

        Assert.Equal(2, decor.Shafts.Count);
        Assert.Equal(0, set.Light.ShaftOf(decor.Shafts[0].Kind).ShimmerDepth);
        Assert.True(set.Light.ShaftOf(decor.Shafts[1].Kind).ShimmerDepth > 0, "the second shaft of the fixture dungeon holds no shimmer (D-921)");
    }

    [Fact]
    public void AShaftOnAFloorTileFailsWithItsListAndIndex()
    {
        // D-918: a shaft hangs on a wall with a floor or a doorway to its south, as a torch does.
        ContentException error = Assert.Throws<ContentException>(() => Load(LightFixtures.DecorBody(string.Empty, shafts: LightFixtures.Shaft("beam", 3, 1))));

        Assert.Equal(LightFixtures.DecorPath, error.File);
        Assert.Equal("shafts[0]", error.Field);
        Assert.Contains("hangs on a wall", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShaftOfAnAbsentKindFails()
    {
        // T-2: the shaft pass reads each kind, so an absent kind fails at load and not on screen.
        string shaft = """{ "id": "piece.beam", "kind": "shaft.absent", "x": 3, "y": 0 }""";

        ContentException error = Assert.Throws<ContentException>(() => Load(LightFixtures.DecorBody(string.Empty, shafts: shaft)));

        Assert.Equal("shafts[0].kind", error.Field);
        Assert.Contains("shaft.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShaftWithTheIdOfAPieceFails()
    {
        // D-166: one id names one piece, the shafts of the file included.
        ContentException error = Assert.Throws<ContentException>(() => Load(LightFixtures.DecorBody(
            LightFixtures.Piece("twin", 3, 0),
            shafts: LightFixtures.Shaft("twin", 5, 0))));

        Assert.Equal("shafts[0].id", error.Field);
        Assert.Contains("one id names one piece", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MoreShaftsThanTheShaderHoldsFail()
    {
        // D-918, T-2: the shader holds the shafts of one map in arrays of 8, and a ninth would
        // draw nothing, in silence.
        var shafts = new List<string>();
        for (int index = 0; index <= ShaftKind.MostShaftsOnMap; index += 1)
        {
            shafts.Add(LightFixtures.Shaft($"beam_{index}", index + 1, 0));
        }

        ContentException error = Assert.Throws<ContentException>(() => Load(LightFixtures.DecorBody(string.Empty, shafts: string.Join(", ", shafts))));

        Assert.Equal("shafts", error.Field);
        Assert.Contains("8 at most", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShaftColorOutsideThePaletteFails()
    {
        // D-181: a beam takes a palette key.
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        int index = files.FindIndex(file => file.Path == LightFixtures.ShaftKindPath);
        files[index] = LightFixtures.File(LightFixtures.ShaftKindPath, Replaced("\"color\": \"j\"", "\"color\": \"Q\""));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal(LightFixtures.ShaftKindPath, error.File);
        Assert.Equal("color", error.Field);
    }

    private static LightContent Load(string decor) => LightFixtures.Load(LightFixtures.Files(decor, LightFixtures.SetupBody()));

    private static string Replaced(string from, string to)
    {
        string body = LightFixtures.ShaftKindBody.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(LightFixtures.ShaftKindBody, body);
        return body;
    }

    private static ShaftKind Read(string body) => ShaftKind.Read(Encoding.UTF8.GetBytes(body), LightFixtures.ShaftKindPath);
}
