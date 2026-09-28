using System;

namespace TheThingBelow.Tools.Import;

/// <summary>
/// The error of the `import` command and the `frame-png` command. The message always names
/// the file and the reason, so no caller must add either one again (T-2, G-18).
/// </summary>
public sealed class ImportException : Exception
{
    private ImportException(string file, string reason)
        : base($"{reason} (file {file})")
    {
        this.File = file;
        this.Reason = reason;
    }

    /// <summary>The path of the PNG or of the drawing file that the error is about.</summary>
    public string File { get; }

    /// <summary>What the command found, with no file name in it.</summary>
    public string Reason { get; }

    /// <summary>Makes an error about one file.</summary>
    /// <param name="file">The path of the PNG or of the drawing file.</param>
    /// <param name="reason">What the command found, such as `the pixel at x 3, y 4 ...`.</param>
    /// <returns>The error, ready to throw.</returns>
    public static ImportException For(string file, string reason)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(reason);

        return new ImportException(file, reason);
    }
}
