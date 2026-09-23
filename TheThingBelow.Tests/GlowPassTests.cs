using System;
using System.Reflection;
using System.Text;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The pulse of each glow, and the glow layer of the pass (D-913).</summary>
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
    public void TheWorldNeverDrawsTheGlowLayerAndTheMaskDrawsItAlone()
    {
        // D-188, F-47: no sprite or tile draws on the glow layer, and the world view never draws it.
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass");
        uint glow = (uint)pass.GetField("GlowLayer")!.GetValue(null)!;
        uint world = (uint)pass.GetField("WorldLayers")!.GetValue(null)!;

        Assert.Equal(1, System.Numerics.BitOperations.PopCount(glow));
        Assert.Equal(0u, world & glow);
        Assert.NotEqual(0u, world & 1u);
    }

    private static float PulseOf(Glow glow, string id, long tick)
    {
        MethodInfo method = GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass").GetMethod("PulseOf")!;
        return (float)method.Invoke(null, [glow, id, tick])!;
    }
}
