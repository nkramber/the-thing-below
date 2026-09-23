using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of one transition file: the look, the length, and the cover color (D-182, D-195,
/// D-939, D-941).
/// </summary>
public sealed class TransitionFileTests
{
    private const string File = "effects/transitions/test.json";

    [Fact]
    public void AWellFormedFileGivesEachField()
    {
        Transition transition = Read(EffectFixtures.TransitionBody("snow_whiteout", ticks: 45, cover: "e"));

        Assert.Equal("transition.snow_whiteout", transition.Id.Value);
        Assert.Equal(TransitionLook.SnowWhiteout, transition.Look);
        Assert.Equal(45, transition.Ticks);
        Assert.Equal('e', transition.Cover);
        Assert.Equal(File, transition.File);
    }

    [Fact]
    public void EachLookHasOneNameThatTheReaderTakesBack()
    {
        // D-195: the library holds ten looks, and each name reads back to its look.
        Assert.Equal(10, Transition.AllLooks.Length);
        foreach (TransitionLook look in Transition.AllLooks)
        {
            string name = Transition.NameOf(look);
            Assert.Equal(look, Read(EffectFixtures.TransitionBody(name)).Look);
            Assert.Contains(name, Transition.EveryLookName, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AnUnknownLookFailsWithTheFileTheFieldAndTheName()
    {
        string body = EffectFixtures.TransitionBody("shatter").Replace("\"look\": \"shatter\"", "\"look\": \"iris_close\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Equal(File, error.File);
        Assert.Equal("look", error.Field);
        Assert.Contains("iris_close", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(601)]
    public void ALengthOutsideTheRangeFails(int ticks)
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(EffectFixtures.TransitionBody("blinds", ticks: ticks)));

        Assert.Equal("ticks", error.Field);
        Assert.Contains($"{ticks} ticks", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACoverOfTwoCharactersFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(EffectFixtures.TransitionBody("blinds", cover: "kk")));

        Assert.Equal("cover", error.Field);
    }

    [Theory]
    [InlineData("\"cover\": \"k\"")]
    [InlineData("\"ticks\": 60")]
    [InlineData("\"look\": \"mosaic\"")]
    public void AnAbsentFieldFails(string removed)
    {
        // T-2: an absent value is an error, never a default.
        string body = EffectFixtures.TransitionBody("mosaic").Replace(", " + removed, string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain(removed, body, StringComparison.Ordinal);

        Assert.Throws<ContentException>(() => Read(body));
    }

    [Fact]
    public void AnIdOfAnotherKindFails()
    {
        string body = EffectFixtures.TransitionBody("mosaic").Replace("transition.mosaic", "effect.mosaic", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(body));

        Assert.Equal("id", error.Field);
    }

    private static Transition Read(string body) => Transition.Read(Encoding.UTF8.GetBytes(body), File);
}
