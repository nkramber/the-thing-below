using System;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The shimmer of each light shaft, and the place of the shaft pass in the overlay (D-919, D-921).</summary>
public sealed class ShaftPassTests
{
    private const ulong AnySeed = 0x73686166UL;

    [Fact]
    public void AShimmerOfNoDepthKeepsTheBeamStill()
    {
        // D-921: a shimmer depth of 0 gives the still beam that the owner compares.
        for (long tick = 0; tick < 600; tick += 1)
        {
            Assert.Equal(1f, PartOf("piece.still", 240, 0, tick));
        }
    }

    [Fact]
    public void EachShimmerStaysBetweenTheLowAndTheTopOfTheWave()
    {
        // D-921: a depth of 4000 takes 40 percent of the beam at the low of the wave, and the wave
        // swells and fades with no jump, as the glow pulses (D-913).
        float most = MathF.PI * 0.4f / 240f;
        float last = PartOf("piece.shimmer", 240, 4000, 0);
        for (long tick = 1; tick < 600; tick += 1)
        {
            float part = PartOf("piece.shimmer", 240, 4000, tick);
            Assert.InRange(part, 0.6f - 0.0001f, 1f + 0.0001f);
            Assert.True(MathF.Abs(part - last) <= most + 0.0001f, $"tick {tick}: the shimmer moved {MathF.Abs(part - last)}, above {most}");
            last = part;
        }
    }

    [Fact]
    public void OneTickGivesOneShimmer()
    {
        // T-7, D-172: a capture of one tick shows one beam on every run.
        Assert.Equal(PartOf("piece.shimmer", 240, 4000, 123), PartOf("piece.shimmer", 240, 4000, 123));
        Assert.Equal(PartOf("piece.shimmer", 240, 4000, 17), PartOf("piece.shimmer", 240, 4000, 17 + 240), 4);
    }

    [Fact]
    public void TheShaftPassDrawsAboveTheFogAndTheHitBursts()
    {
        // D-919: a beam lights the fog, so the pass draws above the fog and the bursts, in the overlay.
        int shaft = (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.ShaftPass").GetField("ShaftZIndex")!.GetValue(null)!;
        int fog = (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.AmbientLayer").GetField("FogZIndex")!.GetValue(null)!;
        int burst = (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.HitBurst").GetField("BurstZIndex")!.GetValue(null)!;

        Assert.True(shaft > fog, $"the shafts take Z {shaft}, at or below the fog of Z {fog}");
        Assert.True(shaft > burst, $"the shafts take Z {shaft}, at or below the bursts of Z {burst}");
    }

    private static float PartOf(string id, int waveTicks, int depth, long tick)
    {
        MethodInfo method = GameAssemblyFile.Type("TheThingBelow.Game.Ui.LightWave").GetMethod("PartOf")!;
        return (float)method.Invoke(null, [id, waveTicks, depth, tick, AnySeed])!;
    }
}
