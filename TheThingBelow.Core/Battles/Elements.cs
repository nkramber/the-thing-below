using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>The eight elements, in four opposed pairs (D-74).</summary>
public enum Element
{
    /// <summary>Fire.</summary>
    Fire,

    /// <summary>Ice.</summary>
    Ice,

    /// <summary>Lightning.</summary>
    Lightning,

    /// <summary>Earth.</summary>
    Earth,

    /// <summary>Wind.</summary>
    Wind,

    /// <summary>Water.</summary>
    Water,

    /// <summary>Holy. A name that people give to one way the thing below answers (D-158).</summary>
    Holy,

    /// <summary>Dark. A name that people give to one way the thing below answers (D-158).</summary>
    Dark,
}

/// <summary>How a combatant takes a hit of one element (D-43, D-794).</summary>
public enum Affinity
{
    /// <summary>The hit takes no rate.</summary>
    Normal,

    /// <summary>The hit takes the weak rate of the rules.</summary>
    Weak,

    /// <summary>The hit takes the resist rate of the rules.</summary>
    Resist,

    /// <summary>The hit deals no damage and heals the target (D-795).</summary>
    Absorb,
}

/// <summary>The names of the elements and the affinities in content (D-797).</summary>
public static class Elements
{
    /// <summary>Every element, in the order of D-74.</summary>
    public static readonly IReadOnlyList<Element> All =
    [
        Element.Fire,
        Element.Ice,
        Element.Lightning,
        Element.Earth,
        Element.Wind,
        Element.Water,
        Element.Holy,
        Element.Dark,
    ];

    /// <summary>Every affinity, in the order of the enum.</summary>
    public static readonly IReadOnlyList<Affinity> Affinities = [Affinity.Normal, Affinity.Weak, Affinity.Resist, Affinity.Absorb];

    /// <summary>The names of every element, for an error (T-2).</summary>
    public const string EveryName = "fire, ice, lightning, earth, wind, water, holy, dark";

    /// <summary>The names of every affinity, for an error (T-2).</summary>
    public const string EveryAffinityName = "normal, weak, resist, absorb";

    /// <summary>Gives the name of an element in content.</summary>
    /// <param name="element">The element.</param>
    /// <returns>The name, such as `fire`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no element (T-2).</exception>
    public static string NameOf(Element element) => element switch
    {
        Element.Fire => "fire",
        Element.Ice => "ice",
        Element.Lightning => "lightning",
        Element.Earth => "earth",
        Element.Wind => "wind",
        Element.Water => "water",
        Element.Holy => "holy",
        Element.Dark => "dark",
        _ => throw new ArgumentOutOfRangeException(nameof(element), element, "the value names no element (D-74)"),
    };

    /// <summary>Gives the name of an affinity in content.</summary>
    /// <param name="affinity">The affinity.</param>
    /// <returns>The name, such as `weak`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no affinity (T-2).</exception>
    public static string NameOf(Affinity affinity) => affinity switch
    {
        Affinity.Normal => "normal",
        Affinity.Weak => "weak",
        Affinity.Resist => "resist",
        Affinity.Absorb => "absorb",
        _ => throw new ArgumentOutOfRangeException(nameof(affinity), affinity, "the value names no affinity (D-794)"),
    };

    /// <summary>Finds the element of a name.</summary>
    /// <param name="name">The name, such as `fire`.</param>
    /// <param name="element">The element, when the name is one.</param>
    /// <returns>True when the name names an element.</returns>
    public static bool TryOf(string name, out Element element)
    {
        foreach (Element candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                element = candidate;
                return true;
            }
        }

        element = Element.Fire;
        return false;
    }

    /// <summary>Finds the affinity of a name.</summary>
    /// <param name="name">The name, such as `weak`.</param>
    /// <param name="affinity">The affinity, when the name is one.</param>
    /// <returns>True when the name names an affinity.</returns>
    public static bool TryAffinityOf(string name, out Affinity affinity)
    {
        foreach (Affinity candidate in Affinities)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                affinity = candidate;
                return true;
            }
        }

        affinity = Affinity.Normal;
        return false;
    }
}

/// <summary>
/// The affinity of one combatant to each of the eight elements (D-74, D-794). An enemy record
/// holds one, and PR-13 gives one to each piece of gear (D-790).
/// </summary>
public sealed class ElementTable
{
    private readonly Affinity[] affinities;

    private ElementTable(Affinity[] affinities)
    {
        this.affinities = affinities;
    }

    /// <summary>The table of a combatant that takes every element at the normal rate. A character holds it until PR-13 (D-790).</summary>
    public static ElementTable AllNormal { get; } = new(new Affinity[8]);

    /// <summary>Gives the affinity to one element.</summary>
    /// <param name="element">The element.</param>
    /// <returns>The affinity.</returns>
    public Affinity Of(Element element) => this.affinities[(int)element];

    /// <summary>Makes a table from one affinity for each element, in the order of D-74. The tests build tables with it.</summary>
    /// <param name="affinities">Eight affinities.</param>
    /// <returns>The table.</returns>
    /// <exception cref="ArgumentException">The list does not hold eight values (T-2).</exception>
    public static ElementTable Of(IReadOnlyList<Affinity> affinities)
    {
        ArgumentNullException.ThrowIfNull(affinities);
        if (affinities.Count != Elements.All.Count)
        {
            throw new ArgumentException($"An element table holds {Elements.All.Count} affinities, and the list holds {affinities.Count} (D-74).", nameof(affinities));
        }

        var copy = new Affinity[affinities.Count];
        for (int index = 0; index < copy.Length; index += 1)
        {
            copy[index] = affinities[index];
        }

        return new ElementTable(copy);
    }

    /// <summary>Reads the object `elements`: every element once, each with one affinity (D-794).</summary>
    /// <param name="reader">The reader of the file.</param>
    /// <returns>The table.</returns>
    /// <exception cref="ContentException">An element is absent, unknown, or repeated, or an affinity is unknown (T-2).</exception>
    public static ElementTable Read(ref ContentReader reader)
    {
        var affinities = new Affinity?[Elements.All.Count];

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (!Elements.TryOf(field, out Element element))
            {
                throw reader.Refuse($"the element '{field}' is not one of {Elements.EveryName} (D-74)");
            }

            string name = reader.ReadString();
            if (!Elements.TryAffinityOf(name, out Affinity affinity))
            {
                throw reader.Refuse($"the affinity '{name}' of '{field}' is not one of {Elements.EveryAffinityName} (D-794)");
            }

            affinities[(int)element] = affinity;
        }

        var table = new Affinity[affinities.Length];
        foreach (Element element in Elements.All)
        {
            table[(int)element] = reader.RequireValue(affinities[(int)element], depth, Elements.NameOf(element));
        }

        return new ElementTable(table);
    }
}
