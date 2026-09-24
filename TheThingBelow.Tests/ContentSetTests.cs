using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The load of a content set runs the rules that span files: every file has a record
/// (D-517), no two rule entries take one id (D-166), every string id that a rule names is
/// in the table (G-7), every palette key of a drawing is in the palette (F-20), and the
/// atlas index matches the drawing files (D-666).
/// </summary>
public sealed class ContentSetTests
{
    private const string PaletteBody =
        """
        {
         "comment": "a test palette",
         "colors": [ { "index": 0, "key": "k", "hex": "0b0a0f", "name": "ink", "height": 0 } ]
        }
        """;

    /// <summary>
    /// The index of a set with no drawing of its own. It still holds the UI page and the
    /// UI drawing, because every set needs the UI base (D-527).
    /// </summary>
    private static readonly string AtlasBody =
        $$"""
        {
         "comment": "a test index",
         "pages": [ {{UiContentFixtures.AtlasPageRecord}} ],
         "drawings": [
        {{UiContentFixtures.AtlasEntries}}
         ]
        }
        """;

    private const string StringsBody =
        """{ "comment": "a test table", "strings": [ { "id": "label.lamp", "text": "Tin lamp" }, """ +
        TestBattles.LessonStrings + ", " + TestBattles.ItemStrings + ", " + TestBattles.NoticeStrings + " ] }";

    [Fact]
    public void AWellFormedSetLoads()
    {
        ContentSet set = ContentSet.Load(Files(Rule("rules/a.json", "fixture.lamp", "label.lamp")));

        Assert.Single(set.Palette.Colors);
        Assert.Equal(49, set.Strings.Count);
        Assert.Equal(64, set.Hash.Length);
        RuleFixtureEntry entry = Assert.Single(set.RuleEntries);
        Assert.Equal("fixture.lamp", entry.Id.Value);
    }

