using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Notices;

/// <summary>One notice that a rule posted, with the values that its line reads (D-221, D-1224).</summary>
/// <param name="Id">The id of the notice, which is the string id of its line (G-7).</param>
/// <param name="Logs">True when the notice also lands in the notice log of the menu (D-983).</param>
/// <param name="Thing">
/// The item, the gear, or the lesson that the line names in the singular, or no value. Game puts
/// the singular string of the thing in the place `{thing}` of the line (D-1224).
/// </param>
/// <param name="Count">The count that the line names in the place `{count}`, such as the gold of a chest, or no value (D-1224).</param>
public sealed record PostedNotice(ContentId Id, bool Logs, ContentId? Thing, int? Count)
{
    /// <summary>The source of the ids of this type, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Notices/PostedNotice.cs";

    /// <summary>
    /// Gives the string id of the singular of a thing, such as `single.fixture_draught` for "a
    /// draught". A notice line reads it in the place `{thing}` (G-7, D-1224).
    /// </summary>
    /// <param name="thing">The id of the item, the gear, or the lesson.</param>
    /// <returns>The string id, `single.` and the name part of the id.</returns>
    /// <exception cref="System.ArgumentNullException">The id is null (T-2).</exception>
    public static ContentId SingleIdOf(ContentId thing)
    {
        System.ArgumentNullException.ThrowIfNull(thing);

        return ContentId.Parse($"single.{thing.Name}", Source, thing.Value);
    }
}
