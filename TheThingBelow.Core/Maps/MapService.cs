using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Maps;

/// <summary>What one service of a hub does (D-28, D-59, D-1131).</summary>
/// <remarks>PR-65 adds the shop (D-530).</remarks>
public enum ServiceKind
{
    /// <summary>The rest, which fills the health and the MP and cures the statuses that last past a fight (D-42, D-390, D-970).</summary>
    Rest,

    /// <summary>The save, which writes the slot save (D-1132).</summary>
    Save,
}

/// <summary>The names of the service kinds, as a map file writes them (D-1131).</summary>
public static class ServiceKinds
{
    /// <summary>Every kind, in one fixed order for a walk of them (G-4).</summary>
    public static readonly ServiceKind[] All = [ServiceKind.Rest, ServiceKind.Save];

    /// <summary>The names of every kind, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "rest, save";

    /// <summary>Gives the kind of one name.</summary>
    /// <param name="name">The name, such as `rest`.</param>
    /// <param name="kind">The kind of that name, when the name names one.</param>
    /// <returns>True when the name names a kind.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out ServiceKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (ServiceKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = ServiceKind.Rest;
        return false;
    }

    /// <summary>Gives the name of one kind, which a map file and an error use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `save`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(ServiceKind kind) => kind switch
    {
        ServiceKind.Rest => "rest",
        ServiceKind.Save => "save",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no service kind (D-1131)"),
    };
}

/// <summary>
/// One service of a hub: what it does, the NPC or the service point that holds it, and its
/// condition (D-543, D-1131, D-1142).
/// </summary>
/// <remarks>
/// Confirm while the lead faces the host opens the service, when its condition holds (D-1131).
/// A story flag can close a service through its condition (D-543, D-544). The load proves that
/// each host exists, that it holds one service, and that each service point holds one service,
/// so no service is out of reach and no host is empty (T-2).
/// </remarks>
/// <param name="Id">The permanent id of the service, of the kind `service` (D-166, D-646).</param>
/// <param name="Kind">What the service does.</param>
/// <param name="Npc">The NPC that holds the service, or no value when a thing holds it.</param>
/// <param name="Thing">The service point that holds the service, or no value when an NPC holds it (D-1142).</param>
/// <param name="Condition">The condition that opens the service, which the always leaf writes for a service that no flag gates (D-1002).</param>
public sealed record MapService(ContentId Id, ServiceKind Kind, ContentId? Npc, ContentId? Thing, Condition Condition)
{
    /// <summary>The kind of the id of a service (D-646).</summary>
    public const string IdKind = "service";

    /// <summary>The id of the NPC or the thing that holds this service (D-1131).</summary>
    public ContentId Host => this.Npc ?? this.Thing ?? throw new InvalidOperationException($"The service '{this.Id.Value}' has no host, and the reader refuses such a service (T-2).");

    /// <summary>Reads the `services` array of a map file (D-1131).</summary>
    /// <param name="reader">The reader of the map file, at the start of the array.</param>
    /// <returns>Each service, in the order of the file.</returns>
    /// <exception cref="ContentException">
    /// An entry breaks a rule of the reader: an unknown kind, which the error names with the file
    /// and the service, or a host that is not exactly one NPC or one thing (G-6, T-2).
    /// </exception>
    /// <remarks>
    /// The map checks each host against its own NPCs and things. The content set checks the
    /// flags of each condition (T-2).
    /// </remarks>
    public static List<MapService> ReadAll(ref ContentReader reader)
    {
        List<MapService> services = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, services.Count))
        {
            services.Add(Read(ref reader));
        }

        return services;
    }

    private static MapService Read(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        ContentId? npc = null;
        ContentId? thing = null;
        Condition? condition = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "kind":
                    kind = reader.ReadString();
                    break;
                case "npc":
                    npc = reader.ReadContentId(Maps.Npc.IdKind);
                    break;
                case "thing":
                    // The map proves that the thing is a service point, with a message that
                    // names the kind of the thing (D-1142, T-2).
                    thing = reader.ReadContentId();
                    break;
                case "condition":
                    condition = Condition.Read(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string name = reader.Require(kind, depth, "kind");
        if (!ServiceKinds.TryOf(name, out ServiceKind parsed))
        {
            throw reader.RefuseField(depth, "kind", $"the service '{readId.Value}' takes the kind '{name}', and a service takes one of {ServiceKinds.EveryName} (D-1131)");
        }

        if ((npc is null) == (thing is null))
        {
            throw reader.RefuseField(
                depth,
                npc is null ? "npc" : "thing",
                $"the service '{readId.Value}' holds {(npc is null ? "no field 'npc' and no field 'thing'" : "the fields 'npc' and 'thing'")}, and one NPC or one thing holds a service (D-1131, D-1142)");
        }

        return new MapService(readId, parsed, npc, thing, reader.Require(condition, depth, "condition"));
    }
}
