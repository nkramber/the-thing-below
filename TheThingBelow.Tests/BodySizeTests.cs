using System;
using System.Reflection;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The body size of the text, and its default on each screen (D-707). The tests read the
/// built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// Exit test 14 of PR-61 reads the default of each of the four screens of M-8. The owner
/// picked a body of 32 on the Steam Deck, and a body of 24 on the three desktop screens, in
/// the probe run of 2026-09-19 (D-621, D-707).
/// </remarks>
public sealed class BodySizeTests
{
    private const string BodyTypeName = "TheThingBelow.Game.Ui.BodySize";
    private const string FitTypeName = "TheThingBelow.Game.Ui.ScreenFit";
    private const string ModeTypeName = "TheThingBelow.Game.Ui.FitMode";

    [Theory]
    [InlineData(1280, 800, 32)]
    [InlineData(1920, 1080, 24)]
    [InlineData(2560, 1440, 24)]
    [InlineData(3840, 2160, 24)]
    public void EachScreenOfTheProbeTakesTheBodyThatTheOwnerPicked(int screenWidth, int screenHeight, int body)
    {
        // Exit test 14. The four screens of M-8: the OLED Steam Deck, a 27-inch 1080p screen,
        // a 32-inch 1440p screen, and a 27-inch 4K screen (D-707).
        UiStyle style = Style();

        Assert.Equal(body, DefaultFor(DrawnHeightOf(screenWidth, screenHeight), style));
    }

    [Fact]
    public void TheStyleFileHoldsTheTwoSizesOfTheOwner()
    {
        // D-707. The setting takes two values and no other value.
        UiStyle style = Style();

        Assert.Equal(24, style.SmallBody);
        Assert.Equal(32, style.LargeBody);
        Assert.Equal(2, style.TitleScale);
    }

    [Theory]
    [InlineData(24, 48)]
    [InlineData(32, 64)]
    public void TheTitleIsTwiceItsBody(int body, int title)
    {
        // D-707. The player changes the title through the body alone, and no setting sets it.
        Assert.Equal(title, Style().TitleSizeOf(body));
    }

    [Fact]
    public void ASizeThatIsNeitherBodyFails()
    {
        // T-2. A layout never asks for a size that the style file does not hold.
        ContentException error = Assert.Throws<ContentException>(() => Style().TitleSizeOf(48));

        Assert.Equal(UiStyle.Path, error.File);
        Assert.Contains("D-707", error.Message);
    }

    [Fact]
    public void AFrameHeightOfZeroFails()
    {
        // T-2. An absent measurement is an error, and never the smaller body.
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => DefaultFor(0, Style()));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    [Fact]
    public void ACrashedSessionNeverBuildsTheScreenAgainOnAResize()
    {
        // Finding P3-5 of the repository review: a resize after a crash built the screen with no
        // run, crashed a second time, and replaced the message of the first crash (D-559).
        Assert.False(RebuildsOnResize(running: false, auto: true, builtBody: 32, defaultBody: 24));
    }

    [Fact]
    public void ARunningSessionBuildsTheScreenAgainWhenTheAutoBodyChanges()
    {
        // D-707, D-874: the auto body follows the window, and the other three cases hold still.
        Assert.True(RebuildsOnResize(running: true, auto: true, builtBody: 32, defaultBody: 24));
        Assert.False(RebuildsOnResize(running: true, auto: true, builtBody: 24, defaultBody: 24));
        Assert.False(RebuildsOnResize(running: true, auto: false, builtBody: 32, defaultBody: 24));
    }

    private static bool RebuildsOnResize(bool running, bool auto, int builtBody, int defaultBody) =>
        (bool)GameAssemblyFile.Type(BodyTypeName)
            .GetMethod("RebuildsOnResize")!
            .Invoke(null, [running, auto, builtBody, defaultBody])!;

    private static int DefaultFor(int drawnHeight, UiStyle style) =>
        (int)GameAssemblyFile.Type(BodyTypeName)
            .GetMethod("DefaultFor")!
            .Invoke(null, [drawnHeight, style.SmallBody, style.LargeBody])!;

    private static int DrawnHeightOf(int screenWidth, int screenHeight)
    {
        Type mode = GameAssemblyFile.Type(ModeTypeName);
        object fill = Enum.Parse(mode, "Fill");
        object fit = GameAssemblyFile.Type(FitTypeName)
            .GetMethod("Of")!
            .Invoke(null, [fill, screenWidth, screenHeight])!;

        return (int)fit.GetType().GetProperty("Height")!.GetValue(fit)!;
    }

    private static UiStyle Style() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())).Style;
}
