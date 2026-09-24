namespace TheThingBelow.Storage;

/// <summary>The format version of the settings file (D-570, D-869).</summary>
/// <remarks>
/// PR-63 wrote format 1 (D-869). PR-62 raised it to 2: the controls gained the map action, and
/// the step from format 1 adds its default key and button (D-986, D-990). PR-91 raised it to 3: the controls gained the torch action, and the step from format 2 adds its default key and button (D-1068). The PR that adds the
/// next setting raises <see cref="Current"/>, adds the step from the version that it leaves,
/// and commits a fixture file of that version. The fixture test of Tests fails a raise with no
/// fixture (D-570).
/// </remarks>
public static class SettingsFormat
{
    /// <summary>The oldest format version that this build reads.</summary>
    public const int Oldest = 1;

    /// <summary>The format version that this build writes.</summary>
    public const int Current = 3;
}