    [Fact]
    public void ARepeatedContentIdInOneFileFails()
    {
        ContentFile file = File(
            "rules/a.json",
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 1 },
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 2 }
             ]
            }
            """);

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Files(file)));

        Assert.Contains("fixture.lamp", error.Message);
        Assert.Contains("D-166", error.Message);
    }

    [Fact]
    public void ARepeatedContentIdAcrossTwoFilesFails()
    {
        IReadOnlyList<ContentFile> files = Files(
            Rule("rules/a.json", "fixture.lamp", "label.lamp"),
            Rule("rules/b.json", "fixture.lamp", "label.lamp"));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal("rules/b.json", error.File);
        Assert.Equal("fixture.lamp", error.Field);
        Assert.Contains("rules/a.json", error.Message);
    }

    [Fact]
    public void AStringIdThatTheTableLacksFails()
    {
        ContentSet Load() => ContentSet.Load(Files(Rule("rules/a.json", "fixture.rope", "label.rope")));

        ContentException error = Assert.Throws<ContentException>(Load);

        Assert.Equal("rules/a.json", error.File);
        Assert.Equal("fixture.rope.label", error.Field);
        Assert.Contains("label.rope", error.Message);
        Assert.Contains("G-7", error.Message);
    }

    [Fact]
    public void AFileThatNoRecordReadsFails()
    {
        IReadOnlyList<ContentFile> files =
        [
            File(Palette.Path, PaletteBody),
            File(StringTable.Path, StringsBody),
            File(AtlasIndex.Path, AtlasBody),
            File("music/first.json", "{}"),
        ];

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal("music/first.json", error.File);
        Assert.Contains("D-517", error.Message);
    }

    [Fact]
    public void TheDeviceTableOfTheButtonPromptsIsAFileThatNoRecordReads()
    {
        // D-815: the game shows no button prompt, so no record of Core reads the device table
        // that picked the glyph set of a prompt, and a leftover copy fails the load (D-517).
        IReadOnlyList<ContentFile> files = Files(File("ui/devices.json", "{}"));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal("ui/devices.json", error.File);
        Assert.Contains("D-517", error.Message);
    }

    [Fact]
    public void AnAbsentPaletteFails()
    {
        IReadOnlyList<ContentFile> files = [File(StringTable.Path, StringsBody)];

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(Palette.Path, error.File);
        Assert.Contains("no such file", error.Message);
    }

    [Fact]
    public void AnAbsentStringTableFails()
    {
        IReadOnlyList<ContentFile> files = [File(Palette.Path, PaletteBody), File(AtlasIndex.Path, AtlasBody)];

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(StringTable.Path, error.File);
        Assert.Contains("no such file", error.Message);
    }

    [Fact]
    public void ARuleEntryReadsByItsId()
    {
        ContentSet set = ContentSet.Load(Files(Rule("rules/a.json", "fixture.lamp", "label.lamp")));

        RuleFixtureEntry entry = set.RuleEntry(Id("fixture.lamp"));

        Assert.Equal("label.lamp", entry.Label.Value);
    }

    [Fact]
    public void AnIdThatTheSetLacksFails()
    {
        ContentSet set = ContentSet.Load(Files(Rule("rules/a.json", "fixture.lamp", "label.lamp")));

        ContentException error = Assert.Throws<ContentException>(() => set.RuleEntry(Id("fixture.nail")));

        Assert.Equal("fixture.nail", error.Field);
        Assert.Contains("no rule entry", error.Message);
    }

    [Fact]
    public void AWellFormedSetWithADrawingLoads()
    {
        ContentSet set = ContentSet.Load(Art());

        Drawing drawing = set.DrawingOf(Id(DrawingId));
        Assert.Equal(DrawingPath, drawing.File);

        // The drawing of the test, and the window frame of the UI base.
        Assert.Equal(2, System.Linq.Enumerable.Count(set.Drawings));
    }

    [Fact]
    public void AKeyThatThePaletteLacksFailsWithTheFrameTheRowAndTheColumn()
    {
        // F-20. The message names the pixel, so a session finds it in the file.
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(drawing: DrawingBody(rows: "\"k.\", \".z\""))));

        Assert.Equal(DrawingPath, error.File);
        Assert.Equal("frames[0].rows[1]", error.Field);
        Assert.Contains("column 1", error.Message);
        Assert.Contains("'z'", error.Message);
    }

    [Fact]
    public void AnIndexPageWithNoFileFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(page: false)));

        Assert.Equal(AtlasIndex.Path, error.File);
        Assert.Equal("map_sprites", error.Field);
        Assert.Contains(PagePath, error.Message);
    }

    [Fact]
    public void APageFileThatNoIndexNamesFails()
    {
        // The atlas command owns every page file, so a leftover page never ships (D-666).
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(extra: File("sprites/atlas-tiles.png", "png"))));

        Assert.Equal("sprites/atlas-tiles.png", error.File);
        Assert.Contains("D-666", error.Message);
    }

    /// <summary>A page of sprites takes scene light, so it needs its normal map (D-184).</summary>
    [Fact]
    public void ALitPageWithNoNormalMapFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(normal: false)));

        Assert.Equal(AtlasIndex.Path, error.File);
        Assert.Equal("map_sprites", error.Field);
        Assert.Contains(NormalPagePath, error.Message);
    }

    [Fact]
    public void ANormalMapThatNoLitPageNamesFails()
    {
        // The UI takes no scene light, so a normal map of the UI page is a leftover (D-210).
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(extra: File("sprites/normal-map-ui.png", "png"))));

        Assert.Equal("sprites/normal-map-ui.png", error.File);
        Assert.Contains("D-210", error.Message);
    }

    [Fact]
    public void AWellFormedOverrideGridLoads()
    {
        ContentSet set = ContentSet.Load(Art(extra: File("sprites/normals/test.json", OverrideBody(DrawingId, "\"8.\", \".5\""))));

        Assert.Equal(DrawingPath, set.DrawingOf(Id(DrawingId)).File);
    }

    [Fact]
    public void AnOverrideGridOfNoDrawingFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(extra: File("sprites/normals/test.json", OverrideBody("drawing.absent", "\"8.\", \".5\"")))));

        Assert.Equal("sprites/normals/test.json", error.File);
        Assert.Contains("drawing.absent", error.Message);
    }

    [Fact]
    public void ASecondOverrideGridOfOneDrawingFails()
    {
        ContentFile first = File("sprites/normals/a.json", OverrideBody(DrawingId, "\"8.\", \".5\""));
        ContentFile second = File("sprites/normals/b.json", OverrideBody(DrawingId, "\"..\", \"..\""));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(extra: [first, second])));

        Assert.Equal("sprites/normals/b.json", error.File);
        Assert.Contains("D-839", error.Message);
    }

    /// <summary>A direction on a transparent pixel changes no pixel, so it fails and never passes in silence (T-2).</summary>
    [Fact]
    public void AnOverrideDirectionOnATransparentPixelFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(extra: File("sprites/normals/test.json", OverrideBody(DrawingId, "\"8.\", \"5.\"")))));

        Assert.Equal("frames[0].rows[1]", error.Field);
        Assert.Contains("column 0", error.Message);
        Assert.Contains("transparent", error.Message);
    }

    [Fact]
    public void AnIndexEntryWithNoDrawingFileFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(drawing: string.Empty)));

        Assert.Equal(AtlasIndex.Path, error.File);
        Assert.Equal(DrawingId, error.Field);
        Assert.Contains("atlas command", error.Message);
    }

    [Fact]
    public void ADrawingWithNoIndexEntryFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(index: IndexBody(entries: string.Empty))));

        Assert.Equal(AtlasIndex.Path, error.File);
        Assert.Equal(DrawingId, error.Field);
    }

    [Theory]
    [InlineData("width", "3", "3 by 2 pixels")]
    [InlineData("ticks", "5", "5 ticks in the index")]
    [InlineData("use", "portrait", "other things that the drawing draws")]
    public void AnIndexEntryThatDiffersFromTheDrawingFails(string part, string value, string reason)
    {
        // G-24. A stale index fails at load, and the message names the difference.
        string index = part switch
        {
            "width" => IndexBody(width: value),
            "ticks" => IndexBody(ticks: value),
            _ => IndexBody(use: value),
        };

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(index: index)));

        Assert.Equal(DrawingPath, error.File);
        Assert.Equal(DrawingId, error.Field);
        Assert.Contains(reason, error.Message);
        Assert.Contains("G-24", error.Message);
    }

    [Fact]
    public void AnIndexEntryWithAnotherFrameCountFails()
    {
        string index = IndexBody(frames: """{ "x": 0, "y": 0, "ticks": 0 }, { "x": 2, "y": 0, "ticks": 0 }""");

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(index: index)));

        Assert.Equal(DrawingId, error.Field);
        Assert.Contains("2 frames", error.Message);
    }

    [Fact]
    public void AnIndexEntryOnAPageOfAnotherKindFails()
    {
        string index = IndexBody(
            pages: """{ "kind": "tiles", "number": 1, "width": 32, "height": 32 }""",
            page: "tiles");

        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Art(index: index, page: false, extra: File("sprites/atlas-tiles.png", "png"))));

        Assert.Equal(DrawingId, error.Field);
        Assert.Contains("the page 'tiles'", error.Message);
    }

    [Fact]
    public void ARepeatedDrawingIdAcrossTwoFilesFails()
    {
        // The set reads its files in ordinal order of the path, so the second file of the id
        // is the one whose path sorts later (F-39).
        ContentFile second = File("sprites/drawings/cast/other-map-front.json", DrawingBody());

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Art(extra: second)));

        Assert.Equal(DrawingPath, error.File);
        Assert.Equal(DrawingId, error.Field);
        Assert.Contains(second.Path, error.Message);
    }

    [Fact]
    public void ADrawingIdThatTheSetLacksFails()
    {
        ContentSet set = ContentSet.Load(Art());

        ContentException error = Assert.Throws<ContentException>(() => set.DrawingOf(Id("drawing.absent")));

        Assert.Equal("drawing.absent", error.Field);
        Assert.Contains("no drawing", error.Message);
    }

    [Fact]
    public void APathInTheSetTwoTimesFails()
    {
        // A second file of one path would replace the first in silence (T-2).
        List<ContentFile> files = [.. Files(), File(Palette.Path, PaletteBody)];

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(Palette.Path, error.File);
        Assert.Contains("two times", error.Message);
    }

    private const string DrawingId = "drawing.test_map_front";
    private const string DrawingPath = "sprites/drawings/cast/test-map-front.json";
    private const string PagePath = "sprites/atlas-map_sprites.png";
    private const string NormalPagePath = "sprites/normal-map-map_sprites.png";

    /// <summary>A set with one drawing, its page, and an index that matches them.</summary>
    /// <param name="index">The text of the index, or null for one that matches the drawing.</param>
    /// <param name="drawing">The text of the drawing file, null for the default, and empty for no file.</param>
    /// <param name="page">True to hold the page file of the index.</param>
    /// <param name="normal">True to hold the normal-map page of that page (D-184).</param>
    /// <param name="extra">Other files of the set.</param>
    private static IReadOnlyList<ContentFile> Art(
        string? index = null,
        string? drawing = null,
        bool page = true,
        bool normal = true,
        params ContentFile[] extra)
    {
        List<ContentFile> files =
        [
            File(Palette.Path, PaletteBody),
            File(StringTable.Path, StringsBody),
            File(AtlasIndex.Path, index ?? IndexBody()),
        ];

        files.AddRange(UiContentFixtures.Files());
        files.AddRange(TestBattles.Files());
        drawing ??= DrawingBody();
        if (drawing.Length > 0)
        {
            files.Add(File(DrawingPath, drawing));
        }

        if (page)
        {
            // Core reads no pixel of a page, so the bytes are free (D-517).
            files.Add(File(PagePath, "png"));
        }

        if (normal)
        {
            files.Add(File(NormalPagePath, "png"));
        }

        files.AddRange(extra);
        return files;
    }

    private static string DrawingBody(string rows = "\"k.\", \".k\"") =>
        $$"""
        {
         "id": "{{DrawingId}}",
         "page": "map_sprites",
         "width": 2,
         "height": 2,
         "draws": [ { "content": "cast.test", "use": "map_front" } ],
         "frames": [ { "ticks": 0, "rows": [ {{rows}} ] } ]
        }
        """;

    private static string OverrideBody(string drawing, string rows) =>
        $$"""
        {
         "drawing": "{{drawing}}",
         "frames": [ { "rows": [ {{rows}} ] } ]
        }
        """;

    private static string IndexBody(
        string? pages = null,
        string? entries = null,
        string page = "map_sprites",
        string width = "2",
        string ticks = "0",
        string use = "map_front",
        string? frames = null)
    {
        pages ??= """{ "kind": "map_sprites", "number": 1, "width": 32, "height": 32 }""";
        frames ??= $$"""{ "x": 0, "y": 0, "ticks": {{ticks}} }""";
        entries ??= $$"""
            {
             "id": "{{DrawingId}}",
             "page": "{{page}}",
             "width": {{width}},
             "height": 2,
             "draws": [ { "content": "cast.test", "use": "{{use}}" } ],
             "frames": [ {{frames}} ]
            }
            """;

        // Every set needs the UI base, so each index of a test names the UI page and its
        // drawing beside the drawing of the test (D-527).
        return $$"""
            {
             "comment": "a test index",
             "pages": [ {{pages}}, {{UiContentFixtures.AtlasPageRecord}} ],
             "drawings": [ {{Beside(entries)}}
            {{UiContentFixtures.AtlasEntries}}
             ]
            }
            """;
    }

    /// <summary>Puts a comma after the entries of a test, and gives nothing for an empty list.</summary>
    private static string Beside(string entries) => entries.Length > 0 ? entries + "," : string.Empty;

    private static ContentId Id(string value) => ContentId.Parse(value, "rules/a.json", "id");

    private static ContentFile File(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));

    private static ContentFile Rule(string path, string id, string label) =>
        File(
            path,
            $$"""
            {
             "comment": "a note",
             "fixtures": [ { "id": "{{id}}", "label": "{{label}}", "weight": 1 } ]
            }
            """);

    [Fact]
    public void AMapFileJoinsTheSetUnderItsId()
    {
        ContentSet set = ContentSet.Load(Files([MapFile("rules/maps/one.json", "map.one", "label.lamp"), .. UiContentFixtures.LightFilesOf("one", "map.one", "day")]));

        GameMap map = set.Map(ContentId.Parse("map.one", "test", "id"));

        Assert.Equal("rules/maps/one.json", map.File);
        Assert.Equal("label.lamp", map.Label.Value);
        Assert.Single(set.Maps);
    }

    [Fact]
    public void AMapIdThatTheSetLacksIsAnError()
    {
        ContentSet set = ContentSet.Load(Files());

        ContentException error = Assert.Throws<ContentException>(
            () => set.Map(ContentId.Parse("map.deep", "test", "id")));

        Assert.Contains("map.deep", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapLabelThatTheStringTableLacksIsAnError()
    {
        // G-7: every string the player reads lives in the string table.
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Files([MapFile("rules/maps/one.json", "map.one", "label.absent"), .. UiContentFixtures.LightFilesOf("one", "map.one", "day")])));

        Assert.Contains("label.absent", error.Message, StringComparison.Ordinal);
        Assert.Contains("rules/maps/one.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AThingOfAMapTakesAnIdThatNoOtherEntryHolds()
    {
        // D-166: an id is permanent, so no two entries of the content take one.
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Files(
            MapFile("rules/maps/one.json", "map.one", "label.lamp"),
            MapFile("rules/maps/two.json", "map.two", "label.lamp"))));

        Assert.Contains("spawn_point.one_start", error.Message, StringComparison.Ordinal);
        Assert.Contains("permanent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapAndARuleEntryNeverShareAnId()
    {
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Files(
            MapFile("rules/maps/one.json", "map.one", "label.lamp"),
            Rule("rules/a.json", "map.one", "label.lamp"))));

        Assert.Contains("map.one", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStorySceneFileJoinsTheSetUnderItsId()
    {
        ContentSet set = ContentSet.Load(Files(SceneFile("label.lamp")));

        StoryScene scene = Assert.Single(set.Story.Scenes);
        Assert.Equal("scene.test_note", scene.Id.Value);
        Assert.Equal("rules/scenes/note.json", scene.File);
    }

    [Fact]
    public void AStorySceneLineThatTheStringTableLacksIsAnError()
    {
        // Exit test 3 of PR-68: the error names the story scene, the step, and the id (G-7).
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Files(SceneFile("line.absent"))));

        Assert.Contains("rules/scenes/note.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("scene.test_note.steps[0].line", error.Message, StringComparison.Ordinal);
        Assert.Contains("line.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASetWithNoFlagFileIsAnError()
    {
        // D-1003: one file declares every flag, so a build with none holds no story.
        List<ContentFile> files = [];
        foreach (ContentFile file in Files())
        {
            if (string.CompareOrdinal(file.Path, FlagList.Path) != 0)
            {
                files.Add(file);
            }
        }

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Contains(FlagList.Path, error.Message, StringComparison.Ordinal);
    }

    /// <summary>The text of one story scene file with one line, for a test of the content set (D-173).</summary>
    private static ContentFile SceneFile(string line) =>
        File(
            "rules/scenes/note.json",
            $$"""
            {
             "comment": "a note",
             "id": "scene.test_note",
             "steps": [ { "kind": "say", "speaker": "none", "line": "{{line}}" } ]
            }
            """);

    /// <summary>The text of one small map file, for a test of the content set (D-528).</summary>
    private static ContentFile MapFile(string path, string id, string label) =>
        File(
            path,
            $$"""
            {
             "comment": "a note",
             "id": "{{id}}",
             "region": "region.test",
             "label": "{{label}}",
             "time": "day",
             "dark": false,
             "terrain": [ "###", "#.#", "###" ],
             "things": [
              { "id": "spawn_point.one_start", "kind": "spawn_point", "x": 1, "y": 1 }
             ],
             "enemies": [], "triggers": []
            }
            """);

    private static IReadOnlyList<ContentFile> Files(params ContentFile[] rules)
    {
        List<ContentFile> files =
        [
            File(Palette.Path, PaletteBody),
            File(StringTable.Path, StringsBody),
            File(AtlasIndex.Path, AtlasBody),
        ];

        files.AddRange(UiContentFixtures.Files());
        files.AddRange(TestBattles.Files());
        files.AddRange(rules);
        return EffectFixtures.WithMapsOf(files);
    }
}
