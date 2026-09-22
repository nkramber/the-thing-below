using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The seed of each particle node of a stream (D-187, D-890).</summary>
/// <remarks>
/// The test reads the built Game assembly, because Tests takes no reference to Game (D-614).
/// It reads the seed alone, which needs no engine.
/// </remarks>
public sealed class ParticleStreamsTests
{
    private const string TypeName = "TheThingBelow.Game.Ui.ParticleStreams";

    [Fact]
    public void TwoNodesOfOneStreamTakeTwoSeeds()
    {
        // Regression: each node kept the seed 0 of Godot under a fixed seed, so the nodes of the
        // colors of one stream held the same particles, and they stacked on each other (D-181).
        uint first = SeedOf("effect.dust_fixture_dungeon_0_0");
        uint second = SeedOf("effect.dust_fixture_dungeon_0_1");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void TwoTorchesOfOneKindTakeTwoSeeds()
    {
        // Regression: each wall torch held the same flame, pixel for pixel (D-890).
        uint west = SeedOf("piece.fixture_dungeon_pit_west_0_0");
        uint east = SeedOf("piece.fixture_dungeon_pit_east_0_0");

        Assert.NotEqual(west, east);
    }

    [Fact]
    public void OneNameGivesOneSeedAndNeverZero()
    {
        // T-7: the seed comes from the name alone, so each run and each platform gives one
        // picture. Godot reads the seed 0 as no seed.
        var seeds = new SortedSet<uint>();
        foreach (string name in new[] { "a_0_0", "b_0_0", "c_0_0", "d_0_0", "e_0_0" })
        {
            uint seed = SeedOf(name);
            Assert.Equal(seed, SeedOf(name));
            Assert.NotEqual(0u, seed);
            seeds.Add(seed);
        }

        Assert.Equal(5, seeds.Count);
    }

    private static uint SeedOf(string name)
    {
        MethodInfo method = GameAssemblyFile.Type(TypeName)
            .GetMethod("SeedOf", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"The type '{TypeName}' holds no method 'SeedOf' (T-2).");

        return (uint)method.Invoke(null, [name])!;
    }
}
