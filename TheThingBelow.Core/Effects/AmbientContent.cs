using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// Every ambient file of one build, checked across files: the weather of each map, and the
/// capture files of the screen test (D-187, D-202, D-889).
/// </summary>
/// <remarks>
/// The checks that span files:
/// <list type="bullet">
/// <item>Each map that a file serves exists, and a map takes one shipped weather or none (D-202).</item>
/// <item>Each color names a key of the palette (D-181).</item>
/// <item>The particles of a map, with its weather, its torches, and the carried light, keep inside the effect budget. So do the particles of a fight on the map, with its largest hit burst (D-523).</item>
/// <item>The fog of a weather keeps inside the row of full-screen passes, as one pass for all its layers (D-523, D-898).</item>
/// <item>The full strength of each layer of fog keeps each enemy of each map that it serves visible (D-885, D-886).</item>
/// </list>
/// </remarks>
public sealed class AmbientContent
{
    private readonly SortedDictionary<string, AmbientEffect> weatherOf;
    private readonly SortedDictionary<string, AmbientEffect> byId;

    private AmbientContent(
        IReadOnlyList<AmbientEffect> all,
        SortedDictionary<string, AmbientEffect> weatherOf,
        SortedDictionary<string, AmbientEffect> byId)
    {
        this.All = all;
        this.weatherOf = weatherOf;
        this.byId = byId;
    }

    /// <summary>Every ambient file and capture file, in the order of its path (F-39).</summary>
    public IReadOnlyList<AmbientEffect> All { get; }

