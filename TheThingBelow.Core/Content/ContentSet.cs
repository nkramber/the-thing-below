using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Content;

/// <summary>
/// Every content file of one build, read and checked. The load fails on the first error, and
/// the error names the file and the field (G-6, T-2).
/// </summary>
/// <remarks>
/// Core reads no folder and no resource (G-1). The host gives the bytes: Game from the
/// resources of its own assembly, and Tools from the `content/` folder (D-508).
/// <para>
/// The load runs the rules that span files. Every file has a record (D-517). No two rule
/// entries take one id (D-166). Every string id that a rule names is in the table (G-7). The
/// content hash covers the rule files alone (D-495, D-648). Every palette key of a drawing
/// is in the palette, and the atlas index matches the drawing files (D-666, F-20).
/// Each piece of a large picture is a drawing of the `pieces` page, and each copy starts
/// inside its picture (D-817, D-818).
/// </para>
/// <para>
/// The load also checks the UI base. Both fonts carry a bitmap of each body size, and the style
/// file names a drawing and a palette key that exist (D-527, D-707, D-710).
/// </para>
/// </remarks>
public sealed class ContentSet
{
    private readonly SortedDictionary<string, RuleFixtureEntry> ruleEntries;
    private readonly SortedDictionary<string, GameMap> maps;
    private readonly SortedDictionary<string, Drawing> drawings;
    private readonly SortedDictionary<string, LargePicture> pictures;
    private readonly SortedDictionary<string, FontStrikes> fonts;

    private ContentSet(
        Palette palette,
        StringTable strings,
        AtlasIndex atlas,
        UiStyle style,
        BattleContent battle,
        NoticeList notices,
        StoryContent story,
        LightContent light,
        EffectContent effects,
        SortedDictionary<string, RuleFixtureEntry> ruleEntries,
        SortedDictionary<string, GameMap> maps,
        SortedDictionary<string, Drawing> drawings,
        SortedDictionary<string, LargePicture> pictures,
        SortedDictionary<string, FontStrikes> fonts,
        string hash)
    {
        this.Palette = palette;
        this.Strings = strings;
        this.Atlas = atlas;
        this.Style = style;
        this.Battle = battle;
        this.Notices = notices;
        this.Story = story;
        this.Light = light;
        this.Effects = effects;
        this.ruleEntries = ruleEntries;
        this.maps = maps;
        this.drawings = drawings;
        this.pictures = pictures;
        this.fonts = fonts;
        this.Hash = hash;
    }

    /// <summary>The palette of the game (D-89, D-121).</summary>
    public Palette Palette { get; }

    /// <summary>The table of every string that the player reads (G-7).</summary>
    public StringTable Strings { get; }

    /// <summary>The place of every frame in the atlas (D-666).</summary>
    public AtlasIndex Atlas { get; }

    /// <summary>The look of every menu, panel, and label (D-527).</summary>
    public UiStyle Style { get; }

    /// <summary>The battle rules and the battle fixture (D-757, D-766).</summary>
    public BattleContent Battle { get; }

    /// <summary>The notice file, which the rule of a notice reads (D-983, D-989).</summary>
    public NoticeList Notices { get; }

    /// <summary>The flag file and every story scene of this build (D-542, D-1003).</summary>
    public StoryContent Story { get; }

    /// <summary>The decor, the light setups, the carried light, and the effect budget (D-523, D-843, D-844, D-847).</summary>
    public LightContent Light { get; }

    /// <summary>The battle file and the hit files (D-182, D-879, D-883).</summary>
    public EffectContent Effects { get; }

    /// <summary>The hash of the rule files, as 64 lowercase hexadecimal characters (G-5).</summary>
    public string Hash { get; }

    /// <summary>Every rule entry, in ordinal order of its id (F-39).</summary>
    public IEnumerable<RuleFixtureEntry> RuleEntries => this.ruleEntries.Values;

    /// <summary>Every map of the game, in the order of its id (D-528, G-4).</summary>
    public IEnumerable<GameMap> Maps => this.maps.Values;

    /// <summary>Every drawing, in ordinal order of its id (F-39).</summary>
    public IEnumerable<Drawing> Drawings => this.drawings.Values;

