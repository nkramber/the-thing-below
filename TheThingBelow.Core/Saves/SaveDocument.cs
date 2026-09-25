using System;
using TheThingBelow.Core.Runs;
using CoreGameVersion = TheThingBelow.Core.GameVersion;

namespace TheThingBelow.Core.Saves;

/// <summary>
/// The first line of a save file, beside the checksum of the snapshot line (D-178, D-655).
/// </summary>
/// <remarks>
/// A load reads the snapshot alone (D-259), and the format version alone picks the reader of
/// the snapshot line (D-166). The simulation version and the content hash tell a resume which
/// build wrote the save (<see cref="OriginFor"/>). A save of another build can follow an edit of
/// a map or of a story scene (D-1111, D-1112). A change of a party rule still refuses it, and
/// the patch that makes one ships a migration (D-1110). The game version is a value for a
/// report alone.
/// </remarks>
/// <param name="FormatVersion">The version of the lines of the save (D-166, D-654).</param>
/// <param name="SimulationVersion">The version of the rules that wrote the save (G-17).</param>
/// <param name="ContentHash">The content hash of the rule files of the run (D-648, G-5).</param>
/// <param name="GameVersion">The version of the build that a person reads (D-448, D-653).</param>
/// <param name="Seed">The seed of the run, which a resume of the state needs (G-3, G-4).</param>
public sealed record SaveHeader(
    int FormatVersion,
    int SimulationVersion,
    string ContentHash,
    string GameVersion,
    ulong Seed)
{
    /// <summary>Makes the header that this build writes for a save.</summary>
    /// <param name="contentHash">The content hash that the run loaded (D-648).</param>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>The header.</returns>
    /// <exception cref="ArgumentException">The content hash has no character (T-2).</exception>
    public static SaveHeader ForThisBuild(string contentHash, ulong seed)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentHash);

        return new SaveHeader(
            SaveFormat.Current,
            TheThingBelow.Core.SimulationVersion.Current,
            contentHash,
            CoreGameVersion.Current,
            seed);
    }

    /// <summary>
    /// Tells which build wrote the save, as a resume reads it: this build holds the same
    /// simulation version and the same content hash (D-1111).
    /// </summary>
    /// <param name="contentHash">The content hash that this build loaded (D-648).</param>
    /// <returns>This build, or another build.</returns>
    /// <exception cref="ArgumentException">The content hash has no character (T-2).</exception>
    public SnapshotOrigin OriginFor(string contentHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentHash);

        bool same = this.SimulationVersion == TheThingBelow.Core.SimulationVersion.Current
            && string.CompareOrdinal(this.ContentHash, contentHash) == 0;
        return same ? SnapshotOrigin.ThisBuild : SnapshotOrigin.OtherBuild;
    }
}

/// <summary>
/// One save: the header and the snapshot of the run at the tick of the save (D-259, D-655).
/// </summary>
/// <remarks>
/// The checksum of the file is no field of this record. It guards the bytes of the file, and
/// <see cref="SaveText"/> alone writes it and reads it (D-178).
/// <para>
/// Core makes this record and its text, and Storage writes the text to a file (D-494).
/// </para>
/// </remarks>
/// <param name="Header">The versions, the content hash, and the seed of the run.</param>
/// <param name="Snapshot">The state that a load resumes from (D-259).</param>
public sealed record SaveDocument(SaveHeader Header, RunSnapshot Snapshot);
