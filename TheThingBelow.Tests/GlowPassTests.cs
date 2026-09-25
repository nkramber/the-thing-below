using System;
using System.Reflection;
using System.Text;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The pulse of each glow, and the layer above the glow (D-913, D-916).</summary>
public sealed class GlowPassTests
{
    private static readonly Glow Pulsing = Glow.Read(Encoding.UTF8.GetBytes(UiContentFixtures.GlowBody), Glow.Path);

    [Fact]
    public void EachPulseStaysBetweenTheLowAndTheTopOfTheWave()
    {
        // D-913: the depth of 2500 takes a quarter of the glow at the low of the wave.
        for (long tick = 0; tick < 400; tick += 1)
        {
            float part = PulseOf(Pulsing, "piece.one", tick);
            Assert.InRange(part, 0.75f - 0.0001f, 1f + 0.0001f);
        }
    }

    [Fact]
    public void ThePulseSwellsAndFadesWithNoJump()
    {
        // D-913: the owner asked for a pulse in place of a flicker. The cosine of a wave of 90 ticks
        // moves at most pi times the depth over the wave on one tick, far below the flicker of a
        // torch light, which steps by a whole level (D-891).
        float most = MathF.PI * 0.25f / 90f;
        float last = PulseOf(Pulsing, "piece.one", 0);
        for (long tick = 1; tick < 400; tick += 1)
        {
            float part = PulseOf(Pulsing, "piece.one", tick);
            Assert.True(MathF.Abs(part - last) <= most + 0.0001f, $"tick {tick}: the pulse moved {MathF.Abs(part - last)}, above {most}");
            last = part;
        }
    }

    [Fact]
    public void OneTickGivesOnePulseAndTheWaveRepeats()
    {
        // T-7, D-172: a capture of one tick shows one glow on every run.
        Assert.Equal(PulseOf(Pulsing, "piece.one", 123), PulseOf(Pulsing, "piece.one", 123));
        Assert.Equal(PulseOf(Pulsing, "piece.one", 17), PulseOf(Pulsing, "piece.one", 17 + Pulsing.PulseTicks), 4);
    }

    [Fact]
    public void TwoTorchesPulseOutOfStep()
    {
        // D-913: a row of torches never pulses together, as their light never steps together (D-891).
        int apart = 0;
        for (long tick = 0; tick < Pulsing.PulseTicks; tick += 1)
        {
            if (MathF.Abs(PulseOf(Pulsing, "piece.fixture_dungeon_camp_west", tick) - PulseOf(Pulsing, "piece.fixture_dungeon_camp_east", tick)) > 0.001f)
            {
                apart += 1;
            }
        }

        Assert.True(apart > Pulsing.PulseTicks / 2, $"the two torches held one pulse on {Pulsing.PulseTicks - apart} ticks of {Pulsing.PulseTicks}");
    }

    [Fact]
    public void TheWorldNeverDrawsTheLayerAboveTheGlow()
    {
        // D-916: the fog, the hit bursts, and the light shafts draw on one layer, which the world view with
        // the glow never draws. Every other node keeps the first layer, which the world draws.
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass");
        uint above = (uint)pass.GetField("AboveGlowLayer")!.GetValue(null)!;
        uint world = (uint)pass.GetField("WorldLayers")!.GetValue(null)!;

        Assert.Equal(1, System.Numerics.BitOperations.PopCount(above));
        Assert.Equal(0u, world & above);
        Assert.NotEqual(0u, world & 1u);
    }

    [Fact]
    public void NoViewButTheMarkViewDrawsTheLayerOfTheMarks()
    {
        // D-919: the marks draw in a view of their own, above the tilt-shift blur and the vignette,
        // so neither the world with its glow nor the overlay under the passes draws them.
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass");
        uint above = (uint)pass.GetField("AboveGlowLayer")!.GetValue(null)!;
        uint marks = (uint)pass.GetField("MarkLayer")!.GetValue(null)!;
        uint world = (uint)pass.GetField("WorldLayers")!.GetValue(null)!;

        Assert.Equal(1, System.Numerics.BitOperations.PopCount(marks));
        Assert.Equal(0u, marks & above);
        Assert.Equal(0u, world & marks);
        Assert.Equal(0u, marks & 1u);
    }

    [Fact]
    public void TheHaloFallsOnASlowCurveAndMeetsItsEdgeWithNoRing()
    {
        // D-1092 and D-1095: the middle holds the full light, and a third of it stays at half the
        // radius. The curve of D-1092 fell to 1% at the edge and stopped there, and the sRGB curve of
        // the screen showed that stop as a faint ring. The curve now reaches 1% at about 82% of the
        // radius, and it meets 0 at the edge with no slope.
        float power = (float)GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass").GetField("HaloPower")!.GetValue(null)!;

        Assert.Equal(1f, HaloShareAt(0f, power));
        Assert.InRange(HaloShareAt(0.5f, power), 0.31f, 0.33f);
        Assert.InRange(HaloShareAt(0.82f, power), 0.011f, 0.012f);
        Assert.Equal(0f, HaloShareAt(1f, power));
        Assert.Equal(0f, HaloShareAt(1.001f, power));

        // The last hundredth of the radius holds less light than one part in a million, where the
        // curve of D-1092 held 1%.
        Assert.True(HaloShareAt(0.99f, power) < 0.000001f, $"the halo holds {HaloShareAt(0.99f, power)} of its middle next to its edge (D-1095)");

        float before = 1f;
        for (int step = 1; step <= 100; step += 1)
        {
            float share = HaloShareAt(step / 100f, power);
            Assert.True(share < before, $"the halo rose at the distance {step / 100f} (D-1092)");
            Assert.True(before - share < 0.05f, $"the halo stepped down by {before - share} at the distance {step / 100f} (D-1092)");
            before = share;
        }
    }

    [Fact]
    public void AHaloCurveOfPowerOneOrLessIsAnError()
    {
        // A power of 1 or less meets the edge with a slope, and the ring of D-1095 comes back (T-2).
        System.Reflection.TargetInvocationException error = Assert.Throws<System.Reflection.TargetInvocationException>(() => HaloShareAt(0.5f, 1f));

        Assert.IsType<ArgumentOutOfRangeException>(error.InnerException);
    }

    private static float HaloShareAt(float distance, float power)
    {
        MethodInfo method = GameAssemblyFile.Type("TheThingBelow.Game.Ui.WorldLights").GetMethod("HaloShareAt")!;
        return (float)method.Invoke(null, [distance, power])!;
    }

    private static float PulseOf(Glow glow, string id, long tick)
    {
        MethodInfo method = GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass").GetMethod("PulseOf")!;
        return (float)method.Invoke(null, [glow, id, tick])!;
    }
}
