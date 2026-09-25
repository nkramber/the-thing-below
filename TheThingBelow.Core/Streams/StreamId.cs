namespace TheThingBelow.Core.Streams;

/// <summary>
/// The number of one random stream. Each subsystem of Core draws from its own stream, and
/// it never draws from the stream of another (G-4).
/// </summary>
/// <remarks>
/// A number never changes, and a new subsystem takes the next free number. The split of
/// D-643 reads the number alone, so a new stream never moves the numbers of an existing
/// stream. The first four subsystems below are the rule areas of Core. PR-11 added the evaluator (D-947),
/// and PR-14 added the NPCs (D-1137).
/// </remarks>
public enum StreamId
{
    /// <summary>The map, the encounters, and the chests (`area-exploration.md`).</summary>
    Exploration = 1,

    /// <summary>Every roll of a fight (`area-battle.md`).</summary>
    Battle = 2,

    /// <summary>Growth, lessons, and rewards (`area-progression.md`).</summary>
    Progression = 3,

    /// <summary>The scenes and their variations (`area-story.md`).</summary>
    Story = 4,

    /// <summary>The tie of two equal scores of the evaluator (D-947). No other roll of a fight draws from it.</summary>
    Evaluator = 5,

    /// <summary>
    /// The walk of the NPCs of a map: the choice of each wander NPC on each pace tick (D-1137,
    /// D-1138). No other rule draws from it, so a hub with more NPCs moves no roll of a fight.
    /// </summary>
    Npc = 6,
}
