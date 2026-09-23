using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The moments and the motions of the battle screen, in ticks and art pixels, from the battle
/// file (D-266, D-829, D-832, D-883). The tests read the built Game assembly, because Tests
/// takes no reference to Game (D-614).
/// </summary>
public sealed class BattleTimesTests
{
    private const string TimesTypeName = "TheThingBelow.Game.Ui.BattleTimes";

    private static readonly Lazy<BattleEffects> Pace =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())).Effects.Battle);

    [Fact]
    public void TheBattleFileKeepsThePaceOfPr10()
    {
        // D-829, D-873: the move into the battle file changes no timing of PR-10.
        BattleEffects pace = Pace.Value;
        Assert.Equal(40, pace.StartTicks);
        Assert.Equal(44, pace.StrikeTicks);
        Assert.Equal(32, pace.LineTicks);
        Assert.Equal(60, pace.EndTicks);
        Assert.Equal(16, pace.PoseTicks);
        Assert.Equal(6, pace.BlowTick);
        Assert.Equal(8, pace.FlashTicks);
        Assert.Equal(4, pace.LungePixels);
        Assert.Equal(2, pace.DriftPixels);
        Assert.Equal(45, pace.DriftStepTicks);
    }

    [Fact]
    public void EveryKindOfEventHasATiming()
    {
        // T-2: a new kind of event without a timing fails here and never on screen.
        foreach (BattleEventKind kind in Enum.GetValues<BattleEventKind>())
        {
            int ticks = TicksOf(kind);
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
            Assert.Equal(kind == BattleEventKind.Turn, TicksOf(kind) == 0);
        }
    }

    [Fact]
    public void TheFlashAndTheNumberStartAtTheBlowAndEndInsideTheStrike()
    {
        // D-96, D-213: a hit flashes from the blow, and a miss never flashes.
        BattleEffects pace = Pace.Value;
        var flashed = new List<int>();
        for (int tick = 0; tick < pace.StrikeTicks + 10; tick += 1)
        {
            if (Flashes(BattleEventKind.Hit, tick))
            {
                flashed.Add(tick);
            }

            Assert.False(Flashes(BattleEventKind.Miss, tick));
            object? rise = Invoke("NumberRise", pace, tick);
            Assert.Equal(tick < pace.BlowTick, rise is null);
            if (rise is int height && tick < pace.StrikeTicks)
            {
                Assert.True(height >= 0, $"At tick {tick} the number stands {height} pixels below its start.");
            }
        }

        Assert.Equal(pace.BlowTick, flashed[0]);
        Assert.True(flashed[^1] < pace.StrikeTicks, "The flash runs past the end of the strike.");
    }

    [Fact]
    public void TheSwayOfTheBackdropStepsOnePixelAtATimeInsideItsRange()
    {
        // D-205, D-831: the backdrop sways by whole art pixels, and never jumps.
        int drift = Pace.Value.DriftPixels;
        int before = (int)Invoke("DriftAt", Pace.Value, 0L)!;
        Assert.Equal(0, before);
        for (long tick = 1; tick < 2000; tick += 1)
        {
            int now = (int)Invoke("DriftAt", Pace.Value, tick)!;
            Assert.InRange(now, -drift, drift);
            Assert.InRange(now - before, -1, 1);
            before = now;
        }
    }

    [Fact]
    public void TheSwayReachesBothSides()
    {
        int drift = Pace.Value.DriftPixels;
        var seen = new HashSet<int>();
        for (long tick = 0; tick < 2000; tick += 1)
        {
            seen.Add((int)Invoke("DriftAt", Pace.Value, tick)!);
        }

        Assert.Contains(drift, seen);
        Assert.Contains(-drift, seen);
    }

    [Fact]
    public void TheMessageSpeedScalesTheHoldOfEachEvent()
    {
        // D-873: normal keeps the pace of PR-10, slow is 1.5 times as long, and fast is half.
        foreach (BattleEventKind kind in Enum.GetValues<BattleEventKind>())
        {
            int normal = TicksOf(kind);

            Assert.Equal(normal * 3 / 2, HoldTicks(kind, MessageSpeed.Slow));
            Assert.Equal(normal, HoldTicks(kind, MessageSpeed.Normal));
            Assert.Equal(normal / 2, HoldTicks(kind, MessageSpeed.Fast));
        }

        Assert.Equal(48, HoldTicks(BattleEventKind.Defend, MessageSpeed.Slow));
        Assert.Equal(22, HoldTicks(BattleEventKind.Hit, MessageSpeed.Fast));
    }

    [Theory]
    [InlineData(BattleEventKind.Hit, Affinity.Weak, true)]
    [InlineData(BattleEventKind.Hit, Affinity.Normal, false)]
    [InlineData(BattleEventKind.Hit, Affinity.Resist, false)]
    [InlineData(BattleEventKind.Absorb, Affinity.Absorb, false)]
    [InlineData(BattleEventKind.Miss, Affinity.Weak, false)]
    public void AHeavyBlowIsAHitOnAWeakness(BattleEventKind kind, Affinity affinity, bool heavy)
    {
        // D-877: the weak affinity of a hit makes a heavy blow, and nothing else does.
        Assert.Equal(heavy, (bool)Invoke("IsHeavy", EventOf(kind, affinity))!);
    }

    [Fact]
    public void AHeavyBlowFreezesThePictureAtTheBlowForTheHitStop()
    {
        // D-880: the ticks of the picture stand at the blow for the hit-stop, and then run on
        // behind the ticks of the event by the length of the stop.
        BattleEffects pace = Pace.Value;
        BattleEvent heavy = EventOf(BattleEventKind.Hit, Affinity.Weak);
        int stopEnd = pace.BlowTick + pace.HitStopTicks;
        for (int tick = 0; tick < pace.StrikeTicks; tick += 1)
        {
            int expected = tick <= pace.BlowTick ? tick : tick < stopEnd ? pace.BlowTick : tick - pace.HitStopTicks;
            Assert.Equal(expected, PictureTicks(heavy, tick));
        }

        Assert.True(pace.HitStopTicks > 0, "The battle file holds no hit-stop.");
    }

    [Theory]
    [InlineData(BattleEventKind.Hit, Affinity.Normal)]
    [InlineData(BattleEventKind.Miss, Affinity.Normal)]
    [InlineData(BattleEventKind.Absorb, Affinity.Absorb)]
    [InlineData(BattleEventKind.Defend, Affinity.Normal)]
    public void AnEventThatIsNoHeavyBlowNeverFreezes(BattleEventKind kind, Affinity affinity)
    {
        // D-880: a heavy blow alone takes the hit-stop.
        for (int tick = 0; tick < Pace.Value.StrikeTicks; tick += 1)
        {
            Assert.Equal(tick, PictureTicks(EventOf(kind, affinity), tick));
        }
    }

    [Theory]
    [InlineData(EffectLevel.Full, 4)]
    [InlineData(EffectLevel.Reduced, 1)]
    [InlineData(EffectLevel.Off, 0)]
    public void TheShakeOfAHeavyBlowTakesTheDistanceOfItsLevel(EffectLevel level, int distance)
    {
        // D-863, D-876: the shake moves the picture to one side and the other by the distance of
        // the level, from the blow to the end of the shake. Reduced is a quarter, and off is none.
        BattleEffects pace = Pace.Value;
        BattleEvent heavy = EventOf(BattleEventKind.Hit, Affinity.Weak);
        var seen = new HashSet<int>();
        for (int tick = 0; tick < pace.StrikeTicks; tick += 1)
        {
            int offset = ShakeAt(heavy, tick, level);
            bool shakes = tick >= pace.BlowTick && tick < pace.BlowTick + pace.Shake.Ticks;
            Assert.Equal(shakes ? distance : 0, Math.Abs(offset));
            seen.Add(offset);
        }

        Assert.Contains(distance, seen);
        Assert.Contains(-distance, seen);
    }

    [Fact]
    public void AHitThatIsNoHeavyBlowNeverShakes()
    {
        // D-876, D-877: a plain hit keeps the picture still at each level.
        foreach (EffectLevel level in Enum.GetValues<EffectLevel>())
        {
            for (int tick = 0; tick < Pace.Value.StrikeTicks; tick += 1)
            {
                Assert.Equal(0, ShakeAt(EventOf(BattleEventKind.Hit, Affinity.Normal), tick, level));
            }
        }
    }

    [Fact]
    public void TheBurstOfAHitStartsAtTheBlowAndAMissOrAnAbsorbHasNone()
    {
        // D-879: each hit plays one burst from the blow. A miss hits nothing, and an absorb heals.
        BattleEffects pace = Pace.Value;
        for (int tick = 0; tick < pace.StrikeTicks; tick += 1)
        {
            object? age = Invoke("BurstAge", pace, EventOf(BattleEventKind.Hit, Affinity.Normal), tick);
            Assert.Equal(tick < pace.BlowTick ? null : tick - pace.BlowTick, (int?)age);
            Assert.Null(Invoke("BurstAge", pace, EventOf(BattleEventKind.Miss, Affinity.Normal), tick));
            Assert.Null(Invoke("BurstAge", pace, EventOf(BattleEventKind.Absorb, Affinity.Absorb), tick));
        }
    }

    [Fact]
    public void ALineOfTheSummarySlidesUpPastItsPlaceAndSettlesBack()
    {
        // D-975: the line shows nothing before its start, rises above its place by the bounce,
        // and then stands at its place until its event ends.
        SummaryValues summary = Pace.Value.Summary;
        Assert.Null(SummaryRise(summary, -1));
        Assert.Equal(0, SummaryRise(summary, 0));

        int highest = 0;
        int before = 0;
        bool fell = false;
        for (int tick = 1; tick < summary.RiseTicks; tick += 1)
        {
            int rise = SummaryRise(summary, tick) ?? throw new InvalidOperationException($"Tick {tick} shows no line.");
            fell |= rise < before;
            Assert.False(fell && rise > before, $"Tick {tick}: the line rose again after it fell.");
            highest = Math.Max(highest, rise);
            before = rise;
        }

        Assert.Equal(summary.RisePixels + summary.BouncePixels, highest);
        Assert.Equal(summary.RisePixels, SummaryRise(summary, summary.RiseTicks));
        Assert.Equal(summary.RisePixels, SummaryRise(summary, summary.LevelUpTicks));
    }

    [Fact]
    public void ABarFillsFromItsValueBeforeTheLevelUpToFullAndNeverFalls()
    {
        // D-975: the health bar and the MP bar fill toward full over the fill ticks.
        SummaryValues summary = Pace.Value.Summary;
        Assert.Equal(12, FillAt(summary, 0, 12, 66));
        Assert.Equal(66, FillAt(summary, summary.FillTicks, 12, 66));
        Assert.Equal(66, FillAt(summary, summary.LevelUpTicks, 12, 66));
        int before = 12;
        for (int tick = 0; tick <= summary.FillTicks; tick += 1)
        {
            int value = FillAt(summary, tick, 12, 66);
            Assert.InRange(value, before, 66);
            before = value;
        }
    }

    [Fact]
    public void TheSummaryEndsInsideItsHoldAtTheFastMessageSpeed()
    {
        // D-873, D-975: six lines start one after another, and each one settles before the fast
        // hold of a level-up ends. The fill ends inside it too.
        SummaryValues summary = Pace.Value.Summary;
        int fastLevelUp = HoldTicks(BattleEventKind.LevelUp, MessageSpeed.Fast);
        Assert.True((summary.LineTicks * (BattleEffects.SummaryLines - 1)) + summary.RiseTicks <= fastLevelUp);
        Assert.True(summary.FillTicks <= fastLevelUp);
        Assert.True(summary.RiseTicks <= HoldTicks(BattleEventKind.Experience, MessageSpeed.Fast));
    }

    private static int? SummaryRise(SummaryValues summary, int ticks) => (int?)Invoke("SummaryRise", summary, ticks);

    private static int FillAt(SummaryValues summary, int ticks, int before, int full) => (int)Invoke("FillAt", summary, ticks, before, full)!;

    private static BattleEvent EventOf(BattleEventKind kind, Affinity affinity) =>
        new(kind, new BattleTarget(BattleSide.Party, 0), new BattleTarget(BattleSide.Enemy, 0), 9, null, affinity);

    private static int TicksOf(BattleEventKind kind) => (int)Invoke("TicksOf", Pace.Value, kind)!;

    private static int HoldTicks(BattleEventKind kind, MessageSpeed speed) =>
        (int)Invoke("HoldTicksOf", Pace.Value, kind, speed)!;

    private static bool Flashes(BattleEventKind kind, int ticks) => (bool)Invoke("Flashes", Pace.Value, kind, ticks)!;

    private static int PictureTicks(BattleEvent played, int ticks) => (int)Invoke("PictureTicks", Pace.Value, played, ticks)!;

    private static int ShakeAt(BattleEvent played, int ticks, EffectLevel level) =>
        (int)Invoke("ShakeAt", Pace.Value, played, ticks, level)!;

    private static object? Invoke(string name, params object[] arguments)
    {
        MethodInfo method = GameAssemblyFile.Type(TimesTypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"The battle times hold no '{name}' method (T-2).");
        return method.Invoke(null, arguments);
    }
}
