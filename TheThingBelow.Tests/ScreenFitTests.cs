using System;
using System.Reflection;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The place of the frame of 1280 by 720 on a screen, in both fit modes (D-232, D-568,
/// D-573). The tests read the built Game assembly, because Tests takes no reference to Game
/// (D-614).
/// </summary>
/// <remarks>
/// Exit tests 1 and 2 of PR-61 lock the three screens of `phase-2-first-playable.md`: 1280
/// by 800, 1920 by 1080, and 2560 by 1440 (F-48).
/// </remarks>
public sealed class ScreenFitTests
{
    private const string FitTypeName = "TheThingBelow.Game.Ui.ScreenFit";
    private const string ModeTypeName = "TheThingBelow.Game.Ui.FitMode";

    [Fact]
    public void TheFrameIsTwelveEightyBySevenTwenty()
    {
        // D-568. One 16 to 9 frame, and black bars on every other screen shape.
        Assert.Equal(1280, Constant("FrameWidth"));
        Assert.Equal(720, Constant("FrameHeight"));
    }

    [Theory]
    [InlineData(1280, 800, 1280, 720, 0, 40, 1, false)]
    [InlineData(1920, 1080, 1920, 1080, 0, 0, 2, true)]
    [InlineData(2560, 1440, 2560, 1440, 0, 0, 2, false)]
    public void TheFillFitPlacesTheFrameOnEachScreen(
        int screenWidth,
        int screenHeight,
        int width,
        int height,
        int left,
        int top,
        int step,
        bool smooth)
    {
        // Exit tests 1 and 2. The Deck at 1280 by 800 shows the frame at 1x with a bar of 40
        // pixels above and below (D-92, D-568). A screen of 1920 by 1080 needs a scale of
        // 1.5, so the first step gives 1440 rows and the second step scales them down
        // (D-573). A screen of 2560 by 1440 takes 2x, and no second step runs.
        Fit fit = Fit.Of(false, screenWidth, screenHeight);

        Assert.Equal(width, fit.Width);
        Assert.Equal(height, fit.Height);
        Assert.Equal(left, fit.Left);
        Assert.Equal(top, fit.Top);
        Assert.Equal(step, fit.WholeStep);
        Assert.Equal(smooth, fit.NeedsSmoothStep);
    }

    [Theory]
    [InlineData(1280, 800, 1280, 720, 0, 40, 1)]
    [InlineData(1920, 1080, 1280, 720, 320, 180, 1)]
    [InlineData(2560, 1440, 2560, 1440, 0, 0, 2)]
    public void TheWholePixelFitPlacesTheFrameOnEachScreen(
        int screenWidth,
        int screenHeight,
        int width,
        int height,
        int left,
        int top,
        int step)
    {
        // D-232. The setting scales by a whole number alone, so every pixel of the frame
        // covers the same square of device pixels and the bars grow. The Deck shows the
        // frame at 1x in both modes (D-568).
        Fit fit = Fit.Of(true, screenWidth, screenHeight);

        Assert.Equal(width, fit.Width);
        Assert.Equal(height, fit.Height);
        Assert.Equal(left, fit.Left);
        Assert.Equal(top, fit.Top);
        Assert.Equal(step, fit.WholeStep);
        Assert.False(fit.NeedsSmoothStep);
    }

    [Fact]
    public void TheStepOfTheFirstScaleCarriesTheWholeFrame()
    {
        // D-573. The first step draws the frame at a whole number with the Nearest filter,
        // and the second step scales that picture down.
        Fit fit = Fit.Of(false, 1920, 1080);

        Assert.Equal(2560, fit.StepWidth);
        Assert.Equal(1440, fit.StepHeight);
    }

    [Theory]
    [InlineData(1280, 800, true)]
    [InlineData(1920, 1080, false)]
    [InlineData(2560, 1440, false)]
    [InlineData(3840, 2160, false)]
    public void TheFrameDrawsAtItsOwnSizeOnTheDeckAlone(int screenWidth, int screenHeight, bool oneToOne)
    {
        // D-707. The fit of 1x sets the larger body size, and every larger fit sets the
        // smaller one.
        Assert.Equal(oneToOne, Fit.Of(false, screenWidth, screenHeight).IsOneToOne);
    }

    [Fact]
    public void AScreenNarrowerThanSixteenToNineGainsBarsAboveAndBelow()
    {
        // D-228. A 4 to 3 screen of 1024 by 768 shows the frame across its full width.
        Fit fit = Fit.Of(false, 1024, 768);

        Assert.Equal(1024, fit.Width);
        Assert.Equal(576, fit.Height);
        Assert.Equal(0, fit.Left);
        Assert.Equal(96, fit.Top);
    }

    [Fact]
    public void AScreenWiderThanSixteenToNineGainsBarsAtTheSides()
    {
        // D-228. A 21 to 9 screen of 2560 by 1080 shows the frame at its full height.
        Fit fit = Fit.Of(false, 2560, 1080);

        Assert.Equal(1920, fit.Width);
        Assert.Equal(1080, fit.Height);
        Assert.Equal(320, fit.Left);
        Assert.Equal(0, fit.Top);
    }

    [Theory]
    [InlineData(0, 720)]
    [InlineData(1280, 0)]
    [InlineData(-1, -1)]
    public void AScreenWithNoSizeFails(int screenWidth, int screenHeight)
    {
        // T-2. A screen side of zero is an error, and never a fit of zero pixels.
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => Fit.Of(false, screenWidth, screenHeight));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    [Theory]
    [InlineData(false, 1280, 800, 0, 40, 0, 0)]
    [InlineData(false, 1280, 800, 1279, 759, 1279, 719)]
    [InlineData(false, 1920, 1080, 960, 540, 640, 360)]
    [InlineData(true, 1920, 1080, 320, 180, 0, 0)]
    public void APointOnTheFrameGivesItsFramePixel(bool wholePixels, int screenWidth, int screenHeight, int x, int y, int frameX, int frameY)
    {
        // D-872. The mouse on a menu reads a device pixel, and the menu needs the frame pixel.
        Assert.Equal((frameX, frameY), Fit.Of(wholePixels, screenWidth, screenHeight).ToFrame(x, y));
    }

    [Theory]
    [InlineData(false, 1280, 800, 640, 39)]
    [InlineData(false, 1280, 800, 640, 760)]
    [InlineData(true, 1920, 1080, 319, 540)]
    public void APointOnABarGivesNoFramePixel(bool wholePixels, int screenWidth, int screenHeight, int x, int y)
    {
        Assert.Null(Fit.Of(wholePixels, screenWidth, screenHeight).ToFrame(x, y));
    }

    private static int Constant(string name) =>
        (int)GameAssemblyFile.Type(FitTypeName).GetField(name)!.GetValue(null)!;

    /// <summary>The place of the frame, read from the Game assembly with no engine (D-614).</summary>
    private sealed class Fit
    {
        private readonly object value;

        private Fit(object value) => this.value = value;

        public int Width => this.Read<int>(nameof(this.Width));

        public int Height => this.Read<int>(nameof(this.Height));

        public int Left => this.Read<int>(nameof(this.Left));

        public int Top => this.Read<int>(nameof(this.Top));

        public int WholeStep => this.Read<int>(nameof(this.WholeStep));

        public int StepWidth => this.Read<int>(nameof(this.StepWidth));

        public int StepHeight => this.Read<int>(nameof(this.StepHeight));

        public bool NeedsSmoothStep => this.Read<bool>(nameof(this.NeedsSmoothStep));

        public bool IsOneToOne => this.Read<bool>(nameof(this.IsOneToOne));

        public (int X, int Y)? ToFrame(int screenX, int screenY) =>
            ((int X, int Y)?)this.value.GetType().GetMethod("ToFrame")!.Invoke(this.value, [screenX, screenY]);

        public static Fit Of(bool wholePixels, int screenWidth, int screenHeight)
        {
            Type mode = GameAssemblyFile.Type(ModeTypeName);
            object picked = Enum.Parse(mode, wholePixels ? "WholePixels" : "Fill");
            MethodInfo method = GameAssemblyFile.Type(FitTypeName).GetMethod("Of")!;

            return new Fit(method.Invoke(null, [picked, screenWidth, screenHeight])!);
        }

        private T Read<T>(string name) =>
            (T)this.value.GetType().GetProperty(name)!.GetValue(this.value)!;
    }
}
