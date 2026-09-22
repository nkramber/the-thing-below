using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict readers of an ambient file: its kind, its maps, its streams, and its fog (D-187, D-887).</summary>
public sealed class AmbientFileTests
{
    [Fact]
    public void AnAmbientFileReadsItsKindItsMapsItsStreamAndItsFog()
    {
        AmbientEffect effect = Read(AmbientFixtures.Body(fogs: AmbientFixtures.Fog));

        Assert.Equal(AmbientKind.Dust, effect.Kind);
        Assert.Equal("map.lit", Assert.Single(effect.Maps).Value);
        Assert.True(effect.Lit);
        Assert.Equal(24, effect.Particles);
        FogLayer fog = Assert.Single(effect.Fogs);
        Assert.Equal('k', fog.Key);
        Assert.Equal(8, fog.Width);
        Assert.Equal(8, fog.Height);
        Assert.Equal(2000, fog.Strongest);
        Assert.Equal(0, fog.BandAt(0, 0));
        Assert.Equal(2, fog.BandAt(4, 3));
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
    [InlineData("\"amount\": 24", "\"amount\": 2049", "1 to 2048")]
    [InlineData("\"lifetime_ticks\": 240", "\"lifetime_ticks\": 601", "1 to 600")]
    [InlineData("\"half_width\": 320", "\"half_width\": 321", "0 to 320")]
    [InlineData("\"slowest_speed\": 2, \"fastest_speed\": 8", "\"slowest_speed\": 9, \"fastest_speed\": 8", "below the slowest speed")]
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
    [InlineData("\"bands\": [1000, 2000]", "\"bands\": [1000, 2000, 3000, 4000]", "1 to 3")]
    [InlineData("\"bands\": [1000, 2000]", "\"bands\": [1000, 8001]", "1 to 8000")]
    [InlineData("\"cell_size\": 2", "\"cell_size\": 9", "1 to 8")]
    [InlineData("\"drift_x\": 4", "\"drift_x\": 61", "-60 to 60")]
    [InlineData("\"..1111..\", ", "", "7 rows")]
    [InlineData("\"11222211\", \"12222221\"", "\"1122221\", \"12222221\"", "7 cells")]
    [InlineData("\"12222221\", \"12222221\"", "\"12222223\", \"12222221\"", "'3'")]
    public void AFogValueOutsideItsLimitFailsWithTheReason(string from, string to, string reason)
    {
        string body = AmbientFixtures.Body(fogs: AmbientFixtures.Fog).Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStreamSplitsItsParticlesOverItsKeys()
    {
        // D-181: Game draws each key as a node of its own, so each particle keeps one color.
        AmbientEffect effect = Read(AmbientFixtures.Body(
            emitters: AmbientFixtures.Stream.Replace("[\"k\"]", "[\"k\", \"j\", \"k\"]", StringComparison.Ordinal)));
        StreamEmitter stream = Assert.Single(effect.Emitters);

        Assert.Equal(8, stream.AmountOf(0));
        Assert.Equal(8, stream.AmountOf(1));
        Assert.Equal(8, stream.AmountOf(2));
        Assert.Equal(24, StreamEmitter.ParticlesOf(new List<StreamEmitter> { stream }));
    }

    private static AmbientEffect Read(string body) => AmbientEffect.Read(Bytes(body), AmbientFixtures.Path);

    private static byte[] Bytes(string body) => Encoding.UTF8.GetBytes(body);
}
