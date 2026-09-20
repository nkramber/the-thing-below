using System;
using System.Collections.Generic;
using System.Text.Json;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of a log line: one JSON object on one line, with the time of the host, the level,
/// the message, the tick, the subsystem, and the context fields (D-179, D-652).
/// </summary>
public sealed class LogLineTextTests
{
    /// <summary>The time that the host gives to every line of these tests (D-179).</summary>
    private const string Moment = "2026-09-18T01:42:53Z";

    [Fact]
    public void ALineIsOneJsonObjectOnOneLine()
    {
        // Exit test 5 of section 7.15: each log line parses as one JSON object.
        string text = LogLineText.Write(Line(LogLevel.Info, "the menu opened"));

        Assert.DoesNotContain('\n', text);
        using JsonDocument document = JsonDocument.Parse(text);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
    }

    [Fact]
    public void ALineHoldsEachFieldOfTheFormat()
    {
        string text = LogLineText.Write(Line(LogLevel.Warning, "the patrol walked one beat"));

        using JsonDocument document = JsonDocument.Parse(text);
        JsonElement root = document.RootElement;
        Assert.Equal(Moment, root.GetProperty("time").GetString());
        Assert.Equal("warning", root.GetProperty("level").GetString());
        Assert.Equal("the patrol walked one beat", root.GetProperty("message").GetString());
        Assert.Equal(7, root.GetProperty("tick").GetInt64());
        Assert.Equal("world", root.GetProperty("subsystem").GetString());
        Assert.Equal("3", root.GetProperty("fields").GetProperty("beats").GetString());
    }

    [Fact]
    public void EachLevelTakesItsName()
    {
        Assert.Equal("debug", LogLineText.NameOf(LogLevel.Debug));
        Assert.Equal("info", LogLineText.NameOf(LogLevel.Info));
        Assert.Equal("warning", LogLineText.NameOf(LogLevel.Warning));
        Assert.Equal("error", LogLineText.NameOf(LogLevel.Error));
    }

    [Fact]
    public void ALevelThatNoNameCarriesIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LogLineText.NameOf((LogLevel)9));
    }

    [Theory]
    [InlineData("\"tick\":7", "\"tick\":-1", "tick")]
    [InlineData("\"beats\":\"3\"", "\"beats\":\"\"", "value")]
    [InlineData("\"beats\":\"3\"", "\"beats\":\"3\",\"beats\":\"4\"", "two times")]
    public void ALineWithAValueThatNoEntryHoldsFailsAsALineError(string from, string to, string reason)
    {
        // The entry constructor refuses the value with an error that names a parameter and
        // no line. The reader turns it into the error of a line (T-2).
        string text = LogLineText.Write(Line(LogLevel.Info, "the patrol walked one beat"));
        string broken = text.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(text, broken);

        ContentException error = Assert.Throws<ContentException>(() => LogLineText.Read(broken));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadOfAWriteGivesTheSameLine()
    {
        LogLine written = Line(LogLevel.Debug, "the patrol walked one beat");

        LogLine read = LogLineText.Read(LogLineText.Write(written));

        Assert.Equal(written.Time, read.Time);
        Assert.Equal(written.Entry.Level, read.Entry.Level);
        Assert.Equal(written.Entry.Message, read.Entry.Message);
        Assert.Equal(written.Entry.Tick, read.Entry.Tick);
        Assert.Equal(written.Entry.Subsystem, read.Entry.Subsystem);
        Assert.Equal(written.Entry.Fields.Count, read.Entry.Fields.Count);
        for (int index = 0; index < written.Entry.Fields.Count; index += 1)
        {
            Assert.Equal(written.Entry.Fields[index].Name, read.Entry.Fields[index].Name);
            Assert.Equal(written.Entry.Fields[index].Value, read.Entry.Fields[index].Value);
        }
    }

    [Fact]
    public void AWriteOfARoundTripGivesTheSameText()
    {
        string first = LogLineText.Write(Line(LogLevel.Error, "the game stopped with an error"));

        Assert.Equal(first, LogLineText.Write(LogLineText.Read(first)));
    }

    [Fact]
    public void ALineOfNoFieldHoldsAnEmptyFieldObject()
    {
        LogLine line = new(Moment, new LogEntry(LogLevel.Info, "the session started", 0, LogSubsystems.Game, LogEntry.NoFields));

        string text = LogLineText.Write(line);

        Assert.Contains("\"fields\":{}", text, StringComparison.Ordinal);
        Assert.Empty(LogLineText.Read(text).Entry.Fields);
    }

    [Fact]
    public void AnAbsentFieldOfTheLineIsAnError()
    {
        string text = LogLineText.Write(Line(LogLevel.Info, "the menu opened"));
        string withNoTick = text.Replace("\"tick\":7,", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => LogLineText.Read(withNoTick));

        Assert.Contains("tick", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFieldThatTheLineDoesNotHoldIsAnError()
    {
        string text = LogLineText.Write(Line(LogLevel.Info, "the menu opened"));
        string withMore = text.Replace("{\"time\"", "{\"host\":\"a machine\",\"time\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => LogLineText.Read(withMore));

        Assert.Contains("host", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALevelThatThisBuildDoesNotKnowIsAnError()
    {
        string text = LogLineText.Write(Line(LogLevel.Info, "the menu opened"));
        string other = text.Replace("\"level\":\"info\"", "\"level\":\"trace\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => LogLineText.Read(other));

        Assert.Contains("trace", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATimeOfNoCharacterIsAnError()
    {
        Assert.Throws<ArgumentException>(
            () => new LogLine(string.Empty, new LogEntry(LogLevel.Info, "a message", 0, LogSubsystems.Run, LogEntry.NoFields)));
    }

    private static LogLine Line(LogLevel level, string message)
    {
        IReadOnlyList<LogField> fields = [LogField.OfNumber("beats", 3), new LogField("action", "menu.open")];
        return new LogLine(Moment, new LogEntry(level, message, 7, LogSubsystems.World, fields));
    }
}
