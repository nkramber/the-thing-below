using System;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The error of a run record: a malformed line, a field that the record cannot hold, or a
/// build that cannot replay the record (G-5, T-2).
/// </summary>
/// <remarks>
/// A record comes from another build, another machine, or a crash file, so every error names
/// what the record holds and what this build expected (D-170, D-473, T-2).
/// </remarks>
public sealed class RunRecordException : Exception
{
    /// <summary>The line value of an error that no one line carries.</summary>
    public const int WholeRecord = 0;

    private RunRecordException(string message, int line)
        : base(message) => this.Line = line;

    private RunRecordException(string message, int line, Exception inner)
        : base(message, inner) => this.Line = line;

    /// <summary>The line of the record, which starts at 1, or <see cref="WholeRecord"/>.</summary>
    public int Line { get; }

    /// <summary>Makes the error of one line of the record.</summary>
    /// <param name="line">The line of the record, which starts at 1.</param>
    /// <param name="message">What the reader found on that line.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The line is below 1 (T-2).</exception>
    public static RunRecordException ForLine(int line, string message)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(line, 1);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new RunRecordException(Describe(message, line), line);
    }

    /// <summary>Makes the error of one line of the record, over the error below it.</summary>
    /// <param name="line">The line of the record, which starts at 1.</param>
    /// <param name="message">What the reader found on that line.</param>
    /// <param name="inner">The error that the reader caught, such as a malformed field.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The line is below 1 (T-2).</exception>
    public static RunRecordException ForLine(int line, string message, Exception inner)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(line, 1);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new RunRecordException(Describe(message, line), line, inner);
    }

    /// <summary>Makes the error of the whole record, such as a record with no line.</summary>
    /// <param name="message">What the reader found.</param>
    /// <returns>The error, ready to throw.</returns>
    public static RunRecordException ForRecord(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new RunRecordException($"{message} (run record)", WholeRecord);
    }

    /// <summary>Makes the error of the whole record, over the error below it.</summary>
    /// <param name="message">What the reader found.</param>
    /// <param name="inner">The error that the reader caught, such as a value outside its range.</param>
    /// <returns>The error, ready to throw.</returns>
    public static RunRecordException ForRecord(string message, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new RunRecordException($"{message} (run record)", WholeRecord, inner);
    }

    private static string Describe(string message, int line) => $"{message} (run record, line {line})";
}
