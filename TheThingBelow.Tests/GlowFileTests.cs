using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict reader of the glow file (D-913, D-914).</summary>
public sealed class GlowFileTests
{
    [Fact]
    public void TheGlowFileReadsEachValue()
    {
        Glow glow = Read(UiContentFixtures.GlowBody);

        Assert.Equal(60000, glow.Intensity);
        Assert.Equal(0, glow.Steps);
        Assert.Equal(2, glow.CellSize);
        Assert.Equal(90, glow.PulseTicks);
        Assert.Equal(2500, glow.PulseDepth);
    }

    [Fact]
    public void AStepGlowReadsItsSteps()
    {
        // D-914: the glow fades in steps over blocks, as the fog does (D-907).
        Glow glow = Read(UiContentFixtures.GlowBody.Replace("\"steps\": 0", "\"steps\": 4", StringComparison.Ordinal));

        Assert.Equal(4, glow.Steps);
    }

    [Theory]
    [InlineData("\"intensity\": 60000", "\"intensity\": 0", "intensity", "1 to 80000")]
    [InlineData("\"intensity\": 60000", "\"intensity\": 80001", "intensity", "1 to 80000")]
    [InlineData("\"steps\": 0", "\"steps\": 1", "steps", "0 is smooth")]
    [InlineData("\"steps\": 0", "\"steps\": 9", "steps", "0 to 8")]
    [InlineData("\"cell_size\": 2", "\"cell_size\": 0", "cell_size", "1 to 8")]
    [InlineData("\"pulse_ticks\": 90", "\"pulse_ticks\": 1", "pulse_ticks", "2 to 600")]
    [InlineData("\"pulse_depth\": 2500", "\"pulse_depth\": 5001", "pulse_depth", "0 to 5000")]
    public void AGlowValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule.
        ContentException error = Assert.Throws<ContentException>(
            () => Read(UiContentFixtures.GlowBody.Replace(from, to, StringComparison.Ordinal)));

        Assert.Equal(Glow.Path, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"intensity\": 60000, ")]
    [InlineData("\"steps\": 0, ")]
    [InlineData("\"pulse_depth\": 2500 ")]
    public void AGlowWithAnAbsentFieldFails(string removed)
    {
        // T-2: an absent value is an error, never a default.
        string body = UiContentFixtures.GlowBody.Replace(removed, string.Empty, StringComparison.Ordinal).Replace(", }", " }", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutReadsItsGlowFile()
    {
        // D-913: the glow pulses and never stands still, so the checkout holds a pulse.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.True(set.Light.Glow.PulseDepth > 0, "the glow of the checkout holds no pulse (D-913)");
        Assert.True(set.Light.Glow.Intensity > 0, "the glow of the checkout holds no intensity");
    }

    private static Glow Read(string body) => Glow.Read(Encoding.UTF8.GetBytes(body), Glow.Path);
}
