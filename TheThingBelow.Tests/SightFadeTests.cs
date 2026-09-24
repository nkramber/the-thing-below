using System;
using System.Reflection;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The fade of each enemy at the edge of the sight of the party on a dark map (D-1062), through
/// the built Game assembly. The type holds no Godot value, so the test needs no engine (D-614).
/// </summary>
public sealed class SightFadeTests
{
    private const string TypeName = "TheThingBelow.Game.Ui.SightFade";
    private const int Tile = 32;
    private const int Full = 1000;

    [Theory]
    [InlineData(0, Full)]
    [InlineData(2 * Tile, Full)]
    [InlineData((2 * Tile) + 16, 500)]
    [InlineData(3 * Tile, 0)]
    [InlineData(9 * Tile, 0)]
    public void TheDistanceFadesAnEnemyAcrossOneTilePastTheRange(int distance, int share)
    {
        Assert.Equal(share, Call<int>("DistanceShare", distance, 2 * Tile));
    }

    [Fact]
    public void TheReachOfABodyIsTheLargerGapToItsNearestPixel()
    {
        // D-716: the reach counts a diagonal as one, as the reach of Core does.
        Assert.Equal(3 * Tile, Call<int>("Reach", 0, 0, 3 * Tile, Tile, 1));

        // An elite holds two by two tiles, so its far column lies one tile nearer to a lead east of it (D-206).
        Assert.Equal(2 * Tile, Call<int>("Reach", 3 * Tile, 0, 0, 0, 2));
        Assert.Equal(0, Call<int>("Reach", Tile, Tile, 0, 0, 2));
    }

    [Fact]
    public void AnEnemyFadesOverAQuarterSecondWhenAWallCloses()
    {
        Fade fade = new([true], 6 * Tile);
        Assert.Equal(Full, fade.ShareOf(0, true, 0, 100));

        Assert.Equal(Full, fade.ShareOf(0, false, 0, 200));
        int middle = fade.ShareOf(0, false, 0, 207);
        Assert.InRange(middle, 1, Full - 1);
        Assert.Equal(0, fade.ShareOf(0, false, 0, 215));
    }

    [Fact]
    public void AWallThatOpensInsideAFadeStartsFromTheShareOfThatTick()
    {
        // No enemy jumps, so a change inside a fade starts where the last fade stood (D-1062).
        Fade fade = new([true], 6 * Tile);
        _ = fade.ShareOf(0, false, 0, 200);
        int closing = fade.ShareOf(0, false, 0, 205);

        Assert.Equal(closing, fade.ShareOf(0, true, 0, 205));
        Assert.True(fade.ShareOf(0, true, 0, 206) > closing);
    }

    [Fact]
    public void TheDarkClosesInOverOneSecondWhenTheTorchGoesAway()
    {
        // D-1063: the range goes from 6 tiles to 2, and an enemy 4 tiles away fades out over the
        // range fade, and never in one tick.
        Fade fade = new([true], 6 * Tile);
        Assert.Equal(Full, fade.ShareOf(0, true, 4 * Tile, 300));

        fade.MoveRange(2 * Tile, 300);
        Assert.Equal(Full, fade.ShareOf(0, true, 4 * Tile, 300));
        Assert.InRange(fade.ShareOf(0, true, 4 * Tile, 340), 1, Full - 1);
        Assert.Equal(0, fade.ShareOf(0, true, 4 * Tile, 360));
    }

    [Fact]
    public void NoShareJumpsInOneTickOverOneThousandSeeds()
    {
        // Exit test 9 of section 7.35 at the level of the rule of the fade: each tick moves the
        // share of an enemy by a small step alone, whatever the walls and the torch do (D-1062).
        const int Step = 150;
        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            Pcg32 generator = Pcg32.FromSeed(seed, 91);
            Fade fade = new([true], 6 * Tile);
            int distance = (int)(generator.Next() % (8 * Tile));
            int last = fade.ShareOf(0, true, distance, 0);
            bool clear = true;
            for (long tick = 1; tick < 200; tick += 1)
            {
                if (generator.Next() % 20 == 0)
                {
                    clear = !clear;
                }

                if (generator.Next() % 40 == 0)
                {
                    fade.MoveRange(generator.Next() % 2 == 0 ? 2 * Tile : 6 * Tile, tick);
                }

                int share = fade.ShareOf(0, clear, distance, tick);
                Assert.True(
                    Math.Abs(share - last) <= Step,
                    $"The seed {seed} moved the share from {last} to {share} at the tick {tick}, which reads as a pop (D-1062).");
                last = share;
            }
        }
    }

    private static T Call<T>(string name, params object[] arguments) =>
        (T)(GameAssemblyFile.Type(TypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)!.Invoke(null, arguments)
            ?? throw new InvalidOperationException($"The fade method '{name}' gave nothing (T-2)."));

    /// <summary>One fade of the Game assembly, through its public members.</summary>
    private sealed class Fade
    {
        private readonly object instance;

        public Fade(bool[] clear, int rangePixels)
        {
            this.instance = Activator.CreateInstance(GameAssemblyFile.Type(TypeName), [clear, rangePixels])
                ?? throw new InvalidOperationException("The fade gave no instance (T-2).");
        }

        public int ShareOf(int index, bool clear, int distance, long tick) =>
            (int)this.instance.GetType().GetMethod("ShareOf")!.Invoke(this.instance, [index, clear, distance, tick])!;

        public void MoveRange(int rangePixels, long tick) =>
            this.instance.GetType().GetMethod("MoveRange")!.Invoke(this.instance, [rangePixels, tick]);
    }
}
