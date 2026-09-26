using System;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Storage;

namespace TheThingBelow.Game;

/// <summary>
/// One save that a rule of the run asked for, with the document that <see cref="GameRun.Save"/>
/// took at the end of its tick (D-224, D-1115, D-1132). The host writes it to its file.
/// </summary>
/// <param name="Kind">The file of the save: the slot save of a hub service, or the autosave of the entry to a hub.</param>
/// <param name="Document">The header of this build and the snapshot of the run.</param>
public sealed record SaveWrite(SaveKind Kind, SaveDocument Document)
{
    /// <summary>Gives the file of the save that a rule asked for (D-224, D-1132).</summary>
    /// <param name="kind">The kind of the request.</param>
    /// <returns>The slot save for a slot request, and the autosave for an autosave request.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind of request (T-2).</exception>
    public static SaveKind FileOf(SaveRequestKind kind) => kind switch
    {
        SaveRequestKind.Slot => SaveKind.Slot,
        SaveRequestKind.Autosave => SaveKind.Autosave,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The run asks for no such save (D-1132, T-2)."),
    };
}
