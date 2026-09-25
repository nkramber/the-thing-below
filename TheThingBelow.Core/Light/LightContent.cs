using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Light;

/// <summary>
/// Every light file of one build, read and checked across files: the decor kinds, the shaft
/// kinds, the decor files, the light setups, the carried light, the effect budget, the glow, and
/// the passes of the HD-2D look (D-523, D-843, D-844, D-847, D-910, D-917, D-918).
/// </summary>
/// <remarks>
/// No rule reads a light file, so none lies in the rule folder and none reaches the content
/// hash (D-495). The load still fails on the first error, with the file and the field (T-2).
/// <para>
/// The checks that span files:
/// </para>
/// <list type="bullet">
/// <item>Each map has one decor file and one light setup for its own time of day (D-442).</item>
/// <item>Each decor file and each light setup names a map that exists.</item>
/// <item>Each piece names a kind that exists, and a drawing draws that kind (D-519).</item>
/// <item>Each light shaft names a shaft kind that exists, and a drawing draws the opening of that kind (D-918, D-924).</item>
/// <item>Each piece and each light shaft hangs on a wall with a floor or a doorway to its south (D-844, D-918).</item>
/// <item>Each change names a piece of its map, and each added light lies on its map (D-843).</item>
/// <item>Each color names a key of the palette, the color of each glow, each shaft, and the vignette included (D-846, D-181).</item>
/// <item>Each map keeps inside the effect budget and the limit of Godot (D-842, F-46).</item>
/// <item>The brightest lit art of each map and each fight stays below the glow threshold, so light alone glows (D-910, F-47).</item>
/// <item>Each glow halo stays below the glow threshold at the top of its pulse, so it never draws as a box (D-1075, T-2).</item>
/// </list>
/// </remarks>
public sealed class LightContent
{
    /// <summary>The use that the map drawing of a decor kind serves (D-519).</summary>
    public const string MapUse = "map";

    private readonly SortedDictionary<string, DecorKind> kinds;
    private readonly SortedDictionary<string, ShaftKind> shaftKinds;
    private readonly SortedDictionary<string, DecorFile> decor;
    private readonly SortedDictionary<string, LightSetup> setups;

    private LightContent(
        SortedDictionary<string, DecorKind> kinds,
        SortedDictionary<string, ShaftKind> shaftKinds,
        SortedDictionary<string, DecorFile> decor,
        SortedDictionary<string, LightSetup> setups,
        CarriedLight carried,
        EffectBudget budget,
        Glow glow,
        Hd2dPasses passes)
    {
        this.kinds = kinds;
        this.shaftKinds = shaftKinds;
        this.decor = decor;
        this.setups = setups;
        this.Carried = carried;
        this.Budget = budget;
        this.Glow = glow;
        this.Passes = passes;
    }

    /// <summary>The carried light (D-847).</summary>
    public CarriedLight Carried { get; }

    /// <summary>The effect budget (D-523).</summary>
    public EffectBudget Budget { get; }

    /// <summary>The glow of the world view (D-910).</summary>
    public Glow Glow { get; }

    /// <summary>The tilt-shift blur and the vignette, and the mode of the passes of the HD-2D look (D-917, D-920).</summary>
    public Hd2dPasses Passes { get; }

    /// <summary>Every decor kind, in the order of its id.</summary>
    public IEnumerable<DecorKind> Kinds => this.kinds.Values;

