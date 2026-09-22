using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict readers of an ambient file: its kind, its maps, its streams, and its fog (D-187, D-897).</summary>
public sealed class AmbientFileTests
{
    [Fact]
    public void AnAmbientFileReadsItsKindItsMapsItsStreamAndItsFog()
    {
        AmbientEffect effect = Read(AmbientFixtures.Body(fogs: AmbientFixtures.Fog));

        Assert.Equal(AmbientKind.Dust, effect.Kind);
        Assert.Equal("map.lit", Assert.Single(effect.Maps).Value);
        Assert.True(effect.Lit);
        Assert.Equal(96, effect.Particles);
        FogLayer fog = Assert.Single(effect.Fogs);
        Assert.Equal('k', fog.Key);
        Assert.Equal([new FogBand(5000, 1000), new FogBand(6000, 2000)], fog.Bands);
        Assert.Equal(32, fog.Scale);
        Assert.Equal(2, fog.CellSize);
        Assert.Equal(7, fog.Seed);
        Assert.Equal(4, fog.DriftX);
        Assert.Equal(0, fog.DriftY);
        Assert.Equal(2000, fog.Strongest);
        Assert.Equal(1, effect.FullScreenPasses);
    }

    [Fact]
    public void ACaptureFileTellsThatTheScreenTestAloneLoadsIt()
    {
        // D-889: the shipped build gives each map the weather of the ambient folder.
        AmbientEffect shipped = Read(AmbientFixtures.Body());
        AmbientEffect capture = AmbientEffect.Read(Bytes(AmbientFixtures.Body()), AmbientFixtures.CapturePath);

        Assert.False(shipped.IsCapture);
        Assert.True(capture.IsCapture);
    }

    [Theory]
    [InlineData("\"kind\": \"dust\"", "\"kind\": \"rain\"", "'snow', 'fog', 'fire', or 'dust'")]
    [InlineData("\"maps\": [\"map.lit\"]", "\"maps\": []", "serves no map")]
    [InlineData("\"amount\": 24", "\"amount\": 513", "1 to 512")]
    [InlineData("\"lifetime_ticks\": 240", "\"lifetime_ticks\": 1801", "1 to 1800")]
    [InlineData("\"sway_pixels\": 3", "\"sway_pixels\": 33", "0 to 32")]
    [InlineData("\"fall_ticks\": 120", "\"fall_ticks\": 241", "1 to 240")]
    public void AnAmbientValueOutsideItsLimitFailsWithTheReason(string from, string to, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule.
        ContentException error = Assert.Throws<ContentException>(
            () => Read(AmbientFixtures.Body().Replace(from, to, StringComparison.Ordinal)));

        Assert.Equal(AmbientFixtures.Path, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAmbientFileWithNoStreamAndNoFogFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(AmbientFixtures.Body(emitters: string.Empty)));

        Assert.Contains("holds no emitter and no fog", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAmbientFileWithAnAbsentFieldFails()
    {
        // T-2: an absent value is an error, never a zero.
        ContentException error = Assert.Throws<ContentException>(
            () => Read(AmbientFixtures.Body().Replace("\"lit\": true,", string.Empty, StringComparison.Ordinal)));

        Assert.Equal("lit", error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{ \"from\": 6000, \"strength\": 2000 } ]", "{ \"from\": 6000, \"strength\": 2000 }, { \"from\": 7000, \"strength\": 3000 }, { \"from\": 8000, \"strength\": 4000 } ]", "1 to 3")]
    [InlineData("\"strength\": 2000", "\"strength\": 8001", "1 to 8000")]
    [InlineData("\"from\": 6000", "\"from\": 10000", "1 to 9999")]
    [InlineData("\"from\": 6000", "\"from\": 5000", "starts higher")]
    [InlineData("\"strength\": 2000", "\"strength\": 1000", "draws stronger")]
    [InlineData("\"cell_size\": 2", "\"cell_size\": 9", "1 to 8")]
    [InlineData("\"drift_x\": 4", "\"drift_x\": 61", "-60 to 60")]
    [InlineData("\"scale\": 32", "\"scale\": 6", "8 to 256")]
    [InlineData("\"scale\": 32", "\"scale\": 258", "8 to 256")]
    [InlineData("\"scale\": 32", "\"scale\": 33", "an even value")]
    [InlineData("\"seed\": 7", "\"seed\": -1", "0 to 65535")]
    [InlineData("\"scale\": 32,", "", "the field is absent")]
    [InlineData("\"seed\": 7,", "\"seed\": 7, \"rows\": [],", "an unknown field")]
    public void AFogValueOutsideItsLimitFailsWithTheReason(string from, string to, string reason)
    {
        string body = AmbientFixtures.Body(fogs: AmbientFixtures.Fog).Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFogOfFourLayersFails()
    {
        // D-898: the shader draws 1 to 3 layers in one pass.
        string fogs = string.Join(", ", AmbientFixtures.Fog, AmbientFixtures.Fog, AmbientFixtures.Fog, AmbientFixtures.Fog);

        ContentException error = Assert.Throws<ContentException>(() => Read(AmbientFixtures.Body(fogs: fogs)));

        Assert.Contains("more than 3 layers", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFogOfThreeLayersIsOneFullScreenPass()
    {
        // D-898: the shader draws every layer of a fog in one pass, and a weather with no fog draws none.
        string fogs = string.Join(", ", AmbientFixtures.Fog, AmbientFixtures.Fog, AmbientFixtures.Fog);

        Assert.Equal(1, Read(AmbientFixtures.Body(fogs: fogs)).FullScreenPasses);
        Assert.Equal(0, Read(AmbientFixtures.Body()).FullScreenPasses);
    }

    [Fact]
    public void AStreamHoldsOneColorInLightAndOneWithNoLight()
    {
        // D-181, D-893: a mote reads as light gray in torchlight and as dark gray outside one.
        AmbientEffect effect = Read(AmbientFixtures.Body(
            emitters: AmbientFixtures.Stream.Replace("\"dark_color\": \"k\"", "\"dark_color\": \"j\"", StringComparison.Ordinal)));
        MoteStream stream = Assert.Single(effect.Emitters);

        Assert.Equal('k', stream.Color);
        Assert.Equal('j', stream.DarkColor);

        // The budget counts the motes of the four cells that one view can hold (D-523, D-893).
        Assert.Equal(96, AmbientMotes.ParticlesOf(stream));
    }

    private static AmbientEffect Read(string body) => AmbientEffect.Read(Bytes(body), AmbientFixtures.Path);

    private static byte[] Bytes(string body) => Encoding.UTF8.GetBytes(body);
}
