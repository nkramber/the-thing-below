using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
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
         "colors": [ { "index": 0, "key": "k", "hex": "0b0a0f", "name": "ink" } ]
        }
        """;

    private const string AtlasBody =
        """
        {
         "comment": "a test index",
         "pages": [
         ],
         "drawings": [
         ]
        }
        """;

    private const string StringsBody =
        """
        {
         "comment": "a test table",
         "strings": [ { "id": "label.lamp", "text": "Tin lamp" } ]
        }
        """;

    [Fact]
    public void AWellFormedSetLoads()
    {
        ContentSet set = ContentSet.Load(Files(Rule("rules/a.json", "fixture.lamp", "label.lamp")));

        Assert.Single(set.Palette.Colors);
        Assert.Equal(1, set.Strings.Count);
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

    private static IReadOnlyList<ContentFile> Files(params ContentFile[] rules)
    {
        List<ContentFile> files =
        [
            File(Palette.Path, PaletteBody),
            File(StringTable.Path, StringsBody),
            File(AtlasIndex.Path, AtlasBody),
        ];

        files.AddRange(rules);
        return files;
    }
}
