using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Maps;

/// <summary>A count of one thing that stays in an opened chest (D-385).</summary>
/// <param name="Thing">The id of the item or the piece of gear.</param>
/// <param name="Count">The count that stays, at least 1.</param>
public sealed record ChestLeft(ContentId Thing, int Count);

/// <summary>What stays in one opened chest (D-385, D-1220).</summary>
/// <param name="Chest">The id of the chest.</param>
/// <param name="Left">What stays, in the order of the chest entries. An empty list is an emptied chest.</param>
public sealed record ChestValues(ContentId Chest, IReadOnlyList<ChestLeft> Left);

/// <summary>The memory of one map in a snapshot (D-385, D-555).</summary>
/// <param name="Map">The id of the map.</param>
/// <param name="Dead">The id of each killed enemy, in ordinal order (G-4).</param>
/// <param name="Doors">The id of each open door, in ordinal order.</param>
/// <param name="Chests">Each opened chest, in the ordinal order of its id.</param>
/// <param name="Spent">The id of each trap that fired or that the lead disarmed, in ordinal order (D-1229).</param>
public sealed record PlaceValues(ContentId Map, IReadOnlyList<ContentId> Dead, IReadOnlyList<ContentId> Doors, IReadOnlyList<ChestValues> Chests, IReadOnlyList<ContentId> Spent);

/// <summary>
/// The memory of one map, which lasts past the exit: each killed enemy, each open door, what
/// stays in each opened chest, and each spent trap (D-385, D-555, D-1229).
/// </summary>
/// <remarks>
/// Nothing resets until a story event reopens the place. A reopen brings back the killed enemies
/// and arms each spent trap again (D-555, D-1229). An open door and an opened chest stay as they are. Each list sits in
/// the ordinal order of its ids, so the snapshot and the hash never depend on the order of the
/// events (G-4).
/// <para>
/// A reopen of the map that the party stands on changes the memory alone: each killed enemy comes
/// back at the next entry, and no enemy rises in front of the party.
/// </para>
/// </remarks>
public sealed class PlaceState
{
    private readonly SortedDictionary<string, ContentId> dead = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ContentId> doors = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ChestValues> chests = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ContentId> spent = new(StringComparer.Ordinal);

    /// <summary>Makes the empty memory of one map.</summary>
    /// <param name="map">The id of the map.</param>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public PlaceState(ContentId map)
    {
        ArgumentNullException.ThrowIfNull(map);

        this.Map = map;
    }

    /// <summary>The id of the map.</summary>
    public ContentId Map { get; }

    /// <summary>True when the memory holds no dead enemy, no open door, no opened chest, and no spent trap.</summary>
    public bool IsEmpty => this.dead.Count == 0 && this.doors.Count == 0 && this.chests.Count == 0 && this.spent.Count == 0;

    /// <summary>Tells whether one enemy of the map is dead (D-555).</summary>
    /// <param name="enemy">The id of the enemy.</param>
    /// <returns>True when the party killed it and no story event reopened the place since.</returns>
    public bool IsDead(ContentId enemy) => this.dead.ContainsKey(enemy.Value);

    /// <summary>Tells whether one door of the map is open (D-41).</summary>
    /// <param name="door">The id of the door.</param>
    /// <returns>True when the party opened it.</returns>
    public bool IsOpen(ContentId door) => this.doors.ContainsKey(door.Value);

    /// <summary>Tells whether one trap of the map is spent (D-1229).</summary>
    /// <param name="trap">The id of the trap.</param>
    /// <returns>True when the trap fired or the lead disarmed it, and no story event reopened the place since.</returns>
    public bool IsSpent(ContentId trap) => this.spent.ContainsKey(trap.Value);

    /// <summary>Gives what stays in one chest, or no value when the party never opened it (D-385).</summary>
    /// <param name="chest">The id of the chest.</param>
    /// <returns>What stays, which an emptied chest holds as an empty list.</returns>
    public IReadOnlyList<ChestLeft>? LeftIn(ContentId chest) =>
        this.chests.TryGetValue(chest.Value, out ChestValues? values) ? values.Left : null;

