using System;
using CoreGameVersion = TheThingBelow.Core.GameVersion;

namespace TheThingBelow.Core.Runs;

/// <summary>The format version of the run record. PR-6 wrote the first one (D-652).</summary>
/// <remarks>
/// A later PR that changes the lines of a record raises this number. A record replays on its
/// own simulation version alone (G-17), so a record of an older format fails with its line
/// and needs no reader (D-764).
/// <para>
/// PR-9 raised it to 2. An intent gained its target and its item (D-764, D-780).
/// </para>
/// <para>
/// PR-103 raised it to 3. PR-68 and PR-12 gave an intent its option, its lesson, and its actor,
/// and neither raised the number, so a record of that time reads as format 2 in error.
/// </para>
/// </remarks>
public static class RunRecordFormat
{
    /// <summary>The format version that this build writes.</summary>
    public const int Current = 3;
}

/// <summary>
/// The first line of a run record: the format version, the simulation version, the content
/// hash, the seed, and the game version (G-5, D-448).
/// </summary>
/// <remarks>
/// A replay compares each version and the content hash with this build, and a mismatch stops
/// with a report that names both values (T-2). The game version is a label for people, and
/// no check reads it, because the simulation version and the content hash carry
/// compatibility (D-448, D-653).
/// </remarks>
/// <param name="FormatVersion">The version of the lines of the record (D-652).</param>
/// <param name="SimulationVersion">The version of the rules that wrote the record (G-17).</param>
/// <param name="ContentHash">The content hash of the rule files of the run (D-648, G-5).</param>
/// <param name="Seed">The seed that started the run (G-3, G-4).</param>
/// <param name="GameVersion">The version of the build that a person reads (D-448, D-653).</param>
public sealed record RunHeader(
    int FormatVersion,
    int SimulationVersion,
    string ContentHash,
    ulong Seed,
    string GameVersion)
{
    /// <summary>Makes the header that this build writes for a new run.</summary>
    /// <param name="contentHash">The content hash that the run loaded (D-648).</param>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>The header.</returns>
    /// <exception cref="ArgumentException">The content hash has no character (T-2).</exception>
    public static RunHeader ForThisBuild(string contentHash, ulong seed)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentHash);

        return new RunHeader(
            RunRecordFormat.Current,
            TheThingBelow.Core.SimulationVersion.Current,
            contentHash,
            seed,
            CoreGameVersion.Current);
    }

    /// <summary>Fails when this build cannot replay a record with this header (G-5, T-2).</summary>
    /// <param name="contentHash">The content hash that this host loaded (D-648).</param>
    /// <exception cref="ArgumentException">The content hash has no character (T-2).</exception>
    /// <exception cref="RunRecordException">
    /// A version or the content hash differs, and the message names both values (T-2).
    /// </exception>
    public void CheckAgainstThisBuild(string contentHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentHash);

        this.CheckVersions();

        if (string.CompareOrdinal(this.ContentHash, contentHash) != 0)
        {
            throw RunRecordException.ForLine(
                1,
                $"the record names the content hash {this.ContentHash}, and this build loaded the content hash {contentHash} (G-5)");
        }
    }

    /// <summary>Compares the format version and the simulation version with this build (G-17, D-652).</summary>
    /// <exception cref="RunRecordException">A version differs. The message names both values (T-2).</exception>
    /// <remarks>
    /// The reader of a record calls this method after line 1 and before line 2. A snapshot of an
    /// older version failed on a field that this build added, and the error hid the version (T-2).
    /// </remarks>
    public void CheckVersions()
    {
        if (this.FormatVersion != RunRecordFormat.Current)
        {
            throw RunRecordException.ForLine(
                1,
                $"the record takes format version {this.FormatVersion}, and this build reads format version {RunRecordFormat.Current}");
        }

        if (this.SimulationVersion != TheThingBelow.Core.SimulationVersion.Current)
        {
            throw RunRecordException.ForLine(
                1,
                $"the record comes from simulation version {this.SimulationVersion}, and this build runs simulation version {TheThingBelow.Core.SimulationVersion.Current} (G-17)");
        }
    }
}
