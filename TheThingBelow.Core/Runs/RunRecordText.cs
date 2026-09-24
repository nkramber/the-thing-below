using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The text of a run record: one JSON object on each line, as the logs are (D-179, D-652).
/// Core writes and reads the text, and Storage writes the bytes to a file (D-494).
/// </summary>
/// <remarks>
/// A person reads a record in a diff, in a review, and in a crash email with no tool, so the
/// triage of a crash needs no dump command (D-170, D-473, T-1).
/// <para>
/// The lines take a fixed order: the header, the snapshot, one line for each tick that holds
/// an intent, and the end tick. A seed and the position of a stream fill 64 bits, so each
/// one takes the hexadecimal text form of `ReadHexUInt64` (T-7).
/// </para>
/// </remarks>
public static class RunRecordText
{
    /// <summary>The name that an error of a line carries, beside the line number (T-2).</summary>
    public const string RecordName = "the record";

    /// <summary>The count of lines that every record holds: the header, the snapshot, and the end.</summary>
    private const int FixedLineCount = 3;

    /// <summary>Writes a record as one JSON object on each line (D-652).</summary>
    /// <param name="record">The record.</param>
    /// <returns>The text, which ends with a line ending.</returns>
    /// <exception cref="ArgumentNullException">The record is null (T-2).</exception>
    public static string Write(RunRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        StringBuilder text = new();
        text.Append(WriteHeader(record.Header)).Append('\n');
        text.Append(RunSnapshotText.Write(record.Snapshot)).Append('\n');
        foreach (TickIntents entry in record.Ticks)
        {
            text.Append(WriteTick(entry)).Append('\n');
        }

        text.Append(WriteEnd(record.EndTick)).Append('\n');
        return text.ToString();
    }

    /// <summary>Reads a record from its text.</summary>
    /// <param name="text">The text of the record, with one JSON object on each line.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <exception cref="RunRecordException">
    /// A line is malformed, the record holds too few lines, or a value breaks a rule of the
    /// record. Every message names the line (T-2).
    /// </exception>
    public static RunRecord Read(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        IReadOnlyList<string> lines = SplitLines(text);
        if (lines.Count < FixedLineCount)
        {
            throw RunRecordException.ForRecord(
                $"it holds {lines.Count} lines, and a record holds the header, the snapshot, and the end tick");
        }

        RunHeader header = ReadLine(lines[0], 1, ReadHeader);
        RunSnapshot snapshot = ReadLine(lines[1], 2, RunSnapshotText.Read);

        List<TickIntents> ticks = [];
        for (int index = 2; index < lines.Count - 1; index += 1)
        {
            ticks.Add(ReadLine(lines[index], index + 1, ReadTick));
        }

        long endTick = ReadLine(lines[^1], lines.Count, ReadEnd);

        try
        {
            return new RunRecord(header, snapshot, ticks, endTick);
        }
        catch (ArgumentException error)
        {
            throw RunRecordException.ForRecord(error.Message, error);
        }
    }

    private static IReadOnlyList<string> SplitLines(string text)
    {
        TextLines split = TextLines.Split(text);
        if (split.Fault is not null)
        {
            throw split.FaultLine == 0
                ? RunRecordException.ForRecord(split.Fault)
                : RunRecordException.ForLine(split.FaultLine, split.Fault);
        }

        return split.Lines;
    }

