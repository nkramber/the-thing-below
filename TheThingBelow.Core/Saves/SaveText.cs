using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Saves;

/// <summary>
/// The text of a save file: the header on line 1, and the snapshot on line 2 (D-652, D-655).
/// Core writes and reads the text, and Storage writes the bytes to a file (D-494).
/// </summary>
/// <remarks>
/// The header carries the SHA-256 digest of the bytes of line 2, so a torn write, a bad
/// sector, or a hand edit of the snapshot fails the load with the file and both digests
/// (D-178, T-2). The digest covers one line and no line ending, so a reader can check a save
/// with one shell command.
/// <para>
/// A snapshot takes one JSON object, as D-652 sets, and the checksum thus needs no field
/// inside that object and no second file beside the save (D-655).
/// </para>
/// </remarks>
public static class SaveText
{
    /// <summary>The count of lines of every save file: the header and the snapshot.</summary>
    public const int LineCount = 2;

    /// <summary>Writes a save as two lines of JSON, each with a line feed after it.</summary>
    /// <param name="document">The header and the snapshot of the save.</param>
    /// <returns>The text of the file.</returns>
    /// <exception cref="ArgumentNullException">The document is null (T-2).</exception>
    /// <exception cref="ArgumentException">
    /// The header names a format version that this build does not write (T-2).
    /// </exception>
    public static string Write(SaveDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(document.Header);
        ArgumentNullException.ThrowIfNull(document.Snapshot);

        if (document.Header.FormatVersion != SaveFormat.Current)
        {
            throw new ArgumentException(
                $"The header takes format version {document.Header.FormatVersion}, and this build writes format version {SaveFormat.Current} (D-166).",
                nameof(document));
        }

        string snapshotLine = RunSnapshotText.Write(document.Snapshot);
        string headerLine = WriteHeader(document.Header, ChecksumOf(snapshotLine));
        return headerLine + "\n" + snapshotLine + "\n";
    }

    /// <summary>Reads a save from its text.</summary>
    /// <param name="text">The text of the file.</param>
    /// <param name="file">The path of the file, which every error names (T-2).</param>
    /// <returns>The header and the snapshot, which describes a state of a run.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <exception cref="ArgumentException">The file has no character (T-2).</exception>
    /// <exception cref="SaveException">
    /// The file holds other than two lines, a line is malformed, the checksum does not
    /// match, or this build reads no such format version (T-2).
    /// </exception>
    public static SaveDocument Read(string text, string file)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(file);

        IReadOnlyList<string> lines = SplitLines(text, file);
        HeaderFields header = ReadLine(lines[0], file, ReadHeaderFields);
        CheckFormat(header.Header.FormatVersion, file);
        CheckChecksum(lines[1], header.Checksum, file);

