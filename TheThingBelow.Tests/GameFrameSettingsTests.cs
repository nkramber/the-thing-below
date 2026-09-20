using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The settings of the Godot project that the frame of PR-61 needs: the frame size, the
/// stretch mode, and the texture filter (D-568, F-45, F-48). The world viewport and the tile
/// count come from the Game assembly (D-633, D-634).
/// </summary>
/// <remarks>
/// Exit tests 3 and 12 of PR-61. The stretch mode is `disabled`, so the root viewport is the
/// window and `FrameRoot` owns every scale. A `canvas_items` mode would scale the world, and
/// that is the default of a new Godot project (F-45).
/// </remarks>
public sealed class GameFrameSettingsTests
{
    private const string ProjectPath = "TheThingBelow.Game/project.godot";
    private const string FrameTypeName = "TheThingBelow.Game.Ui.FrameRoot";
    private const string FitTypeName = "TheThingBelow.Game.Ui.ScreenFit";

    /// <summary>The side of one tile of the map, in art pixels (D-228).</summary>
    private const int TileSize = 32;

    [Fact]
    public void TheProjectHoldsTheFrameOfTwelveEightyBySevenTwenty()
    {
        // D-568. One 16 to 9 frame, which every screen shows in full.
        IReadOnlyDictionary<string, string> display = Section("display");

        Assert.Equal("1280", display["window/size/viewport_width"]);
        Assert.Equal("720", display["window/size/viewport_height"]);
    }

    [Fact]
    public void TheProjectScalesNothingByItself()
    {
        // Exit test 3, F-45, F-48. Godot has no mode that scales up by a whole number and
        // then scales down, so Game builds both steps of D-573 itself.
        IReadOnlyDictionary<string, string> display = Section("display");

        Assert.Equal("\"disabled\"", display["window/stretch/mode"]);
        Assert.Equal("\"ignore\"", display["window/stretch/aspect"]);
    }

    [Fact]
    public void EveryCanvasTextureDrawsWithTheNearestFilter()
    {
        // Exit test 3, F-45. The value 0 is the Nearest filter, so an art pixel keeps its
        // hard edge at every scale (D-230).
        Assert.Equal("0", Section("rendering")["textures/canvas_textures/default_texture_filter"]);
    }

    [Fact]
    public void TheWorldViewportIsSixFortyByThreeSixty()
    {
        // Exit test 12, D-634. One art pixel of the world is one viewport pixel, so the light
        // of a later PR falls on art pixels (F-48).
        Assert.Equal(640, Constant(FrameTypeName, "WorldWidth"));
        Assert.Equal(360, Constant(FrameTypeName, "WorldHeight"));
    }

    [Fact]
    public void TheFrameHoldsTwentyByElevenAndAQuarterTiles()
    {
        // Exit test 12, D-633. The world draws at 2x, so the frame of 1280 by 720 holds 20
        // columns and 11.25 rows of 32-pixel tiles.
        int scale = Constant(FrameTypeName, "WorldScale");
        int width = Constant(FitTypeName, "FrameWidth");
        int height = Constant(FitTypeName, "FrameHeight");

        Assert.Equal(2, scale);
        Assert.Equal(20, width / (TileSize * scale));
        Assert.Equal(0, width % (TileSize * scale));

        // A quarter row sits at the bottom edge, and it is 16 frame pixels. That is a whole
        // pixel count, so the art stays sharp (D-568, D-633).
        Assert.Equal(11, height / (TileSize * scale));
        Assert.Equal(16, height % (TileSize * scale));
    }

    [Fact]
    public void TheWorldViewportFillsTheFrameAtItsScale()
    {
        // D-633, D-634. The world at 2x covers the whole frame, so no band of the frame
        // shows nothing.
        int scale = Constant(FrameTypeName, "WorldScale");

        Assert.Equal(Constant(FitTypeName, "FrameWidth"), Constant(FrameTypeName, "WorldWidth") * scale);
        Assert.Equal(Constant(FitTypeName, "FrameHeight"), Constant(FrameTypeName, "WorldHeight") * scale);
    }

    [Fact]
    public void TheStyleFileHoldsTheColorsAndTheWindowFrame()
    {
        // D-527. Game builds the Godot theme from this file, and no theme resource exists.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.Equal(1, content.Style.BorderPixels);
        Assert.Equal("drawing.ui_window_frame", content.Style.FrameOf(UiStyle.WindowFrameRole).Drawing.Value);

        foreach (UiColor color in content.Style.Colors)
        {
            Assert.True(
                content.Palette.TryColorOf(color.KeyCharacter, out _),
                $"The palette holds no color with the key '{color.Key}' of the role '{color.Role}' (D-181).");
        }
    }

    [Fact]
    public void NoThemeResourceFileExists()
    {
        // G-6, D-527. Game data never lives in a `.tres` file.
        string[] found = System.IO.Directory.GetFiles(
            RepositoryRoot.PathTo("TheThingBelow.Game"),
            "*.tres",
            System.IO.SearchOption.AllDirectories);

        Assert.Empty(found);
    }

    private static IReadOnlyDictionary<string, string> Section(string name) =>
        GodotConfigFile.Read(ProjectPath)[name];

    private static int Constant(string typeName, string field) =>
        (int)GameAssemblyFile.Type(typeName).GetField(field)!.GetValue(null)!;
}