    /// <summary>Notes one killed enemy (D-555).</summary>
    /// <param name="enemy">The id of the enemy.</param>
    /// <exception cref="InvalidOperationException">The memory already holds the enemy as dead (T-2).</exception>
    public void MarkDead(ContentId enemy)
    {
        if (!this.dead.TryAdd(enemy.Value, enemy))
        {
            throw new InvalidOperationException($"The enemy '{enemy.Value}' of the map '{this.Map.Value}' died two times, and a dead enemy fights no battle (D-555, T-2).");
        }
    }

    /// <summary>Notes one open door (D-41).</summary>
    /// <param name="door">The id of the door.</param>
    /// <exception cref="InvalidOperationException">The door is already open (T-2).</exception>
    public void Open(ContentId door)
    {
        if (!this.doors.TryAdd(door.Value, door))
        {
            throw new InvalidOperationException($"The door '{door.Value}' of the map '{this.Map.Value}' opened two times, and an open door takes no confirm (D-41, T-2).");
        }
    }

    /// <summary>Notes one trap that fired or that the lead disarmed (D-1229).</summary>
    /// <param name="trap">The id of the trap.</param>
    /// <exception cref="InvalidOperationException">The trap is already spent (T-2).</exception>
    public void Spend(ContentId trap)
    {
        if (!this.spent.TryAdd(trap.Value, trap))
        {
            throw new InvalidOperationException($"The trap '{trap.Value}' of the map '{this.Map.Value}' was spent two times, and a spent trap never fires (D-1229, T-2).");
        }
    }

    /// <summary>Notes what stays in one chest after the party opened it (D-385).</summary>
    /// <param name="chest">The id of the chest.</param>
    /// <param name="left">What stays, in the order of the chest entries.</param>
    public void Keep(ContentId chest, IReadOnlyList<ChestLeft> left)
    {
        ArgumentNullException.ThrowIfNull(left);

        List<ChestLeft> kept = [];
        foreach (ChestLeft entry in left)
        {
            kept.Add(entry);
        }

        this.chests[chest.Value] = new ChestValues(chest, kept);
    }

    /// <summary>Brings back each killed enemy of the map at the next entry, and arms each spent trap again (D-555, D-1229).</summary>
    /// <returns>The count of enemies that come back.</returns>
    public int Reopen()
    {
        int count = this.dead.Count;
        this.dead.Clear();
        this.spent.Clear();
        return count;
    }

    /// <summary>Gives the values of this memory for a snapshot (D-166).</summary>
    /// <returns>The values, in ordinal order.</returns>
    public PlaceValues Values()
    {
        List<ContentId> dead = [];
        foreach (ContentId enemy in this.dead.Values)
        {
            dead.Add(enemy);
        }

        List<ContentId> doors = [];
        foreach (ContentId door in this.doors.Values)
        {
            doors.Add(door);
        }

        List<ChestValues> chests = [];
        foreach (ChestValues chest in this.chests.Values)
        {
            chests.Add(chest);
        }

        List<ContentId> spent = [];
        foreach (ContentId trap in this.spent.Values)
        {
            spent.Add(trap);
        }

        return new PlaceValues(this.Map, dead, doors, chests, spent);
    }

    /// <summary>Adds every value of this memory to the state hash, in the order of <see cref="Values"/> (G-5).</summary>
    /// <param name="hasher">The hasher.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddText(this.Map.Value);
        hasher.AddInt32(this.dead.Count);
        foreach (string enemy in this.dead.Keys)
        {
            hasher.AddText(enemy);
        }

        hasher.AddInt32(this.doors.Count);
        foreach (string door in this.doors.Keys)
        {
            hasher.AddText(door);
        }

        hasher.AddInt32(this.chests.Count);
        foreach (ChestValues chest in this.chests.Values)
        {
            hasher.AddText(chest.Chest.Value);
            hasher.AddInt32(chest.Left.Count);
            foreach (ChestLeft left in chest.Left)
            {
                hasher.AddText(left.Thing.Value);
                hasher.AddInt32(left.Count);
            }
        }

        hasher.AddInt32(this.spent.Count);
        foreach (string trap in this.spent.Keys)
        {
            hasher.AddText(trap);
        }
    }
}
