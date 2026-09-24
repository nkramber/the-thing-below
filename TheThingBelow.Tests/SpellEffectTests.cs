using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The flash of each spell: its reader, its shape over its length, and the checks of the load
/// (D-186, D-863, D-1032). Each error names the file and the field (T-2).
/// </summary>
public sealed class SpellEffectTests
{
    [Fact]
    public void EachFormOfARiteOfTheCheckoutTakesItsOwnFlash()
    {
        // Exit test 12 of PR-12 (D-1032): the load checks each rite form and each look, so the
        // checkout loads with six flashes, and each serves one form.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        var looks = new SortedSet<string>(StringComparer.Ordinal);
        foreach (string form in EffectFixtures.RiteForms)
        {
            SpellEffect spell = content.Effects.SpellOf(ContentId.Parse($"ability.fixture_{form}", "test", "ability"))
                ?? throw new InvalidOperationException($"The form '{form}' has no flash.");
            Assert.True(looks.Add(spell.Look), $"The flash of '{form}' has the look of another flash.");
        }

        Assert.Equal(6, content.Effects.Spells.Count);
        Assert.Null(content.Effects.SpellOf(ContentId.Parse("ability.fixture_hew", "test", "ability")));
    }

    [Fact]
    public void ASpikeStartsAtItsPeakAndFallsToNothingAtItsEnd()
    {
        SpellEffect spell = Read(EffectFixtures.SpellBody("cinder", 20));

        Assert.Equal(10000, spell.LightAt(0));
        Assert.Equal(5000, spell.LightAt(10));
        Assert.True(spell.LightAt(19) > 0);
        Assert.Equal(0, spell.LightAt(20));
        Assert.Equal(0, spell.LightAt(500));
        Assert.Throws<ArgumentOutOfRangeException>(() => spell.LightAt(-1));
    }

    [Fact]
    public void ASwellPeaksInTheMiddle()
    {
        SpellEffect spell = Read(EffectFixtures.SpellBody("salve", 20, "swell"));

        Assert.True(spell.LightAt(0) < spell.LightAt(9));
        Assert.Equal(10000, spell.LightAt(9));
        Assert.Equal(10000, spell.LightAt(10));
        Assert.True(spell.LightAt(19) < spell.LightAt(10));
    }

    [Fact]
    public void ADoublePulsePeaksTwoTimesAndTintsTheFrameOneTime()
    {
        // D-863: the whole frame flashes one time for each spell, and the light of a double
        // pulse peaks two times. No shape peaks more than three times in one second.
        SpellEffect spell = Read(EffectFixtures.SpellBody("purge", 20, "double"));

        Assert.Equal(10000, spell.LightAt(0));
        Assert.Equal(10000, spell.LightAt(10));
        Assert.Equal(spell.LightAt(9), spell.TintAt(9));
        Assert.Equal(0, spell.TintAt(10));
        Assert.Equal(2, Peaks(spell, spell.LightAt));
        Assert.Equal(1, Peaks(spell, spell.TintAt));
    }

    [Fact]
    public void NoShapeFlashesMoreThanThreeTimesInOneSecond()
    {
        // D-863: at each level, no effect flashes more than three times in one second.
        foreach (string shape in new[] { "spike", "swell", "double" })
        {
            SpellEffect spell = Read(EffectFixtures.SpellBody("rot", SpellEffect.FewestTicks, shape));

            Assert.True(Peaks(spell, spell.LightAt) <= 3, $"The shape '{shape}' flashes more than three times.");
            Assert.Equal(1, Peaks(spell, spell.TintAt));
        }
    }

