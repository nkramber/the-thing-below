using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The timings and the motions of the battle screen, in ticks and art pixels (D-266, D-829,
/// D-832). The tests read the built Game assembly, because Tests takes no reference to Game
/// (D-614).
/// </summary>
public sealed class BattleTimesTests
{
    private const string TimesTypeName = "TheThingBelow.Game.Ui.BattleTimes";

    [Fact]
    public void EveryKindOfEventHasATiming()
    {
        // T-2: a new kind of event without a timing fails here and never on screen.
        foreach (BattleEventKind kind in Enum.GetValues<BattleEventKind>())
        {
            int ticks = (int)Method("TicksOf").Invoke(null, [kind])!;
            Assert.True(ticks >= 0, $"The event '{kind}' plays {ticks} ticks.");
        }
    }

    [Fact]
    public void OnlyATurnTakesNoTicks()
    {
        // A turn changes only who acts, so the next event starts at once. Every other event
        // stands on screen long enough to read (D-213).
        foreach (BattleEventKind kind in Enum.GetValues<BattleEventKind>())
        {
            int ticks = (int)Method("TicksOf").Invoke(null, [kind])!;
            Assert.Equal(kind == BattleEventKind.Turn, ticks == 0);
        }
    }

    [Fact]
    public void TheFlashAndTheNumberStartAtTheBlowAndEndInsideTheStrike()
    {
        // D-96, D-213: a hit flashes from the blow, and a miss never flashes.
        int strike = Const("StrikeTicks");
        int blow = Const("BlowTick");
        var flashed = new List<int>();
        for (int tick = 0; tick < strike + 10; tick += 1)
        {
            if ((bool)Method("Flashes").Invoke(null, [BattleEventKind.Hit, tick])!)
            {
                flashed.Add(tick);
            }

            Assert.False((bool)Method("Flashes").Invoke(null, [BattleEventKind.Miss, tick])!);
            object? rise = Method("NumberRise").Invoke(null, [tick]);
            Assert.Equal(tick < blow, rise is null);
            if (rise is int height && tick < strike)
            {
                Assert.True(height >= 0, $"At tick {tick} the number stands {height} pixels below its start.");
            }
        }

        Assert.Equal(blow, flashed[0]);
        Assert.True(flashed[^1] < strike, "The flash runs past the end of the strike.");
    }

    [Fact]
    public void TheSwayOfTheBackdropStepsOnePixelAtATimeInsideItsRange()
    {
        // D-205, D-831: the backdrop sways by whole art pixels, and never jumps.
        int drift = Const("DriftPixels");
        int before = (int)Method("DriftAt").Invoke(null, [0L])!;
        Assert.Equal(0, before);
        for (long tick = 1; tick < 2000; tick += 1)
        {
            int now = (int)Method("DriftAt").Invoke(null, [tick])!;
            Assert.InRange(now, -drift, drift);
            Assert.InRange(now - before, -1, 1);
            before = now;
        }
    }

    [Fact]
    public void TheSwayReachesBothSides()
    {
        int drift = Const("DriftPixels");
        var seen = new HashSet<int>();
        for (long tick = 0; tick < 2000; tick += 1)
        {
            seen.Add((int)Method("DriftAt").Invoke(null, [tick])!);
        }

        Assert.Contains(drift, seen);
        Assert.Contains(-drift, seen);
    }

    private static int Const(string name) =>
        (int)(GameAssemblyFile.Type(TimesTypeName).GetField(name)?.GetValue(null)
            ?? throw new InvalidOperationException($"The battle times hold no constant '{name}' (T-2)."));

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(TimesTypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"The battle times hold no '{name}' method (T-2).");
}
