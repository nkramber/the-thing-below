using System.Collections.Generic;

namespace TheThingBelow.Tools.Night;

/// <summary>The legs of the night job, the names of their night records, and the seed step of a night (D-505, D-509, D-1190).</summary>
public static class NightLegs
{
    /// <summary>
    /// The runner label of each leg, in ordinal order. The matrix of `.github/workflows/night.yml`
    /// names the same labels, and a test compares the two lists.
    /// </summary>
    public static readonly IReadOnlyList<string> Labels = ["macos-26", "ubuntu-24.04", "windows-2025"];

    /// <summary>
    /// The seed step of D-1190. The night with run number N plays from the seed N times this step,
    /// so no night meets the seeds of another night or of the bot job of a PR, which start at 1.
    /// </summary>
    public const ulong SeedStep = 1_000_000_000;

    /// <summary>The start of the name of each night record and of its artifact.</summary>
    public const string RecordPrefix = "night-record-";

    /// <summary>Gives the name of the artifact of the night record of one leg, such as `night-record-macos-26`.</summary>
    /// <param name="leg">The runner label of the leg.</param>
    /// <returns>The name.</returns>
    public static string ArtifactOf(string leg) => RecordPrefix + leg;

    /// <summary>Gives the file name of the night record of one leg, such as `night-record-macos-26.json`.</summary>
    /// <param name="leg">The runner label of the leg.</param>
    /// <returns>The file name with no folder.</returns>
    public static string FileOf(string leg) => ArtifactOf(leg) + ".json";
}
