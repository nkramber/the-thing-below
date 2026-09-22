using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict readers of the battle file, the hit files, and the budget row of particles (D-182, D-523, D-879, D-883).</summary>
public sealed class EffectFileTests
{
    [Fact]
    public void TheBattleFileReadsEachTimingAndTheShake()
    {
        BattleEffects pace = BattleEffects.Read(Bytes(EffectFixtures.BattleBody), BattleEffects.Path);

        Assert.Equal(44, pace.StrikeTicks);
        Assert.Equal(6, pace.HitStopTicks);
        Assert.Equal(new ShakeValues(12, 2, 4, 1, 0), pace.Shake);
    }

    [Theory]
    [InlineData("\"strike_ticks\": 44", "\"strike_ticks\": 0", "strike_ticks", "2 to 300")]
    [InlineData("\"blow_tick\": 6", "\"blow_tick\": -1", "blow_tick", "0 to 300")]
    [InlineData("\"drift_pixels\": 2", "\"drift_pixels\": 0", "drift_pixels", "1 to 16")]
    [InlineData("\"hit_stop_ticks\": 6", "\"hit_stop_ticks\": 9", "flash_ticks", "fast message speed")]
    [InlineData("\"pose_ticks\": 16", "\"pose_ticks\": 23", "pose_ticks", "fast message speed")]
    [InlineData("\"ticks\": 12", "\"ticks\": 17", "shake", "fast message speed")]
    [InlineData("\"reduced\": 1", "\"reduced\": 2", "shake.reduced", "a quarter")]
    [InlineData("\"off\": 0", "\"off\": 1", "shake.off", "no shake")]
    [InlineData("\"full\": 4", "\"full\": 17", "shake.full", "1 to 16")]
    public void ABattleValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // T-2, D-863, D-873: each bad value names the file, the field, and the rule.
        ContentException error = Assert.Throws<ContentException>(() => BattleEffects.Read(
            Bytes(EffectFixtures.BattleBody.Replace(from, to, StringComparison.Ordinal)), BattleEffects.Path));

