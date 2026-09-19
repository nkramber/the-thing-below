using System;
using System.Collections.Generic;
using TheThingBelow.Core.Logging;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules of one log entry: the level, the message, the tick, the subsystem, and the
/// context fields (D-179). An absent value is an error, and never an empty one (T-2).
/// </summary>
public sealed class LogEntryTests
{
    [Fact]
    public void AnEntryHoldsItsLevelMessageTickSubsystemAndFields()
    {
        LogEntry entry = new(
            LogLevel.Warning,
            "the file holds no field",
            42,
            LogSubsystems.World,
            [new LogField("file", "rules/enemy.json")]);

        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("the file holds no field", entry.Message);
        Assert.Equal(42, entry.Tick);
        Assert.Equal("world", entry.Subsystem);
        Assert.Equal("file", Assert.Single(entry.Fields).Name);
    }

    [Fact]
    public void AnEntryOfNoFieldTakesTheEmptyList()
    {
        LogEntry entry = new(LogLevel.Info, "the session started", 0, LogSubsystems.Game, LogEntry.NoFields);

        Assert.Empty(entry.Fields);
    }

    [Fact]
    public void AMessageOfNoCharacterIsAnError()
    {
        Assert.Throws<ArgumentException>(
            () => new LogEntry(LogLevel.Info, string.Empty, 0, LogSubsystems.Run, LogEntry.NoFields));
    }

    [Fact]
    public void ASubsystemOfNoCharacterIsAnError()
    {
        Assert.Throws<ArgumentException>(
            () => new LogEntry(LogLevel.Info, "a message", 0, string.Empty, LogEntry.NoFields));
    }

    [Fact]
    public void ATickBelowZeroIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new LogEntry(LogLevel.Info, "a message", -1, LogSubsystems.Run, LogEntry.NoFields));
    }

    [Fact]
    public void ALevelThatNoNameCarriesIsAnError()
    {
        // A cast of a number reaches the constructor, and the entry refuses it (T-2).
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => new LogEntry((LogLevel)7, "a message", 0, LogSubsystems.Run, LogEntry.NoFields));

        Assert.Contains("7", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoFieldsOfOneNameAreAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => new LogEntry(
                LogLevel.Info,
                "a message",
                0,
                LogSubsystems.Run,
                [new LogField("beats", "1"), new LogField("beats", "2")]));

        Assert.Contains("beats", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFieldOfNoNameOrNoValueIsAnError()
    {
        Assert.Throws<ArgumentException>(() => new LogField(string.Empty, "a value"));
        Assert.Throws<ArgumentException>(() => new LogField("name", string.Empty));
    }

    [Fact]
    public void ANumberFieldTakesTheInvariantCulture()
    {
        // The text of a field never follows the culture of the machine (F-39, T-7).
        Assert.Equal("-1234567", LogField.OfNumber("beats", -1234567).Value);
    }

    [Fact]
    public void NoMemberOfAnEntryHoldsATimeOrAPath()
    {
        // Exit test 6 of section 7.15: Core adds no time value and no file path to an entry
        // (G-1, G-3). A member of a clock type or of a path type would break the rule here.
        IReadOnlyList<Type> types = [typeof(LogEntry), typeof(LogField)];
        foreach (Type type in types)
        {
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
                Assert.NotEqual(typeof(DateTime), property.PropertyType);
                Assert.NotEqual(typeof(DateTimeOffset), property.PropertyType);
                Assert.NotEqual(typeof(TimeSpan), property.PropertyType);
                Assert.DoesNotContain("Time", property.Name, StringComparison.Ordinal);
                Assert.DoesNotContain("Path", property.Name, StringComparison.Ordinal);
            }
        }
    }
}
