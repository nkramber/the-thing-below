using System;

namespace TheThingBelow.Core.Crashes;

/// <summary>
/// The error of a crash file: a malformed line, an absent field, or a format version that
/// this build cannot read (D-170, T-2).
/// </summary>
/// <remarks>
/// A crash file comes from the machine of a player, by mail (D-473). Thus every message names
/// the file and what the reader found, and the reader of the studio sees the reason with no
/// tool (T-2, G-18).
/// </remarks>
public sealed class CrashException : Exception
{
    private CrashException(string message, string file)
        : base(message) => this.File = file;

    private CrashException(string message, string file, Exception inner)
        : base(message, inner) => this.File = file;

    /// <summary>The file that the reader read, which the message also names.</summary>
    public string File { get; }

    /// <summary>Makes the error of one crash file.</summary>
    /// <param name="file">The name or the path of the crash file.</param>
    /// <param name="message">What the reader found.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentException">The file or the message has no character (T-2).</exception>
    public static CrashException ForFile(string file, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new CrashException(Describe(message, file), file);
    }

    /// <summary>Makes the error of one crash file, over the error below it.</summary>
    /// <param name="file">The name or the path of the crash file.</param>
    /// <param name="message">What the reader found.</param>
    /// <param name="inner">The error that the reader caught, such as an absent field.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentException">The file or the message has no character (T-2).</exception>
    /// <exception cref="ArgumentNullException">The inner error is null (T-2).</exception>
    public static CrashException ForFile(string file, string message, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new CrashException(Describe(message, file), file, inner);
    }

    private static string Describe(string message, string file) => $"{message} (the crash file '{file}')";
}
