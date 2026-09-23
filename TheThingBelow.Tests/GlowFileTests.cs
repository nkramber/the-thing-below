using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict reader of the glow file (D-910, D-913, D-915).</summary>
public sealed class GlowFileTests
{
    [Fact]
    public void TheGlowFileReadsEachValue()
    {
        Glow glow = Read(UiContentFixtures.GlowBody);

        Assert.Equal(70000, glow.Threshold);
        Assert.Equal(20000, glow.Knee);
        Assert.Equal(8000, glow.Intensity);
        Assert.Equal(10000, glow.Strength);
        Assert.Equal(new[] { 0, 0, 10000, 0, 10000, 0, 0 }, glow.Levels);
        Assert.Equal(90, glow.PulseTicks);
        Assert.Equal(2500, glow.PulseDepth);
    }

    [Theory]
    [InlineData("\"threshold\": 70000", "\"threshold\": 10000", "threshold", "above full white")]
    [InlineData("\"threshold\": 70000", "\"threshold\": 80001", "threshold", "10001 to 80000")]
    [InlineData("\"knee\": 20000", "\"knee\": 0", "knee", "1 to 40000")]
    [InlineData("\"intensity\": 8000", "\"intensity\": 80001", "intensity", "1 to 80000")]
    [InlineData("\"strength\": 10000", "\"strength\": 20001", "strength", "1 to 20000")]
    [InlineData("[0, 0, 10000, 0, 10000, 0, 0]", "[0, 0, 10000, 0, 10000, 0]", "levels", "takes 7")]
    [InlineData("[0, 0, 10000, 0, 10000, 0, 0]", "[0, 0, 10001, 0, 10000, 0, 0]", "levels", "0 to 10000")]
    [InlineData("[0, 0, 10000, 0, 10000, 0, 0]", "[0, 0, 0, 0, 0, 0, 0]", "levels", "draws nothing")]
    [InlineData("\"pulse_ticks\": 90", "\"pulse_ticks\": 1", "pulse_ticks", "2 to 600")]
    [InlineData("\"pulse_depth\": 2500", "\"pulse_depth\": 5001", "pulse_depth", "0 to 5000")]
    public void AGlowValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule. A threshold at full white
        // would let unlit art glow (D-910).
        ContentException error = Assert.Throws<ContentException>(
            () => Read(UiContentFixtures.GlowBody.Replace(from, to, StringComparison.Ordinal)));

        Assert.Equal(Glow.Path, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"threshold\": 70000, ")]
    [InlineData("\"knee\": 20000, ")]
    [InlineData("\"levels\": [0, 0, 10000, 0, 10000, 0, 0], ")]
    [InlineData("\"pulse_depth\": 2500 ")]
    public void AGlowWithAnAbsentFieldFails(string removed)
    {
        // T-2: an absent value is an error, never a default.
        string body = UiContentFixtures.GlowBody.Replace(removed, string.Empty, StringComparison.Ordinal).Replace(", }", " }", StringComparison.Ordinal);
        Assert.NotEqual(UiContentFixtures.GlowBody, body);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutReadsItsGlowAboveFullWhiteWithAPulse()
    {
        // D-910: the threshold sits above full white, so no art that draws with no light glows.
        // D-913: the glow pulses, and never stands still.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.True(set.Light.Glow.Threshold > 10000, $"the threshold is {set.Light.Glow.Threshold}, at or below full white (D-910)");
        Assert.True(set.Light.Glow.PulseDepth > 0, "the glow of the checkout holds no pulse (D-913)");
    }

    private static Glow Read(string body) => Glow.Read(Encoding.UTF8.GetBytes(body), Glow.Path);
}