    /// <summary>Gives the shipped weather of one map, or no value for a map with no weather (D-202).</summary>
    /// <param name="map">The id of the map.</param>
    /// <returns>The weather, or no value.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public AmbientEffect? WeatherOf(ContentId map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return this.weatherOf.TryGetValue(map.Value, out AmbientEffect? found) ? found : null;
    }

    /// <summary>Gives one ambient effect by its id, such as the capture file that a capture loads (D-889).</summary>
    /// <param name="id">The id of the effect.</param>
    /// <returns>The effect.</returns>
    /// <exception cref="ContentException">No ambient file holds the id (T-2).</exception>
    public AmbientEffect Effect(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.byId.TryGetValue(id.Value, out AmbientEffect? found)
            ? found
            : throw ContentException.ForFile(AmbientEffect.Folder, $"no ambient file holds the effect '{id.Value}'");
    }

    /// <summary>Reads every ambient file, and checks each one against the maps, the palette, the budget, and the enemies.</summary>
    /// <param name="files">The ambient files, which <see cref="AmbientEffect.IsAmbientFile"/> picked.</param>
    /// <param name="world">The maps, the fight, the light, the drawings, and the palette that the checks read.</param>
    /// <param name="hits">The hit effects, for the largest burst of a fight.</param>
    /// <param name="ids">The id of each effect file read before, by its path, so one id names one effect (D-166).</param>
    /// <returns>The ambient content.</returns>
    /// <exception cref="ContentException">A file breaks a rule of its reader, or a check across files fails (T-2).</exception>
    public static AmbientContent Load(
        IReadOnlyList<ContentFile> files,
        AmbientWorld world,
        IReadOnlyList<HitEffect> hits,
        SortedDictionary<string, string> ids)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(hits);
        ArgumentNullException.ThrowIfNull(ids);

        var all = new List<AmbientEffect>();
        var byId = new SortedDictionary<string, AmbientEffect>(StringComparer.Ordinal);
        foreach (ContentFile file in files)
        {
            AmbientEffect effect = AmbientEffect.Read(file.Bytes, file.Path);
            if (!ids.TryAdd(effect.Id.Value, file.Path))
            {
                throw ContentException.ForField(file.Path, "id", $"the file '{ids[effect.Id.Value]}' holds the effect '{effect.Id.Value}' too, and one id names one effect (D-166)");
            }

            all.Add(effect);
            byId[effect.Id.Value] = effect;
        }

        all.Sort(static (first, second) => string.CompareOrdinal(first.File, second.File));
        SortedDictionary<string, AmbientEffect> weatherOf = WeatherOfEachMap(all, world.Maps);
        int burst = LargestBurst(hits);
        foreach (AmbientEffect effect in all)
        {
            RefuseAbsentColor(effect, world.Palette);
            foreach (ContentId map in effect.Maps)
            {
                GameMap served = world.Maps[map.Value];
                RefuseOverBudget(effect, served, world, burst);
                RefuseFaintEnemy(effect, served, world);
            }
        }

        return new AmbientContent(all, weatherOf, byId);
    }

    /// <summary>Gives the live particles of the torches of a map and of the carried light (D-890).</summary>
    /// <param name="map">The map.</param>
    /// <param name="light">The light content, for the decor and the carried light.</param>
    /// <returns>The count of particles.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static int TorchParticlesOf(GameMap map, LightContent light)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(light);

        int total = StreamEmitter.ParticlesOf(light.Carried.Fire.Emitters);
        foreach (DecorPiece piece in light.DecorOf(map.Id).Pieces)
        {
            total = checked(total + StreamEmitter.ParticlesOf(light.KindOf(piece.Kind).Fire.Emitters));
        }

        return total;
    }

    private static SortedDictionary<string, AmbientEffect> WeatherOfEachMap(List<AmbientEffect> all, SortedDictionary<string, GameMap> maps)
    {
        var weatherOf = new SortedDictionary<string, AmbientEffect>(StringComparer.Ordinal);
        foreach (AmbientEffect effect in all)
        {
            for (int index = 0; index < effect.Maps.Count; index += 1)
            {
                string map = effect.Maps[index].Value;
                string field = $"maps[{index}]";
                if (!maps.ContainsKey(map))
                {
                    throw ContentException.ForField(effect.File, field, $"no map holds the id '{map}', and a weather serves a map (D-202)");
                }

                if (!effect.IsCapture && !weatherOf.TryAdd(map, effect))
                {
                    throw ContentException.ForField(effect.File, field, $"the file '{weatherOf[map].File}' serves the map '{map}' too, and a map takes one weather (D-202)");
                }
            }
        }

        return weatherOf;
    }

    private static int LargestBurst(IReadOnlyList<HitEffect> hits)
    {
        int largest = 0;
        foreach (HitEffect hit in hits)
        {
            largest = Math.Max(largest, hit.Particles);
        }

        return largest;
    }

    private static void RefuseAbsentColor(AmbientEffect effect, Palette palette)
    {
        for (int emitter = 0; emitter < effect.Emitters.Count; emitter += 1)
        {
            MoteStream stream = effect.Emitters[emitter];
            RefuseAbsentKey(effect, palette, $"emitters[{emitter}].color", stream.Color);
            RefuseAbsentKey(effect, palette, $"emitters[{emitter}].dark_color", stream.DarkColor);
        }

        for (int fog = 0; fog < effect.Fogs.Count; fog += 1)
        {
            RefuseAbsentKey(effect, palette, $"fogs[{fog}].key", effect.Fogs[fog].Key);
        }
    }

    private static void RefuseAbsentKey(AmbientEffect effect, Palette palette, string field, char key)
    {
        if (!palette.TryColorOf(key, out _))
        {
            throw ContentException.ForField(effect.File, field, $"the palette holds no key '{key}', and an effect takes a palette key (D-181, L-10)");
        }
    }

    /// <summary>
    /// Refuses a weather whose particles or passes pass the budget on a map (D-523, T-2). The
    /// map count holds every torch of the map, the carried light, and the weather. A fight shows
    /// the weather and one hit burst at a time.
    /// </summary>
    private static void RefuseOverBudget(AmbientEffect effect, GameMap map, AmbientWorld world, int burst)
    {
        EffectBudget budget = world.Light.Budget;
        int onMap = checked(effect.Particles + TorchParticlesOf(map, world.Light));
        int inFight = checked(effect.Particles + burst);
        int live = Math.Max(onMap, inFight);
        if (live > budget.LiveParticles)
        {
            throw ContentException.ForField(
                effect.File,
                "emitters",
                $"the map '{map.Id.Value}' shows {live} live particles with this weather, and the row `live_particles` of `{EffectBudget.Path}` allows {budget.LiveParticles} (D-523)");
        }

        if (effect.FullScreenPasses > budget.FullScreenPasses)
        {
            throw ContentException.ForField(
                effect.File,
                "fogs",
                $"the weather draws {effect.FullScreenPasses} full-screen passes, and the row `full_screen_passes` of `{EffectBudget.Path}` allows {budget.FullScreenPasses} (D-523, D-898)");
        }
    }

    /// <summary>Refuses a fog whose full strength makes an enemy of the map too faint on its floor (D-885, D-886, T-2).</summary>
    private static void RefuseFaintEnemy(AmbientEffect effect, GameMap map, AmbientWorld world)
    {
        if (effect.Fogs.Count == 0)
        {
            return;
        }

        SortedSet<char> floor = world.FloorKeysOf(map);
        foreach (Drawing enemy in world.EnemyDrawingsOf(map))
        {
            SortedSet<char> outline = FogContrast.OutlineOf(enemy);
            for (int index = 0; index < effect.Fogs.Count; index += 1)
            {
                if (FogContrast.FirstFault(effect.Fogs[index], outline, floor, world.Palette) is FogFault fault)
                {
                    throw ContentException.ForField(
                        effect.File,
                        $"fogs[{index}].strength",
                        $"on the map '{map.Id.Value}', the outline key '{fault.Outline}' of '{enemy.Id.Value}' and the floor key '{fault.Floor}' "
                        + $"keep a luma gap of {fault.Gap} under the full strength of the layer, and the fog test needs {FogContrast.LeastGap} with no fog and under it (D-886, D-892, D-906)");
                }
            }
        }
    }
}
