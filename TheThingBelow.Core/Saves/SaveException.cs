using System;

namespace TheThingBelow.Core.Saves;

/// <summary>
/// The error of a save file: a malformed line, a checksum that does not match, or a format
/// version that this build cannot read (D-178, T-2).
/// </summary>
/// <remarks>
/// A save comes from another build, another machine, or a file that a person changed, so
/// every message names the file and what the reader found (D-178, T-2).
/// </remarks>
public sealed class SaveException : Exception
{
    private SaveException(string message, string file)
        : base(message) => this.File = file;

    private SaveException(string message, string file, Exception inner)
        : base(message, inner) => this.File = file;

    /// <summary>The file that the reader read, which the message also names.</summary>
    public string File { get; }

    /// <summary>Makes the error of one save file.</summary>
    /// <param name="file">The name or the path of the save file.</param>
    /// <param name="message">What the reader found.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentException">The file or the message has no character (T-2).</exception>
    public static SaveException ForFile(string file, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new SaveException(Describe(message, file), file);
    }

    /// <summary>Makes the error of one save file, over the error below it.</summary>
    /// <param name="file">The name or the path of the save file.</param>
    /// <param name="message">What the reader found.</param>
    /// <param name="inner">The error that the reader caught, such as an absent field.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentException">The file or the message has no character (T-2).</exception>
    /// <exception cref="ArgumentNullException">The inner error is null (T-2).</exception>
    public static SaveException ForFile(string file, string message, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new SaveException(Describe(message, file), file, inner);
    }

    private static string Describe(string message, string file) => $"{message} (the save '{file}')";
}
