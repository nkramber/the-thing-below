using System;

namespace TheThingBelow.Core.Runs;

/// <summary>The kind of a save that a rule of Core asks the host to write (D-224, D-1132).</summary>
/// <remarks>
/// Core does no file work (G-1). A rule emits a save request as output, and Game writes the save
/// through `GameRun.Save` after the tick (D-1132). The request is output and not state, so no
/// snapshot and no hash reads it, as for a notice (D-168).
/// </remarks>
public enum SaveRequestKind
{
    /// <summary>The slot save, which the save service of a hub writes (D-1132).</summary>
    Slot,

    /// <summary>The autosave, which the entry to a hub writes (D-224, D-1132).</summary>
    Autosave,
}

/// <summary>The names of the save request kinds, for a log field and an error (T-2).</summary>
public static class SaveRequestKinds
{
    /// <summary>Every kind, in one fixed order for a walk of them (G-4).</summary>
    public static readonly SaveRequestKind[] All = [SaveRequestKind.Slot, SaveRequestKind.Autosave];

    /// <summary>Gives the name of one kind.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `slot`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(SaveRequestKind kind) => kind switch
    {
        SaveRequestKind.Slot => "slot",
        SaveRequestKind.Autosave => "autosave",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no save request kind (D-1132)"),
    };
}
