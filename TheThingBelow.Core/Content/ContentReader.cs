using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace TheThingBelow.Core.Content;

/// <summary>
/// The one strict reader of content JSON (D-116, D-177). It refuses an absent field, an
/// unknown field, a wrong type, and a number that is not a whole number, and every error
/// names the file and the field (G-6, T-2).
/// </summary>
/// <remarks>
/// The reader calls `JsonSerializer` nowhere, so no reflection path exists in Core (D-647,
/// F-36). `Utf8JsonReader` is a scanner with no metadata and no type map, and each record of
/// `TheThingBelow.Core.Content` names its own fields in a `switch`.
/// <para>
/// The reader keeps the path of the current field, such as `colors[2].hex`, so an error
/// names the exact field of a nested object or an array element (T-2). A record reads one
/// object with this shape:
/// </para>
/// <code>
/// int depth = reader.ReadObjectStart();
/// while (reader.ReadNextField(depth, out string field))
/// {
///     switch (field)
///     {
///         case "index": index = reader.ReadInt(); break;
///         default: throw reader.UnknownField(field);
///     }
/// }
/// </code>
/// </remarks>
public ref struct ContentReader
{
    private readonly string file;
    private readonly List<string> path;
    private Utf8JsonReader reader;

    /// <summary>Makes a reader over the bytes of one content file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    public ContentReader(ReadOnlySpan<byte> bytes, string file)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);

        this.file = file;
        this.path = [];
        this.reader = new Utf8JsonReader(
            bytes,
            new JsonReaderOptions
            {
                // A comment and a trailing comma are both silent ways to hide a mistake, and
                // the default of each option refuses them. The options stay explicit, because
                // a reader of this method must see what the file may hold (T-1, G-6).
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false,
            });
    }

    /// <summary>The path of the file, under `content/`.</summary>
    public readonly string File => this.file;

    /// <summary>Starts an object, and gives the depth that each field of it takes.</summary>
    /// <returns>The depth to pass to <see cref="ReadNextField"/> and to each `Require` call.</returns>
    /// <exception cref="ContentException">The next value is not an object.</exception>
    public int ReadObjectStart()
    {
        this.MoveNext();
        if (this.reader.TokenType != JsonTokenType.StartObject)
        {
            throw this.WrongType("an object");
        }

        return this.path.Count;
    }

    /// <summary>Reads the name of the next field of an object, and stops at the end of it.</summary>
    /// <param name="depth">The value that <see cref="ReadObjectStart"/> gave.</param>
    /// <param name="field">The name of the field, and an empty string at the end.</param>
    /// <returns>True when a field follows, and false at the end of the object.</returns>
    /// <exception cref="ContentException">The file ends inside the object.</exception>
    public bool ReadNextField(int depth, out string field)
    {
        this.TruncateTo(depth);
        this.MoveNext();
        if (this.reader.TokenType == JsonTokenType.EndObject)
        {
            field = string.Empty;
            return false;
        }

        if (this.reader.TokenType != JsonTokenType.PropertyName)
        {
            throw this.WrongType("a field name");
        }

        field = this.reader.GetString() ?? throw this.WrongType("a field name");
        this.path.Add($".{field}");
        return true;
    }

    /// <summary>Starts an array, and gives the depth that each element of it takes.</summary>
    /// <returns>The depth to pass to <see cref="ReadNextElement"/>.</returns>
    /// <exception cref="ContentException">The next value is not an array.</exception>
    public int ReadArrayStart()
    {
        this.MoveNext();
        if (this.reader.TokenType != JsonTokenType.StartArray)
        {
            throw this.WrongType("an array");
        }

        return this.path.Count;
    }

    /// <summary>Moves to the next element of an array, and stops at the end of it.</summary>
    /// <param name="depth">The value that <see cref="ReadArrayStart"/> gave.</param>
    /// <param name="index">The position of the element, which starts at zero.</param>
    /// <returns>True when an element follows, and false at the end of the array.</returns>
    /// <exception cref="ContentException">The file ends inside the array.</exception>
    /// <remarks>
    /// The method reads the end of the array itself, and it leaves an element for the read
    /// that follows. `Utf8JsonReader` has no look-ahead, and it is a value type, so the copy
    /// below reads one token and the method keeps that copy only at the end of the array.
    /// </remarks>
    public bool ReadNextElement(int depth, int index)
    {
        this.TruncateTo(depth);

        Utf8JsonReader ahead = this.reader;
        if (!this.TryRead(ref ahead))
        {
            throw ContentException.ForField(this.file, this.CurrentField(), "the file ends inside an array");
        }

        if (ahead.TokenType == JsonTokenType.EndArray)
        {
            this.reader = ahead;
            return false;
        }

        this.path.Add($"[{index}]");
        return true;
    }

    /// <summary>Reads a whole number.</summary>
    /// <returns>The value of the field.</returns>
    /// <exception cref="ContentException">
    /// The value is not a number, holds a fraction or an exponent, or does not fit in an `int`.
    /// </exception>
    /// <remarks>
    /// Core computes with integers alone, and a fraction in content takes basis points
    /// (D-169, G-2). Thus `1.5` and `1e2` are both errors, and the check below reads the
    /// characters of the number itself, so the message names which one the file holds.
    /// </remarks>
    public int ReadInt()
    {
        this.MoveNext();
        if (this.reader.TokenType != JsonTokenType.Number)
        {
            throw this.WrongType("a whole number");
        }

        foreach (byte character in this.reader.ValueSpan)
        {
            bool whole = character is (>= (byte)'0' and <= (byte)'9') or (byte)'-';
            if (!whole)
            {
                throw ContentException.ForField(
                    this.file,
                    this.CurrentField(),
                    $"the number '{this.NumberAsText()}' holds a fraction or an exponent, and Core reads whole numbers alone (G-2, D-169)");
            }
        }

        if (!this.reader.TryGetInt32(out int value))
        {
            throw ContentException.ForField(
                this.file,
                this.CurrentField(),
                $"the number '{this.NumberAsText()}' does not fit in a 32-bit whole number");
        }

        return value;
    }

    /// <summary>Reads text.</summary>
    /// <returns>The value of the field.</returns>
    /// <exception cref="ContentException">The value is not text.</exception>
    public string ReadString()
    {
        this.MoveNext();
        if (this.reader.TokenType != JsonTokenType.String)
        {
            throw this.WrongType("text");
        }

        return this.reader.GetString() ?? throw this.WrongType("text");
    }

    /// <summary>Reads a content id, and fails on any other form (D-646).</summary>
    /// <returns>The id of the field.</returns>
    /// <exception cref="ContentException">The value is not text, or is not a legal id.</exception>
    public ContentId ReadContentId() => ContentId.Parse(this.ReadString(), this.file, this.CurrentField());

    /// <summary>Reads the permanent id of an entry, and refuses an id of another kind (D-646).</summary>
    /// <param name="kind">The kind that the record of this file owns, such as `enemy`.</param>
    /// <returns>The id of the field.</returns>
    /// <exception cref="ContentException">
    /// The value is not text, is not a legal id, or carries another kind.
    /// </exception>
    /// <remarks>
    /// The kind of an entry id agrees with the file that holds the entry (D-646). The record
    /// owns the kind, and not the path, because one record reads every file of its kind.
    /// <para>
    /// A field that points at an entry of another record takes <see cref="ReadContentId()"/>
    /// instead, because the kind of such a field names the other record. A string id takes it
    /// too, because the kind of a string id names where the player reads the text (G-7).
    /// </para>
    /// </remarks>
    public ContentId ReadContentId(string kind)
    {
        ArgumentException.ThrowIfNullOrEmpty(kind);

        ContentId id = this.ReadContentId();
        if (string.CompareOrdinal(id.Kind, kind) != 0)
        {
            throw ContentException.ForField(
                this.file,
                this.CurrentField(),
                $"the id '{id.Value}' carries the kind '{id.Kind}', and this file holds entries of the kind '{kind}' (D-646)");
        }

        return id;
    }

    /// <summary>Reads the end of the file, and fails on any token after the content.</summary>
    /// <exception cref="ContentException">The file holds a token after the content.</exception>
    public void ReadFileEnd()
    {
        if (this.TryRead(ref this.reader))
        {
            throw ContentException.ForFile(
                this.file,
                $"the file holds a '{this.reader.TokenType}' token after the content");
        }
    }

    /// <summary>Makes the error for a field that no record of this file holds (G-6).</summary>
    /// <param name="field">The name of the field that the file holds.</param>
    /// <returns>The error, ready to throw.</returns>
    public readonly ContentException UnknownField(string field)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);

        return ContentException.ForField(this.file, this.CurrentField(), "an unknown field");
    }

    /// <summary>Gives a value that a field must hold, and fails when the file has none.</summary>
    /// <typeparam name="T">The type of the value, such as `string` or `ContentId`.</typeparam>
    /// <param name="value">The value that the read set, or null when the file has no field.</param>
    /// <param name="depth">The value that <see cref="ReadObjectStart"/> gave.</param>
    /// <param name="field">The name of the field, for the error (T-2).</param>
    /// <returns>The value.</returns>
    /// <exception cref="ContentException">The file holds no such field.</exception>
    public readonly T Require<T>(T? value, int depth, string field)
        where T : class =>
        value ?? throw this.AbsentField(depth, field);

    /// <summary>Gives a number that a field must hold, and fails when the file has none.</summary>
    /// <param name="value">The value that the read set, or null when the file has no field.</param>
    /// <param name="depth">The value that <see cref="ReadObjectStart"/> gave.</param>
    /// <param name="field">The name of the field, for the error (T-2).</param>
    /// <returns>The value.</returns>
    /// <exception cref="ContentException">The file holds no such field.</exception>
    public readonly int RequireInt(int? value, int depth, string field) =>
        value ?? throw this.AbsentField(depth, field);

    private readonly ContentException AbsentField(int depth, string field)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);

        string owner = string.Concat(this.path.GetRange(0, depth)).TrimStart('.');
        string whole = owner.Length == 0 ? field : $"{owner}.{field}";
        return ContentException.ForField(this.file, whole, "the field is absent");
    }

    private void MoveNext()
    {
        if (!this.TryRead(ref this.reader))
        {
            throw ContentException.ForField(this.file, this.CurrentField(), "the file ends before the value");
        }
    }

    /// <summary>
    /// Moves one token, and turns a JSON fault into an error that names the file and the
    /// field. `Utf8JsonReader` throws `JsonException` on a comment, a trailing comma, and
    /// every other fault of the form, and that message names no file (T-2).
    /// </summary>
    private readonly bool TryRead(ref Utf8JsonReader target)
    {
        try
        {
            return target.Read();
        }
        catch (JsonException error)
        {
            throw ContentException.ForField(
                this.file,
                this.CurrentField(),
                "the file is not well-formed JSON, and the reader refuses a comment and a trailing comma",
                error);
        }
    }

    // The reader reads one span, never a sequence of segments, because the constructor takes
    // a `ReadOnlySpan<byte>`. `ValueSpan` thus always holds the whole number.
    private readonly string NumberAsText() => Encoding.UTF8.GetString(this.reader.ValueSpan);

    private readonly ContentException WrongType(string expected) =>
        ContentException.ForField(
            this.file,
            this.CurrentField(),
            $"the file holds a '{this.reader.TokenType}' token where the record needs {expected}");

    // Each segment of the path starts with a dot or a bracket, so the whole path reads as
    // `colors[2].hex` once the leading dot of the first segment goes.
    private readonly string CurrentField() =>
        this.path.Count == 0 ? ContentException.WholeFile : string.Concat(this.path).TrimStart('.');

    private void TruncateTo(int depth)
    {
        if (this.path.Count > depth)
        {
            this.path.RemoveRange(depth, this.path.Count - depth);
        }
    }
}
