using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// One time change of a map: a time of day, and the condition that sets it when the party
/// enters the map (D-442, D-1349).
/// </summary>
/// <remarks>
/// A map file holds its changes in a list, and the first change whose condition holds sets the
/// time of the map at each entry. With no change that holds, the map keeps its base time
/// (D-1349). A change never takes the always leaf, because the base time then never shows and
/// the file reads like a mistake (D-1002).
/// </remarks>
/// <param name="Time">The time of day that the change sets.</param>
/// <param name="Condition">The condition of the change, in the one form of a condition (D-543).</param>
public sealed record TimeChange(TimeOfDay Time, Condition Condition)
{
    /// <summary>The field of a map file that holds the time changes (D-1349).</summary>
    public const string ListField = "time_changes";

    /// <summary>Reads the time change list of a map file (D-1349).</summary>
    /// <param name="reader">The reader of the map file, at the start of the array.</param>
    /// <returns>Each change, in the order of the file.</returns>
    /// <exception cref="ContentException">
    /// An entry holds an unknown field, an absent field, a name that names no time, or the always
    /// leaf as its condition (G-6, T-2).
    /// </exception>
    /// <remarks>The content set checks each flag of each condition against the flag file (D-543).</remarks>
    public static List<TimeChange> ReadAll(ref ContentReader reader)
    {
        List<TimeChange> changes = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, changes.Count))
        {
            changes.Add(Read(ref reader));
        }

        return changes;
    }

    private static TimeChange Read(ref ContentReader reader)
    {
        string? time = null;
        Condition? condition = null;
        TimeOfDay parsed = TimeOfDay.Day;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "time":
                    time = reader.ReadString();
                    if (!TimesOfDay.TryOf(time, out parsed))
                    {
                        throw reader.Refuse(
                            $"the time change names the time '{time}', and a map takes one of {TimesOfDay.EveryName} (D-442, D-1349)");
                    }

                    break;
                case GameMap.ConditionField:
                    condition = Condition.Read(ref reader);
                    if (condition.Kind == ConditionKind.Always)
                    {
                        throw reader.Refuse(
                            "the time change takes the always leaf, which holds on each entry, so the base time of the map never shows. Write the time as the base time (D-1349)");
                    }

                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(time, depth, "time");
        return new TimeChange(parsed, reader.Require(condition, depth, GameMap.ConditionField));
    }
}
