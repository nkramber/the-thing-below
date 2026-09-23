using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// Every effect file of one build, read and checked across files: the battle file, the hit
/// files, the ambient files, and the transitions (D-182, D-187, D-195, D-879, D-883). The effect budget lives beside them, and the light reader reads
/// it (<see cref="LightContent"/>).
/// </summary>
/// <remarks>
/// No rule reads an effect file, so none lies in the rule folder and none reaches the content
/// hash (D-495, D-522). The load still fails on the first error, with the file and the field
/// (T-2).
/// <para>
/// The checks that span files:
/// </para>
/// <list type="bullet">
/// <item>Each id that a hit file serves names a character or an enemy (D-879).</item>
/// <item>Each character and each enemy takes exactly one hit file (D-879).</item>
/// <item>Each color names a key of the palette (D-181).</item>
/// <item>The particles of one burst keep inside the effect budget (D-523).</item>
/// <item>The ambient files keep the checks of <see cref="AmbientContent"/> (D-202, D-523, D-886).</item>
/// <item>The transitions keep the checks of <see cref="TransitionContent"/> (D-195, D-934, D-936).</item>
/// </list>
/// </remarks>
public sealed class EffectContent
{
    private readonly SortedDictionary<string, HitEffect> hitOf;

    private EffectContent(
        BattleEffects battle,
        IReadOnlyList<HitEffect> hits,
        SortedDictionary<string, HitEffect> hitOf,
        AmbientContent ambient,
        TransitionContent transitions)
    {
        this.Ambient = ambient;
        this.Transitions = transitions;
        this.Battle = battle;
        this.Hits = hits;
        this.hitOf = hitOf;
    }

    /// <summary>The pace of every fight on screen (D-883).</summary>
    public BattleEffects Battle { get; }

    /// <summary>Every hit effect, in the order of its path (F-39).</summary>
    public IReadOnlyList<HitEffect> Hits { get; }

    /// <summary>The weather of each map and the capture files of the screen test (D-187, D-889).</summary>
    public AmbientContent Ambient { get; }

    /// <summary>The ten transitions and the table that gives each fight one (D-195, D-934).</summary>
    public TransitionContent Transitions { get; }

