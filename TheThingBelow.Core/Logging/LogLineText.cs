using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Logging;

/// <summary>
/// The text of a log line: one JSON object on one line (D-179, D-652). Core writes and reads
/// the text, and Storage writes the bytes to a file (D-494).
/// </summary>
/// <remarks>
/// The fields take a fixed order: the time, the level, the message, the tick, the subsystem,
/// and the context fields. A tool and a bot read the file line by line, so a line never holds
/// a line feed inside it (D-179).
/// <para>
/// The level takes its text name, and never its number, so a new level of a later PR never
/// changes the meaning of an old line (T-2).
/// </para>
/// </remarks>
public static class LogLineText
{
    /// <summary>The name that an error of a line carries (T-2).</summary>
    public const string LineName = "the log line";

    /// <summary>Writes one line, with no line ending.</summary>
    /// <param name="line">The time and the entry.</param>
    /// <returns>The text of the line, as one JSON object.</returns>
    /// <exception cref="ArgumentNullException">The line is null (T-2).</exception>
    public static string Write(LogLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ArrayBufferWriter<byte> bytes = new();
        using (Utf8JsonWriter writer = new(bytes, new JsonWriterOptions { Indented = false, SkipValidation = false }))
        {
            writer.WriteStartObject();
            writer.WriteString("time", line.Time);
            writer.WriteString("level", NameOf(line.Entry.Level));
            writer.WriteString("message", line.Entry.Message);
            writer.WriteNumber("tick", line.Entry.Tick);
            writer.WriteString("subsystem", line.Entry.Subsystem);
            writer.WriteStartObject("fields");
            foreach (LogField field in line.Entry.Fields)
            {
                writer.WriteString(field.Name, field.Value);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(bytes.WrittenSpan);
    }

    /// <summary>Reads one line.</summary>
    /// <param name="text">The text of the line, with no line ending.</param>
    /// <returns>The time and the entry.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <exception cref="ContentException">
    /// The line is malformed, a field is absent, or the level is no level of this build (T-2).
    /// </exception>
    public static LogLine Read(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var reader = new ContentReader(Encoding.UTF8.GetBytes(text), LineName);
        LogLine line = ReadLine(ref reader);
        reader.ReadFileEnd();
        return line;
    }

    /// <summary>Gives the text name of one level (D-179).</summary>
    /// <param name="level">The level.</param>
    /// <returns>The name, such as `info`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The log holds no such level (T-2).</exception>
    public static string NameOf(LogLevel level) => level switch
    {
        LogLevel.Debug => "debug",
        LogLevel.Info => "info",
        LogLevel.Warning => "warning",
        LogLevel.Error => "error",
        _ => throw new ArgumentOutOfRangeException(
            nameof(level), level, $"The log holds no level with the number {(int)level} (D-179)."),
    };

    private static LogLine ReadLine(ref ContentReader reader)
    {
        string? time = null;
        string? level = null;
        string? message = null;
        long? tick = null;
        string? subsystem = null;
        List<LogField>? fields = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "time":
                    time = reader.ReadString();
                    break;
                case "level":
                    level = reader.ReadString();
                    break;
                case "message":
                    message = reader.ReadString();
                    break;
                case "tick":
                    tick = reader.ReadLong();
                    break;
                case "subsystem":
                    subsystem = reader.ReadString();
                    break;
                case "fields":
                    fields = ReadFields(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        LogEntry entry = new(
            LevelOf(reader.Require(level, depth, "level"), reader.File),
            reader.Require(message, depth, "message"),
            reader.RequireValue(tick, depth, "tick"),
            reader.Require(subsystem, depth, "subsystem"),
            reader.Require(fields, depth, "fields"));

        return new LogLine(reader.Require(time, depth, "time"), entry);
    }

    private static List<LogField> ReadFields(ref ContentReader reader)
    {
        List<LogField> fields = [];
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string name))
        {
            fields.Add(new LogField(name, reader.ReadString()));
        }

        return fields;
    }

    private static LogLevel LevelOf(string name, string file)
    {
        switch (name)
        {
            case "debug":
                return LogLevel.Debug;
            case "info":
                return LogLevel.Info;
            case "warning":
                return LogLevel.Warning;
            case "error":
                return LogLevel.Error;
            default:
                throw ContentException.ForField(
                    file,
                    "level",
                    $"the level '{name}' is no level of this build (D-179)");
        }
    }
}
