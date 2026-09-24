using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// One form of a lesson: an ability and the point total that opens it (D-539, D-1026). The name
/// of the form is the name of its ability: `name.` and the name part of the ability id (G-7).
/// </summary>
/// <param name="Ability">The ability that the form gives, an id of the ability file (D-785).</param>
/// <param name="Points">The point total that opens the form. The first form opens at zero.</param>
/// <param name="Mp">The MP that one use of the form costs (D-42).</param>
/// <param name="Description">The string id of the short description of the form, which the list of forms shows (D-1027, G-7).</param>
public sealed record LessonForm(ContentId Ability, int Points, int Mp, ContentId Description);

/// <summary>
/// One lesson: a rite or a drill with its kind and its forms (D-278, D-281, D-1026). The name of
/// the lesson is `name.` and the name part of its id (G-7).
/// </summary>
/// <param name="Id">The id, of the kind `lesson`.</param>
/// <param name="Kind">The kind of ability, which the aptitude bonus reads (D-358).</param>
/// <param name="Forms">The forms, in the order of their point totals, from the first form at zero.</param>
public sealed record LessonRecord(ContentId Id, AptitudeKind Kind, IReadOnlyList<LessonForm> Forms)
{
    /// <summary>True when the lesson is a rite, which silence stops (D-393, D-806).</summary>
    public bool IsRite => Aptitudes.IsRite(this.Kind);

    /// <summary>The point total of the last form, where the points of the lesson stop (D-1021).</summary>
    public int MostPoints => this.Forms[^1].Points;

    /// <summary>Gives the count of the forms that a point total opens (D-539).</summary>
    /// <param name="points">The points of one character for this lesson, from zero.</param>
    /// <returns>The count, from 1 to the count of forms.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The points are below zero (T-2).</exception>
    public int OpenedAt(int points)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(points);

        int opened = 0;
        foreach (LessonForm form in this.Forms)
        {
            if (points >= form.Points)
            {
                opened += 1;
            }
        }

        return opened;
    }
}

/// <summary>
/// The lesson file, `content/rules/lessons.json`: the kind and the forms of each lesson
/// (D-1026). Each form names an ability of the ability file, and the battle content checks each
/// one (D-785). No two forms name one ability, so each spell has one flash (D-1032).
/// </summary>
public sealed class LessonList
{
    /// <summary>The path of the file under the content folder (D-1026).</summary>
    public const string Path = "rules/lessons.json";

    /// <summary>The kind of a lesson id (D-646, D-1026).</summary>
    public const string Kind = "lesson";

    private LessonList(string file, IReadOnlyList<LessonRecord> records)
    {
        this.File = file;
        this.Records = records;
        List<ContentId> ids = [];
        foreach (LessonRecord record in records)
        {
            ids.Add(record.Id);
        }

        this.Ids = ids;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every lesson, in the order of the file.</summary>
    public IReadOnlyList<LessonRecord> Records { get; }

    /// <summary>Every lesson id, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Ids { get; }

    /// <summary>Reads the lesson file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, repeated, or out of its range, a lesson holds no form, the
    /// first form opens above zero, the point totals do not rise, or an id repeats (G-6, T-2).
    /// </exception>
    public static LessonList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<LessonRecord>? records = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "lessons":
                    records = ReadLessons(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new LessonList(file, reader.Require(records, depth, "lessons"));
        reader.ReadFileEnd();

        list.RefuseRepeatedId();
        list.RefuseSharedAbility();
        return list;
    }

    /// <summary>Tells whether the file holds a lesson id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when an entry of the file has this id.</returns>
    public bool Holds(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id) is not null;
    }