        return new SaveDocument(
            header.Header,
            ReadSnapshotLine(lines[1], header.Header.FormatVersion, file));
    }

    /// <summary>Gives the checksum that the header of a save carries (D-178).</summary>
    /// <param name="snapshotLine">The snapshot line, with no line ending.</param>
    /// <returns>The SHA-256 digest of the bytes of that line, as 64 hexadecimal characters.</returns>
    /// <exception cref="ArgumentNullException">The line is null (T-2).</exception>
    public static string ChecksumOf(string snapshotLine)
    {
        ArgumentNullException.ThrowIfNull(snapshotLine);

        return Sha256.ComputeHex(Encoding.UTF8.GetBytes(snapshotLine));
    }

    private static IReadOnlyList<string> SplitLines(string text, string file)
    {
        TextLines split = TextLines.Split(text);
        if (split.Fault is not null)
        {
            throw SaveException.ForFile(
                file,
                split.FaultLine == 0 ? split.Fault : $"line {split.FaultLine}: {split.Fault}");
        }

        IReadOnlyList<string> lines = split.Lines;
        if (lines.Count != LineCount)
        {
            throw SaveException.ForFile(
                file,
                $"it holds {lines.Count} lines, and a save holds the header and the snapshot");
        }

        return lines;
    }

    private static void CheckFormat(int format, string file)
    {
        if (format > SaveFormat.Current)
        {
            throw SaveException.ForFile(
                file,
                $"it takes format version {format}, and this build writes format version {SaveFormat.Current}. A newer build wrote it (D-166)");
        }

        if (format < SaveFormat.Oldest)
        {
            throw SaveException.ForFile(
                file,
                $"it takes format version {format}, and this build reads format version {SaveFormat.Oldest} and later (D-166)");
        }
    }

    private static void CheckChecksum(string snapshotLine, string checksum, string file)
    {
        string found = ChecksumOf(snapshotLine);
        if (string.CompareOrdinal(found, checksum) != 0)
        {
            throw SaveException.ForFile(
                file,
                $"the header names the checksum {checksum}, and the snapshot line gives {found}. A write stopped, or the file changed (D-178)");
        }
    }

    /// <summary>
    /// Reads the snapshot line with the reader of its format version (D-166, D-654). Each
    /// reader gives the snapshot of this build, so the caller needs no second step.
    /// </summary>
    /// <remarks>
    /// Format 1 holds the world of Phase 1, which is a patrol on a beat, and it holds no
    /// map. Its reader gives a snapshot with no map, and `RunState.Resume` then puts the
    /// party on the spawn point of the first map (D-166, D-654).
    /// <para>
    /// The PR that next changes the snapshot raises <see cref="SaveFormat.Current"/>, adds a
    /// reader of each older version, and commits a fixture save of the version that it
    /// leaves (D-166, D-654).
    /// </para>
    /// </remarks>
    private static RunSnapshot ReadSnapshotLine(string line, int format, string file) =>
        format switch
        {
            1 => ReadLine(line, file, RunSnapshotText.ReadFormatOne),
            2 => ReadLine(line, file, RunSnapshotText.Read),

            // `CheckFormat` passed, so this build named the version and wrote no reader for
            // it. The message thus names a fault of the build and never a fault of the file (T-2).
            _ => throw SaveException.ForFile(
                file,
                $"this build reads format version {SaveFormat.Oldest} to {SaveFormat.Current} and holds no reader for version {format} (D-166)"),
        };

    private static string WriteHeader(SaveHeader header, string checksum)
    {
        ArrayBufferWriter<byte> bytes = new();

        // The writer holds no indent, so one object takes one line, and it escapes every
        // text value itself (D-652).
        using (Utf8JsonWriter writer = new(bytes, new JsonWriterOptions { Indented = false, SkipValidation = false }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("format", header.FormatVersion);
            writer.WriteNumber("simulation", header.SimulationVersion);
            writer.WriteString("content", header.ContentHash);
            writer.WriteString("game", header.GameVersion);
            writer.WriteString("seed", HexText.Of(header.Seed));
            writer.WriteString("checksum", checksum);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(bytes.WrittenSpan);
    }

    private static HeaderFields ReadHeaderFields(ref ContentReader reader)
    {
        int? format = null;
        int? simulation = null;
        string? content = null;
        string? game = null;
        ulong? seed = null;
        string? checksum = null;

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
                case "game":
                    game = reader.ReadString();
                    break;
                case "seed":
                    seed = reader.ReadHexUInt64();
                    break;
                case "checksum":
                    checksum = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        SaveHeader header = new(
            reader.RequireInt(format, depth, "format"),
            reader.RequireInt(simulation, depth, "simulation"),
            reader.Require(content, depth, "content"),
            reader.Require(game, depth, "game"),
            reader.RequireValue(seed, depth, "seed"));

        string digest = reader.Require(checksum, depth, "checksum");
        if (digest.Length != Sha256.DigestSize * 2)
        {
            throw SaveException.ForFile(
                reader.File,
                $"the checksum holds {digest.Length} characters, and a SHA-256 digest holds {Sha256.DigestSize * 2} (D-178)");
        }

        return new HeaderFields(header, digest);
    }

    private static T ReadLine<T>(string line, string file, ReadLineFunction<T> read)
    {
        try
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes(line), file);
            T value = read(ref reader);
            reader.ReadFileEnd();
            return value;
        }
        catch (ContentException error)
        {
            throw SaveException.ForFile(file, error.Message, error);
        }
        catch (ArgumentException error)
        {
            throw SaveException.ForFile(file, error.Message, error);
        }
    }

    private delegate T ReadLineFunction<T>(ref ContentReader reader);

    private sealed record HeaderFields(SaveHeader Header, string Checksum);
}