    private static T ReadLine<T>(string line, int number, ReadLineFunction<T> read)
    {
        try
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes(line), RecordName);
            T value = read(ref reader);
            reader.ReadFileEnd();
            return value;
        }
        catch (ContentException error)
        {
            throw RunRecordException.ForLine(number, error.Message, error);
        }
        catch (ArgumentException error)
        {
            throw RunRecordException.ForLine(number, error.Message, error);
        }
    }

    private delegate T ReadLineFunction<T>(ref ContentReader reader);

    private static string WriteHeader(RunHeader header)
    {
        ArrayBufferWriter<byte> bytes = new();
        using (Utf8JsonWriter writer = NewWriter(bytes))
        {
            writer.WriteStartObject();
            writer.WriteNumber("format", header.FormatVersion);
            writer.WriteNumber("simulation", header.SimulationVersion);
            writer.WriteString("content", header.ContentHash);
            writer.WriteString("seed", HexText.Of(header.Seed));
            writer.WriteString("game", header.GameVersion);
            writer.WriteEndObject();
        }

        return AsText(bytes);
    }

    private static RunHeader ReadHeader(ref ContentReader reader)
    {
        int? format = null;
        int? simulation = null;
        string? content = null;
        ulong? seed = null;
        string? game = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "format":
                    format = reader.ReadInt();
                    break;
                case "simulation":
                    simulation = reader.ReadInt();
                    break;
                case "content":
                    content = reader.ReadString();
                    break;
                case "seed":
                    seed = reader.ReadHexUInt64();
                    break;
                case "game":
                    game = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new RunHeader(
            reader.RequireInt(format, depth, "format"),
            reader.RequireInt(simulation, depth, "simulation"),
            reader.Require(content, depth, "content"),
            reader.RequireValue(seed, depth, "seed"),
            reader.Require(game, depth, "game"));
    }

    private static string WriteTick(TickIntents entry)
    {
        ArrayBufferWriter<byte> bytes = new();
        using (Utf8JsonWriter writer = NewWriter(bytes))
        {
            writer.WriteStartObject();
            writer.WriteNumber("tick", entry.Tick);
            writer.WriteStartArray("intents");
            foreach (Intent intent in entry.Intents)
            {
                writer.WriteStartObject();
                writer.WriteString("action", intent.Action.Value);
                writer.WriteBoolean("debug", intent.IsDebug);
                if (intent.Item is ContentId item)
                {
                    writer.WriteString("item", item.Value);
                }

                if (intent.Target is BattleTarget target)
                {
                    writer.WriteStartObject("target");
                    writer.WriteString("side", BattleSides.NameOf(target.Side));
                    writer.WriteNumber("slot", target.Slot);
                    writer.WriteEndObject();
                }

                if (intent.Option is int option)
                {
                    writer.WriteNumber("option", option);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return AsText(bytes);
    }

    private static TickIntents ReadTick(ref ContentReader reader)
    {
        long? tick = null;
        List<Intent>? intents = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "tick":
                    tick = reader.ReadLong();
                    break;
                case "intents":
                    intents = ReadIntents(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new TickIntents(
            reader.RequireValue(tick, depth, "tick"),
            reader.Require(intents, depth, "intents"));
    }

    private static List<Intent> ReadIntents(ref ContentReader reader)
    {
        List<Intent> intents = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, intents.Count))
        {
            intents.Add(ReadIntent(ref reader));
        }

        return intents;
    }

    private static Intent ReadIntent(ref ContentReader reader)
    {
        ContentId? action = null;
        bool? debug = null;
        ContentId? item = null;
        BattleTarget? target = null;
        int? option = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "action":
                    action = reader.ReadContentId();
                    break;
                case "debug":
                    debug = reader.ReadBoolean();
                    break;
                case "item":
                    item = reader.ReadContentId(BattleFixture.ItemKind);
                    break;
                case "target":
                    target = ReadTarget(ref reader);
                    break;
                case "option":
                    option = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new Intent(
            reader.Require(action, depth, "action"),
            reader.RequireValue(debug, depth, "debug"),
            target,
            item,
            option);
    }

    private static BattleTarget ReadTarget(ref ContentReader reader)
    {
        string? side = null;
        int? slot = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "side":
                    side = reader.ReadString();
                    break;
                case "slot":
                    slot = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string name = reader.Require(side, depth, "side");
        if (!BattleSides.TrySideOf(name, out BattleSide parsed))
        {
            throw reader.RefuseField(depth, "side", $"the side '{name}' is not one of {BattleSides.EverySideName} (D-764)");
        }

        return new BattleTarget(parsed, reader.RequireInt(slot, depth, "slot"));
    }

    private static string WriteEnd(long endTick)
    {
        ArrayBufferWriter<byte> bytes = new();
        using (Utf8JsonWriter writer = NewWriter(bytes))
        {
            writer.WriteStartObject();
            writer.WriteNumber("end", endTick);
            writer.WriteEndObject();
        }

        return AsText(bytes);
    }

    private static long ReadEnd(ref ContentReader reader)
    {
        long? end = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "end":
                    end = reader.ReadLong();
                    break;
                default:
                    // A record that a crash cut off ends on a tick line, so the message
                    // names the rule of the last line and not an unknown field (T-2).
                    throw ContentException.ForField(
                        RecordName, field, "the last line of a record holds the end tick alone");
            }
        }

        return reader.RequireValue(end, depth, "end");
    }

    /// <summary>
    /// Makes the writer of one line. The writer holds no indent, so one object takes one
    /// line, and it escapes every text value itself (D-652).
    /// </summary>
    private static Utf8JsonWriter NewWriter(ArrayBufferWriter<byte> bytes) =>
        new(bytes, new JsonWriterOptions { Indented = false, SkipValidation = false });

    private static string AsText(ArrayBufferWriter<byte> bytes) =>
        Encoding.UTF8.GetString(bytes.WrittenSpan);
}
