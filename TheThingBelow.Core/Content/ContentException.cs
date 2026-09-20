using System;

namespace TheThingBelow.Core.Content;

/// <summary>
/// The error that a content load throws. The message always names the file and the field,
/// so no caller must add either one again (T-2, G-18).
/// </summary>
/// <remarks>
/// A content load runs outside a run, so it carries no seed and no tick. That is the one
/// difference from <see cref="SimulationException"/>, which carries the run context.
/// </remarks>
public sealed class ContentException : Exception
{
    private ContentException(string message, string file, string field)
        : base(Describe(message, file, field))
    {
        this.File = file;
        this.Field = field;
    }

    private ContentException(string message, string file, string field, Exception inner)
        : base(Describe(message, file, field), inner)
    {
        this.File = file;
        this.Field = field;
    }

    /// <summary>The path of the content file, under `content/`, with `/` separators.</summary>
    public string File { get; }

    /// <summary>The field that failed, such as `colors[2].hex`, or `the file` for the whole file.</summary>
    public string Field { get; }

    /// <summary>The field name that an error about the whole file carries.</summary>
    public const string WholeFile = "the file";

    /// <summary>Makes an error about one field of a file.</summary>
    /// <param name="file">The path of the file, under `content/`.</param>
    /// <param name="field">The field that failed, such as `colors[2].hex`.</param>
    /// <param name="message">What the reader found, such as `an unknown field`.</param>
    /// <returns>The error, ready to throw.</returns>
    public static ContentException ForField(string file, string field, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(field);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new ContentException(message, file, field);
    }

    /// <summary>Makes an error about the whole file.</summary>
    /// <param name="file">The path of the file, under `content/`.</param>
    /// <param name="message">What the reader found, such as `the file holds no object`.</param>
    /// <returns>The error, ready to throw.</returns>
    public static ContentException ForFile(string file, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new ContentException(message, file, WholeFile);
    }

    /// <summary>Makes an error about one field from the error below it.</summary>
    /// <param name="file">The path of the file, under `content/`.</param>
    /// <param name="field">The field that failed, such as `colors[2].hex`.</param>
    /// <param name="message">What the reader found, such as `the file is not JSON`.</param>
    /// <param name="inner">The error that the reader caught.</param>
    /// <returns>The error, ready to throw.</returns>
    public static ContentException ForField(string file, string field, string message, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(field);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new ContentException(message, file, field, inner);
    }

    /// <summary>Makes an error about the whole file from the error below it.</summary>
    /// <param name="file">The path of the file, under `content/`.</param>
    /// <param name="message">What the reader found, such as `the file is not JSON`.</param>
    /// <param name="inner">The error that the reader caught.</param>
    /// <returns>The error, ready to throw.</returns>
    public static ContentException ForFile(string file, string message, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new ContentException(message, file, WholeFile, inner);
    }

    private static string Describe(string message, string file, string field) =>
        $"{message} (file {file}, field {field})";
}
