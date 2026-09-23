using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The fire of a torch: its streams, and the steps of its light (D-888, D-890, D-891).</summary>
public sealed class TorchFireTests
{
    [Fact]
    public void ADecorKindReadsItsFireAndItsStreams()
    {
        DecorKind kind = DecorKind.Read(Encoding.UTF8.GetBytes(LightFixtures.KindBody), LightFixtures.KindPath);

        Assert.Equal(4, kind.Fire.StepTicks);
        Assert.Equal(2, kind.Fire.Levels.Count);
        Assert.Equal(1, kind.Fire.Jump);
        Assert.Equal(6, StreamEmitter.ParticlesOf(kind.Fire.Emitters));
    }

    [Fact]
    public void OneTickGivesOneStepOfOneTorch()
    {
        // T-7, D-172: each capture of a tick shows the same light and the same flame.
        TorchFire fire = Fire();

        FlickerStep first = fire.StepAt("piece.one", 120);
        FlickerStep again = fire.StepAt("piece.one", 120);

        Assert.Equal(first, again);
    }

    [Fact]
    public void TwoTorchesChangeOutOfStep()
    {
        // D-891: a row of torches never pulses together.
        TorchFire fire = Fire();
        int apart = 0;
        for (int tick = 0; tick < 600; tick += 1)
        {
            if (fire.StepAt("piece.one", tick) != fire.StepAt("piece.two", tick))
            {
                apart += 1;
            }
        }

        Assert.True(apart > 300, $"the two torches held the same step on {600 - apart} ticks of 600");
    }

    [Fact]
    public void EachTickOfOneStepHoldsTheSameLevel()
    {
        // D-891: the step of the file holds the light still for its ticks.
        TorchFire fire = Fire();
        FlickerStep start = fire.StepAt("piece.one", 40);

        for (int tick = 41; tick < 40 + fire.StepTicks; tick += 1)
        {
            Assert.Equal(start, fire.StepAt("piece.one", tick));
        }

        // The next step reads a new hash, and two steps of the same value can follow each other.
        bool changed = false;
        for (int step = 1; step < 20 && !changed; step += 1)
        {
            changed = fire.StepAt("piece.one", 40 + (step * fire.StepTicks)) != start;
        }

        Assert.True(changed, "the light of the torch held one step for 20 steps of the file (D-891)");
    }

    [Fact]
    public void EachStepStaysInsideTheLevelsAndTheJumpOfTheFile()
    {
        // T-2: no step gives a value that the file does not hold.
        TorchFire fire = Fire();
        var seen = new SortedSet<int>();
        for (int tick = 0; tick < 2000; tick += 1)
        {
            FlickerStep step = fire.StepAt("piece.one", tick);
            Assert.Contains(step.Level, fire.Levels);
            Assert.InRange(step.JumpX, -fire.Jump, fire.Jump);
            Assert.InRange(step.JumpY, -fire.Jump, fire.Jump);
            seen.Add(step.Level.Strength);
        }

        Assert.Equal(fire.Levels.Count, seen.Count);
    }

    [Fact]
    public void TheFlameOfATorchJumpsOnSomeStep()
    {
        // D-891, F-99: the flame jumps, and the light of the torch never does. A fire with a
        // jump of 0 pixels on every tick would hold no motion at all.
        TorchFire fire = Fire();
        var jumps = new SortedSet<(int X, int Y)>();
        for (int tick = 0; tick < 600; tick += 1)
        {
            FlickerStep step = fire.StepAt("piece.one", tick);
            jumps.Add((step.JumpX, step.JumpY));
        }

        Assert.Contains((0, 0), jumps);
        Assert.True(jumps.Count > 1, "the flame of the torch never jumped from its place (D-891)");
    }

