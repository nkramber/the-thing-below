using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
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
            WriteMap(writer, snapshot.Map);
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

    /// <summary>
    /// Writes the party on its map. A snapshot of save format 1 holds no map, and this build
    /// writes save format 2, so the field is always present in a file that this build writes
    /// (D-166, D-654).
    /// </summary>
    private static void WriteMap(Utf8JsonWriter writer, MapSnapshot? map)
    {
        if (map is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds a map. A snapshot with no map comes from save format 1, and this build never writes one (T-2, D-166).",
                nameof(map));
        }

        writer.WriteStartObject("map");
        writer.WriteString("id", map.Map.Value);
        writer.WriteNumber("x", map.LeadX);
        writer.WriteNumber("y", map.LeadY);
        writer.WriteString("facing", StepDirections.NameOf(map.Facing));
        if (map.Stepping is StepDirection stepping)
        {
            writer.WriteString("stepping", StepDirections.NameOf(stepping));
        }

        writer.WriteNumber("step_ticks", map.StepTicks);
        writer.WriteStartArray("walked");
        foreach (string row in map.Walked)
        {
            writer.WriteStringValue(row);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
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
        MapSnapshot? map = null;
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
                case "map":
                    map = ReadMap(ref reader);
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
            reader.Require(map, depth, "map"),
            reader.Require(streams, depth, "streams"));

        snapshot.Check(reader.File);
        return snapshot;
    }

    /// <summary>
    /// Reads a snapshot of save format 1, which holds no map (D-166, D-654). The migration
    /// runs in `RunState.Resume`, which holds the content of this build and can find the
    /// spawn point of the first map.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <returns>The snapshot, with no map.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatOne(ref ContentReader reader)
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

        // The patrol of the first world walked on a beat, and PR-7 replaced it with the
        // party on a tile map. The two counts take a read, so an unknown field still fails,
        // and the state of that patrol reaches no rule of this build (T-2).
        _ = reader.RequireInt(beats, depth, "beats");
        _ = reader.RequireInt(choice, depth, "choice");

        RunSnapshot snapshot = new(
            reader.RequireValue(tick, depth, "tick"),
            reader.RequireValue(menu, depth, "menu"),
            reader.RequireValue(world, depth, "world"),
            null,
            reader.Require(streams, depth, "streams"));

        snapshot.Check(reader.File);
        return snapshot;
    }

    private static MapSnapshot ReadMap(ref ContentReader reader)
    {
        ContentId? id = null;
        int? x = null;
        int? y = null;
        string? facing = null;
        string? stepping = null;
        int? stepTicks = null;
        List<string>? walked = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(GameMap.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                case "stepping":
                    stepping = reader.ReadString();
                    break;
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "walked":
                    walked = ReadWalked(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new MapSnapshot(
            reader.Require(id, depth, "id"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            ReadDirection(ref reader, reader.Require(facing, depth, "facing"), "facing"),
            stepping is null ? null : ReadDirection(ref reader, stepping, "stepping"),
            reader.RequireInt(stepTicks, depth, "step_ticks"),
            reader.Require(walked, depth, "walked"));
    }

    private static StepDirection ReadDirection(ref ContentReader reader, string name, string field)
    {
        foreach (StepDirection candidate in StepDirections.All)
        {
            if (string.CompareOrdinal(StepDirections.NameOf(candidate), name) == 0)
            {
                return candidate;
            }
        }

        throw reader.Refuse($"the field '{field}' holds '{name}', and a direction is north, south, east, or west (D-716)");
    }

    private static List<string> ReadWalked(ref ContentReader reader)
    {
        List<string> rows = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, rows.Count))
        {
            rows.Add(reader.ReadString());
        }

        return rows;
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
