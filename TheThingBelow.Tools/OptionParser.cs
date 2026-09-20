using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheThingBelow.Tools;

/// <summary>
/// The options of one command on the command line: each option with a value, and each flag.
/// Every command reads its arguments through this class, so an unknown option, an absent
/// value, an empty value, and a repeated option read the same in each command (T-1, T-2).
/// </summary>
public sealed class OptionParser
{
    private readonly SortedDictionary<string, string> values;
    private readonly SortedSet<string> flags;

    private OptionParser(SortedDictionary<string, string> values, SortedSet<string> flags)
    {
        this.values = values;
        this.flags = flags;
    }

    /// <summary>Reads the arguments of a command, and writes each fault of the command line.</summary>
    /// <param name="command">The name of the command, for the message of a fault.</param>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="valueOptions">Each option that takes a value, such as `--root`.</param>
    /// <param name="flagOptions">Each option that takes no value, such as `--write`.</param>
    /// <param name="errors">The writer that takes the message of a fault.</param>
    /// <returns>The options, or null when the command line holds a fault and the command stops.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static OptionParser? Read(
        string command,
        IReadOnlyList<string> args,
        IReadOnlyList<string> valueOptions,
        IReadOnlyList<string> flagOptions,
        TextWriter errors)
    {
        ArgumentException.ThrowIfNullOrEmpty(command);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(valueOptions);
        ArgumentNullException.ThrowIfNull(flagOptions);
        ArgumentNullException.ThrowIfNull(errors);

        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var flags = new SortedSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < args.Count; index += 1)
        {
            string option = args[index];
            bool takesValue = valueOptions.Contains(option);
            if (!takesValue && !flagOptions.Contains(option))
            {
                errors.WriteLine(
                    $"Error: the option '{option}' is unknown. {command} takes {Usage(valueOptions, flagOptions)}.");
                return null;
            }

            // A repeated option would take the last value in silence (T-2).
            if (values.ContainsKey(option) || flags.Contains(option))
            {
                errors.WriteLine($"Error: the option {option} is on the command line two times.");
                return null;
            }

            if (!takesValue)
            {
                flags.Add(option);
                continue;
            }

            if (index + 1 >= args.Count)
            {
                errors.WriteLine($"Error: the option {option} needs a value after it.");
                return null;
            }

            string value = args[index + 1];
            if (OptionValue.ReportEmpty(option, value, errors))
            {
                return null;
            }

            values[option] = value;
            index += 1;
        }

        return new OptionParser(values, flags);
    }

    /// <summary>Gives the value of an option, or null when the command line holds no such option.</summary>
    /// <param name="option">The option, such as `--root`.</param>
    /// <returns>The value, or null.</returns>
    public string? Value(string option)
    {
        ArgumentException.ThrowIfNullOrEmpty(option);
        return this.values.TryGetValue(option, out string? value) ? value : null;
    }

    /// <summary>Gives the value of an option, or a fallback when the command line holds no such option.</summary>
    /// <param name="option">The option, such as `--root`.</param>
    /// <param name="fallback">The value of the command when the option is absent.</param>
    /// <returns>The value of the command line, or the fallback.</returns>
    public string ValueOr(string option, string fallback)
    {
        ArgumentException.ThrowIfNullOrEmpty(fallback);
        return this.Value(option) ?? fallback;
    }

    /// <summary>Tells whether the command line holds a flag.</summary>
    /// <param name="flag">The flag, such as `--write`.</param>
    /// <returns>True when the flag is on the command line.</returns>
    public bool Holds(string flag)
    {
        ArgumentException.ThrowIfNullOrEmpty(flag);
        return this.flags.Contains(flag);
    }

    private static string Usage(IReadOnlyList<string> valueOptions, IReadOnlyList<string> flagOptions)
    {
        List<string> parts = [];
        foreach (string option in valueOptions)
        {
            parts.Add($"{option} <value>");
        }

        foreach (string option in flagOptions)
        {
            parts.Add(option);
        }

        if (parts.Count <= 1)
        {
            return string.Join(string.Empty, parts);
        }

        return string.Join(", ", parts.GetRange(0, parts.Count - 1)) + ", and " + parts[^1];
    }
}
