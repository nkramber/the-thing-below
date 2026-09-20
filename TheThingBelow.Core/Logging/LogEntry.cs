using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheThingBelow.Core.Logging;

/// <summary>How much one log entry matters (D-179).</summary>
public enum LogLevel
{
    /// <summary>The step of a subsystem. A log file holds it only when the session asks for it.</summary>
    Debug,

    /// <summary>A change of the run that a reader of a report follows, such as a menu.</summary>
    Info,

    /// <summary>A rule or a file was not as the code expected, and the run went on (T-2).</summary>
    Warning,

    /// <summary>The run stopped, or it dropped the work of a step (T-2).</summary>
    Error,
}

/// <summary>
/// One context field of a log entry: a name and its value as text (D-179).
/// </summary>
/// <remarks>
/// Every value is text, so one writer and one reader serve every field and a new field needs
/// no type tag (T-1). <see cref="OfNumber"/> writes a count with the invariant culture, so the
/// text of a field never follows the culture of the machine (F-39, T-7).
/// </remarks>
public sealed record LogField
{
    /// <summary>Makes a field from a name and its value.</summary>
    /// <param name="name">The name of the field, such as `action`.</param>
    /// <param name="value">The value as text. A field with no value names nothing (T-2).</param>
    /// <exception cref="ArgumentException">The name or the value has no character (T-2).</exception>
    public LogField(string name, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(value);

        this.Name = name;
        this.Value = value;
    }

    /// <summary>The name of the field.</summary>
    public string Name { get; }

    /// <summary>The value of the field, as text.</summary>
    public string Value { get; }

    /// <summary>Makes a field from a count.</summary>
    /// <param name="name">The name of the field, such as `beats`.</param>
    /// <param name="value">The count.</param>
    /// <returns>The field, with the count in the invariant culture (T-7).</returns>
    /// <exception cref="ArgumentException">The name has no character (T-2).</exception>
    public static LogField OfNumber(string name, long value) =>
        new(name, value.ToString(CultureInfo.InvariantCulture));
}

/// <summary>
/// One log entry that a step of Core returns: the level, the message, the tick, the
/// subsystem, and the context fields (D-179).
/// </summary>
/// <remarks>
/// Core adds no wall-clock time and no file path to an entry, because Core reads no clock and
/// no file (G-1, G-3). Game adds the time when it writes the line, and <see cref="LogLine"/>
/// holds the pair (D-179).
/// <para>
/// A step returns the entries of that step alone, and Core keeps none. The entries of a seed
/// are thus the same on every machine and in every replay (T-7).
/// </para>
/// </remarks>
public sealed class LogEntry
{
    /// <summary>The fields of an entry that carries no context field.</summary>
    public static readonly IReadOnlyList<LogField> NoFields = [];

    /// <summary>Makes one entry.</summary>
    /// <param name="level">How much the entry matters.</param>
    /// <param name="message">What happened, such as `the menu opened`.</param>
    /// <param name="tick">The tick of the step that made the entry (D-164, D-650).</param>
    /// <param name="subsystem">The subsystem that made the entry (<see cref="LogSubsystems"/>).</param>
    /// <param name="fields">The context fields, in the order that the line holds them.</param>
    /// <exception cref="ArgumentException">
    /// The message or the subsystem has no character, or two fields carry one name (T-2).
    /// </exception>
    /// <exception cref="ArgumentNullException">The field list is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The level is no level, or the tick is below zero (T-2).</exception>
    public LogEntry(
        LogLevel level,
        string message,
        long tick,
        string subsystem,
        IReadOnlyList<LogField> fields)
    {
        // A cast of a number that no level names reaches this constructor, and a comparison
        // reads the range with no reflection, which Core never uses (T-2).
        if (level < LogLevel.Debug || level > LogLevel.Error)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level), level, $"The log holds no level with the number {(int)level} (D-179).");
        }

        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);
        ArgumentException.ThrowIfNullOrEmpty(subsystem);
        ArgumentNullException.ThrowIfNull(fields);

        for (int index = 0; index < fields.Count; index += 1)
        {
            LogField field = fields[index];
            ArgumentNullException.ThrowIfNull(field);

            for (int before = 0; before < index; before += 1)
            {
                if (string.CompareOrdinal(fields[before].Name, field.Name) == 0)
                {
                    throw new ArgumentException(
                        $"The entry holds the field '{field.Name}' two times, and one line holds one value for each name (D-179).",
                        nameof(fields));
                }
            }
        }

        this.Level = level;
        this.Message = message;
        this.Tick = tick;
        this.Subsystem = subsystem;
        this.Fields = fields;
    }

    /// <summary>How much the entry matters.</summary>
    public LogLevel Level { get; }

    /// <summary>What happened.</summary>
    public string Message { get; }

    /// <summary>The tick of the step that made the entry (D-650).</summary>
    public long Tick { get; }

    /// <summary>The subsystem that made the entry.</summary>
    public string Subsystem { get; }

    /// <summary>The context fields, in the order that the line holds them.</summary>
    public IReadOnlyList<LogField> Fields { get; }
}