    /// <summary>Tells whether a content path is a light file, which <see cref="Load"/> reads.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True for a decor kind, a shaft kind, a decor file, a light setup, the carried light, the budget, the glow, or the passes.</returns>
    public static bool IsLightFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return DecorKind.IsKindFile(path)
            || ShaftKind.IsShaftFile(path)
            || DecorFile.IsDecorFile(path)
            || LightSetup.IsSetupFile(path)
            || string.CompareOrdinal(path, CarriedLight.Path) == 0
            || string.CompareOrdinal(path, EffectBudget.Path) == 0
            || string.CompareOrdinal(path, Glow.Path) == 0
            || string.CompareOrdinal(path, Hd2dPasses.Path) == 0;
    }

    /// <summary>Gives the decor file of one map.</summary>
    /// <param name="map">The id of the map.</param>
    /// <returns>The decor file.</returns>
    /// <exception cref="ContentException">No decor file names the map (T-2).</exception>
    public DecorFile DecorOf(ContentId map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return this.decor.TryGetValue(map.Value, out DecorFile? found)
            ? found
            : throw ContentException.ForFile(DecorFile.Folder, $"no decor file names the map '{map.Value}' (D-844)");
    }

    /// <summary>Gives one decor kind.</summary>
    /// <param name="kind">The id of the kind.</param>
    /// <returns>The kind.</returns>
    /// <exception cref="ContentException">No kind file holds the id (T-2).</exception>
    public DecorKind KindOf(ContentId kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return this.kinds.TryGetValue(kind.Value, out DecorKind? found)
            ? found
            : throw ContentException.ForFile(DecorKind.Folder, $"no kind file holds the decor kind '{kind.Value}' (D-844)");
    }

    /// <summary>Gives one shaft kind.</summary>
    /// <param name="kind">The id of the kind.</param>
    /// <returns>The kind.</returns>
    /// <exception cref="ContentException">No shaft file holds the id (T-2).</exception>
    public ShaftKind ShaftOf(ContentId kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return this.shaftKinds.TryGetValue(kind.Value, out ShaftKind? found)
            ? found
            : throw ContentException.ForFile(ShaftKind.Folder, $"no shaft file holds the shaft kind '{kind.Value}' (D-918)");
    }

    /// <summary>Gives the full-screen passes of the light shafts of one map: one for a map with a shaft, and none for a map with no shaft (D-523, D-918).</summary>
    /// <param name="map">The id of the map.</param>
    /// <returns>The count of passes.</returns>
    /// <exception cref="ContentException">No decor file names the map (T-2).</exception>
    public int ShaftPassesOf(ContentId map) => this.DecorOf(map).Shafts.Count == 0 ? 0 : ShaftKind.FullScreenPasses;

    /// <summary>Gives the light setup of one map at one time of day.</summary>
    /// <param name="map">The id of the map.</param>
    /// <param name="time">The time of day.</param>
    /// <returns>The light setup.</returns>
    /// <exception cref="ContentException">No light setup names the map at that time (T-2).</exception>
    public LightSetup SetupOf(ContentId map, TimeOfDay time)
    {
        ArgumentNullException.ThrowIfNull(map);

        return this.setups.TryGetValue(SetupKey(map, time), out LightSetup? found)
            ? found
            : throw ContentException.ForFile(
                LightSetup.Folder,
                $"no light setup names the map '{map.Value}' at the time '{TimesOfDay.NameOf(time)}' (D-442)");
    }

    /// <summary>Gives the point lights of one map at one time of day (D-843).</summary>
    /// <param name="map">The id of the map.</param>
    /// <param name="time">The time of day.</param>
    /// <returns>The lights, pieces first, then each added light.</returns>
    /// <exception cref="ContentException">The map has no decor file or no light setup at that time (T-2).</exception>
    public IReadOnlyList<MapLight> LightsOf(ContentId map, TimeOfDay time) =>
        MapLighting.Resolve(this.DecorOf(map), this.kinds, this.SetupOf(map, time));

    /// <summary>Reads every light file, and checks each one against the maps, the palette, and the atlas.</summary>
    /// <param name="files">The light files of the content set, which <see cref="IsLightFile"/> picked.</param>
    /// <param name="maps">Every map, by the value of its id.</param>
    /// <param name="palette">The palette (D-846).</param>
    /// <param name="atlas">The atlas index, for the drawing of each decor kind (D-519).</param>
    /// <returns>The light content.</returns>
    /// <exception cref="ContentException">A file breaks a rule of its reader, or a check across files fails (T-2).</exception>
    public static LightContent Load(
        IReadOnlyList<ContentFile> files,
        SortedDictionary<string, GameMap> maps,
        Palette palette,
        AtlasIndex atlas)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(atlas);

        var kinds = new SortedDictionary<string, DecorKind>(StringComparer.Ordinal);
        var shaftKinds = new SortedDictionary<string, ShaftKind>(StringComparer.Ordinal);
        var decor = new SortedDictionary<string, DecorFile>(StringComparer.Ordinal);
        var setups = new SortedDictionary<string, LightSetup>(StringComparer.Ordinal);
        CarriedLight? carried = null;
        EffectBudget? budget = null;
        Glow? glow = null;
        Hd2dPasses? passes = null;

        foreach (ContentFile file in files)
        {
            if (DecorKind.IsKindFile(file.Path))
            {
                DecorKind kind = DecorKind.Read(file.Bytes, file.Path);
                AddOnce(kinds, kind.Id.Value, kind, file.Path, "id", "a second file holds this decor kind");
            }
            else if (ShaftKind.IsShaftFile(file.Path))
            {
                ShaftKind kind = ShaftKind.Read(file.Bytes, file.Path);
                AddOnce(shaftKinds, kind.Id.Value, kind, file.Path, "id", "a second file holds this shaft kind");
            }
            else if (DecorFile.IsDecorFile(file.Path))
            {
                DecorFile read = DecorFile.Read(file.Bytes, file.Path);
                AddOnce(decor, read.Map.Value, read, file.Path, "map", "a second decor file names this map, and a map has one (D-844)");
            }
            else if (LightSetup.IsSetupFile(file.Path))
            {
                LightSetup setup = LightSetup.Read(file.Bytes, file.Path);
                AddOnce(setups, SetupKey(setup.Map, setup.Time), setup, file.Path, "time", "a second light setup names this map at this time (D-442)");
            }
            else if (string.CompareOrdinal(file.Path, CarriedLight.Path) == 0)
            {
                carried = CarriedLight.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, EffectBudget.Path) == 0)
            {
                budget = EffectBudget.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, Glow.Path) == 0)
            {
                glow = Glow.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, Hd2dPasses.Path) == 0)
            {
                passes = Hd2dPasses.Read(file.Bytes, file.Path);
            }
            else
            {
                throw ContentException.ForFile(file.Path, "the file is not a light file, and the content set gave it to the light reader");
            }
        }

        var content = new LightContent(
            kinds,
            shaftKinds,
            decor,
            setups,
            carried ?? throw ContentException.ForFile(CarriedLight.Path, "the content set holds no such file"),
            budget ?? throw ContentException.ForFile(EffectBudget.Path, "the content set holds no such file"),
            glow ?? throw ContentException.ForFile(Glow.Path, "the content set holds no such file"),
            passes ?? throw ContentException.ForFile(Hd2dPasses.Path, "the content set holds no such file"));

        content.RefuseWrongKind(atlas);
        content.RefuseWrongDecor(maps);
        content.RefuseWrongSetup(maps);
        content.RefuseAbsentColor(palette);
        content.RefuseOverBudget(maps);
        content.RefuseGlowOnArt(maps, palette);
        content.RefuseBrightGlow(palette);
        return content;
    }

    private static string SetupKey(ContentId map, TimeOfDay time) => $"{map.Value}@{TimesOfDay.NameOf(time)}";

    private static void AddOnce<T>(SortedDictionary<string, T> into, string key, T value, string file, string field, string reason)
    {
        if (!into.TryAdd(key, value))
        {
            throw ContentException.ForField(file, field, $"'{key}': {reason}");
        }
    }

    private void RefuseWrongKind(AtlasIndex atlas)
    {
        foreach (DecorKind kind in this.kinds.Values)
        {
            // Game draws each piece, so a kind with no drawing would draw nothing (D-519, T-2).
            if (!atlas.Draws(kind.Id, MapUse))
            {
                throw ContentException.ForField(
                    kind.File,
                    "id",
                    $"no drawing draws '{kind.Id.Value}' for the use '{MapUse}', and Game draws each decor piece (D-519)");
            }
        }

        foreach (ShaftKind kind in this.shaftKinds.Values)
        {
            // A beam falls from an opening that a drawing shows, and never from a bare wall (D-924).
            if (!atlas.Draws(kind.Id, MapUse))
            {
                throw ContentException.ForField(
                    kind.File,
                    "id",
                    $"no drawing draws '{kind.Id.Value}' for the use '{MapUse}', and a light shaft falls from an opening that a drawing shows (D-924)");
            }
        }
    }

    private void RefuseWrongDecor(SortedDictionary<string, GameMap> maps)
    {
        var pieceIds = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (DecorFile file in this.decor.Values)
        {
            if (!maps.TryGetValue(file.Map.Value, out GameMap? map))
            {
                throw ContentException.ForField(file.File, "map", $"no map file holds the id '{file.Map.Value}' (D-528)");
            }

            file.RefuseWrongTile(map);
            for (int index = 0; index < file.Pieces.Count; index += 1)
            {
                DecorPiece piece = file.Pieces[index];
                if (!this.kinds.ContainsKey(piece.Kind.Value))
                {
                    throw ContentException.ForField(
                        file.File,
                        $"pieces[{index}].kind",
                        $"no file of `{DecorKind.Folder}` holds the decor kind '{piece.Kind.Value}' (D-843)");
                }

                RefuseSecondFile(pieceIds, piece, file.File, $"pieces[{index}].id");
            }

            for (int index = 0; index < file.Shafts.Count; index += 1)
            {
                DecorPiece shaft = file.Shafts[index];
                if (!this.shaftKinds.ContainsKey(shaft.Kind.Value))
                {
                    throw ContentException.ForField(
                        file.File,
                        $"shafts[{index}].kind",
                        $"no file of `{ShaftKind.Folder}` holds the shaft kind '{shaft.Kind.Value}' (D-918)");
                }

                RefuseSecondFile(pieceIds, shaft, file.File, $"shafts[{index}].id");
            }
        }

        foreach (GameMap map in maps.Values)
        {
            if (!this.decor.ContainsKey(map.Id.Value))
            {
                throw ContentException.ForFile(map.File, $"no file of `{DecorFile.Folder}` names the map '{map.Id.Value}', and each map has one (D-844)");
            }
        }
    }

    /// <summary>
    /// Refuses a piece or a shaft whose id another decor file holds too. A light setup names a
    /// piece by its id alone, so one id names one piece in the whole build (D-166, D-843).
    /// </summary>
    private static void RefuseSecondFile(SortedDictionary<string, string> pieceIds, DecorPiece piece, string file, string field)
    {
        if (!pieceIds.TryAdd(piece.Id.Value, file))
        {
            throw ContentException.ForField(
                file,
                field,
                $"the file '{pieceIds[piece.Id.Value]}' holds the piece '{piece.Id.Value}' too, and one id names one piece (D-166)");
        }
    }

    private void RefuseWrongSetup(SortedDictionary<string, GameMap> maps)
    {
        foreach (LightSetup setup in this.setups.Values)
        {
            if (!maps.TryGetValue(setup.Map.Value, out GameMap? map))
            {
                throw ContentException.ForField(setup.File, "map", $"no map file holds the id '{setup.Map.Value}' (D-528)");
            }

            DecorFile pieces = this.DecorOf(setup.Map);
            for (int index = 0; index < setup.Changes.Count; index += 1)
            {
                ContentId piece = setup.Changes[index].Piece;
                if (!Holds(pieces, piece))
                {
                    throw ContentException.ForField(
                        setup.File,
                        $"changes[{index}].piece",
                        $"the decor file '{pieces.File}' of the map '{map.Id.Value}' holds no piece '{piece.Value}', and a change names a piece of its map (D-843)");
                }
            }

            for (int index = 0; index < setup.Added.Count; index += 1)
            {
                AddedLight added = setup.Added[index];
                if (!map.Holds(added.Tile))
                {
                    throw ContentException.ForField(
                        setup.File,
                        $"added[{index}]",
                        $"the light '{added.Id.Value}' lies at {added.Tile}, outside the map '{map.Id.Value}' of {map.Width} by {map.Height} tiles");
                }
            }
        }

        foreach (GameMap map in maps.Values)
        {
            // The map draws in the light of its own time, so that setup must exist (D-442, T-2).
            _ = this.SetupOf(map.Id, map.Time);
        }
    }

    private static bool Holds(DecorFile file, ContentId piece)
    {
        foreach (DecorPiece each in file.Pieces)
        {
            if (string.CompareOrdinal(each.Id.Value, piece.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private void RefuseAbsentColor(Palette palette)
    {
        foreach (DecorKind kind in this.kinds.Values)
        {
            RefuseAbsentKey(palette, kind.Light.Color, kind.File, "light.color");
            RefuseAbsentKey(palette, new LightColor(kind.Fire.Glow.Key, 0), kind.File, "fire.glow.color");
        }

        foreach (LightSetup setup in this.setups.Values)
        {
            RefuseAbsentKey(palette, setup.Ambient, setup.File, "ambient.color");
            RefuseAbsentKey(palette, setup.Battle.Color, setup.File, "battle.color");
            for (int index = 0; index < setup.Changes.Count; index += 1)
            {
                RefuseAbsentKey(palette, setup.Changes[index].Light.Color, setup.File, $"changes[{index}].color");
            }

            for (int index = 0; index < setup.Added.Count; index += 1)
            {
                RefuseAbsentKey(palette, setup.Added[index].Light.Color, setup.File, $"added[{index}].color");
            }
        }

        RefuseAbsentKey(palette, this.Carried.Light.Color, CarriedLight.Path, "color");
        RefuseAbsentKey(palette, new LightColor(this.Carried.Fire.Glow.Key, 0), CarriedLight.Path, "fire.glow.color");

        foreach (ShaftKind kind in this.shaftKinds.Values)
        {
            RefuseAbsentKey(palette, new LightColor(kind.Key, 0), kind.File, "color");
        }

        RefuseAbsentKey(palette, new LightColor(this.Passes.VignetteKey, 0), Hd2dPasses.Path, "vignette_color");
    }

    private static void RefuseAbsentKey(Palette palette, LightColor color, string file, string field)
    {
        if (!palette.TryColorOf(color.Key, out _))
        {
            throw ContentException.ForField(file, field, $"the palette holds no key '{color.Key}', and a light names a palette key (D-846, L-10)");
        }
    }

    /// <summary>
    /// Refuses a light setup whose lights can push lit art to the glow threshold, because that
    /// art would glow (D-910, F-47). The bound of <see cref="BrightestLight"/> reads full white art.
    /// </summary>
    /// <remarks>
    /// Each glow halo of a map adds its light onto the lit tiles under it, so the bound reads each
    /// halo too (D-1075).
    /// </remarks>
    private void RefuseGlowOnArt(SortedDictionary<string, GameMap> maps, Palette palette)
    {
        FlickerLevel brightest = this.BrightestFlicker();
        foreach (LightSetup setup in this.setups.Values)
        {
            GameMap map = maps[setup.Map.Value];
            IReadOnlyList<MapLight> lights = this.LightsOf(setup.Map, setup.Time);
            LitPeak peak = BrightestLight.OnMap(
                lights,
                this.HalosOf(setup.Map, lights),
                setup.Ambient,
                (this.Carried.Light, this.Carried.Fire.Glow),
                brightest,
                palette,
                map.Width,
                map.Height);
            if (peak.Level >= this.Glow.Threshold)
            {
                throw ContentException.ForFile(
                    setup.File,
                    $"the light at the tile ({peak.Column}, {peak.Row}) of the map '{setup.Map.Value}' can light white art to {peak.Level} basis points, the carried light and each glow halo included, and the threshold of `{Glow.Path}` is {this.Glow.Threshold}, so that art would glow (D-910, D-1075, F-47)");
            }

            int fight = BrightestLight.InFight(setup.Ambient, setup.Battle, palette);
            if (fight >= this.Glow.Threshold)
            {
                throw ContentException.ForField(
                    setup.File,
                    "battle",
                    $"the ambient light and the key light can light white art to {fight} basis points in a fight, and the threshold of `{Glow.Path}` is {this.Glow.Threshold}, so that art would glow (D-910, F-47)");
            }
        }
    }

    /// <summary>
    /// Refuses a glow halo bright enough to pass the threshold at the top of its pulse, because it
    /// would clip to full light and draw as a box (D-1075, T-2).
    /// </summary>
    private void RefuseBrightGlow(Palette palette)
    {
        foreach (DecorKind kind in this.kinds.Values)
        {
            this.RefuseBrightHalo(kind.Fire.Glow, palette, kind.File);
        }

        this.RefuseBrightHalo(this.Carried.Fire.Glow, palette, CarriedLight.Path);
    }

    /// <remarks>
    /// The linear value of an sRGB channel is never above the channel itself, so the check reads
    /// the brightest channel at the top of the pulse, and a halo that passes it stays below the
    /// threshold on screen too. A halo above the threshold clips to full light, and the glow
    /// of Godot then spreads it, which drew a box over each torch (D-1075).
    /// </remarks>
    private void RefuseBrightHalo(GlowSeed seed, Palette palette, string file)
    {
        long most = BrightestLight.HaloPeak(seed, palette);
        if (most >= this.Glow.Threshold)
        {
            throw ContentException.ForField(
                file,
                "fire.glow.strength",
                $"the glow {seed.Strength} gives its color up to {most} basis points at the top of its pulse, and the threshold of `{Glow.Path}` is {this.Glow.Threshold}, so the halo would draw as a box of full light (D-1075, T-2)");
        }
    }


    /// <summary>Gives the glow halo of each decor piece of one map, at the place of its light (D-1075).</summary>
    /// <param name="map">The id of the map.</param>
    /// <param name="lights">The lights of the map at one time of day, which give the place of each piece light.</param>
    /// <returns>The halo of each piece whose light the list holds, in the order of the lights.</returns>
    public IReadOnlyList<HaloPlace> HalosOf(ContentId map, IReadOnlyList<MapLight> lights)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(lights);

        DecorFile decor = this.DecorOf(map);
        var halos = new List<HaloPlace>();
        foreach (MapLight light in lights)
        {
            foreach (DecorPiece piece in decor.Pieces)
            {
                if (string.CompareOrdinal(piece.Id.Value, light.Id.Value) == 0)
                {
                    GlowSeed glow = this.KindOf(piece.Kind).Fire.Glow;
                    halos.Add(new HaloPlace(checked(light.X + glow.X), checked(light.Y + glow.Y), glow));
                }
            }
        }

        return halos;
    }

    /// <summary>Gives the strongest strength and the widest range of every fire of the build, which each light of the bound takes (D-891).</summary>
    private FlickerLevel BrightestFlicker()
    {
        int strength = 0;
        int range = 0;
        var fires = new List<TorchFire>();
        foreach (DecorKind kind in this.kinds.Values)
        {
            fires.Add(kind.Fire);
        }

        fires.Add(this.Carried.Fire);
        foreach (TorchFire fire in fires)
        {
            foreach (FlickerLevel level in fire.Levels)
            {
                strength = Math.Max(strength, level.Strength);
                range = Math.Max(range, level.Range);
            }
        }

        return new FlickerLevel(strength, range);
    }

    private void RefuseOverBudget(SortedDictionary<string, GameMap> maps)
    {
        // The carried light follows the lead, so each view and each canvas item can hold it
        // (D-847). Thus each count below adds one source.
        const int Carried = 1;

        foreach (LightSetup setup in this.setups.Values)
        {
            GameMap map = maps[setup.Map.Value];
            IReadOnlyList<MapLight> lights = this.LightsOf(setup.Map, setup.Time);
            int width = checked(map.Width * AtlasPages.TileSize);
            int height = checked(map.Height * AtlasPages.TileSize);

            LightCount view = LightBudget.WorstWindow(lights, width, height, LightBudget.ViewWidth, LightBudget.ViewHeight);
            RefuseCount(setup, view, Carried, EffectBudget.LightsPerSource, this.Budget.LightsInView, $"a view of {LightBudget.ViewWidth} by {LightBudget.ViewHeight} art pixels", $"the row `lights_in_view` of `{EffectBudget.Path}` (D-523, D-842)");

            LightCount quadrant = LightBudget.WorstQuadrant(lights, width, height);
            RefuseCount(setup, quadrant, Carried, 1, EffectBudget.GodotLightsPerItem, "a quadrant of the ground layer, which Godot draws as one canvas item", "the lights that Godot draws on one canvas item (F-46)");

            LightCount sprite = LightBudget.WorstWindow(lights, width, height, LightBudget.LargestSprite, LightBudget.LargestSprite);
            RefuseCount(setup, sprite, Carried, 1, EffectBudget.GodotLightsPerItem, "the largest sprite of a map, which Godot draws as one canvas item", "the lights that Godot draws on one canvas item (F-46)");
        }
    }

    /// <remarks>
    /// Each source of a map is a pair of Godot lights, so a view counts two lights for each
    /// source. One canvas item takes one light of each pair, so it counts one (D-853).
    /// </remarks>
    private static void RefuseCount(LightSetup setup, LightCount count, int carried, int lightsPerSource, int limit, string place, string rule)
    {
        int total = checked((count.Count + carried) * lightsPerSource);
        if (total > limit)
        {
            throw ContentException.ForFile(
                setup.File,
                $"{total} lights, the carried light included, reach {place} at ({count.Left}, {count.Top}) of the map '{setup.Map.Value}', and {rule} allows {limit}");
        }
    }
}
