using System;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The fixed-step clock of Game, which calls Core 60 times a second (D-164, G-3). The tests
/// read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class FixedStepLoopTests
{
    /// <summary>The name of the type of Game that holds the clock.</summary>
    private const string LoopTypeName = "TheThingBelow.Game.FixedStepLoop";

    /// <summary>The time of one tick, in seconds.</summary>
    private const double OneTick = 1.0 / 60;

    [Fact]
    public void TheLoopRunsSixtyTicksInOneSecond()
    {
        // D-164. The clock lives in Game, and Core reads no clock (G-3).
        Loop loop = new();
        int ticks = 0;

        for (int frame = 0; frame < 60; frame += 1)
        {
            ticks += loop.Advance(OneTick);
        }

        Assert.Equal(60, ticks);
        Assert.Equal(0, loop.DroppedTicks);
    }

    [Fact]
    public void AFrameShorterThanOneTickRunsNoTick()
    {
        Loop loop = new();

        Assert.Equal(0, loop.Advance(OneTick / 3));
        Assert.Equal(0, loop.Advance(OneTick / 3));
    }

    [Fact]
    public void ThePartOfAFrameBelowOneTickCarriesToTheNextFrame()
    {
        Loop loop = new();

        loop.Advance(OneTick / 3);
        loop.Advance(OneTick / 3);

        Assert.Equal(1, loop.Advance(OneTick / 3 + OneTick / 100));
    }

    [Fact]
    public void OneFrameOfFiveTicksRunsFiveTicks()
    {
        Loop loop = new();

        Assert.Equal(5, loop.Advance(OneTick * 5));
    }

    [Fact]
    public void ALongFrameRunsTheMaximumAndDropsTheRest()
    {
        // A loop that runs every late tick falls further behind on each frame, and it never
        // catches up. The loop drops the rest of the time and counts it (T-2).
        Loop loop = new();

        int ticks = loop.Advance(2.0);

        Assert.Equal(loop.MaxTicksInOneFrame, ticks);
        Assert.Equal(120 - loop.MaxTicksInOneFrame, loop.DroppedTicks);
        Assert.Equal(0, loop.Advance(0));
    }

    [Fact]
    public void ATimeBelowZeroIsAnError()
    {
        Loop loop = new();

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => loop.Advance(-1.0));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ATimeThatIsNotFiniteIsAnError(double seconds)
    {
        // A NaN passes a negative check, and the loop would then never run a tick again (T-2).
        Loop loop = new();

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => loop.Advance(seconds));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    [Fact]
    public void TheLoopRunsSixtyTicksASecond()
    {
        Assert.Equal(60, ReadConstant("TicksPerSecond"));
    }

    private static int ReadConstant(string name)
    {
        FieldInfo field = GameAssemblyFile.Type(LoopTypeName).GetField(name)
            ?? throw new InvalidOperationException($"The loop holds no constant '{name}' (T-2).");

        return (int)(field.GetRawConstantValue()
            ?? throw new InvalidOperationException($"The constant '{name}' has no value (T-2)."));
    }

    /// <summary>The fixed-step loop of the built Game assembly, through its public members.</summary>
    [Fact]
    public void ThePartOfATickCountsTheTimeLeftAfterTheWholeTicks()
    {
        // D-820. Game draws each slide at this part, so a screen of 144 Hz moves a sprite on
        // each frame and not on two frames of every five.
        Loop loop = new();

        Assert.Equal(0, loop.Advance(OneTick / 4));
        Assert.InRange(loop.TickPart, 249, 250);

        Assert.Equal(1, loop.Advance(OneTick));
        Assert.InRange(loop.TickPart, 249, 250);
    }

    [Fact]
    public void ThePartOfATickStartsAtZero()
    {
        Assert.Equal(0, new Loop().TickPart);
    }

    private sealed class Loop
    {
        private readonly object instance;
        private readonly MethodInfo advance;
        private readonly PropertyInfo dropped;

        public Loop()
        {
            Type type = GameAssemblyFile.Type(LoopTypeName);
            this.instance = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"The type '{LoopTypeName}' made no value (T-2).");
            this.advance = type.GetMethod("Advance", [typeof(double)])
                ?? throw new InvalidOperationException("The loop holds no 'Advance' method (T-2).");
            this.dropped = type.GetProperty("DroppedTicks")
                ?? throw new InvalidOperationException("The loop holds no 'DroppedTicks' value (T-2).");
        }

        public int MaxTicksInOneFrame => ReadConstant("MaxTicksInOneFrame");

        public int TickPart => (int)(this.instance.GetType().GetProperty("TickPart")!.GetValue(this.instance)
            ?? throw new InvalidOperationException("The part of a tick has no value (T-2)."));

        public long DroppedTicks => (long)(this.dropped.GetValue(this.instance)
            ?? throw new InvalidOperationException("The dropped count has no value (T-2)."));

        public int Advance(double seconds) => (int)(this.advance.Invoke(this.instance, [seconds])
            ?? throw new InvalidOperationException("The 'Advance' method gave nothing (T-2)."));
    }

}
