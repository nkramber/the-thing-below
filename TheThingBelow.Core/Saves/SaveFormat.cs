namespace TheThingBelow.Core.Saves;

/// <summary>The format version of a save file. PR-43 wrote the first one (D-166, D-654).</summary>
/// <remarks>
/// A save holds one snapshot, and a load reads that snapshot alone (D-259). Thus the shape of
/// the snapshot line is the format of a save, and a PR that changes the snapshot raises
/// <see cref="Current"/>, keeps a reader for each older version, and commits a fixture save of
/// each one (D-166, D-654). The fixture test of Tests fails a raise with no fixture.
/// </remarks>
public static class SaveFormat
{
    /// <summary>The oldest format version that this build reads.</summary>
    public const int Oldest = 1;

    /// <summary>The format version that this build writes.</summary>
    public const int Current = 1;
}