    /// <summary>Tells whether a content path is an effect file, which <see cref="Load"/> reads.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True for the battle file, each hit file, each ambient file, each transition file, and the transition table. The budget is a light file (<see cref="LightContent.IsLightFile"/>).</returns>
    public static bool IsEffectFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return string.CompareOrdinal(path, BattleEffects.Path) == 0
            || HitEffect.IsHitFile(path)
            || AmbientEffect.IsAmbientFile(path)
            || TransitionContent.IsTransitionContent(path);
    }

    /// <summary>Gives the hit effect that serves one character or one enemy (D-879).</summary>
    /// <param name="combatant">The content id of the character or the enemy.</param>
    /// <returns>The hit effect.</returns>
    /// <exception cref="ContentException">No hit file serves the id (T-2).</exception>
    public HitEffect HitOf(ContentId combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        return this.hitOf.TryGetValue(combatant.Value, out HitEffect? found)
            ? found
            : throw ContentException.ForFile(HitEffect.Folder, $"no hit file serves '{combatant.Value}', and each combatant takes one (D-879)");
    }

    /// <summary>Reads every effect file, and checks each one against the fight, the palette, and the budget.</summary>
    /// <param name="files">The effect files of the content set, which <see cref="IsEffectFile"/> picked.</param>
    /// <param name="world">The maps, the fight, the light with the effect budget, the drawings, and the palette (D-181, D-523).</param>
    /// <returns>The effect content.</returns>
    /// <exception cref="ContentException">A file breaks a rule of its reader, or a check across files fails (T-2).</exception>
    public static EffectContent Load(IReadOnlyList<ContentFile> files, AmbientWorld world)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(world);

        BattleContent battle = world.Battle;
        Palette palette = world.Palette;
        EffectBudget budget = world.Light.Budget;
        var ambientFiles = new List<ContentFile>();
        var transitionFiles = new List<ContentFile>();

        BattleEffects? pace = null;
        var hits = new List<HitEffect>();
        var ids = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (ContentFile file in files)
        {
            if (string.CompareOrdinal(file.Path, BattleEffects.Path) == 0)
            {
                pace = BattleEffects.Read(file.Bytes, file.Path);
            }
            else if (AmbientEffect.IsAmbientFile(file.Path))
            {
                // An ambient file needs the hit files for the budget of a fight, so it reads last.
                ambientFiles.Add(file);
            }
            else if (TransitionContent.IsTransitionContent(file.Path))
            {
                // The transitions need the ids of the hit files, so they read after the loop (D-166).
                transitionFiles.Add(file);
            }
            else if (HitEffect.IsHitFile(file.Path))
            {
                HitEffect hit = HitEffect.Read(file.Bytes, file.Path);
                if (!ids.TryAdd(hit.Id.Value, file.Path))
                {
                    throw ContentException.ForField(file.Path, "id", $"the file '{ids[hit.Id.Value]}' holds the effect '{hit.Id.Value}' too, and one id names one effect (D-166)");
                }

                hits.Add(hit);
            }
            else
            {
                throw ContentException.ForFile(file.Path, "the file is not an effect file, and the content set gave it to the effect reader");
            }
        }

        hits.Sort(static (first, second) => string.CompareOrdinal(first.File, second.File));
        SortedDictionary<string, HitEffect> hitOf = ServedBy(hits, battle);
        RefuseAbsentColor(hits, palette);
        RefuseOverBudget(hits, budget);
        return new EffectContent(
            pace ?? throw ContentException.ForFile(BattleEffects.Path, "the content set holds no such file"),
            hits,
            hitOf,
            AmbientContent.Load(ambientFiles, world, hits, ids),
            TransitionContent.Load(transitionFiles, world.Maps, palette, ids));
    }

    /// <summary>
    /// Gives the hit file of each combatant, and refuses an id that names no combatant, a
    /// combatant that two files serve, and a combatant that no file serves (D-879, T-2).
    /// </summary>
    private static SortedDictionary<string, HitEffect> ServedBy(List<HitEffect> hits, BattleContent battle)
    {
        var combatants = new SortedSet<string>(StringComparer.Ordinal);
        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            combatants.Add(character.Id.Value);
        }

        foreach (EnemyRecord enemy in battle.Enemies)
        {
            combatants.Add(enemy.Id.Value);
        }

        var hitOf = new SortedDictionary<string, HitEffect>(StringComparer.Ordinal);
        foreach (HitEffect hit in hits)
        {
            for (int index = 0; index < hit.Serves.Count; index += 1)
            {
                string id = hit.Serves[index].Value;
                string field = $"serves[{index}]";
                if (!combatants.Contains(id))
                {
                    throw ContentException.ForField(hit.File, field, $"no character and no enemy holds the id '{id}', and a hit file serves combatants (D-879)");
                }

                if (!hitOf.TryAdd(id, hit))
                {
                    throw ContentException.ForField(hit.File, field, $"the file '{hitOf[id].File}' serves '{id}' too, and each combatant takes one hit file (D-879)");
                }
            }
        }

        foreach (string combatant in combatants)
        {
            if (!hitOf.ContainsKey(combatant))
            {
                throw ContentException.ForFile(HitEffect.Folder, $"no hit file serves '{combatant}', and each combatant takes one (D-879)");
            }
        }

        return hitOf;
    }

    private static void RefuseAbsentColor(List<HitEffect> hits, Palette palette)
    {
        foreach (HitEffect hit in hits)
        {
            for (int emitter = 0; emitter < hit.Emitters.Count; emitter += 1)
            {
                IReadOnlyList<char> colors = hit.Emitters[emitter].Colors;
                for (int index = 0; index < colors.Count; index += 1)
                {
                    if (!palette.TryColorOf(colors[index], out _))
                    {
                        throw ContentException.ForField(
                            hit.File,
                            $"emitters[{emitter}].colors[{index}]",
                            $"the palette holds no key '{colors[index]}', and a particle takes a palette key (D-181, L-10)");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Refuses a burst whose particles pass the row of the budget (D-523, T-2). The screen
    /// plays one event at a time, and it ends the burst of an event when the next one starts,
    /// so one burst is the most that a fight shows at once.
    /// </summary>
    private static void RefuseOverBudget(List<HitEffect> hits, EffectBudget budget)
    {
        foreach (HitEffect hit in hits)
        {
            int live = hit.Particles;
            if (live > budget.LiveParticles)
            {
                throw ContentException.ForField(
                    hit.File,
                    "emitters",
                    $"the burst holds {live} live particles in one fight, and the row `live_particles` of `{EffectBudget.Path}` allows {budget.LiveParticles} (D-523)");
            }
        }
    }
}
