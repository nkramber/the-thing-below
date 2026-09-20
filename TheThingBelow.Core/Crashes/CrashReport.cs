using System;
using TheThingBelow.Core.Runs;
using CoreGameVersion = TheThingBelow.Core.GameVersion;
using CoreSimulationVersion = TheThingBelow.Core.SimulationVersion;

namespace TheThingBelow.Core.Crashes;

/// <summary>The format version of a crash file. PR-44 writes the first one (D-170, D-652).</summary>
/// <remarks>
/// A later PR that changes the fields of the crash line raises this number, and the reader of
/// the studio then reads both formats (D-166, D-473).
/// </remarks>
public static class CrashFormat
{
    /// <summary>The format version that this build writes.</summary>
    public const int Current = 1;
}

/// <summary>
/// What a crash file holds: the error with its context, the versions, and the run record
/// (D-170, D-448).
/// </summary>
/// <remarks>
/// The report carries no personal data, and no field names a folder of the person (D-170).
/// Storage hides the folders of the person in the text of an error before it writes the file,
/// because a file error carries its path (T-2, D-494).
/// <para>
/// The record is absent when no run exists, because a load of the content can fail before the
/// first tick (D-170, T-2). The line thus carries the versions of this build itself, and a
/// reader needs no record to read them.
/// </para>
/// </remarks>
/// <param name="FormatVersion">The version of the fields of the crash line (D-652).</param>
/// <param name="Time">The wall-clock time of the crash, as text. Game gives it, and Core reads no clock (G-3).</param>
/// <param name="ErrorType">The name of the type of the error, such as `SimulationException`.</param>
/// <param name="Error">The message of the error, and the message of each error below it (T-2).</param>
/// <param name="Stack">The error and its stacks, as the runtime writes them. It can hold no character.</param>
/// <param name="SimulationVersion">The version of the rules of this build (G-17).</param>
/// <param name="GameVersion">The version of the build that a person reads (D-448, D-653).</param>
/// <param name="Record">The record of the run, or null when the crash came before a run (G-5).</param>
public sealed record CrashReport(
    int FormatVersion,
    string Time,
    string ErrorType,
    string Error,
    string Stack,
    int SimulationVersion,
    string GameVersion,
    RunRecord? Record)
{
    /// <summary>The count of errors below the first one that the message holds.</summary>
    /// <remarks>
    /// An error chain of a length that no bound holds would fill the file, and a chain with a
    /// cycle would fill it without end (T-2).
    /// </remarks>
    public const int MaxErrorCount = 8;

    /// <summary>The text between two messages of the error chain.</summary>
    public const string ErrorSeparator = " <- ";

    /// <summary>Makes the report of one crash for this build.</summary>
    /// <param name="fault">The error that stopped the game.</param>
    /// <param name="errorType">
    /// The name of the type of the error. The caller reads it, because Core runs with no
    /// reflection (F-36, D-647).
    /// </param>
    /// <param name="time">The wall-clock time of the crash, as text, which Game gives (G-3).</param>
    /// <param name="record">The record of the run, or null when no run exists (D-170).</param>
    /// <returns>The report, with the versions of this build.</returns>
    /// <exception cref="ArgumentNullException">The error is null (T-2).</exception>
    /// <exception cref="ArgumentException">The type name or the time has no character (T-2).</exception>
    public static CrashReport Of(Exception fault, string errorType, string time, RunRecord? record)
    {
        ArgumentNullException.ThrowIfNull(fault);
        ArgumentException.ThrowIfNullOrEmpty(errorType);
        ArgumentException.ThrowIfNullOrEmpty(time);

        return new CrashReport(
            CrashFormat.Current,
            time,
            errorType,
            DescribeChain(fault),
            fault.ToString(),
            CoreSimulationVersion.Current,
            CoreGameVersion.Current,
            record);
    }

    /// <summary>Gives the message of the error and the message of each error below it (T-2).</summary>
    /// <param name="fault">The error that stopped the game.</param>
    /// <returns>The messages, with <see cref="ErrorSeparator"/> between two of them.</returns>
    /// <exception cref="ArgumentNullException">The error is null (T-2).</exception>
    /// <remarks>
    /// The first message of the text is the message of the error that the host caught, and the
    /// last one is the message of the first error. No error of the chain is dropped, up to
    /// <see cref="MaxErrorCount"/> errors below the first one (T-2, G-18).
    /// </remarks>
    public static string DescribeChain(Exception fault)
    {
        ArgumentNullException.ThrowIfNull(fault);

        string text = fault.Message;
        Exception? below = fault.InnerException;
        for (int count = 0; below is not null && count < MaxErrorCount; count += 1)
        {
            text += ErrorSeparator + below.Message;
            below = below.InnerException;
        }

        if (below is not null)
        {
            text += ErrorSeparator + $"and more errors below, after {MaxErrorCount} of them";
        }

        return text;
    }
}