        Assert.Equal(BattleEffects.Path, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"hit_stop_ticks\": 6,", "hit_stop_ticks")]
    [InlineData("\"lunge_pixels\": 4,", "lunge_pixels")]
    [InlineData("\"step_ticks\": 2, ", "shake.step_ticks")]
    public void ABattleFileWithAnAbsentFieldFails(string removed, string field)
    {
        // T-2: an absent value is an error, never a zero.
        ContentException error = Assert.Throws<ContentException>(() => BattleEffects.Read(
            Bytes(EffectFixtures.BattleBody.Replace(removed, string.Empty, StringComparison.Ordinal)), BattleEffects.Path));

        Assert.Equal(field, error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABattleFileWithAnUnknownFieldFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => BattleEffects.Read(
            Bytes(EffectFixtures.BattleBody.Replace("\"hit_stop_ticks\": 6,", "\"hit_stop_ticks\": 6, \"flash_strength\": 3,", StringComparison.Ordinal)),
            BattleEffects.Path));

        Assert.Equal("flash_strength", error.Field);
    }

    [Fact]
    public void AHitFileReadsItsIdItsTargetsAndItsEmitters()
    {
        HitEffect hit = HitEffect.Read(Bytes(EffectFixtures.HitBody("\"enemy.fixture_grunt\"")), EffectFixtures.HitPath);

        Assert.Equal("effect.test_blood", hit.Id.Value);
        Assert.Equal("enemy.fixture_grunt", Assert.Single(hit.Serves).Value);
        Assert.True(hit.Lit);
        ParticleEmitter emitter = Assert.Single(hit.Emitters);
        Assert.Equal(10, emitter.Amount);
        Assert.Equal(['k'], emitter.Colors);
        Assert.Equal(10, hit.Particles);
    }

    [Theory]
    [InlineData("\"amount\": 10", "\"amount\": 0", "emitters[0].amount", "1 to 256")]
    [InlineData("\"lifetime_ticks\": 20", "\"lifetime_ticks\": 121", "emitters[0].lifetime_ticks", "1 to 120")]
    [InlineData("\"size\": 2", "\"size\": 5", "emitters[0].size", "1 to 4")]
    [InlineData("\"spread\": 40", "\"spread\": 181", "emitters[0].spread", "0 to 180")]
    [InlineData("\"direction\": -30", "\"direction\": 181", "emitters[0].direction", "-180 to 180")]
    [InlineData("\"fastest_speed\": 120", "\"fastest_speed\": 40", "emitters[0].fastest_speed", "below the slowest")]
    [InlineData("\"colors\": [\"k\"]", "\"colors\": [\"kk\"]", "emitters[0].colors[0]", "one palette key")]
    [InlineData("\"colors\": [\"k\"]", "\"colors\": []", "emitters[0].colors", "1 to 8")]
    public void AnEmitterValueOutsideItsLimitFailsWithTheField(string from, string to, string field, string reason)
    {
        // Exit test 2 of PR-57: an effect file with a bad field fails the load with the file and the field.
        ContentException error = Assert.Throws<ContentException>(() => HitEffect.Read(
            Bytes(EffectFixtures.HitBody("\"enemy.fixture_grunt\"").Replace(from, to, StringComparison.Ordinal)), EffectFixtures.HitPath));

        Assert.Equal(EffectFixtures.HitPath, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHitFileWithNoEmitterFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => HitEffect.Read(
            Bytes(EffectFixtures.HitBody("\"enemy.fixture_grunt\"", emitters: string.Empty)), EffectFixtures.HitPath));

        Assert.Equal("emitters", error.Field);
    }

    [Fact]
    public void AHitFileThatNamesOneTargetTwiceFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => HitEffect.Read(
            Bytes(EffectFixtures.HitBody("\"enemy.fixture_grunt\", \"enemy.fixture_grunt\"")), EffectFixtures.HitPath));

        Assert.Equal("serves[1]", error.Field);
        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHitFileWithAnIdOfAnotherKindFails()
    {
        // D-646: the kind of an entry id agrees with the file that holds it.
        ContentException error = Assert.Throws<ContentException>(() => HitEffect.Read(
            Bytes(EffectFixtures.HitBody("\"enemy.fixture_grunt\"", id: "light.test_blood")), EffectFixtures.HitPath));

        Assert.Equal("id", error.Field);
    }

    [Theory]
    [InlineData(10, 3, new[] { 4, 3, 3 })]
    [InlineData(2, 3, new[] { 1, 1, 0 })]
    [InlineData(9, 1, new[] { 9 })]
    public void TheAmountOfAnEmitterSplitsOverItsKeysAndAddsUpToTheAmount(int amount, int keys, int[] expected)
    {
        // D-181: each palette key takes a node of its own, and no particle drops out.
        var colors = new char[keys];
        Array.Fill(colors, 'k');
        var emitter = new ParticleEmitter(amount, 10, colors, 1, 0, 0, 0, 0, 0, 0);

        var split = new int[keys];
        for (int key = 0; key < keys; key += 1)
        {
            split[key] = emitter.AmountOf(key);
        }

        Assert.Equal(expected, split);
        Assert.Throws<ArgumentOutOfRangeException>(() => emitter.AmountOf(keys));
    }

    [Fact]
    public void TheBudgetReadsTheRowOfLiveParticles()
    {
        // D-617: the sweep of 2026-09-17 held 8192 live particles.
        EffectBudget budget = EffectBudget.Read(Bytes(LightFixtures.BudgetBody(24)), EffectBudget.Path);

        Assert.Equal(8192, budget.LiveParticles);
    }

    [Fact]
    public void TheBudgetReadsItsRowOfFullScreenPasses()
    {
        // D-617: the sweep of 2026-09-17 held 3 full-screen passes.
        EffectBudget budget = EffectBudget.Read(Bytes(LightFixtures.BudgetBody(24)), EffectBudget.Path);

        Assert.Equal(3, budget.FullScreenPasses);
    }

    [Fact]
    public void ABudgetWithNoRowOfPassesFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => EffectBudget.Read(
            Bytes("""{ "comment": "a test budget", "lights_in_view": 24, "live_particles": 8192 }"""), EffectBudget.Path));

        Assert.Equal("full_screen_passes", error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABudgetWithNoRowOfParticlesFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => EffectBudget.Read(
            Bytes("""{ "comment": "a test budget", "lights_in_view": 24 }"""), EffectBudget.Path));

        Assert.Equal("live_particles", error.Field);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
}
