using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The strict reads of the JSON files of the night: the night record, the facts of the night run,
/// and the facts of the pull request. An absent field, an unknown field, and a value of the wrong
/// kind are each an error that names the file and the field (T-2).
/// </summary>
public static class NightJson
{
    /// <summary>Checks that a value is an object that holds each field of a list and no other field.</summary>
    /// <param name="value">The value.</param>
    /// <param name="where">The file, and the place in it, for the message of an error.</param>
    /// <param name="fields">Each field of the object.</param>
    /// <exception cref="InvalidOperationException">The value is not such an object.</exception>
    public static void RequireFields(JsonElement value, string where, IReadOnlyList<string> fields)
    {
        ArgumentNullException.ThrowIfNull(where);
        ArgumentNullException.ThrowIfNull(fields);
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException($"{where} holds {value.ValueKind}, and the night gate needs an object.");
        }

        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!Contains(fields, property.Name))
            {
                throw new InvalidOperationException($"{where} holds the unknown field '{property.Name}'. The fields are {string.Join(", ", fields)}.");
            }
        }

        foreach (string field in fields)
        {
            if (!value.TryGetProperty(field, out _))
            {
                throw new InvalidOperationException($"{where} holds no field '{field}'. An absent field is an error (T-2).");
            }
        }
    }

    /// <summary>Reads a field that holds text with at least one character.</summary>
    /// <param name="owner">The object.</param>
    /// <param name="field">The name of the field.</param>
    /// <param name="where">The file, and the place in it, for the message of an error.</param>
    /// <returns>The text.</returns>
    /// <exception cref="InvalidOperationException">The field holds no text.</exception>
    public static string Text(JsonElement owner, string field, string where)
    {
        JsonElement value = owner.GetProperty(field);
        string? text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidOperationException($"The field '{field}' of {where} holds '{value}', and the night gate needs text.");
        }

        return text;
    }

    /// <summary>Reads a field that holds a whole number at or above a minimum.</summary>
    /// <param name="owner">The object.</param>
    /// <param name="field">The name of the field.</param>
    /// <param name="where">The file, and the place in it, for the message of an error.</param>
    /// <param name="minimum">The smallest value that the field can hold.</param>
    /// <returns>The number.</returns>
    /// <exception cref="InvalidOperationException">The field holds no such number.</exception>
    public static long Whole(JsonElement owner, string field, string where, long minimum)
    {
        JsonElement value = owner.GetProperty(field);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out long number) || number < minimum)
        {
            throw new InvalidOperationException($"The field '{field}' of {where} holds '{value}', and the night gate needs a whole number of {minimum.ToString(CultureInfo.InvariantCulture)} or more.");
        }

        return number;
    }

    /// <summary>Reads a field that holds a seed: a whole number of 0 or more.</summary>
    /// <param name="owner">The object.</param>
    /// <param name="field">The name of the field.</param>
    /// <param name="where">The file, and the place in it, for the message of an error.</param>
    /// <returns>The seed.</returns>
    /// <exception cref="InvalidOperationException">The field holds no seed.</exception>
    public static ulong Seed(JsonElement owner, string field, string where)
    {
        JsonElement value = owner.GetProperty(field);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetUInt64(out ulong seed))
        {
            throw new InvalidOperationException($"The field '{field}' of {where} holds '{value}', and the night gate needs a seed.");
        }

        return seed;
    }

    /// <summary>Reads a field that holds a time in UTC, in the form `2026-09-27T04:17:43Z` that GitHub writes.</summary>
    /// <param name="owner">The object.</param>
    /// <param name="field">The name of the field.</param>
    /// <param name="where">The file, and the place in it, for the message of an error.</param>
    /// <returns>The time.</returns>
    /// <exception cref="InvalidOperationException">The field holds no such time.</exception>
    public static DateTimeOffset Time(JsonElement owner, string field, string where)
    {
        string text = Text(owner, field, where);
        return TryTime(text, out DateTimeOffset time)
            ? time
            : throw new InvalidOperationException($"The field '{field}' of {where} holds '{text}', and the night gate needs a UTC time such as 2026-09-27T04:17:43Z.");
    }

    /// <summary>Reads a time in UTC, in the form `2026-09-27T04:17:43Z`.</summary>
    /// <param name="text">The text.</param>
    /// <param name="time">The time, when the text holds one.</param>
    /// <returns>True when the text holds a time in that form.</returns>
    public static bool TryTime(string text, out DateTimeOffset time) =>
        DateTimeOffset.TryParseExact(text, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out time);

    /// <summary>Writes a time in UTC in the form that <see cref="TryTime"/> reads.</summary>
    /// <param name="time">The time.</param>
    /// <returns>The text.</returns>
    public static string TextOf(DateTimeOffset time) =>
        time.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    /// <summary>Tells whether a text is a full commit hash: 40 digits of lowercase hexadecimal.</summary>
    /// <param name="text">The text.</param>
    /// <returns>True for a full commit hash.</returns>
    public static bool IsCommit(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length != 40)
        {
            return false;
        }

        foreach (char digit in text)
        {
            if (digit is not ((>= '0' and <= '9') or (>= 'a' and <= 'f')))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Parses a JSON file.</summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="where">The name of the file for the message of an error.</param>
    /// <returns>The document, which the caller disposes.</returns>
    /// <exception cref="InvalidOperationException">The file holds no valid JSON.</exception>
    public static JsonDocument Parse(string path, string where)
    {
        try
        {
            return JsonDocument.Parse(File.ReadAllText(path));
        }
        catch (JsonException fault)
        {
            throw new InvalidOperationException($"{where} holds no valid JSON: {fault.Message}", fault);
        }
    }

    private static bool Contains(IReadOnlyList<string> fields, string name)
    {
        foreach (string field in fields)
        {
            if (string.Equals(field, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