    /// <summary>Every large picture, in the order of its id (D-516).</summary>
    public IEnumerable<LargePicture> Pictures => this.pictures.Values;

    /// <summary>The bitmap sizes of the body font (D-263, D-710).</summary>
    public FontStrikes BodyFont => this.FontOf(FontStrikes.BodyPath);

    /// <summary>The bitmap sizes of the title font (D-264, D-710).</summary>
    public FontStrikes TitleFont => this.FontOf(FontStrikes.TitlePath);

    /// <summary>Reads and checks every content file of one build.</summary>
    /// <param name="files">Every file of `content/`, in any order.</param>
    /// <returns>The content set.</returns>
    /// <exception cref="ContentException">
    /// A file breaks a rule of the reader, a file has no record, two rule entries take one
    /// id, or a rule names a string id that the table lacks (T-2).
    /// </exception>
    public static ContentSet Load(IReadOnlyList<ContentFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        Palette? palette = null;
        StringTable? strings = null;
        AtlasIndex? atlas = null;
        UiStyle? style = null;
        BattleRules? battleRules = null;
        BattleFixture? battleFixture = null;
        AbilityList? abilities = null;
        LessonList? lessons = null;
        NoticeList? notices = null;
        FlagList? flags = null;
        List<StoryScene> scenes = [];
        List<EnemyRecord> enemies = [];
        List<GroupFile> groups = [];
        List<ProfileRecord> profiles = [];
        var fonts = new SortedDictionary<string, FontStrikes>(StringComparer.Ordinal);
        var ruleEntries = new SortedDictionary<string, RuleFixtureEntry>(StringComparer.Ordinal);
        var maps = new SortedDictionary<string, GameMap>(StringComparer.Ordinal);
        var sources = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var drawings = new SortedDictionary<string, Drawing>(StringComparer.Ordinal);
        var pictures = new SortedDictionary<string, LargePicture>(StringComparer.Ordinal);
        var pageFiles = new SortedSet<string>(StringComparer.Ordinal);
        var normalPageFiles = new SortedSet<string>(StringComparer.Ordinal);
        List<NormalOverride> overrides = [];
        List<ContentFile> lightFiles = [];
        List<ContentFile> effectFiles = [];
        var paths = new SortedSet<string>(StringComparer.Ordinal);

        foreach (ContentFile file in Ordered(files))
        {
            // A second file of one path would replace the first in silence (T-2).
            if (!paths.Add(file.Path))
            {
                throw ContentException.ForFile(file.Path, "the content set holds this path two times, and one path names one file");
            }

            if (string.CompareOrdinal(file.Path, Palette.Path) == 0)
            {
                palette = Palette.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, StringTable.Path) == 0)
            {
                strings = StringTable.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, AtlasIndex.Path) == 0)
            {
                atlas = AtlasIndex.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, UiStyle.Path) == 0)
            {
                style = UiStyle.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, BattleRules.Path) == 0)
            {
                battleRules = BattleRules.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, BattleFixture.Path) == 0)
            {
                // The fixture lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-766).
                battleFixture = BattleFixture.Read(file.Bytes, file.Path);
                AddIds(file.Path, battleFixture.DefinedIds(), sources);
            }
            else if (string.CompareOrdinal(file.Path, AbilityList.Path) == 0)
            {
                abilities = AbilityList.Read(file.Bytes, file.Path);
                AddIds(file.Path, abilities.Ids, sources);
            }
            else if (string.CompareOrdinal(file.Path, LessonList.Path) == 0)
            {
                // The lesson file lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-1026).
                lessons = LessonList.Read(file.Bytes, file.Path);
                AddIds(file.Path, lessons.Ids, sources);
            }
            else if (string.CompareOrdinal(file.Path, NoticeList.Path) == 0)
            {
                // The notice file lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-989).
                notices = NoticeList.Read(file.Bytes, file.Path);
                AddIds(file.Path, notices.Ids, sources);
            }
            else if (string.CompareOrdinal(file.Path, FlagList.Path) == 0)
            {
                // The flag file lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-1003).
                flags = FlagList.Read(file.Bytes, file.Path);
                AddIds(file.Path, flags.Ids(), sources);
            }
            else if (StoryScene.IsSceneFile(file.Path))
            {
                // A story scene file lies under the rule folder, so this branch comes before
                // the branch of the rule fixtures below (D-173).
                StoryScene scene = StoryScene.Read(file.Bytes, file.Path);
                AddIds(file.Path, [scene.Id], sources);
                scenes.Add(scene);
            }
            else if (EnemyRecord.IsEnemyFile(file.Path))
            {
                // An enemy file lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-786).
                EnemyRecord enemy = EnemyRecord.Read(file.Bytes, file.Path);
                AddIds(file.Path, [enemy.Id], sources);
                enemies.Add(enemy);
            }
            else if (GroupFile.IsGroupFile(file.Path))
            {
                // A group file lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-957).
                GroupFile groupFile = GroupFile.Read(file.Bytes, file.Path);
                AddIds(file.Path, groupFile.DefinedIds(), sources);
                groups.Add(groupFile);
            }
            else if (ProfileRecord.IsProfileFile(file.Path))
            {
                // A profile file lies under the rule folder, so this branch comes before the
                // branch of the rule fixtures below (D-956).
                ProfileRecord profile = ProfileRecord.Read(file.Bytes, file.Path);
                AddIds(file.Path, [profile.Id], sources);
                profiles.Add(profile);
            }
            else if (ContentPaths.IsFontFile(file.Path))
            {
                // A font carries its glyph bitmaps, so no JSON record holds it. The reader
                // gives the sizes that the file draws without the traced outline (D-710).
                fonts.Add(file.Path, FontStrikes.Read(file.Bytes, file.Path));
            }
            else if (Drawing.IsDrawingFile(file.Path))
            {
                AddDrawing(file, drawings, sources);
            }
            else if (LargePicture.IsPictureFile(file.Path))
            {
                AddPicture(file, pictures, sources);
            }
            else if (ContentPaths.IsAtlasPage(file.Path))
            {
                // A page is an image, and the atlas index holds its record (D-517, D-666).
                pageFiles.Add(file.Path);
            }
            else if (ContentPaths.IsNormalPage(file.Path))
            {
                // A normal-map page takes the place of each frame from the atlas index of the
                // color atlas (D-184, D-517).
                normalPageFiles.Add(file.Path);
            }
            else if (NormalOverride.IsOverrideFile(file.Path))
            {
                // Each grid needs its drawing, so the check runs after the loop (D-839).
                overrides.Add(NormalOverride.Read(file.Bytes, file.Path));
            }
            else if (LightContent.IsLightFile(file.Path))
            {
                // A light file needs the maps, the palette, and the atlas, so the reader of the
                // light files runs after the loop (D-843, D-844).
                lightFiles.Add(file);
            }
            else if (EffectContent.IsEffectFile(file.Path))
            {
                // An effect file needs the fight, the palette, and the budget, so the reader of
                // the effect files runs after the loop (D-523, D-879).
                effectFiles.Add(file);
            }
            else if (GameMap.IsMapFile(file.Path))
            {
                // A map lies under the rule folder, so this branch comes before the fixture
                // branch below (D-495, D-528).
                AddMapFile(file, maps, sources);
            }
            else if (ContentPaths.IsRuleFile(file.Path))
            {
                AddRuleFile(file, ruleEntries, sources);
            }
            else
            {
                throw ContentException.ForFile(
                    file.Path,
                    "no record of Core reads this file, and every content file needs one (D-517)");
            }
        }

        Palette readPalette = palette ?? throw AbsentFile(Palette.Path);
        AtlasIndex readAtlas = atlas ?? throw AbsentFile(AtlasIndex.Path);
        StringTable readStrings = strings ?? throw AbsentFile(StringTable.Path);
        UiStyle readStyle = style ?? throw AbsentFile(UiStyle.Path);
        var battle = new BattleContent(
            battleRules ?? throw AbsentFile(BattleRules.Path),
            battleFixture ?? throw AbsentFile(BattleFixture.Path),
            enemies,
            abilities ?? throw AbsentFile(AbilityList.Path),
            lessons ?? throw AbsentFile(LessonList.Path),
            groups,
            profiles);

        // Each map names a group of its region. The check runs before the effect files, whose
        // ambient check reads the enemies of each group that a map names (D-957).
        foreach (GameMap map in maps.Values)
        {
            battle.RequireGroupsOf(map);
        }

        // Each trigger of a map names a story scene, its flags, and its markers (D-1004).
        StoryContent story = StoryContent.Load(flags ?? throw AbsentFile(FlagList.Path), scenes, battle);
        foreach (GameMap map in maps.Values)
        {
            story.RequireScenesOf(map);
        }

        LightContent light = LightContent.Load(lightFiles, maps, readPalette, readAtlas);
        var set = new ContentSet(
            readPalette,
            readStrings,
            readAtlas,
            readStyle,
            battle,
            notices ?? throw AbsentFile(NoticeList.Path),
            story,
            light,
            EffectContent.Load(effectFiles, new AmbientWorld(maps, battle, light, drawings, readPalette)),
            ruleEntries,
            maps,
            drawings,
            pictures,
            fonts,
            ContentHash.Compute(files));

        set.RefuseAbsentString(sources);
        set.RefuseAbsentColor();
        set.RefuseStaleAtlas(pageFiles);
        set.RefuseStaleNormalPages(normalPageFiles);
        set.RefuseWrongOverride(overrides);
        set.RefuseAbsentFont();
        set.RefuseAbsentStyleDrawing();
        set.RefuseWrongPiece();
        return set;
    }

    /// <summary>Gives a rule entry by its id.</summary>
    /// <param name="id">The content id of the entry.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="ContentException">The set holds no entry with that id (T-2).</exception>
    public RuleFixtureEntry RuleEntry(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.ruleEntries.TryGetValue(id.Value, out RuleFixtureEntry? entry))
        {
            throw ContentException.ForField(
                ContentPaths.RuleFolder,
                id.Value,
                "the content set holds no rule entry with this id");
        }

        return entry;
    }

    /// <summary>Gives one map by its id (D-528).</summary>
    /// <param name="id">The content id of the map, such as `map.fixture_dungeon`.</param>
    /// <returns>The map.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    /// <exception cref="ContentException">The set holds no map with that id (T-2).</exception>
    /// <remarks>
    /// A snapshot holds the id of the map and never its content, so a load reads the map of
    /// this build through this method (D-166, D-651).
    /// </remarks>
    public GameMap Map(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.maps.TryGetValue(id.Value, out GameMap? map))
        {
            throw ContentException.ForField(
                GameMap.Folder,
                id.Value,
                "the content set holds no map with this id");
        }

        return map;
    }

    private static IEnumerable<ContentFile> Ordered(IReadOnlyList<ContentFile> files)
    {
        // An ordinal order, so the first error of a broken set is the same on every platform
        // and every host (G-4, F-39, T-2).
        // The list takes the files one at a time. A spread element makes the compiler emit a
        // call of `System.Linq`, and G-1 keeps that assembly out of the reference list of Core.
        List<ContentFile> ordered = new(files.Count);
        foreach (ContentFile file in files)
        {
            ordered.Add(file);
        }

        ordered.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        return ordered;
    }

    /// <summary>Gives one drawing by its id.</summary>
    /// <param name="id">The id of the drawing, such as `drawing.marrek_map_front`.</param>
    /// <returns>The drawing.</returns>
    /// <exception cref="ContentException">The set holds no drawing with that id (T-2).</exception>
    public Drawing DrawingOf(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.drawings.TryGetValue(id.Value, out Drawing? drawing))
        {
            throw ContentException.ForField(Drawing.Folder, id.Value, "the content set holds no drawing with this id");
        }

        return drawing;
    }

    /// <summary>Gives one large picture by its id.</summary>
    /// <param name="id">The id of the picture, such as `picture.fixture_backdrop`.</param>
    /// <returns>The picture.</returns>
    /// <exception cref="ContentException">The set holds no picture with that id (T-2).</exception>
    public LargePicture PictureOf(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.pictures.TryGetValue(id.Value, out LargePicture? picture))
        {
            throw ContentException.ForField(LargePicture.Folder, id.Value, "the content set holds no large picture with this id");
        }

        return picture;
    }

    private static void AddPicture(
        ContentFile file,
        SortedDictionary<string, LargePicture> pictures,
        SortedDictionary<string, string> sources)
    {
        LargePicture picture = LargePicture.Read(file.Bytes, file.Path);
        if (sources.TryGetValue(picture.Id.Value, out string? first))
        {
            throw ContentException.ForField(
                file.Path,
                picture.Id.Value,
                $"the content id '{picture.Id.Value}' is already the id of an entry of '{first}', and an id is permanent (D-166)");
        }

        pictures.Add(picture.Id.Value, picture);
        sources.Add(picture.Id.Value, file.Path);
    }

    private static void AddDrawing(
        ContentFile file,
        SortedDictionary<string, Drawing> drawings,
        SortedDictionary<string, string> sources)
    {
        Drawing drawing = Drawing.Read(file.Bytes, file.Path);
        if (sources.TryGetValue(drawing.Id.Value, out string? first))
        {
            throw ContentException.ForField(
                file.Path,
                drawing.Id.Value,
                $"the content id '{drawing.Id.Value}' is already the id of an entry of '{first}', and an id is permanent (D-166)");
        }

        drawings.Add(drawing.Id.Value, drawing);
        sources.Add(drawing.Id.Value, file.Path);
    }

    /// <summary>
    /// Reads one map file, and records the id of the map and the id of each of its things.
    /// An id is permanent, so no two entries of the content take one (D-166, D-528).
    /// </summary>
    private static void AddMapFile(
        ContentFile file,
        SortedDictionary<string, GameMap> maps,
        SortedDictionary<string, string> sources)
    {
        GameMap map = GameMap.Read(file.Bytes, file.Path);
        RefuseTakenId(file.Path, map.Id, sources);
        maps.Add(map.Id.Value, map);
        sources.Add(map.Id.Value, file.Path);

        foreach (MapThing thing in map.Things)
        {
            RefuseTakenId(file.Path, thing.Id, sources);
            sources.Add(thing.Id.Value, file.Path);
        }
    }

    private static void RefuseTakenId(string path, ContentId id, SortedDictionary<string, string> sources)
    {
        if (sources.TryGetValue(id.Value, out string? first))
        {
            throw ContentException.ForField(
                path,
                id.Value,
                $"the content id '{id.Value}' is already the id of an entry of '{first}', and an id is permanent (D-166)");
        }
    }

    private static void AddIds(string path, IReadOnlyList<ContentId> ids, SortedDictionary<string, string> sources)
    {
        foreach (ContentId id in ids)
        {
            RefuseTakenId(path, id, sources);
            sources.Add(id.Value, path);
        }
    }

    private static void AddRuleFile(
        ContentFile file,
        SortedDictionary<string, RuleFixtureEntry> ruleEntries,
        SortedDictionary<string, string> sources)
    {
        RuleFixture fixture = RuleFixture.Read(file.Bytes, file.Path);
        foreach (RuleFixtureEntry entry in fixture.Entries)
        {
            if (sources.TryGetValue(entry.Id.Value, out string? first))
            {
                throw ContentException.ForField(
                    file.Path,
                    entry.Id.Value,
                    $"the content id '{entry.Id.Value}' is already the id of an entry of '{first}', and an id is permanent (D-166)");
            }

            ruleEntries.Add(entry.Id.Value, entry);
            sources.Add(entry.Id.Value, file.Path);
        }
    }

    /// <summary>Gives the bitmap sizes of one font file.</summary>
    /// <param name="path">The path of the font, under `content/`.</param>
    /// <returns>The sizes of that file.</returns>
    /// <exception cref="ContentException">The set holds no such font (T-2).</exception>
    public FontStrikes FontOf(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (!this.fonts.TryGetValue(path, out FontStrikes? strikes))
        {
            throw AbsentFile(path);
        }

        return strikes;
    }

    private static ContentException AbsentFile(string path) =>
        ContentException.ForFile(path, "the content set holds no such file");

    /// <summary>
    /// Refuses a build with no body font or no title font. Every screen draws through those
    /// two files, so a build without one draws nothing (D-263, D-264, T-2).
    /// </summary>
    private void RefuseAbsentFont()
    {
        if (!this.fonts.ContainsKey(FontStrikes.BodyPath))
        {
            throw AbsentFile(FontStrikes.BodyPath);
        }

        if (!this.fonts.ContainsKey(FontStrikes.TitlePath))
        {
            throw AbsentFile(FontStrikes.TitlePath);
        }

        // The title is twice the body, and the largest bitmap of the file must reach the
        // larger body size. A title above the largest bitmap doubles a smaller one (D-707).
        this.BodyFont.RequireSize(this.Style.SmallBody);
        this.BodyFont.RequireSize(this.Style.LargeBody);
        this.TitleFont.RequireSize(this.Style.SmallBody);
        this.TitleFont.RequireSize(this.Style.LargeBody);
    }

    /// <summary>
    /// Refuses an entry of a large picture whose piece is absent, lies on another page, or
    /// moves, and an entry whose last copy starts outside the picture (D-817, D-818, T-2).
    /// </summary>
    private void RefuseWrongPiece()
    {
        foreach (LargePicture picture in this.pictures.Values)
        {
            for (int index = 0; index < picture.Places.Count; index += 1)
            {
                PicturePlace place = picture.Places[index];
                string field = $"places[{index}]";
                if (!this.drawings.TryGetValue(place.Piece.Value, out Drawing? piece))
                {
                    throw ContentException.ForField(
                        picture.File, field, $"the entry names the piece '{place.Piece.Value}', and no drawing file holds it (D-516)");
                }

                if (piece.Page != AtlasPageKind.Pieces || piece.Frames.Count != 1)
                {
                    throw ContentException.ForField(
                        picture.File,
                        field,
                        $"the piece '{piece.Id.Value}' lies on the page '{AtlasPages.NameOf(piece.Page)}' with {piece.Frames.Count} frames, and a piece lies on the page 'pieces' with one frame (D-818)");
                }

                // A copy that starts outside the picture draws nothing, so the last copy of
                // each row and each column starts inside it (D-817, T-2).
                long lastX = place.X + ((long)(place.Across - 1) * piece.Width);
                long lastY = place.Y + ((long)(place.Down - 1) * piece.Height);
                if (lastX >= picture.Width || lastY >= picture.Height)
                {
                    throw ContentException.ForField(
                        picture.File,
                        field,
                        $"the last copy of '{piece.Id.Value}' starts at {lastX},{lastY}, outside the picture of {picture.Width} by {picture.Height} (D-817)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses a style file that names a window frame which no drawing file holds (D-527,
    /// T-2).
    /// </summary>
    private void RefuseAbsentStyleDrawing()
    {
        foreach (UiFrame frame in this.Style.Frames)
        {
            if (!this.drawings.ContainsKey(frame.Drawing.Value))
            {
                throw ContentException.ForField(
                    UiStyle.Path,
                    frame.Role,
                    $"the style names the drawing '{frame.Drawing.Value}', and no drawing file holds it (D-527)");
            }
        }

        foreach (UiColor color in this.Style.Colors)
        {
            if (!this.Palette.TryColorOf(color.KeyCharacter, out _))
            {
                throw ContentException.ForField(
                    UiStyle.Path,
                    color.Role,
                    $"the style names the palette key '{color.Key}', and the palette has no such color (D-181)");
            }
        }
    }

    /// <summary>
    /// Refuses a drawing that names a key which the palette lacks. The message names the
    /// file, the frame, the row, and the column, so a session finds the pixel (F-20, T-2).
    /// </summary>
    private void RefuseAbsentColor()
    {
        foreach (Drawing drawing in this.drawings.Values)
        {
            for (int frame = 0; frame < drawing.Frames.Count; frame += 1)
            {
                IReadOnlyList<string> rows = drawing.Frames[frame].Rows;
                for (int row = 0; row < rows.Count; row += 1)
                {
                    this.RefuseAbsentColorInRow(drawing.File, frame, row, rows[row]);
                }
            }
        }
    }

    private void RefuseAbsentColorInRow(string file, int frame, int row, string keys)
    {
        for (int column = 0; column < keys.Length; column += 1)
        {
            char key = keys[column];
            if (key == Drawing.Transparent || this.Palette.TryColorOf(key, out _))
            {
                continue;
            }

            throw ContentException.ForField(
                file,
                $"frames[{frame}].rows[{row}]",
                $"column {column} holds the key '{key}', and the palette has no such color (F-20)");
        }
    }

    /// <summary>
    /// Refuses an atlas index that does not match the drawing files or the page files. A
    /// stale atlas fails here, and the pixel test of Tools compares the pages themselves
    /// (F-19, G-24).
    /// </summary>
    private void RefuseStaleAtlas(SortedSet<string> pageFiles)
    {
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (AtlasPage page in this.Atlas.Pages)
        {
            named.Add(page.File);
            if (!pageFiles.Contains(page.File))
            {
                throw ContentException.ForField(
                    AtlasIndex.Path,
                    page.Name,
                    $"the index names the page '{page.Name}', and the content set holds no file '{page.File}'");
            }
        }

        foreach (string pageFile in pageFiles)
        {
            // The atlas command owns every page file, so a page that no index names is a
            // leftover that would ship in the build (D-666, T-2).
            if (!named.Contains(pageFile))
            {
                throw ContentException.ForFile(
                    pageFile,
                    "no page of the atlas index names this file. Run the atlas command again (D-666)");
            }
        }

        foreach (AtlasEntry entry in this.Atlas.Entries)
        {
            if (!this.drawings.ContainsKey(entry.Id.Value))
            {
                throw ContentException.ForField(
                    AtlasIndex.Path,
                    entry.Id.Value,
                    "the index holds a drawing that no file of the drawing folder holds. Run the atlas command again");
            }
        }

        foreach (Drawing drawing in this.drawings.Values)
        {
            this.RefuseStaleEntry(drawing);
        }
    }

    /// <summary>
    /// Refuses a normal-map atlas that does not match the color atlas: a page of a kind that
    /// takes scene light with no normal page, or a normal page that no such page names
    /// (D-184, D-210). The pixel test of Tools compares the pages themselves (F-19).
    /// </summary>
    private void RefuseStaleNormalPages(SortedSet<string> normalPageFiles)
    {
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (AtlasPage page in this.Atlas.Pages)
        {
            if (!AtlasPages.TakesLight(page.Kind))
            {
                continue;
            }

            named.Add(page.NormalFile);
            if (!normalPageFiles.Contains(page.NormalFile))
            {
                throw ContentException.ForField(
                    AtlasIndex.Path,
                    page.Name,
                    $"the page '{page.Name}' takes scene light, and the content set holds no normal map '{page.NormalFile}'. Run the atlas command again (D-184)");
            }
        }

        foreach (string normalFile in normalPageFiles)
        {
            // The atlas command owns every normal page, so a page that no color page names
            // is a leftover that would ship in the build (D-666, T-2).
            if (!named.Contains(normalFile))
            {
                throw ContentException.ForFile(
                    normalFile,
                    "no page of the atlas index that takes scene light names this normal map. Run the atlas command again (D-184, D-210)");
            }
        }
    }

    /// <summary>
    /// Refuses an override grid that names no drawing, a second grid of one drawing, or a
    /// grid that does not fit its drawing (D-839, T-2).
    /// </summary>
    private void RefuseWrongOverride(List<NormalOverride> overrides)
    {
        var seen = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (NormalOverride grid in overrides)
        {
            if (!this.drawings.TryGetValue(grid.Drawing.Value, out Drawing? drawing))
            {
                throw ContentException.ForField(
                    grid.File,
                    "drawing",
                    $"the grid names the drawing '{grid.Drawing.Value}', and no drawing file holds it");
            }

            if (!seen.TryAdd(grid.Drawing.Value, grid.File))
            {
                throw ContentException.ForField(
                    grid.File,
                    "drawing",
                    $"the file '{seen[grid.Drawing.Value]}' already holds the grid of '{grid.Drawing.Value}', and a drawing takes one grid (D-839)");
            }

            grid.RefuseWrongShape(drawing);
        }
    }

    private void RefuseStaleEntry(Drawing drawing)
    {
        string file = drawing.File;
        AtlasEntry entry = this.Atlas.Entry(drawing.Id);
        if (!this.Atlas.TryPage(entry.Page, out AtlasPage? page) || page.Kind != drawing.Page)
        {
            throw StaleAtlas(file, drawing, $"the index puts it on the page '{entry.Page}'");
        }

        if (entry.Width != drawing.Width || entry.Height != drawing.Height)
        {
            throw StaleAtlas(
                file,
                drawing,
                $"the index holds {entry.Width} by {entry.Height} pixels, and the file holds {drawing.Width} by {drawing.Height}");
        }

        if (entry.Frames.Count != drawing.Frames.Count)
        {
            throw StaleAtlas(
                file,
                drawing,
                $"the index holds {entry.Frames.Count} frames, and the file holds {drawing.Frames.Count}");
        }

        for (int frame = 0; frame < entry.Frames.Count; frame += 1)
        {
            int ticks = entry.Frames[frame].Ticks;
            if (ticks != drawing.Frames[frame].Ticks)
            {
                throw StaleAtlas(
                    file,
                    drawing,
                    $"frame {frame} holds {ticks} ticks in the index, and {drawing.Frames[frame].Ticks} in the file");
            }
        }

        // Game finds a drawing through the uses of the index, so a stale use list would
        // answer a lookup with the wrong drawing (D-519).
        if (!SameDraws(entry.Draws, drawing.Draws))
        {
            throw StaleAtlas(file, drawing, "the index names other things that the drawing draws");
        }
    }

    private static bool SameDraws(IReadOnlyList<DrawingUse> index, IReadOnlyList<DrawingUse> file)
    {
        if (index.Count != file.Count)
        {
            return false;
        }

        for (int use = 0; use < index.Count; use += 1)
        {
            // A content id compares by its text, and never by its instance (F-39).
            if (string.CompareOrdinal(index[use].Content.Value, file[use].Content.Value) != 0
                || string.CompareOrdinal(index[use].Use, file[use].Use) != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static ContentException StaleAtlas(string file, Drawing drawing, string difference) =>
        ContentException.ForField(
            file,
            drawing.Id.Value,
            $"the atlas index does not match this drawing, because {difference}. Run the atlas command again (G-24)");

    private void RequireString(string file, string field, ContentId id)
    {
        if (!this.Strings.Contains(id))
        {
            throw ContentException.ForField(file, field, $"the string table holds no id '{id.Value}' (G-7)");
        }
    }

    private void RefuseAbsentString(SortedDictionary<string, string> sources)
    {
        // Each line of a say step and each option of a choose step lives in the string table (G-7).
        this.Story.RequireStrings(this.Strings);

        foreach (GameMap map in this.maps.Values)
        {
            if (!this.Strings.Contains(map.Label))
            {
                throw ContentException.ForField(
                    map.File,
                    $"{map.Id.Value}.label",
                    $"the string table holds no id '{map.Label.Value}' (G-7)");
            }
        }

        // The line of a notice lives in the string table under the id of the notice (G-7, D-989).
        foreach (NoticeRecord notice in this.Notices.Records)
        {
            if (!this.Strings.Contains(notice.Id))
            {
                throw ContentException.ForField(
                    this.Notices.File,
                    notice.Id.Value,
                    $"the string table holds no id '{notice.Id.Value}', which holds the line of the notice (G-7)");
            }
        }

        // The name of each lesson, and the name and the description of each form (G-7, D-1027).
        LessonList lessons = this.Battle.Lessons;
        foreach (LessonRecord lesson in lessons.Records)
        {
            this.RequireString(lessons.File, $"{lesson.Id.Value}.name", lesson.Name);
            foreach (LessonForm form in lesson.Forms)
            {
                this.RequireString(lessons.File, $"{lesson.Id.Value}.{form.Ability.Value}.name", form.Name);
                this.RequireString(lessons.File, $"{lesson.Id.Value}.{form.Ability.Value}.description", form.Description);
            }
        }

        foreach (RuleFixtureEntry entry in this.ruleEntries.Values)
        {
            if (!this.Strings.Contains(entry.Label))
            {
                throw ContentException.ForField(
                    sources[entry.Id.Value],
                    $"{entry.Id.Value}.label",
                    $"the string table holds no id '{entry.Label.Value}' (G-7)");
            }
        }
    }
}
