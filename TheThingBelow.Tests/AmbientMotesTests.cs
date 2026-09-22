using System;
using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Core.Effects;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The motion of each mote of a weather: the fall, the sway, and the rest (D-893, F-100).</summary>
public sealed class AmbientMotesTests
{
    /// <summary>A stream of one mote: it falls 24 pixels over 120 ticks, and it lies still for the 300 ticks after.</summary>
    private static readonly MoteStream Dust = new(
        Amount: 1,
        LifetimeTicks: 420,
        Colors: ['l'],
        Size: 1,
        FallPixels: 24,
        FallTicks: 120,
        DriftPixels: 0,
        SwayPixels: 4,
        SwayTicks: 60);

    [Fact]
    public void AMoteFallsOverTheTicksOfItsFall()
    {
        // The owner asked for a slow fall to the ground: 24 pixels over 2 seconds.
        int first = Dust.FallPixels * 0 / Dust.FallTicks;
        int last = Mote(120).Y - Mote(0).Y;

        Assert.Equal(0, first);
        Assert.Equal(Dust.FallPixels, last);
    }

    [Fact]
    public void AMoteNeverRisesWhileItFalls()
    {
        // F-100: the particles of Godot pushed a mote up, and the owner saw it. A mote of the
        // drawn weather moves down, or it holds its place, on every tick of its life.
        int before = Mote(0).Y;
        for (int tick = 1; tick <= Dust.LifetimeTicks - 1; tick += 1)
        {
            int now = Mote(tick).Y;
            Assert.True(now >= before, $"the mote rose from {before} to {now} on tick {tick}");
            before = now;
        }
    }

    [Fact]
    public void AMoteLiesStillFromTheEndOfItsFallToTheEndOfItsLife()
    {
        // The owner asked for 5 seconds on the ground. This stream rests 300 ticks (D-893).
        Mote landed = Mote(Dust.FallTicks);
        for (int tick = Dust.FallTicks; tick < Dust.LifetimeTicks; tick += 1)
        {
            Assert.Equal(landed.X, Mote(tick).X);
            Assert.Equal(landed.Y, Mote(tick).Y);
        }
    }

    [Fact]
    public void TheSwayPullsAMoteToEachSideWhileItFalls()
    {
        // The owner asked for the curve of a sheet of paper: the mote goes to one side, then to
        // the other, while it falls.
        var sway = new SortedSet<int>();
        for (int tick = 0; tick <= Dust.FallTicks; tick += 1)
        {
            sway.Add(AmbientMotes.SwayAt(Dust, phase: 0, ticks: tick));
        }

        Assert.True(sway.Min < 0, "the sway never pulled the mote west");
        Assert.True(sway.Max > 0, "the sway never pulled the mote east");
        Assert.InRange(sway.Min, -Dust.SwayPixels, 0);
        Assert.InRange(sway.Max, 0, Dust.SwayPixels);
    }

    [Fact]
    public void AMoteStartsAgainAtAnotherPlaceAfterItsLife()
    {
        // A mote goes out at the end of its life, and the next round starts it somewhere else.
        Mote first = Mote(0);
        Mote next = Mote(Dust.LifetimeTicks);

        Assert.NotEqual((first.X, first.Y), (next.X, next.Y));
    }

    [Fact]
    public void OneTickGivesOnePicture()
    {
        // T-7, D-172: each capture of a tick shows the same motes.
        for (int tick = 0; tick < 200; tick += 7)
        {
            Assert.Equal(Mote(tick), Mote(tick));
            Assert.Equal(
                AmbientMotes.InView(Dust, 0, 0, tick),
                AmbientMotes.InView(Dust, 0, 0, tick));
        }
    }

    [Fact]
    public void AMoteKeepsItsPlaceInTheWorldWhileTheViewMoves()
    {
        // F-97: the weather rode the view, and the owner saw it. A mote of the drawn weather
        // holds one place of the world, and the view moves past it.
        IReadOnlyList<Mote> first = AmbientMotes.InView(Dust, 0, 0, 40);
        IReadOnlyList<Mote> moved = AmbientMotes.InView(Dust, 64, 32, 40);

        Assert.NotEmpty(first);
        foreach (Mote mote in first)
        {
            bool inside = mote.X >= 64 && mote.Y >= 32;
            Assert.Equal(inside, moved.Contains(mote));
        }
    }

    [Fact]
    public void AViewHoldsTheMotesOfEachCellThatItCovers()
    {
        // Each cell of the world holds the motes of the stream, so the view never runs out of
        // them, however far the party walks (D-893).
        var many = Dust with { Amount = 40 };

        IReadOnlyList<Mote> corner = AmbientMotes.InView(many, 0, 0, 90);
        IReadOnlyList<Mote> across = AmbientMotes.InView(many, 320, 180, 90);
        IReadOnlyList<Mote> far = AmbientMotes.InView(many, 4096, 2048, 90);

        Assert.NotEmpty(corner);
        Assert.NotEmpty(across);
        Assert.NotEmpty(far);
        Assert.NotEqual(corner, far);
    }

    [Fact]
    public void AStreamWithNoSwayFallsStraight()
    {
        var straight = Dust with { SwayPixels = 0 };

        Assert.Equal(0, AmbientMotes.SwayAt(straight, phase: 7, ticks: 30));
        Assert.Equal(
            AmbientMotes.MoteAt(straight, 0, 0, 0, 0).X,
            AmbientMotes.MoteAt(straight, 0, 0, 0, 60).X);
    }

    [Fact]
    public void AMoteOutsideTheStreamFails()
    {
        // T-2: the caller names a mote of the stream, and never another one.
        Assert.Throws<ArgumentOutOfRangeException>(() => AmbientMotes.MoteAt(Dust, 0, 0, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AmbientMotes.MoteAt(Dust, 0, 0, 0, -1));
    }

    private static Mote Mote(long tick) => AmbientMotes.MoteAt(Dust, 0, 0, 0, tick);
}
