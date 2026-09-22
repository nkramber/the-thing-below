using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The checks that span the effect files: the targets of each hit file, the palette, and the
/// budget of live particles (D-523, D-879). The last tests prove that no rule reads an effect
/// (D-495, D-522).
/// </summary>
public sealed class EffectContentTests
{
    [Fact]
    public void EachCombatantTakesTheHitFileThatServesIt()
    {
        EffectContent effects = Load(EffectFixtures.Files());

        foreach (string id in new[] { "character.marrek", "enemy.fixture_grunt", "enemy.fixture_brute" })
        {
            Assert.Equal("effect.test_blood", effects.HitOf(Id(id)).Id.Value);
        }

        Assert.Equal(44, effects.Battle.StrikeTicks);
    }

    [Fact]
    public void AHitFileThatServesAnAbsentIdFails()
    {
        // Exit test 5 of PR-57: each hit file names the content ids that it serves, and an
        // absent id fails.
        ContentException error = Assert.Throws<ContentException>(() => Load(
        [
            EffectFixtures.File(BattleEffects.Path, EffectFixtures.BattleBody),
            EffectFixtures.File(EffectFixtures.HitPath, EffectFixtures.HitBody(EffectFixtures.EveryCombatant + ", \"enemy.absent\"")),
        ]));

        Assert.Equal(EffectFixtures.HitPath, error.File);
        Assert.Equal("serves[5]", error.Field);
        Assert.Contains("enemy.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACombatantThatNoHitFileServesFails()
    {
        // D-879: each hit plays one burst, so each combatant takes one hit file.
        ContentException error = Assert.Throws<ContentException>(() => Load(
        [
            EffectFixtures.File(BattleEffects.Path, EffectFixtures.BattleBody),
            EffectFixtures.File(EffectFixtures.HitPath, EffectFixtures.HitBody("\"character.marrek\", \"character.test_second\", \"character.test_third\", \"enemy.fixture_grunt\"")),
        ]));

        Assert.Equal(HitEffect.Folder, error.File);
        Assert.Contains("enemy.fixture_brute", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACombatantThatTwoHitFilesServeFails()
    {
        // D-879: one hit plays one burst, so a second file for one combatant is an error.
        ContentException error = Assert.Throws<ContentException>(() => Load(
        [
            EffectFixtures.File(BattleEffects.Path, EffectFixtures.BattleBody),
            EffectFixtures.File(EffectFixtures.HitPath, EffectFixtures.HitBody(EffectFixtures.EveryCombatant)),
            EffectFixtures.File("effects/hits/test-sparks.json", EffectFixtures.HitBody("\"enemy.fixture_brute\"", id: "effect.test_sparks")),
        ]));

        Assert.Equal("effects/hits/test-sparks.json", error.File);
        Assert.Contains("each combatant takes one hit file", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AColorOutsideThePaletteFails()
    {
        // D-181, L-10: a particle takes a key of the one palette.
        string emitter = EffectFixtures.Emitter.Replace("\"colors\": [\"k\"]", "\"colors\": [\"k\", \"~\"]", StringComparison.Ordinal);
        ContentException error = Assert.Throws<ContentException>(() => Load(
        [
            EffectFixtures.File(BattleEffects.Path, EffectFixtures.BattleBody),
            EffectFixtures.File(EffectFixtures.HitPath, EffectFixtures.HitBody(EffectFixtures.EveryCombatant, emitters: emitter)),
        ]));

        Assert.Equal("emitters[0].colors[1]", error.Field);
    }

    [Fact]
    public void ABurstOverTheRowOfLiveParticlesFails()
    {
        // Exit test 3 of PR-57: the budget test counts each live emitter against the particle
        // row. Two emitters of 256 pass a row of 500 together, and neither passes it alone.
        string two = EffectFixtures.Emitter.Replace("\"amount\": 10", "\"amount\": 256", StringComparison.Ordinal);
        ContentException error = Assert.Throws<ContentException>(() => Load(
            [
                EffectFixtures.File(BattleEffects.Path, EffectFixtures.BattleBody),
                EffectFixtures.File(EffectFixtures.HitPath, EffectFixtures.HitBody(EffectFixtures.EveryCombatant, emitters: two + ", " + two)),
            ],
            liveParticles: 500));

        Assert.Equal(EffectFixtures.HitPath, error.File);
        Assert.Contains("512 live particles", error.Message, StringComparison.Ordinal);
        Assert.Contains("allows 500", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryBurstOfTheGameKeepsInsideTheBudget()
    {
        // D-523: the load of the content set runs the budget check on every CI leg.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.NotEmpty(content.Effects.Hits);
        foreach (HitEffect hit in content.Effects.Hits)
        {
            Assert.InRange(hit.Particles, 1, content.Light.Budget.LiveParticles);
        }
    }

    [Fact]
    public void TheFixtureBruteSparksAndTheOthersBleed()
    {
        // D-882: the brute counts as armored, and Marrek and the grunt take the blood.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.Equal("effect.sparks", content.Effects.HitOf(Id("enemy.fixture_brute")).Id.Value);
        Assert.Equal("effect.blood", content.Effects.HitOf(Id("enemy.fixture_grunt")).Id.Value);
        Assert.Equal("effect.blood", content.Effects.HitOf(Id("character.marrek")).Id.Value);
    }

    [Fact]
    public void AChangeOfAnEffectFileLeavesTheContentHash()
    {
        // D-495, D-522: no rule reads an effect, so a tune of an effect never breaks a record.
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        string before = ContentHash.Compute(files);
        int index = files.FindIndex(file => string.CompareOrdinal(file.Path, BattleEffects.Path) == 0);
        Assert.True(index >= 0, "The content folder holds no battle file.");
        string changed = Encoding.UTF8.GetString(files[index].Bytes).Replace("\"hit_stop_ticks\": 6", "\"hit_stop_ticks\": 4", StringComparison.Ordinal);
        files[index] = new ContentFile(BattleEffects.Path, Encoding.UTF8.GetBytes(changed));

        Assert.Equal(before, ContentHash.Compute(files));
    }

    [Fact]
    public void NoRuleOfCoreNamesTheEffectTypes()
    {
        // Exit test 4 of PR-57: no rule reads the length of an effect (D-522). The effect types
        // live in their own folder, and only the content set outside it loads them.
        string core = Path.Combine(RepositoryRoot.Find(), "TheThingBelow.Core");
        string effects = Path.Combine(core, "Effects") + Path.DirectorySeparatorChar;
        string set = Path.Combine(core, "Content", "ContentSet.cs");
        foreach (string file in Directory.EnumerateFiles(core, "*.cs", SearchOption.AllDirectories))
        {
            bool built = file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
            if (built || file.StartsWith(effects, StringComparison.Ordinal) || string.Equals(file, set, StringComparison.Ordinal))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            Assert.False(
                text.Contains("TheThingBelow.Core.Effects", StringComparison.Ordinal),
                $"The file '{file}' names the effect types, and no rule reads an effect (D-522).");
        }
    }

    private static EffectContent Load(IReadOnlyList<ContentFile> files, int liveParticles = 8192)
    {
        string budget = $$"""{ "comment": "a test budget", "lights_in_view": 24, "live_particles": {{liveParticles}}, "full_screen_passes": 3 }""";
        LightContent light = LightFixtures.Load(LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody(), budget));
        var world = new AmbientWorld(
            LightFixtures.Maps(),
            TestBattles.Content,
            light,
            new SortedDictionary<string, Drawing>(StringComparer.Ordinal),
            LightFixtures.Palette());
        return EffectContent.Load(files, world);
    }

    private static ContentId Id(string value) => ContentId.Parse(value, EffectFixtures.HitPath, "serves");
}
