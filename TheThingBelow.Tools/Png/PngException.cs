using System;

namespace TheThingBelow.Tools.Png;

/// <summary>
/// The error that the PNG reader and the PNG writer throw. The message always names the file
/// and the reason, so no caller must add either one again (T-2, G-18, D-176).
/// </summary>
public sealed class PngException : Exception
{
    private PngException(string file, string reason)
        : base(Describe(file, reason))
    {
        this.File = file;
        this.Reason = reason;
    }

    private PngException(string file, string reason, Exception inner)
        : base(Describe(file, reason), inner)
    {
        this.File = file;
        this.Reason = reason;
    }

    /// <summary>The path of the file, or the name that the caller gave to the bytes.</summary>
    public string File { get; }

    /// <summary>What the reader or the writer found, with no file name in it.</summary>
    public string Reason { get; }

    /// <summary>Makes an error about one file.</summary>
    /// <param name="file">The path of the file, or the name of the bytes.</param>
    /// <param name="reason">What the code found, such as `the bit depth is 16`.</param>
    /// <returns>The error, ready to throw.</returns>
    public static PngException For(string file, string reason)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(reason);

        return new PngException(file, reason);
    }

    /// <summary>Makes an error about one file from the error below it.</summary>
    /// <param name="file">The path of the file, or the name of the bytes.</param>
    /// <param name="reason">What the code found, such as `the image data is not zlib data`.</param>
    /// <param name="inner">The error that the code caught.</param>
    /// <returns>The error, ready to throw.</returns>
    public static PngException For(string file, string reason, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(reason);
        ArgumentNullException.ThrowIfNull(inner);

        return new PngException(file, reason, inner);
    }

    private static string Describe(string file, string reason) => $"{reason} (file {file})";
}
