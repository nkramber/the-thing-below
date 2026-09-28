using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Import;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The pixels of the hand-edit mode and of the generator mode (D-688, D-689, D-1310,
/// D-1312, D-1314, D-1315).
/// </summary>
public sealed class FrameImportTests
{
    private const string Png = "picture.png";
    private const string Ink = "0b0a0f";
    private const string Red = "c8353a";

    private static readonly PaletteMatch Match = new(DrawingFixtures.Palette());

    [Fact]
    public void TheHandEditModeGivesTheKeyOfEachColorAndTheDotForAlphaZero()
    {
        Drawing drawing = DrawingFixtures.Solid("edit", width: 3, height: 2);
        PngImage image = new ImportPicture(3, 2)
            .Set(0, 0, Ink).Set(1, 0, Red).Set(2, 0, "ffffff", alpha: 0)
            .Set(0, 1, "f2eeea").Set(1, 1, "e8f2f7").Set(2, 1, Ink)
            .ToImage();

        IReadOnlyList<string> rows = FrameImport.FromHandEdit(image, Png, drawing, Match);

        // A pixel of alpha 0 is transparent whatever color a paint program kept under it.
        Assert.Equal(["kx.", "wek"], rows);
    }

    [Fact]
    public void TheHandEditModeReadsAnRgbPngAsOpaque()
    {
        Drawing drawing = DrawingFixtures.Solid("edit", width: 2, height: 1);
        var image = new PngImage(2, 1, PngColorKind.Rgb, [0x0b, 0x0a, 0x0f, 0xc8, 0x35, 0x3a]);

        Assert.Equal(["kx"], FrameImport.FromHandEdit(image, Png, drawing, Match));
    }

    /// <summary>Exit test 2 of PR-51: the hand-edit mode never picks a near color (D-688).</summary>
    [Fact]
    public void AColorOutsideThePaletteFailsInTheHandEditModeWithTheFileThePixelAndTheColor()
    {
        Drawing drawing = DrawingFixtures.Solid("edit", width: 3, height: 3);
        PngImage image = new ImportPicture(3, 3).Fill(0, 0, 3, 3, Ink).Set(1, 2, "c8353b").ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromHandEdit(image, Png, drawing, Match));

        Assert.Equal(Png, fault.File);
        Assert.Contains("x 1, y 2", fault.Message);
        Assert.Contains("c8353b", fault.Message);
        Assert.Contains("never picks a near color", fault.Message);
    }

    [Fact]
    public void TheHandEditModeFailsOnAPngOfAnotherSize()
    {
        Drawing drawing = DrawingFixtures.Solid("edit", width: 32, height: 32);
        PngImage image = new ImportPicture(33, 32).ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromHandEdit(image, Png, drawing, Match));

        Assert.Contains("33 by 32", fault.Message);
        Assert.Contains("32 by 32", fault.Message);
        Assert.Contains(drawing.File, fault.Message);
    }

    /// <summary>Exit test 8 of PR-51, the hand-edit half (D-1315).</summary>
    [Fact]
    public void APixelOfPartialAlphaFailsInTheHandEditMode()
    {
        Drawing drawing = DrawingFixtures.Solid("edit", width: 2, height: 2);
        PngImage image = new ImportPicture(2, 2).Fill(0, 0, 2, 2, Ink).Set(1, 0, Ink, alpha: 128).ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromHandEdit(image, Png, drawing, Match));

        Assert.Equal(Png, fault.File);
        Assert.Contains("x 1, y 0 holds the alpha 128", fault.Message);
    }

    /// <summary>Exit test 5 of PR-51: the crop of F-87 and the center of D-1312.</summary>
    [Fact]
    public void APictureOf42PixelsWithContentOf30GivesAFrameOf32WithTheContentAtTheCenter()
    {
        Drawing drawing = DrawingFixtures.Solid("target");
        PngImage image = new ImportPicture(42, 42).Fill(5, 7, 30, 30, Ink).ToImage();

        GeneratorFrame frame = FrameImport.FromGenerator(image, Png, drawing, Match);

        Assert.Equal(new ContentBox(5, 7, 30, 30), frame.Content);
        Assert.Equal(32, frame.Rows.Count);
        Assert.Equal(1, frame.FrameLeft);
        Assert.Equal(1, frame.FrameTop);
        Assert.Equal(new string('.', 32), frame.Rows[0]);
        Assert.Equal("." + new string('k', 30) + ".", frame.Rows[1]);
        Assert.Equal("." + new string('k', 30) + ".", frame.Rows[30]);
        Assert.Equal(new string('.', 32), frame.Rows[31]);
    }

    /// <summary>The extra pixel of an odd space goes to the right and to the bottom (D-1312).</summary>
    [Fact]
    public void TheExtraPixelOfAnOddSpaceGoesToTheRightAndToTheBottom()
    {
        Drawing drawing = DrawingFixtures.Solid("target");
        PngImage image = new ImportPicture(36, 36).Fill(0, 0, 23, 30, Ink).ToImage();

        GeneratorFrame frame = FrameImport.FromGenerator(image, Png, drawing, Match);

        // 9 free columns give 4 on the left and 5 on the right. 2 free rows give 1 and 1.
        Assert.Equal(4, frame.FrameLeft);
        Assert.Equal(1, frame.FrameTop);
        Assert.Equal("...." + new string('k', 23) + ".....", frame.Rows[1]);

        // 3 free rows of a content of 29 give 1 at the top and 2 at the bottom.
        GeneratorFrame taller = FrameImport.FromGenerator(
            new ImportPicture(32, 32).Fill(0, 0, 10, 29, Ink).ToImage(), Png, drawing, Match);
        Assert.Equal(1, taller.FrameTop);
        Assert.Equal(new string('.', 32), taller.Rows[30]);
        Assert.Equal(new string('.', 32), taller.Rows[31]);
    }

    /// <summary>Exit test 6 of PR-51: content above 64 fails with the file and the size.</summary>
    [Fact]
    public void ContentOf70PixelsFailsWithTheFileAndTheSize()
    {
        Drawing drawing = DrawingFixtures.Solid("target", page: "portraits", width: 64, height: 64);
        PngImage image = new ImportPicture(72, 72).Fill(1, 1, 70, 40, Ink).ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromGenerator(image, Png, drawing, Match));

        Assert.Equal(Png, fault.File);
        Assert.Contains("the content is 70 by 40 pixels", fault.Message);
        Assert.Contains("64 by 64", fault.Message);
    }

    /// <summary>The target sets the frame, so content of 36 fails for a target of 32 (D-1310).</summary>
    [Fact]
    public void ContentOf36PixelsFailsForATargetOf32AndFitsATargetOf64()
    {
        PngImage image = new ImportPicture(38, 38).Fill(4, 0, 27, 36, Ink).ToImage();
        Drawing small = DrawingFixtures.Solid("target");
        Drawing large = DrawingFixtures.Solid("large", page: "battle_sprites", width: 64, height: 64);

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromGenerator(image, Png, small, Match));
        GeneratorFrame frame = FrameImport.FromGenerator(image, Png, large, Match);

        Assert.Contains("the content is 27 by 36 pixels", fault.Message);
        Assert.Contains("Redraw the picture by hand to fit", fault.Message);
        Assert.Equal(64, frame.Rows.Count);
        Assert.Equal(18, frame.FrameLeft);
        Assert.Equal(14, frame.FrameTop);
    }

    [Fact]
    public void TheGeneratorModeFailsOnATargetOfAnotherSize()
    {
        Drawing drawing = DrawingFixtures.Solid("window", page: "ui", width: 24, height: 24);
        PngImage image = new ImportPicture(24, 24).Fill(0, 0, 10, 10, Ink).ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromGenerator(image, Png, drawing, Match));

        Assert.Equal(drawing.File, fault.File);
        Assert.Contains("24 by 24", fault.Message);
    }

    /// <summary>Exit test 7 of PR-51: the nearest color, and the count of the map (D-688, D-1314).</summary>
    [Fact]
    public void TheGeneratorModeMapsAColorOutsideThePaletteAndCountsIt()
    {
        Drawing drawing = DrawingFixtures.Solid("target");
        PngImage image = new ImportPicture(32, 32)
            .Set(10, 10, Ink).Set(11, 10, "c03030").Set(12, 10, "f0eee8")
            .ToImage();

        GeneratorFrame frame = FrameImport.FromGenerator(image, Png, drawing, Match);

        Assert.Equal(2, frame.Mapped);
        Assert.Equal(3, frame.Opaque);
        Assert.Equal(new string('.', 14) + "kxw" + new string('.', 15), frame.Rows[15]);
    }

    /// <summary>Exit test 8 of PR-51, the generator half (D-1315).</summary>
    [Fact]
    public void APixelOfPartialAlphaFailsInTheGeneratorMode()
    {
        Drawing drawing = DrawingFixtures.Solid("target");
        PngImage image = new ImportPicture(32, 32).Fill(4, 4, 10, 10, Ink).Set(20, 3, Ink, alpha: 1).ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromGenerator(image, Png, drawing, Match));

        Assert.Equal(Png, fault.File);
        Assert.Contains("x 20, y 3 holds the alpha 1", fault.Message);
    }

    [Fact]
    public void APictureWithNoOpaquePixelFails()
    {
        Drawing drawing = DrawingFixtures.Solid("target");
        PngImage image = new ImportPicture(32, 32).ToImage();

        ImportException fault = Assert.Throws<ImportException>(() => FrameImport.FromGenerator(image, Png, drawing, Match));

        Assert.Contains("no content", fault.Message);
    }
}
