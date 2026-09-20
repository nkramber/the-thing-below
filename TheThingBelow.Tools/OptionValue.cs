using System;
using System.IO;

namespace TheThingBelow.Tools;

/// <summary>
/// The value of one option on the command line of a tool. Every command reads its option
/// values through this class, so the message of a fault reads the same in each command.
/// </summary>
public static class OptionValue
{
    /// <summary>
    /// Reports an empty option value as a fault of the command line, and gives the caller
    /// the answer to stop (T-2, F-83).
    /// </summary>
    /// <remarks>
    /// An empty value reaches a path check further down, which throws `ArgumentException`.
    /// No command names that type in its catch filter, so the process ends with a stack
    /// trace and the exit code of a crash. The parse reads the fault here instead.
    /// </remarks>
    /// <param name="option">The name of the option, such as `--root`.</param>
    /// <param name="value">The value that follows the option on the command line.</param>
    /// <param name="errors">The writer that takes the message of the fault.</param>
    /// <returns>True when the value is empty, and the command stops with the fault code.</returns>
    public static bool ReportEmpty(string option, string value, TextWriter errors)
    {
        ArgumentException.ThrowIfNullOrEmpty(option);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(errors);

        if (value.Length > 0)
        {
            return false;
        }

        errors.WriteLine($"Error: the value of the option {option} is empty.");
        return true;
    }
}