    [Theory]
    [InlineData("\"step_ticks\": 4", "\"step_ticks\": 61", "1 to 60")]
    [InlineData("\"jump\": 1", "\"jump\": 3", "0 to 2")]
    [InlineData("\"strength\": 10000", "\"strength\": 4000", "5000 to 12000")]
    public void AFireValueOutsideItsLimitFailsWithTheReason(string from, string to, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule.
        string body = LightFixtures.KindBody.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(
            () => DecorKind.Read(Encoding.UTF8.GetBytes(body), LightFixtures.KindPath));

        Assert.Equal(LightFixtures.KindPath, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADecorKindWithNoFireFails()
    {
        // T-2: an absent value is an error, and each torch of D-890 holds a fire.
        string body = """{ "comment": "a test torch", "id": "decor.torch", "light": { "color": "j", "strength": 12000, "range": 96, "height": 24, "x": 16, "y": 36 } }""";

        ContentException error = Assert.Throws<ContentException>(
            () => DecorKind.Read(Encoding.UTF8.GetBytes(body), LightFixtures.KindPath));

        Assert.Equal("fire", error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutGivesTheWallTorchAndTheCarriedLightTheirFire()
    {
        // D-890: each wall torch and the carried light show a flame, embers, and a light that changes.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        ContentId torch = ContentId.Parse("decor.wall_torch", "test", "id");

        TorchFire wall = set.Light.KindOf(torch).Fire;
        TorchFire carried = set.Light.Carried.Fire;

        Assert.Equal(3, wall.Emitters.Count);
        Assert.Equal(2, carried.Emitters.Count);
        Assert.True(wall.Levels.Count > 1, "a wall torch steps between two levels or more (D-891)");
        Assert.True(carried.Levels.Count > 1, "the carried light steps between two levels or more (D-891)");
    }

    [Fact]
    public void AFireReadsItsGlow()
    {
        // D-912, D-913: the fixture fire holds a glow of strength 0, so it never glows.
        GlowSeed glow = Fire().Glow;

        Assert.Equal(new GlowSeed('k', 0, 1, 1, 0, 0), glow);
    }

    [Theory]
    [InlineData("\"strength\": 0, \"width\"", "\"strength\": 10001, \"width\"", "0 to 10000")]
    [InlineData("\"width\": 1", "\"width\": 0", "1 to 16")]
    [InlineData("\"height\": 1", "\"height\": 17", "1 to 16")]
    [InlineData("\"height\": 1, \"x\": 0", "\"height\": 1, \"x\": 65", "-64 to 64")]
    [InlineData("\"glow\": { \"color\": \"k\"", "\"glow\": { \"color\": \"kk\"", "one palette key")]
    public void AGlowValueOutsideItsLimitFailsWithTheReason(string from, string to, string reason)
    {
        // T-2: each bad value names the file, the field, and the rule.
        string body = LightFixtures.KindBody.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(LightFixtures.KindBody, body);

        ContentException error = Assert.Throws<ContentException>(
            () => DecorKind.Read(Encoding.UTF8.GetBytes(body), LightFixtures.KindPath));

        Assert.Equal(LightFixtures.KindPath, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFireWithNoGlowFails()
    {
        // T-2: an absent value is an error, and a fire that never glows says so with a strength of 0.
        string body = LightFixtures.KindBody.Replace(", \"glow\": { \"color\": \"k\", \"strength\": 0, \"width\": 1, \"height\": 1, \"x\": 0, \"y\": 0 }", string.Empty, StringComparison.Ordinal);
        Assert.NotEqual(LightFixtures.KindBody, body);

        ContentException error = Assert.Throws<ContentException>(
            () => DecorKind.Read(Encoding.UTF8.GetBytes(body), LightFixtures.KindPath));

        Assert.Equal("fire.glow", error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWallTorchGlowsAndTheCarriedTorchDoesNot()
    {
        // D-912: the fire of each wall torch glows. The flame of the carried torch draws over the
        // lead, so its glow would read as a sprite that glows (D-188), and it keeps a strength of 0.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        TorchFire wall = set.Light.KindOf(ContentId.Parse("decor.wall_torch", "test", "id")).Fire;

        Assert.True(wall.Glow.Strength > 0, "the wall torch holds no glow (D-912)");
        Assert.Equal(0, set.Light.Carried.Fire.Glow.Strength);
    }

    private static TorchFire Fire() => DecorKind
        .Read(Encoding.UTF8.GetBytes(LightFixtures.KindBody), LightFixtures.KindPath)
        .Fire;
}
