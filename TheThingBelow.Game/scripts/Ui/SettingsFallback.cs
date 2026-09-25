using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The field that the message of a refused settings file names (D-1099, D-1100). The class reads
/// no engine type, so a test calls it with no Godot session.
/// </summary>
public static class SettingsFallback
{
    /// <summary>The field that the message names for bindings that lack an action or hold an unknown one.</summary>
    public const string BindingsField = "controls.bindings";

    /// <summary>
    /// Gives the field of the settings file that failed: the field of the content error under a
    /// refusal of Storage, the bindings for an action that the game lacks, or the whole file for a
    /// file that the system refused or that holds no text of UTF-8 (T-2).
    /// </summary>
    /// <param name="refused">The error of the read, or of the check of the actions.</param>
    /// <returns>The field.</returns>
    /// <exception cref="ArgumentNullException">The error is null (T-2).</exception>
    public static string FieldOf(Exception refused)
    {
        ArgumentNullException.ThrowIfNull(refused);

        return refused switch
        {
            StorageException { InnerException: ContentException content } => content.Field,
            InvalidOperationException => BindingsField,
            _ => ContentException.WholeFile,
        };
    }
}
