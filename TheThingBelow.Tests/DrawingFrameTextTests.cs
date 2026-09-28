using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Import;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The write of the rows of one frame, which keeps every other byte (D-1311).</summary>
public sealed class DrawingFrameTextTests
{
    private const string File = "sprites/drawings/cast/walk.json";

    private const string ThreeFrames =
        """
        {
         "id": "drawing.walk",
         "page": "map_sprites",
         "width": 2,
         "height": 2,
         "draws": [
          { "content": "cast.walk", "use": "map_front" }
         ],
         "frames": [
          {
           "ticks": 8,
           "rows": [
            "kk",
            "kk"
           ]
          },
          {
           "ticks": 9,
           "rows": [
            "KK",
            "KK"
           ]
          },
          {
           "ticks": 10,
           "rows": [
            "dd",
            "dd"
           ]
          }
         ]
        }
        """;

    [Fact]
    public void TheWriteChangesTheRowsOfTheFrameAlone()
    {
        byte[] result = DrawingFrameText.ReplaceRows(Encoding.UTF8.GetBytes(ThreeFrames), File, 1, ["x.", ".x"]);

        string expected = ThreeFrames.Replace(
            "\"KK\",\n    \"KK\"",
            "\"x.\",\n    \".x\"",
            System.StringComparison.Ordinal);
        Assert.Equal(expected, Encoding.UTF8.GetString(result));
    }

    /// <summary>A drawing file of the repository keeps each byte when its rows stay (D-515).</summary>
    [Fact]
    public void TheSameRowsGiveTheSameBytesForADrawingOfTheRepository()
    {
        string path = RepositoryRoot.PathTo("content/sprites/drawings/cast/marrek-map-front.json");
        byte[] bytes = System.IO.File.ReadAllBytes(path);
        Drawing drawing = Drawing.Read(bytes, path);

        byte[] result = DrawingFrameText.ReplaceRows(bytes, path, 0, drawing.Frames[0].Rows);

        Assert.Equal(bytes, result);
    }

    [Fact]
    public void AFrameOnOneLineGetsOneRowOnEachLine()
    {
        string body = DrawingFixtures.BodyOfRows("one", ["kk", "kk"]);

        byte[] result = DrawingFrameText.ReplaceRows(Encoding.UTF8.GetBytes(body), File, 0, ["xx", "ww"]);

        Drawing drawing = Drawing.Read(result, File);
        Assert.Equal(["xx", "ww"], drawing.Frames[0].Rows);
        Assert.Contains("\"rows\": [\n   \"xx\",\n   \"ww\"\n  ] }", Encoding.UTF8.GetString(result));
    }

    /// <summary>A quote mark and a backslash are printable keys, and each one takes an escape.</summary>
    [Fact]
    public void AQuoteMarkAndABackslashReadBack()
    {
        byte[] result = DrawingFrameText.ReplaceRows(Encoding.UTF8.GetBytes(ThreeFrames), File, 0, ["\"\\", "k."]);

        Assert.Equal(["\"\\", "k."], Drawing.Read(result, File).Frames[0].Rows);
    }

    [Fact]
    public void AnAbsentFrameFailsWithTheFileAndTheCount()
    {
        ImportException fault = Assert.Throws<ImportException>(
            () => DrawingFrameText.ReplaceRows(Encoding.UTF8.GetBytes(ThreeFrames), File, 3, ["kk", "kk"]));

        Assert.Equal(File, fault.File);
        Assert.Contains("holds 3 frames", fault.Message);
    }

    /// <summary>A new file that does not read back fails, and the caller writes nothing (T-2).</summary>
    [Fact]
    public void RowsOfTheWrongWidthFailTheReadBack()
    {
        ImportException fault = Assert.Throws<ImportException>(
            () => DrawingFrameText.ReplaceRows(Encoding.UTF8.GetBytes(ThreeFrames), File, 0, ["kkk", "kkk"]));

        Assert.Contains("does not read back", fault.Message);
    }

    [Fact]
    public void AFileThatIsNotJsonFails()
    {
        ImportException fault = Assert.Throws<ImportException>(
            () => DrawingFrameText.ReplaceRows(Encoding.UTF8.GetBytes("{ \"frames\": [ { \"rows\": "), File, 0, ["k"]));

        Assert.Contains("not JSON", fault.Message);
    }
}