    [Theory]
    [InlineData("\"shape\": \"spike\"", "\"shape\": \"flicker\"", "the shape 'flicker'")]
    [InlineData("\"length_ticks\": 10", "\"length_ticks\": 3", "the length is 3 ticks")]
    [InlineData("\"length_ticks\": 10", "\"length_ticks\": 121", "the length is 121 ticks")]
    [InlineData("\"tint\": { \"color\": \"k\", \"strength\": 500 }", "\"tint\": { \"color\": \"k\", \"strength\": 2501 }", "0 to 2500")]
    [InlineData("\"serves\": [\"ability.fixture_cinder\"]", "\"serves\": []", "serves no ability")]
    [InlineData("\"serves\": [\"ability.fixture_cinder\"]", "\"serves\": [\"character.marrek\"]", "the kind 'ability'")]
    [InlineData("\"range\": 64,", "\"range\": 0,", "range")]
    [InlineData("\"shape\": \"spike\", ", "", "shape")]
    [InlineData("\"shape\": \"spike\", ", "\"shape\": \"spike\", \"pulse\": 2, ", "unknown field")]
    public void ASpellFileThatBreaksARuleFailsWithTheFile(string from, string to, string reason)
    {
        string body = EffectFixtures.SpellBody("cinder", 10);
        Assert.Contains(from, body, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(body.Replace(from, to, StringComparison.Ordinal)));

        Assert.Equal(EffectFixtures.SpellPath("cinder"), error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASpellFileThatServesADrillFails()
    {
        // D-1032: a drill is no spell, so it takes no flash.
        ContentException error = Assert.Throws<ContentException>(() => Load(With(EffectFixtures.File(
            EffectFixtures.SpellPath("hew"), EffectFixtures.SpellBody("hew", 40)))));

        Assert.Equal(EffectFixtures.SpellPath("hew"), error.File);
        Assert.Contains("no form of a rite gives the ability 'ability.fixture_hew'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFormOfARiteWithNoFlashFails()
    {
        List<ContentFile> files = With();
        files.RemoveAll(file => string.CompareOrdinal(file.Path, EffectFixtures.SpellPath("blaze")) == 0);

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Contains("no spell file serves 'ability.fixture_blaze'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoFlashesOfOneLookFail()
    {
        // D-1032: each spell has a flash of its own, so the load compares every value but the id and the serves.
        List<ContentFile> files = With();
        int blaze = files.FindIndex(file => string.CompareOrdinal(file.Path, EffectFixtures.SpellPath("blaze")) == 0);
        files[blaze] = EffectFixtures.File(EffectFixtures.SpellPath("blaze"), EffectFixtures.SpellBody("blaze", 10));

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Contains("the look of", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1032", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoFlashesOfOneFormFail()
    {
        ContentException error = Assert.Throws<ContentException>(() => Load(With(EffectFixtures.File(
            $"{SpellEffect.Folder}test-second-cinder.json",
            EffectFixtures.SpellBody("cinder", 40).Replace("effect.test_cinder", "effect.test_second_cinder", StringComparison.Ordinal)))));

        Assert.Contains("serves 'ability.fixture_cinder' too", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AKeyOutsideThePaletteFails()
    {
        List<ContentFile> files = With();
        int cinder = files.FindIndex(file => string.CompareOrdinal(file.Path, EffectFixtures.SpellPath("cinder")) == 0);
        files[cinder] = EffectFixtures.File(
            EffectFixtures.SpellPath("cinder"),
            EffectFixtures.SpellBody("cinder", 10).Replace("\"tint\": { \"color\": \"k\"", "\"tint\": { \"color\": \"~\"", StringComparison.Ordinal));

        ContentException error = Assert.Throws<ContentException>(() => Load(files));

        Assert.Equal("tint.color", error.Field);
    }

    private static SpellEffect Read(string body) => SpellEffect.Read(Encoding.UTF8.GetBytes(body), EffectFixtures.SpellPath("cinder"));

    /// <summary>Counts each tick where a curve reaches its peak after a lower tick, and the first tick when it starts at the peak.</summary>
    private static int Peaks(SpellEffect spell, Func<int, int> curve)
    {
        int peaks = 0;
        int before = 0;
        for (int tick = 0; tick < spell.LengthTicks; tick += 1)
        {
            int now = curve(tick);
            if (now == 10000 && before < 10000)
            {
                peaks += 1;
            }

            before = now;
        }

        return peaks;
    }

    private static List<ContentFile> With(params ContentFile[] more)
    {
        List<ContentFile> files = [.. EffectFixtures.Files()];
        files.AddRange(more);
        return files;
    }

    private static EffectContent Load(IReadOnlyList<ContentFile> files)
    {
        const string budget = """{ "comment": "a test budget", "lights_in_view": 24, "live_particles": 8192, "full_screen_passes": 4 }""";
        LightContent light = LightFixtures.Load(LightFixtures.Files(LightFixtures.DecorBody(string.Empty), LightFixtures.SetupBody(), budget));
        var world = new AmbientWorld(
            LightFixtures.Maps(),
            TestBattles.Content,
            light,
            new SortedDictionary<string, Drawing>(StringComparer.Ordinal),
            LightFixtures.Palette());
        return EffectContent.Load(files, world);
    }
}
