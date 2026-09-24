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
/// <item>Each form of a rite takes exactly one spell file, and no two spell files show one look (D-1032).</item>
/// <item>The ambient files keep the checks of <see cref="AmbientContent"/> (D-202, D-523, D-886).</item>
/// <item>The transitions keep the checks of <see cref="TransitionContent"/> (D-195, D-934, D-936).</item>
/// </list>
/// </remarks>
public sealed class EffectContent
{
    private readonly SortedDictionary<string, HitEffect> hitOf;
    private readonly SortedDictionary<string, SpellEffect> spellOf;

    private EffectContent(
        BattleEffects battle,
        IReadOnlyList<HitEffect> hits,
        SortedDictionary<string, HitEffect> hitOf,
        IReadOnlyList<SpellEffect> spells,
        SortedDictionary<string, SpellEffect> spellOf,
        AmbientContent ambient,
        TransitionContent transitions)
    {
        this.Spells = spells;
        this.spellOf = spellOf;
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

    /// <summary>Every spell effect, in the order of its path (F-39, D-1032).</summary>
    public IReadOnlyList<SpellEffect> Spells { get; }

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
            || SpellEffect.IsSpellFile(path)
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

    /// <summary>Gives the flash of a form of a rite, or no value for a form of a drill (D-1032).</summary>
    /// <param name="ability">The ability of the form.</param>
    /// <returns>The spell effect, or no value when no spell file serves the ability.</returns>
    public SpellEffect? SpellOf(ContentId ability)
    {
        ArgumentNullException.ThrowIfNull(ability);

        return this.spellOf.TryGetValue(ability.Value, out SpellEffect? found) ? found : null;
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
        var spells = new List<SpellEffect>();
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
            else if (SpellEffect.IsSpellFile(file.Path))
            {
                SpellEffect spell = SpellEffect.Read(file.Bytes, file.Path);
                if (!ids.TryAdd(spell.Id.Value, file.Path))
                {
                    throw ContentException.ForField(file.Path, "id", $"the file '{ids[spell.Id.Value]}' holds the effect '{spell.Id.Value}' too, and one id names one effect (D-166)");
                }

                spells.Add(spell);
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
        spells.Sort(static (first, second) => string.CompareOrdinal(first.File, second.File));
        SortedDictionary<string, SpellEffect> spellOf = SpellsOf(spells, battle);
        RefuseWrongSpell(spells, palette, budget);
        return new EffectContent(
            pace ?? throw ContentException.ForFile(BattleEffects.Path, "the content set holds no such file"),
            hits,
            hitOf,
            spells,
            spellOf,
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
    /// <summary>
    /// Gives the spell file of each form of a rite, and refuses a file that serves another ability,
    /// two files of one ability, and a form of a rite with no file (D-1032).
    /// </summary>
    private static SortedDictionary<string, SpellEffect> SpellsOf(List<SpellEffect> spells, BattleContent battle)
    {
        var rites = new SortedSet<string>(StringComparer.Ordinal);
        foreach (LessonRecord lesson in battle.Lessons.Records)
        {
            if (!lesson.IsRite)
            {
                continue;
            }

            foreach (LessonForm form in lesson.Forms)
            {
                _ = rites.Add(form.Ability.Value);
            }
        }

        var spellOf = new SortedDictionary<string, SpellEffect>(StringComparer.Ordinal);
        foreach (SpellEffect spell in spells)
        {
            for (int index = 0; index < spell.Serves.Count; index += 1)
            {
                string id = spell.Serves[index].Value;
                string field = $"serves[{index}]";
                if (!rites.Contains(id))
                {
                    throw ContentException.ForField(spell.File, field, $"no form of a rite gives the ability '{id}', and a spell file serves the forms of rites (D-1032)");
                }

                if (!spellOf.TryAdd(id, spell))
                {
                    throw ContentException.ForField(spell.File, field, $"the file '{spellOf[id].File}' serves '{id}' too, and each spell has one flash (D-1032)");
                }
            }
        }

        foreach (string rite in rites)
        {
            if (!spellOf.ContainsKey(rite))
            {
                throw ContentException.ForFile(SpellEffect.Folder, $"no spell file serves '{rite}', and each form of a rite has a flash of its own (D-1032)");
            }
        }

        return spellOf;
    }

    /// <summary>Refuses a spell file with a key that the palette lacks, a burst over the budget, or the look of another spell file (D-181, D-523, D-1032).</summary>
    private static void RefuseWrongSpell(List<SpellEffect> spells, Palette palette, EffectBudget budget)
    {
        var looks = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (SpellEffect spell in spells)
        {
            RequireKey(palette, spell.File, "light.color", spell.Light.Color.Key);
            RequireKey(palette, spell.File, "tint.color", spell.Tint.Key);
            for (int emitter = 0; emitter < spell.Emitters.Count; emitter += 1)
            {
                IReadOnlyList<char> colors = spell.Emitters[emitter].Colors;
                for (int index = 0; index < colors.Count; index += 1)
                {
                    RequireKey(palette, spell.File, $"emitters[{emitter}].colors[{index}]", colors[index]);
                }
            }

            if (spell.Particles > budget.LiveParticles)
            {
                throw ContentException.ForField(
                    spell.File,
                    "emitters",
                    $"the burst holds {spell.Particles} live particles in one fight, and the row `live_particles` of `{EffectBudget.Path}` allows {budget.LiveParticles} (D-523)");
            }

            if (!looks.TryAdd(spell.Look, spell.File))
            {
                throw ContentException.ForFile(spell.File, $"the flash has the look of '{looks[spell.Look]}', and each spell has a flash of its own (D-1032)");
            }
        }
    }

    private static void RequireKey(Palette palette, string file, string field, char key)
    {
        if (!palette.TryColorOf(key, out _))
        {
            throw ContentException.ForField(file, field, $"the palette holds no key '{key}', and a flash takes a palette key (D-181, L-10)");
        }
    }

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
