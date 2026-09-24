using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Story;

/// <summary>The kind of one node of a condition (D-1001, D-1002).</summary>
public enum ConditionKind
{
    /// <summary>A leaf that is always true, for a thing that no flag gates (D-1002).</summary>
    Always,

    /// <summary>A leaf that is true when its flag is on (D-542).</summary>
    Flag,

    /// <summary>A node that is true when each child is true.</summary>
    All,

    /// <summary>A node that is true when one child or more is true.</summary>
    Any,

    /// <summary>A node that is true when its one child is false.</summary>
    Not,
}

/// <summary>
/// One condition of content: a tree of all, any, and not over flag leaves, with an always
/// leaf (D-1001, D-1002). Every reader of a condition uses this one form (D-543).
/// </summary>
/// <remarks>
/// The readers are a story scene trigger of PR-68, and later a route, a hub line, a quest, an
/// enemy group, and the step condition of PR-18 (D-543, D-1007). Each reader holds a required
/// condition field, so a forgotten gate fails the load and never passes in silence (D-1002, T-2).
/// <para>
/// In a file, each node is an object with one field: `{ "always": true }`, `{ "flag":
/// "flag.x" }`, `{ "all": [...] }`, `{ "any": [...] }`, or `{ "not": {...} }`. An all node and
/// an any node hold one child or more, because an empty list reads like a mistake (D-1002).
/// </para>
/// </remarks>
public sealed class Condition
{
    /// <summary>The deepest tree that a file can hold. The limit bounds the recursion of the reader (T-2).</summary>
    public const int MostDepth = 8;

    private static readonly Condition AlwaysLeaf = new(ConditionKind.Always, null, []);

    private readonly Condition[] children;

    private Condition(ConditionKind kind, ContentId? flag, Condition[] children)
    {
        this.Kind = kind;
        this.Flag = flag;
        this.children = children;
    }

    /// <summary>The kind of this node.</summary>
    public ConditionKind Kind { get; }

    /// <summary>The flag of a flag leaf, or no value for every other kind.</summary>
    public ContentId? Flag { get; }

    /// <summary>The children of an all node, an any node, or a not node, in the order of the file.</summary>
    public IReadOnlyList<Condition> Children => this.children;

    /// <summary>Gives the always leaf (D-1002).</summary>
    /// <returns>The leaf.</returns>
    public static Condition Always() => AlwaysLeaf;

    /// <summary>Gives a flag leaf.</summary>
    /// <param name="flag">The flag.</param>
    /// <returns>The leaf.</returns>
    /// <exception cref="ArgumentNullException">The flag is null (T-2).</exception>
    public static Condition OfFlag(ContentId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return new Condition(ConditionKind.Flag, flag, []);
    }

    /// <summary>Reads one condition, and every node under it (D-1001).</summary>
    /// <param name="reader">The reader, at the value of the condition field.</param>
    /// <returns>The condition.</returns>
    /// <exception cref="ContentException">
    /// A node holds no field or two fields, an unknown field, an always leaf of false, an empty
    /// list, or a tree deeper than <see cref="MostDepth"/> (G-6, T-2).
    /// </exception>
    public static Condition Read(ref ContentReader reader) => ReadNode(ref reader, 1);

    /// <summary>Tells whether the condition holds for one set of flags.</summary>
    /// <param name="flags">The flags that are on.</param>
    /// <returns>True when the condition holds.</returns>
    /// <exception cref="ArgumentNullException">The set is null (T-2).</exception>
    public bool Holds(FlagSet flags)
    {
        ArgumentNullException.ThrowIfNull(flags);

        switch (this.Kind)
        {
            case ConditionKind.Always:
                return true;
            case ConditionKind.Flag:
                return flags.IsOn(this.Flag!);
            case ConditionKind.All:
                foreach (Condition child in this.children)
                {
                    if (!child.Holds(flags))
                    {
                        return false;
                    }
                }

                return true;
            case ConditionKind.Any:
                foreach (Condition child in this.children)
                {
                    if (child.Holds(flags))
                    {
                        return true;
                    }
                }

                return false;
            case ConditionKind.Not:
                return !this.children[0].Holds(flags);
            default:
                throw new InvalidOperationException($"The condition holds the kind '{this.Kind}', which no rule reads (T-2).");
        }
    }

    /// <summary>Fails when a flag leaf names an id that the flag file does not declare (D-543).</summary>
    /// <param name="flags">The flag file of this build.</param>
    /// <param name="file">The file of the reader, for the error.</param>
    /// <param name="field">The field of the condition, for the error.</param>
    /// <exception cref="ContentException">A leaf names an undeclared id (T-2).</exception>
    public void RequireDeclared(FlagList flags, string file, string field)
    {
        ArgumentNullException.ThrowIfNull(flags);

        if (this.Flag is ContentId flag)
        {
            flags.RequireDeclared(flag, file, field);
        }

        foreach (Condition child in this.children)
        {
            child.RequireDeclared(flags, file, field);
        }
    }

    private static Condition ReadNode(ref ContentReader reader, int level)
    {
        if (level > MostDepth)
        {
            throw reader.Refuse($"the condition is deeper than {MostDepth} levels, and a deeper tree reads poorly (D-1001)");
        }

        Condition? node = null;
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (node is not null)
            {
                throw reader.Refuse($"the condition node holds a second field '{field}', and a node holds one field (D-1001)");
            }

            node = field switch
            {
                "always" => ReadAlways(ref reader),
                "flag" => OfFlag(reader.ReadContentId(FlagList.Kind)),
                "all" => new Condition(ConditionKind.All, null, ReadChildren(ref reader, level)),
                "any" => new Condition(ConditionKind.Any, null, ReadChildren(ref reader, level)),
                "not" => new Condition(ConditionKind.Not, null, [ReadNode(ref reader, level + 1)]),
                _ => throw reader.UnknownField(field),
            };
        }

        return node ?? throw reader.Refuse("the condition node holds no field, and a node holds one of always, flag, all, any, and not (D-1001)");
    }

    private static Condition ReadAlways(ref ContentReader reader)
    {
        if (!reader.ReadBoolean())
        {
            throw reader.Refuse("the always leaf holds false, and it holds true alone. A thing that never happens takes no trigger (D-1002)");
        }

        return AlwaysLeaf;
    }

    private static Condition[] ReadChildren(ref ContentReader reader, int level)
    {
        List<Condition> children = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, children.Count))
        {
            children.Add(ReadNode(ref reader, level + 1));
        }

        if (children.Count == 0)
        {
            throw reader.Refuse("the list of the condition holds no child, and an empty list reads like a mistake. Write the always leaf (D-1002)");
        }

        return [.. children];
    }
}
