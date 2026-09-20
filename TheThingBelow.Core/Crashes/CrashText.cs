using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Crashes;

/// <summary>
/// The text of a crash file: the crash line, and then the lines of the run record (D-170,
/// D-652). Core writes and reads the text, and Storage writes the bytes to a file (D-494).
/// </summary>
/// <remarks>
/// One file holds the crash and the record, so the player sends one file to the address of
/// D-473 and the report loses no replay (D-170, T-2). A reader skips line 1 and reads the rest
/// with <see cref="RunRecordText"/>, and a replay of that record reaches the state of the
/// crash (G-5).
/// <para>
/// The crash line carries the versions of the build itself, because a crash can come before a
/// run exists and the record header is then absent (D-170). The field `record` says whether
/// the lines of a record follow, so a reader never reads an absent record as an empty one
/// (T-2).
/// </para>
/// </remarks>
public static class CrashText
{
    /// <summary>Writes a crash file, with a line feed after each line.</summary>
    /// <param name="report">The crash and the record of the run.</param>
    /// <returns>The text of the file.</returns>
    /// <exception cref="ArgumentNullException">The report is null (T-2).</exception>
    /// <exception cref="ArgumentException">
    /// The report names a format version that this build does not write (T-2).
    /// </exception>
    public static string Write(CrashReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.FormatVersion != CrashFormat.Current)
        {
            throw new ArgumentException(
                $"The report takes format version {report.FormatVersion}, and this build writes format version {CrashFormat.Current} (D-166).",
                nameof(report));
        }

        string text = WriteCrashLine(report) + "\n";
        if (report.Record is not null)
        {
            // The record text ends with a line feed of its own (D-652).
            text += RunRecordText.Write(report.Record);
        }

        return text;
    }

    /// <summary>Reads a crash file.</summary>
    /// <param name="text">The text of the file.</param>
    /// <param name="file">The path of the file, which every error names (T-2).</param>
    /// <returns>The crash and the record of the run.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <exception cref="ArgumentException">The file has no character (T-2).</exception>
    /// <exception cref="CrashException">
    /// The file holds no line, a line is malformed, a field is absent, the format version is
    /// not the version of this build, or the record lines do not match the crash line (T-2).
    /// </exception>
    public static CrashReport Read(string text, string file)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(file);

        IReadOnlyList<string> lines = SplitLines(text, file);
        Head head = ReadHead(lines[0], file);

        if (head.Report.FormatVersion != CrashFormat.Current)
        {
            throw CrashException.ForFile(
                file,
                $"the file takes format version {head.Report.FormatVersion}, and this build reads format version {CrashFormat.Current}");
        }

        if (!head.HasRecord)
        {
            if (lines.Count > 1)
            {
                throw CrashException.ForFile(
                    file,
                    $"line 1 says that the file holds no record, and the file holds {lines.Count - 1} lines after it");
            }

            return head.Report;
        }

        if (lines.Count < 2)
        {
            throw CrashException.ForFile(
                file, "line 1 says that the file holds a record, and the file holds no line after it");
        }

        return head.Report with { Record = ReadRecord(lines, file) };
    }

    private static string WriteCrashLine(CrashReport report)
    {
        ArrayBufferWriter<byte> bytes = new();
        using (Utf8JsonWriter writer = new(bytes, new JsonWriterOptions { Indented = false, SkipValidation = false }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("format", report.FormatVersion);
            writer.WriteString("time", report.Time);
            writer.WriteString("type", report.ErrorType);
            writer.WriteString("error", report.Error);
            writer.WriteString("stack", report.Stack);
            writer.WriteNumber("simulation", report.SimulationVersion);
            writer.WriteString("game", report.GameVersion);
            writer.WriteBoolean("record", report.Record is not null);
            writer.WriteEndObject();
        }

        // The writer escapes every line feed of the stack, so the crash line stays one line.
        return Encoding.UTF8.GetString(bytes.WrittenSpan);
    }

    private static Head ReadHead(string line, string file)
    {
        try
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes(line), file);
            Head head = ReadFields(ref reader);
            reader.ReadFileEnd();
            return head;
        }
        catch (ContentException error)
        {
            throw CrashException.ForFile(file, $"line 1 is not a crash line: {error.Message}", error);
        }
        catch (ArgumentException error)
        {
            throw CrashException.ForFile(file, $"line 1 is not a crash line: {error.Message}", error);
        }
    }

    private static Head ReadFields(ref ContentReader reader)
    {
        int? format = null;
        string? time = null;
        string? type = null;
        string? error = null;
        string? stack = null;
        int? simulation = null;
        string? game = null;
        bool? record = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "format":
                    format = reader.ReadInt();
                    break;
                case "time":
                    time = reader.ReadString();
                    break;
                case "type":
                    type = reader.ReadString();
                    break;
                case "error":
                    error = reader.ReadString();
                    break;
                case "stack":
                    // The field can hold no character, because an error that no throw carried
                    // holds no stack. An empty text is thus a value, and not an absent field,
                    // and `Require` below fails on the absent field alone (T-2).
                    stack = reader.ReadString();
                    break;
                case "simulation":
                    simulation = reader.ReadInt();
                    break;
                case "game":
                    game = reader.ReadString();
                    break;
                case "record":
                    record = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        CrashReport report = new(
            reader.RequireInt(format, depth, "format"),
            reader.Require(time, depth, "time"),
            reader.Require(type, depth, "type"),
            reader.Require(error, depth, "error"),
            reader.Require(stack, depth, "stack"),
            reader.RequireInt(simulation, depth, "simulation"),
            reader.Require(game, depth, "game"),
            null);

        return new Head(report, reader.RequireValue(record, depth, "record"));
    }

    private static RunRecord ReadRecord(IReadOnlyList<string> lines, string file)
    {
        StringBuilder text = new();
        for (int index = 1; index < lines.Count; index += 1)
        {
            text.Append(lines[index]).Append('\n');
        }

        try
        {
            return RunRecordText.Read(text.ToString());
        }
        catch (RunRecordException error)
        {
            throw CrashException.ForFile(
                file, $"the lines after line 1 are not a record: {error.Message}", error);
        }
    }

    private static IReadOnlyList<string> SplitLines(string text, string file)
    {
        TextLines split = TextLines.Split(text);
        if (split.Fault is not null)
        {
            throw CrashException.ForFile(
                file,
                split.FaultLine == 0 ? split.Fault : $"line {split.FaultLine}: {split.Fault}");
        }

        IReadOnlyList<string> lines = split.Lines;
        if (lines.Count == 0)
        {
            throw CrashException.ForFile(file, "it holds no line, and every crash file holds the crash line");
        }

        return lines;
    }

    private sealed record Head(CrashReport Report, bool HasRecord);
}
