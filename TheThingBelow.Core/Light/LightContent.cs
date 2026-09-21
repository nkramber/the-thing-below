using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Light;

/// <summary>
/// Every light file of one build, read and checked across files: the decor kinds, the decor
/// files, the light setups, the carried light, and the effect budget (D-523, D-843, D-844,
/// D-847).
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
/// <item>Each piece hangs on a wall with a floor or a doorway to its south (D-844).</item>
/// <item>Each change names a piece of its map, and each added light lies on its map (D-843).</item>
/// <item>Each color names a key of the palette (D-846).</item>
/// <item>Each map keeps inside the effect budget and the limit of Godot (D-842, F-46).</item>
/// </list>
/// </remarks>
public sealed class LightContent
{
    /// <summary>The use that the map drawing of a decor kind serves (D-519).</summary>
    public const string MapUse = "map";

    private readonly SortedDictionary<string, DecorKind> kinds;
    private readonly SortedDictionary<string, DecorFile> decor;
    private readonly SortedDictionary<string, LightSetup> setups;

    private LightContent(
        SortedDictionary<string, DecorKind> kinds,
        SortedDictionary<string, DecorFile> decor,
        SortedDictionary<string, LightSetup> setups,
        CarriedLight carried,
        EffectBudget budget)
    {
        this.kinds = kinds;
        this.decor = decor;
        this.setups = setups;
        this.Carried = carried;
        this.Budget = budget;
    }

    /// <summary>The carried light (D-847).</summary>
    public CarriedLight Carried { get; }

    /// <summary>The effect budget (D-523).</summary>
    public EffectBudget Budget { get; }

    /// <summary>Every decor kind, in the order of its id.</summary>
    public IEnumerable<DecorKind> Kinds => this.kinds.Values;

    /// <summary>Tells whether a content path is a light file, which <see cref="Load"/> reads.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True for a decor kind, a decor file, a light setup, the carried light, or the budget.</returns>
    public static bool IsLightFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return DecorKind.IsKindFile(path)
            || DecorFile.IsDecorFile(path)
            || LightSetup.IsSetupFile(path)
            || string.CompareOrdinal(path, CarriedLight.Path) == 0
            || string.CompareOrdinal(path, EffectBudget.Path) == 0;
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
        var decor = new SortedDictionary<string, DecorFile>(StringComparer.Ordinal);
        var setups = new SortedDictionary<string, LightSetup>(StringComparer.Ordinal);
        CarriedLight? carried = null;
        EffectBudget? budget = null;

        foreach (ContentFile file in files)
        {
            if (DecorKind.IsKindFile(file.Path))
            {
                DecorKind kind = DecorKind.Read(file.Bytes, file.Path);
                AddOnce(kinds, kind.Id.Value, kind, file.Path, "id", "a second file holds this decor kind");
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
            else
            {
                throw ContentException.ForFile(file.Path, "the file is not a light file, and the content set gave it to the light reader");
            }
        }

        var content = new LightContent(
            kinds,
            decor,
            setups,
            carried ?? throw ContentException.ForFile(CarriedLight.Path, "the content set holds no such file"),
            budget ?? throw ContentException.ForFile(EffectBudget.Path, "the content set holds no such file"));

        content.RefuseWrongKind(atlas);
        content.RefuseWrongDecor(maps);
        content.RefuseWrongSetup(maps);
        content.RefuseAbsentColor(palette);
        content.RefuseOverBudget(maps);
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

                // A light setup names a piece by its id alone, so one id names one piece in the
                // whole build (D-166, D-843).
                if (!pieceIds.TryAdd(piece.Id.Value, file.File))
                {
                    throw ContentException.ForField(
                        file.File,
                        $"pieces[{index}].id",
                        $"the file '{pieceIds[piece.Id.Value]}' holds the piece '{piece.Id.Value}' too, and one id names one piece (D-166)");
                }
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
    }

    private static void RefuseAbsentKey(Palette palette, LightColor color, string file, string field)
    {
        if (!palette.TryColorOf(color.Key, out _))
        {
            throw ContentException.ForField(file, field, $"the palette holds no key '{color.Key}', and a light names a palette key (D-846, L-10)");
        }
    }

    private void RefuseOverBudget(SortedDictionary<string, GameMap> maps)
    {
        // The carried light follows the lead, so each view and each canvas item can hold it
        // (D-847). Thus each count below adds one light.
        const int Carried = 1;

        foreach (LightSetup setup in this.setups.Values)
        {
            GameMap map = maps[setup.Map.Value];
            IReadOnlyList<MapLight> lights = this.LightsOf(setup.Map, setup.Time);
            int width = checked(map.Width * AtlasPages.TileSize);
            int height = checked(map.Height * AtlasPages.TileSize);

            LightCount view = LightBudget.WorstWindow(lights, width, height, LightBudget.ViewWidth, LightBudget.ViewHeight);
            RefuseCount(setup, view, Carried, this.Budget.LightsInView, $"a view of {LightBudget.ViewWidth} by {LightBudget.ViewHeight} art pixels", $"the row `lights_in_view` of `{EffectBudget.Path}` (D-523, D-842)");

            LightCount quadrant = LightBudget.WorstQuadrant(lights, width, height);
            RefuseCount(setup, quadrant, Carried, EffectBudget.GodotLightsPerItem, "a quadrant of the ground layer, which Godot draws as one canvas item", "the lights that Godot draws on one canvas item (F-46)");

            LightCount sprite = LightBudget.WorstWindow(lights, width, height, LightBudget.LargestSprite, LightBudget.LargestSprite);
            RefuseCount(setup, sprite, Carried, EffectBudget.GodotLightsPerItem, "the largest sprite of a map, which Godot draws as one canvas item", "the lights that Godot draws on one canvas item (F-46)");
        }
    }

    private static void RefuseCount(LightSetup setup, LightCount count, int carried, int limit, string place, string rule)
    {
        int total = checked(count.Count + carried);
        if (total > limit)
        {
            throw ContentException.ForFile(
                setup.File,
                $"{total} lights, the carried light included, reach {place} at ({count.Left}, {count.Top}) of the map '{setup.Map.Value}', and {rule} allows {limit}");
        }
    }
}
