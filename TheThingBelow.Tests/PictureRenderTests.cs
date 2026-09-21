using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Pictures;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The render of a large picture and the `picture` command (D-516, D-518, D-816, D-817). Each
/// test decodes the PNG and compares pixels, never bytes (F-19).
/// </summary>
public sealed class PictureRenderTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), $"picture-render-{Guid.NewGuid():N}");

    /// <summary>Removes the output folder of the test.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    [Fact]
    public void TheDecodedRenderMatchesThePiecesPixelByPixel()
    {
        // Exit test 1 of PR-55. The later entry covers the earlier one, a dot leaves the pixel
        // under it, and the copies of the second row of piece A clip at the bottom edge (D-817).
        ContentSet set = ContentSet.Load(PictureFixtures.Files());
        LargePicture picture = set.PictureOf(PictureFixtures.Id(PictureFixtures.PictureId));

        PngImage decoded = PngReader.Read(PngWriter.Write(PictureRender.Render(picture, set)), "test render");

        string[] expected = ["DDDDDD", "kwkwkw", "k.k.k.", "DDDwkw"];
        for (int y = 0; y < expected.Length; y += 1)
        {
            for (int x = 0; x < expected[y].Length; x += 1)
            {
                Assert.True(
                    ColorAt(decoded, x, y) == ColorOf(set.Palette, expected[y][x]),
                    $"The pixel {x},{y} holds {ColorAt(decoded, x, y)}, and the pieces give '{expected[y][x]}'.");
            }
        }
    }

    [Fact]
    public void TheRenderHasTheSizeOfThePicture()
    {
        ContentSet set = ContentSet.Load(PictureFixtures.Files());
        LargePicture picture = set.PictureOf(PictureFixtures.Id(PictureFixtures.PictureId));

        PngImage image = PictureRender.Render(picture, set);

        Assert.Equal((6, 4), (image.Width, image.Height));
    }

    [Fact]
    public void TheFixtureBackdropCoversTheWholeWorldViewportWithNoGap()
    {
        // Exit test 3 of PR-55. A backdrop is 640 by 360 art pixels, and the frame shows it at
        // 2x over the frame of 1280 by 720 (D-568, D-816). No pixel stays transparent.
        using var output = new StringWriter();
        using var errors = new StringWriter();

        int exitCode = PictureCommand.Run(["--root", RepositoryRoot.Find(), "--out", this.folder], output, errors);

        Assert.True(exitCode == 0, errors.ToString());
        PngImage decoded = PngReader.ReadFile(Path.Combine(this.folder, "fixture_backdrop.png"));
        Assert.Equal((640, 360), (decoded.Width, decoded.Height));
        for (int y = 0; y < decoded.Height; y += 1)
        {
            for (int x = 0; x < decoded.Width; x += 1)
            {
                if (decoded.Pixels[(((y * decoded.Width) + x) * 4) + 3] != 255)
                {
                    Assert.Fail($"The pixel {x},{y} of the fixture backdrop is transparent, and a backdrop covers the frame (D-568, D-816).");
                }
            }
        }
    }

    [Fact]
    public void TheCommandWritesTheSamePixelsAsTheRender()
    {
        using var output = new StringWriter();
        using var errors = new StringWriter();
        string root = RepositoryRoot.Find();

        PictureCommand.Run(["--root", root, "--out", this.folder], output, errors);

        ContentSet set = ContentSet.Load(ContentFolder.Read(root));
        LargePicture picture = set.PictureOf(PictureFixtures.Id("picture.fixture_backdrop"));
        byte[] rendered = PictureRender.Render(picture, set).Pixels.ToArray();
        byte[] written = PngReader.ReadFile(Path.Combine(this.folder, "fixture_backdrop.png")).Pixels.ToArray();
        Assert.Equal(rendered, written);
        Assert.Contains("picture: pictures 1.", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWithNoOutputFolderFails()
    {
        using var output = new StringWriter();
        using var errors = new StringWriter();

        int exitCode = PictureCommand.Run(["--root", RepositoryRoot.Find()], output, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains("--out", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ACheckoutWithNoPictureFails()
    {
        // T-2: a run that writes nothing reads as a pass, so the command fails.
        string root = Path.Combine(this.folder, "checkout");
        foreach (ContentFile file in PictureFixtures.Files())
        {
            if (LargePicture.IsPictureFile(file.Path))
            {
                continue;
            }

            string path = Path.Combine(root, ContentFolder.FolderName, file.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, file.Bytes);
        }

        using var output = new StringWriter();
        using var errors = new StringWriter();

        int exitCode = PictureCommand.Run(["--root", root, "--out", Path.Combine(this.folder, "out")], output, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains("no large picture", errors.ToString(), StringComparison.Ordinal);
    }

    private static string ColorAt(PngImage image, int x, int y)
    {
        int start = ((y * image.Width) + x) * 4;
        ReadOnlySpan<byte> pixel = image.Pixels.Slice(start, 4);
        return pixel[3] == 0 ? "clear" : $"{pixel[0]:x2}{pixel[1]:x2}{pixel[2]:x2}";
    }

    private static string ColorOf(Palette palette, char key)
    {
        if (key == Drawing.Transparent)
        {
            return "clear";
        }

        Assert.True(palette.TryColorOf(key, out PaletteColor? color), $"The test palette holds no key '{key}'.");
        return $"{color.Red:x2}{color.Green:x2}{color.Blue:x2}";
    }
}
