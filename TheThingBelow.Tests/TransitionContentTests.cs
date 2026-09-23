using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The transition library and the transition table, checked across files, and the pick of the
/// transition of each fight (D-195, D-196, D-934 to D-941).
/// </summary>
public sealed class TransitionContentTests
{
    private const string LitMap = "\"" + LightFixtures.MapId + "\"";

    private static readonly ContentId Map = ContentId.Parse(LightFixtures.MapId, "test", "map");

    [Fact]
    public void TheCheckoutHoldsTheTenTransitionsAndTheTableOfTheOwner()
    {
        // D-194, D-195, D-938, D-940, D-941: the checkout holds the library, the picks of the
        // fixed kinds, the pool of region one with snow whiteout, and the lengths.
        TransitionContent content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())).Effects.Transitions;

        Assert.Equal(10, content.Transitions.Count);
        foreach (Transition transition in content.Transitions)
        {
            Assert.Equal(60, transition.Ticks);
            Assert.Equal(transition.Look == TransitionLook.SnowWhiteout ? 'e' : 'k', transition.Cover);
        }

        Assert.Equal(20, content.Table.FadeTicks);
        Assert.Equal('k', content.Table.BackCover);
        Assert.Equal("transition.color_split", content.Table.FixedOf(EncounterKind.Ambush).Value);
        Assert.Equal("transition.swirl", content.Table.FixedOf(EncounterKind.Elite).Value);
        Assert.Equal("transition.shatter", content.Table.FixedOf(EncounterKind.Boss).Value);
        Assert.Equal("transition.ripple", content.Table.FixedOf(EncounterKind.WrongThing).Value);

        TransitionRegion one = Assert.Single(content.Table.Regions);
        Assert.Equal("region.one", one.Id.Value);
        Assert.Equal("map.fixture_dungeon", Assert.Single(one.Maps).Value);
        Assert.Equal(
            ["transition.pixel_dissolve", "transition.mosaic", "transition.crt_power_off", "transition.snow_whiteout", "transition.blinds", "transition.scanline_sweep"],
            Ids(one.Pool));
    }

    [Fact]
    public void ATableThatNamesAnAbsentKindFailsWithTheFileAndTheKind()
    {
        // Exit test 3 of PR-60: a kind that no encounter takes fails, and the error names the kind.
        string kinds = EffectFixtures.Kinds.Replace("\"wrong_thing\"", "\"wrong_things\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(kinds: kinds, maps: LitMap)));

        Assert.Equal(TransitionTable.Path, error.File);
        Assert.Contains("the kind 'wrong_things'", error.Message, StringComparison.Ordinal);
        Assert.Contains(EncounterKinds.FixedNames, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATableThatGivesTheCommonKindOneTransitionFails()
    {
        // D-934: a common fight takes a transition from the pool of its region.
        string kinds = EffectFixtures.Kinds.Replace("{ ", "{ \"common\": \"transition.blinds\", ", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(kinds: kinds, maps: LitMap)));

        Assert.Contains("the kind 'common'", error.Message, StringComparison.Ordinal);
        Assert.Contains("pool of its region", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATableWithNoTransitionForAFixedKindFails()
    {
        string kinds = EffectFixtures.Kinds.Replace(", \"boss\": \"transition.shatter\"", string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain("boss", kinds, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(kinds: kinds, maps: LitMap)));

        Assert.Equal("kinds", error.Field);
        Assert.Contains("the kind 'boss'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AKindThatNamesNoTransitionFileFails()
    {
        string kinds = EffectFixtures.Kinds.Replace("transition.ripple", "transition.burn", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(kinds: kinds, maps: LitMap)));

        Assert.Equal("kinds.wrong_thing", error.Field);
        Assert.Contains("transition.burn", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APoolThatHoldsTheTransitionOfAFixedKindFails()
    {
        // D-934: a special kind reads the same in each region, so no pool holds its transition.
        string pool = EffectFixtures.Pool + ", \"transition.swirl\"";

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(maps: LitMap, pool: pool)));

        Assert.Equal(TransitionTable.Path, error.File);
        Assert.Equal("regions[0].pool[6]", error.Field);
        Assert.Contains("the kind 'elite'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APoolOfOneTransitionFails()
    {
        // D-935: the pick skips the last one, so a pool holds two or more.
        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(maps: LitMap, pool: "\"transition.blinds\"")));

        Assert.Equal("regions[0].pool", error.Field);
        Assert.Contains("holds 1 transitions", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapThatNoRegionHoldsFails()
    {
        // D-936: each map of the rules belongs to one region.
        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody()));

        Assert.Equal("regions", error.Field);
        Assert.Contains($"no region holds the map '{LightFixtures.MapId}'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapInTwoRegionsFails()
    {
        string second = $$""", { "region": "region.two", "maps": [{{LitMap}}], "pool": [{{EffectFixtures.Pool}}] }""";

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(maps: LitMap, regions: second)));

        Assert.Equal("regions[1].maps[0]", error.Field);
        Assert.Contains("the region 'region.one' holds the map", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapThatTheRulesDoNotHoldFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(maps: LitMap + ", \"map.absent\"")));

        Assert.Equal("regions[0].maps[1]", error.Field);
        Assert.Contains("map.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARegionNamedTwoTimesFails()
    {
        string second = $$""", { "region": "region.one", "maps": [], "pool": [{{EffectFixtures.Pool}}] }""";

        ContentException error = Assert.Throws<ContentException>(() => Load(table: EffectFixtures.TableBody(maps: LitMap, regions: second)));

        Assert.Equal("regions[1].region", error.Field);
    }

    [Fact]
    public void ALibraryWithNoFileForALookFails()
    {
        // D-195: the library holds ten transitions.
        List<ContentFile> files = Files();
        files.RemoveAll(file => string.CompareOrdinal(file.Path, EffectFixtures.TransitionPath("mosaic")) == 0);

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Equal(Transition.Folder, error.File);
        Assert.Contains("the look 'mosaic'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoFilesOfOneLookFail()
    {
        List<ContentFile> files = Files();
        string twin = EffectFixtures.TransitionBody("mosaic").Replace("transition.mosaic", "transition.mosaic_twin", StringComparison.Ordinal);
        files.Add(EffectFixtures.File($"{Transition.Folder}test-twin.json", twin));

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Equal("look", error.Field);
        Assert.Contains("the look 'mosaic'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACoverThatThePaletteLacksFails()
    {
        List<ContentFile> files = Files();
        int index = files.FindIndex(file => string.CompareOrdinal(file.Path, EffectFixtures.TransitionPath("blinds")) == 0);
        files[index] = EffectFixtures.File(files[index].Path, EffectFixtures.TransitionBody("blinds", cover: "~"));

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Equal(EffectFixtures.TransitionPath("blinds"), error.File);
        Assert.Equal("cover", error.Field);
    }

    [Fact]
    public void AnAbsentTableFails()
    {
        List<ContentFile> files = Files();
        files.RemoveAll(file => string.CompareOrdinal(file.Path, TransitionTable.Path) == 0);

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Equal(TransitionTable.Path, error.File);
    }

    [Fact]
    public void EachFixedKindTakesItsTransitionWhateverTheSeed()
    {
        // D-934, D-940: a special kind looks the same in each fight.
        TransitionContent content = Load();
        for (ulong seed = 0; seed < 50; seed += 1)
        {
            Assert.Equal(TransitionLook.ColorSplit, content.Pick(EncounterKind.Ambush, Map, seed, 100, null).Look);
            Assert.Equal(TransitionLook.Swirl, content.Pick(EncounterKind.Elite, Map, seed, 100, null).Look);
            Assert.Equal(TransitionLook.Shatter, content.Pick(EncounterKind.Boss, Map, seed, 100, null).Look);
            Assert.Equal(TransitionLook.Ripple, content.Pick(EncounterKind.WrongThing, Map, seed, 100, null).Look);
        }
    }

    [Fact]
    public void ACommonPickNeverRepeatsTheLastOne()
    {
        // D-935: a property over seeds and ticks. Each pick lies in the pool, and it skips the
        // transition of the last common fight.
        TransitionContent content = Load();
        TransitionRegion region = Assert.Single(content.Table.Regions);
        for (ulong seed = 0; seed < 200; seed += 1)
        {
            ContentId? last = null;
            for (long tick = 1; tick <= 20; tick += 1)
            {
                Transition picked = content.Pick(EncounterKind.Common, Map, seed, tick * 97, last);
                Assert.True(Ids(region.Pool).Contains(picked.Id.Value), $"seed {seed}, tick {tick * 97}: '{picked.Id.Value}' is outside the pool");
                Assert.True(last is null || string.CompareOrdinal(last.Value, picked.Id.Value) != 0, $"seed {seed}, tick {tick * 97}: the pick repeats '{picked.Id.Value}'");
                last = picked.Id;
            }
        }
    }

    [Fact]
    public void ACommonPickIsTheSameForTheSameSeedAndTick()
    {
        // D-935, T-7: a replay and a screen test show the same pick.
        TransitionContent first = Load();
        TransitionContent second = Load();
        for (ulong seed = 0; seed < 100; seed += 1)
        {
            Assert.Equal(
                first.Pick(EncounterKind.Common, Map, seed, 1234, null).Id.Value,
                second.Pick(EncounterKind.Common, Map, seed, 1234, null).Id.Value);
        }
    }

    [Fact]
    public void CommonPicksReachEveryTransitionOfThePool()
    {
        // D-934: the pool gives variety, so each of its six transitions comes up over the seeds.
        TransitionContent content = Load();
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        for (ulong seed = 0; seed < 200; seed += 1)
        {
            seen.Add(content.Pick(EncounterKind.Common, Map, seed, 600, null).Id.Value);
        }

        Assert.Equal(6, seen.Count);
    }

    private static List<string> Ids(IReadOnlyList<ContentId> ids)
    {
        var values = new List<string>(ids.Count);
        foreach (ContentId id in ids)
        {
            values.Add(id.Value);
        }

        return values;
    }

    private static List<ContentFile> Files(string? table = null)
    {
        var files = new List<ContentFile>(EffectFixtures.TransitionFiles());
        files.Add(EffectFixtures.File(TransitionTable.Path, table ?? EffectFixtures.TableBody(maps: LitMap)));
        return files;
    }

    private static TransitionContent Load(string? table = null) => Load(Files(table));

    private static TransitionContent Load(List<ContentFile> files) =>
        TransitionContent.Load(files, LightFixtures.Maps(), LightFixtures.Palette(), new SortedDictionary<string, string>(StringComparer.Ordinal));
}
