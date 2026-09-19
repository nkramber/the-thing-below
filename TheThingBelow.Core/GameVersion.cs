namespace TheThingBelow.Core;

/// <summary>
/// The version of the build that a person reads. One constant holds it, and the export
/// preset and the release workflow read that constant (D-653).
/// </summary>
/// <remarks>
/// The version is a label for people. The simulation version, the snapshot format versions,
/// and the content hash carry compatibility, and never this number (D-448, G-17).
/// <para>
/// The form is 0.MINOR.PATCH until the full game ships as 1.0.0. The minor number follows
/// the gate of the build: 0.2 at the first playable, and 0.5 at the prologue (D-448). A
/// release tag is <see cref="TagPrefix"/> and this value, such as `v0.5.0`. PR-31 writes the
/// test that the tag of a release matches this constant.
/// </para>
/// </remarks>
public static class GameVersion
{
    /// <summary>The version of this build. Phase 1 builds carry 0.1.0 (D-448).</summary>
    public const string Current = "0.1.0";

    /// <summary>The first character of a release tag, such as the `v` of `v0.5.0` (D-448).</summary>
    public const string TagPrefix = "v";

    /// <summary>The tag of a release of this build, such as `v0.1.0`.</summary>
    public const string Tag = TagPrefix + Current;
}
