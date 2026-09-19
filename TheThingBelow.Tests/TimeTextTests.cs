using System;
using Xunit;
using TheThingBelow.Storage;

namespace TheThingBelow.Tests;

/// <summary>
/// The two text forms of a wall-clock time: the stamp of a file name, and the moment of a line
/// (D-179, D-658). Storage reads no clock, and the caller passes the time (T-3).
/// </summary>
public sealed class TimeTextTests
{
    /// <summary>The time that every test of this class passes.</summary>
    private static readonly DateTime Moment = new(2026, 9, 18, 1, 42, 53, DateTimeKind.Utc);

    [Fact]
    public void AStampSortsAsTheTimeSorts()
    {
        Assert.Equal("20260918-014253", TimeText.Stamp(Moment));
        Assert.True(
            string.CompareOrdinal(TimeText.Stamp(Moment), TimeText.Stamp(Moment.AddSeconds(1))) < 0);
        Assert.True(
            string.CompareOrdinal(TimeText.Stamp(Moment), TimeText.Stamp(Moment.AddYears(1))) < 0);
    }

    [Fact]
    public void AMomentNamesTheUtcZone()
    {
        Assert.Equal("2026-09-18T01:42:53Z", TimeText.Moment(Moment));
    }

    [Fact]
    public void ATimeOfTheMachineIsAnError()
    {
        // A local time names two moments two times a year, and a report cannot tell them (T-2).
        DateTime local = new(2026, 9, 18, 1, 42, 53, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() => TimeText.Stamp(local));
        Assert.Throws<ArgumentException>(() => TimeText.Moment(local));
    }

    [Fact]
    public void ATimeOfNoZoneIsAnError()
    {
        DateTime unspecified = new(2026, 9, 18, 1, 42, 53, DateTimeKind.Unspecified);

        Assert.Throws<ArgumentException>(() => TimeText.Stamp(unspecified));
        Assert.Throws<ArgumentException>(() => TimeText.Moment(unspecified));
    }
}