    /// <summary>Finds a lesson by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The file holds no such lesson (T-2).</exception>
    public LessonRecord Lesson(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id)
            ?? throw ContentException.ForField(this.File, id.Value, "the lesson file holds no lesson with this id (T-2, D-1026)");
    }

    /// <summary>Finds the form that gives an ability. No two forms name one ability (D-1032).</summary>
    /// <param name="ability">The id of the ability.</param>
    /// <returns>The lesson and the index of the form.</returns>
    /// <exception cref="ContentException">No form of the file gives the ability (T-2).</exception>
    public (LessonRecord Lesson, int Form) FormOf(ContentId ability)
    {
        ArgumentNullException.ThrowIfNull(ability);

        foreach (LessonRecord record in this.Records)
        {
            for (int index = 0; index < record.Forms.Count; index += 1)
            {
                if (string.CompareOrdinal(record.Forms[index].Ability.Value, ability.Value) == 0)
                {
                    return (record, index);
                }
            }
        }

        throw ContentException.ForField(this.File, ability.Value, "no form of the lesson file gives this ability (T-2, D-1026)");
    }

    private LessonRecord? Find(ContentId id)
    {
        foreach (LessonRecord record in this.Records)
        {
            if (string.CompareOrdinal(record.Id.Value, id.Value) == 0)
            {
                return record;
            }
        }

        return null;
    }

    private static List<LessonRecord> ReadLessons(ref ContentReader reader)
    {
        List<LessonRecord> records = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, records.Count))
        {
            records.Add(ReadLesson(ref reader));
        }

        return records;
    }

    private static LessonRecord ReadLesson(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        List<LessonForm>? forms = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "kind":
                    kind = reader.ReadString();
                    break;
                case "forms":
                    forms = ReadForms(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string readKind = reader.Require(kind, depth, "kind");
        if (!Aptitudes.TryOf(readKind, out AptitudeKind aptitude))
        {
            throw reader.RefuseField(depth, "kind", $"the kind '{readKind}' of '{readId.Value}' is not one of {Aptitudes.EveryName} (D-281)");
        }

        List<LessonForm> readForms = reader.Require(forms, depth, "forms");
        if (readForms.Count == 0)
        {
            throw reader.RefuseField(depth, "forms", $"the lesson '{readId.Value}' holds no form, and a lesson gives at least one ability (D-539)");
        }

        if (readForms[0].Points != 0)
        {
            throw reader.RefuseField(depth, "forms", $"the first form of '{readId.Value}' opens at {readForms[0].Points} points, and a lesson starts at its first form (D-361, D-1026)");
        }

        for (int index = 1; index < readForms.Count; index += 1)
        {
            if (readForms[index].Points <= readForms[index - 1].Points)
            {
                throw reader.RefuseField(
                    depth,
                    "forms",
                    $"form {index} of '{readId.Value}' opens at {readForms[index].Points} points, and each form opens above the form before it ({readForms[index - 1].Points}) (D-539)");
            }
        }

        return new LessonRecord(readId, aptitude, readForms);
    }

    private static List<LessonForm> ReadForms(ref ContentReader reader)
    {
        List<LessonForm> forms = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, forms.Count))
        {
            forms.Add(ReadForm(ref reader));
        }

        return forms;
    }

    private static LessonForm ReadForm(ref ContentReader reader)
    {
        ContentId? ability = null;
        int? points = null;
        int? mp = null;
        ContentId? description = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "ability":
                    ability = reader.ReadContentId(AbilityList.Kind);
                    break;
                case "points":
                    points = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "mp":
                    mp = ReadMp(ref reader);
                    break;
                case "description":
                    description = reader.ReadContentId();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new LessonForm(
            reader.Require(ability, depth, "ability"),
            reader.RequireInt(points, depth, "points"),
            reader.RequireInt(mp, depth, "mp"),
            reader.Require(description, depth, "description"));
    }

    /// <summary>Reads an MP cost: zero for a drill, and at most the most MP of a level (D-42, D-981).</summary>
    private static int ReadMp(ref ContentReader reader)
    {
        int mp = reader.ReadInt();
        if (mp < 0 || mp > StatCurve.MostPool)
        {
            throw reader.Refuse($"the MP cost {mp} is outside 0 to {StatCurve.MostPool} (D-42, D-981)");
        }

        return mp;
    }

    /// <summary>Refuses two forms that name one ability, because each spell has a flash of its own (D-1032).</summary>
    private void RefuseSharedAbility()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (LessonRecord record in this.Records)
        {
            foreach (LessonForm form in record.Forms)
            {
                if (!seen.Add(form.Ability.Value))
                {
                    throw ContentException.ForField(
                        this.File,
                        record.Id.Value,
                        $"a form names the ability '{form.Ability.Value}', which an earlier form names, and each spell has a flash of its own (D-1032)");
                }
            }
        }
    }

    private void RefuseRepeatedId()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (LessonRecord record in this.Records)
        {
            if (!seen.Add(record.Id.Value))
            {
                throw ContentException.ForField(this.File, record.Id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }
    }
}
