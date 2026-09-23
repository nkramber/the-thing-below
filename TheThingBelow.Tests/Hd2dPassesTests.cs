using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict reader of the file of the passes of the HD-2D look (D-849, D-917, D-920).</summary>
public sealed class Hd2dPassesTests
{
    [Fact]
    public void ThePassesFileReadsEachValue()
    {
        Hd2dPasses passes = Read(UiContentFixtures.PassesBody);

        Assert.Equal(PassMode.Smooth, passes.Mode);
        Assert.Equal(4, passes.Steps);
        Assert.Equal(2, passes.CellSize);
        Assert.Equal(72, passes.BlurBand);
        Assert.Equal(3, passes.BlurRadius);
        Assert.Equal('k', passes.VignetteKey);
        Assert.Equal(5000, passes.VignetteStrength);
        Assert.Equal(4000, passes.VignetteStart);
    }

    [Theory]
    [InlineData("\"mode\": \"smooth\"", "\"mode\": \"soft\"", "mode", "'smooth' or 'stepped'")]
    [InlineData("\"steps\": 4", "\"steps\": 1", "steps", "2 to 8")]
    [InlineData("\"steps\": 4", "\"steps\": 9", "steps", "2 to 8")]
    [InlineData("\"cell_size\": 2", "\"cell_size\": 0", "cell_size", "1 to 8")]
    [InlineData("\"cell_size\": 2", "\"cell_size\": 9", "cell_size", "1 to 8")]
    [InlineData("\"blur_band\": 72", "\"blur_band\": 0", "blur_band", "draws no blur")]
    [InlineData("\"blur_band\": 72", "\"blur_band\": 181", "blur_band", "1 to 180")]
    [InlineData("\"blur_radius\": 3", "\"blur_radius\": 0", "blur_radius", "draws nothing")]
    [InlineData("\"blur_radius\": 3", "\"blur_radius\": 9", "blur_radius", "1 to 8")]
    [InlineData("\"vignette_color\": \"k\"", "\"vignette_color\": \"kk\"", "vignette_color", "one palette key")]
    [InlineData("\"vignette_strength\": 5000", "\"vignette_strength\": 0", "vignette_strength", "draws nothing")]
    [InlineData("\"vignette_strength\": 5000", "\"vignette_strength\": 10001", "vignette_strength", "1 to 10000")]
    [InlineData("\"vignette_start\": 4000", "\"vignette_start\": 10000", "vignette_start", "0 to 9999")]
    public void AValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule. A pass of no strength would
        // draw nothing, in silence.
        ContentException error = Assert.Throws<ContentException>(
            () => Read(Replaced(from, to)));

        Assert.Equal(Hd2dPasses.Path, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"mode\": \"smooth\", ")]
    [InlineData("\"steps\": 4, ")]
    [InlineData("\"blur_band\": 72, ")]
    [InlineData("\"vignette_color\": \"k\", ")]
    [InlineData(", \"vignette_start\": 4000")]
    public void APassesFileWithAnAbsentFieldFails(string removed)
    {
        // T-2: an absent value is an error, never a default.
        string body = Replaced(removed, string.Empty);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFieldFails()
    {
        // G-6: the reader refuses a field that it does not know.
        ContentException error = Assert.Throws<ContentException>(
            () => Read(Replaced("\"steps\": 4", "\"steps\": 4, \"shafts\": 1")));

        Assert.Equal("shafts", error.Field);
    }

    [Fact]
    public void AnotherModeKeepsEveryOtherValue()
    {
        // D-917: a capture shows the other mode with the same values, so the two modes compare.
        Hd2dPasses smooth = Read(UiContentFixtures.PassesBody);
        Hd2dPasses stepped = smooth.WithMode(PassMode.Stepped);

        Assert.Equal(PassMode.Stepped, stepped.Mode);
        Assert.Equal(smooth.Steps, stepped.Steps);
        Assert.Equal(smooth.CellSize, stepped.CellSize);
        Assert.Equal(smooth.BlurBand, stepped.BlurBand);
        Assert.Equal(smooth.BlurRadius, stepped.BlurRadius);
        Assert.Equal(smooth.VignetteKey, stepped.VignetteKey);
        Assert.Equal(smooth.VignetteStrength, stepped.VignetteStrength);
        Assert.Equal(smooth.VignetteStart, stepped.VignetteStart);
        Assert.Equal("stepped", Hd2dPasses.NameOf(stepped.Mode));
    }

    [Fact]
    public void TheCheckoutShowsTheSmoothModeUntilTheOwnerPicks()
    {
        // D-917: the file holds the mode of the three passes, and the captures show the other mode.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.Equal(PassMode.Smooth, set.Light.Passes.Mode);
    }

    [Fact]
    public void AVignetteColorOutsideThePaletteFails()
    {
        // D-181: the dark of the vignette takes a palette key.
        List<ContentFile> files = LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody());
        int index = files.FindIndex(file => file.Path == Hd2dPasses.Path);
        files[index] = LightFixtures.File(Hd2dPasses.Path, Replaced("\"vignette_color\": \"k\"", "\"vignette_color\": \"Q\""));

        ContentException error = Assert.Throws<ContentException>(() => LightFixtures.Load(files));

        Assert.Equal(Hd2dPasses.Path, error.File);
        Assert.Equal("vignette_color", error.Field);
    }

    private static string Replaced(string from, string to)
    {
        string body = UiContentFixtures.PassesBody.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(UiContentFixtures.PassesBody, body);
        return body;
    }

    private static Hd2dPasses Read(string body) => Hd2dPasses.Read(Encoding.UTF8.GetBytes(body), Hd2dPasses.Path);
}
