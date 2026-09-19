using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The text of one snapshot: one JSON object on one line (D-652). A run record holds it on
/// line 2, and a save file holds it on line 2, so one writer and one reader serve both
/// (D-494, D-655, T-1).
/// </summary>
/// <remarks>
/// Core makes the bytes of a snapshot and loads a state from them, and Storage writes those
/// bytes to a file (D-494). Thus no file code reads a field of a snapshot.
/// </remarks>
public static class RunSnapshotText
{
    /// <summary>Writes a snapshot as one JSON object with no line ending.</summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The line.</returns>
    /// <exception cref="ArgumentNullException">The snapshot is null (T-2).</exception>
    public static string Write(RunSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        ArrayBufferWriter<byte> bytes = new();

        // The writer holds no indent, so one object takes one line, and it escapes every
        // text value itself (D-652).
        using (Utf8JsonWriter writer = new(bytes, new JsonWriterOptions { Indented = false, SkipValidation = false }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("tick", snapshot.Tick);
            writer.WriteBoolean("menu", snapshot.MenuOpen);
            writer.WriteNumber("world", snapshot.WorldTick);
            writer.WriteNumber("beats", snapshot.PatrolBeats);
            writer.WriteNumber("choice", snapshot.PatrolChoice);
            writer.WriteStartArray("streams");
            foreach (StreamPosition position in snapshot.Streams)
            {
                writer.WriteStartObject();
                writer.WriteNumber("stream", (int)position.Stream);
                writer.WriteString("state", HexText.Of(position.State));
                writer.WriteString("increment", HexText.Of(position.Increment));
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(bytes.WrittenSpan);
    }

    /// <summary>Reads a snapshot from the object under the reader, and checks it (T-2).</summary>
    /// <param name="reader">The reader of the line, which names the record or the save file.</param>
    /// <returns>The snapshot, which describes a state of a run.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    /// <remarks>
    /// The check runs here, inside the read of the line, so the caller names the line and
    /// the file of every fault of a snapshot. A check after the read loses both (T-2).
    /// </remarks>
    public static RunSnapshot Read(ref ContentReader reader)
    {
        long? tick = null;
        bool? menu = null;
        long? world = null;
        int? beats = null;
        int? choice = null;
        List<StreamPosition>? streams = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "tick":
                    tick = reader.ReadLong();
                    break;
                case "menu":
                    menu = reader.ReadBoolean();
                    break;
                case "world":
                    world = reader.ReadLong();
                    break;
                case "beats":
                    beats = reader.ReadInt();
                    break;
                case "choice":
                    choice = reader.ReadInt();
                    break;
                case "streams":
                    streams = ReadStreams(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        RunSnapshot snapshot = new(
            reader.RequireValue(tick, depth, "tick"),
            reader.RequireValue(menu, depth, "menu"),
            reader.RequireValue(world, depth, "world"),
            reader.RequireInt(beats, depth, "beats"),
            reader.RequireInt(choice, depth, "choice"),
            reader.Require(streams, depth, "streams"));

        snapshot.Check(reader.File);
        return snapshot;
    }

    private static List<StreamPosition> ReadStreams(ref ContentReader reader)
    {
        List<StreamPosition> streams = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, streams.Count))
        {
            streams.Add(ReadStream(ref reader));
        }

        return streams;
    }

    private static StreamPosition ReadStream(ref ContentReader reader)
    {
        int? number = null;
        ulong? state = null;
        ulong? increment = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "stream":
                    number = reader.ReadInt();
                    break;
                case "state":
                    state = reader.ReadHexUInt64();
                    break;
                case "increment":
                    increment = reader.ReadHexUInt64();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StreamPosition(
            (StreamId)reader.RequireInt(number, depth, "stream"),
            reader.RequireValue(state, depth, "state"),
            reader.RequireValue(increment, depth, "increment"));
    }
}
