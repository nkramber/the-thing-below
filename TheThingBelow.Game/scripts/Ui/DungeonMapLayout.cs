using System;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>What the dungeon map screen draws on one tile (D-567, D-993).</summary>
public enum DungeonMapMark
{
    /// <summary>A tile that the party never walked, which draws nothing (D-567).</summary>
    None,

    /// <summary>A walked tile.</summary>
    Floor,

    /// <summary>A walked tile that holds a door.</summary>
    Door,

    /// <summary>A walked tile that holds a save point.</summary>
    SavePoint,

    /// <summary>The tile of the lead of the party.</summary>
    Party,
}

/// <summary>
/// The place and the marks of the dungeon map screen: each walked tile at 16 frame pixels,
/// with the doors, the save points, and the party on it (D-567, D-982, D-993).
/// </summary>
/// <remarks>
/// The map sits in the middle of the frame of 1280 by 720, which holds 80 by 45 tiles. No
/// dungeon of the plan passes that size, so the screen builds no pan, and a larger map is an
/// error that names its size (D-982, T-2). PR-16 adds the exit and its mark (D-993). This type
/// holds no Godot value, so a test reads it with no engine (D-614).
/// </remarks>
public sealed class DungeonMapLayout
{
    /// <summary>The frame pixels of one tile on the screen (D-982).</summary>
    public const int TilePixels = 16;

    /// <summary>The most columns that the frame holds at <see cref="TilePixels"/> (D-982).</summary>
    public const int MostColumns = ScreenFit.FrameWidth / TilePixels;

    /// <summary>The most rows that the frame holds at <see cref="TilePixels"/> (D-982).</summary>
    public const int MostRows = ScreenFit.FrameHeight / TilePixels;

    private readonly MapState party;

    private DungeonMapLayout(MapState party)
    {
        this.party = party;
        this.Width = party.Map.Width * TilePixels;
        this.Height = party.Map.Height * TilePixels;
        this.Left = (ScreenFit.FrameWidth - this.Width) / 2;
        this.Top = (ScreenFit.FrameHeight - this.Height) / 2;
    }

    /// <summary>The left edge of the map in the frame, in frame pixels.</summary>
    public int Left { get; }

    /// <summary>The top edge of the map in the frame, in frame pixels.</summary>
    public int Top { get; }

    /// <summary>The width of the map, in frame pixels.</summary>
    public int Width { get; }

    /// <summary>The height of the map, in frame pixels.</summary>
    public int Height { get; }

    /// <summary>Lays out the map of the party in the middle of the frame (D-982).</summary>
    /// <param name="party">The party on its map, with the walked tiles.</param>
    /// <returns>The layout.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <exception cref="ArgumentException">The map is larger than 80 by 45 tiles, and the screen builds no pan (D-982, T-2).</exception>
    public static DungeonMapLayout Of(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        GameMap map = party.Map;
        if (map.Width > MostColumns || map.Height > MostRows)
        {
            throw new ArgumentException(
                $"The map '{map.Id.Value}' holds {map.Width} by {map.Height} tiles, and the dungeon map screen shows {MostColumns} by {MostRows} at most, with no pan (D-982).",
                nameof(party));
        }

        return new DungeonMapLayout(party);
    }

    /// <summary>Gives the mark of one tile of the map.</summary>
    /// <param name="at">The tile.</param>
    /// <returns>The mark. A tile that the party never walked draws nothing, the things on it included (D-567).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tile is outside the map (T-2).</exception>
    public DungeonMapMark MarkAt(TilePoint at)
    {
        if (!this.party.Map.Holds(at))
        {
            throw new ArgumentOutOfRangeException(nameof(at), at, $"The map '{this.party.Map.Id.Value}' holds no such tile (T-2).");
        }

        if (at == this.party.LeadAt)
        {
            return DungeonMapMark.Party;
        }

        if (!this.party.Walked.WasWalked(at))
        {
            return DungeonMapMark.None;
        }

        DungeonMapMark mark = DungeonMapMark.Floor;
        foreach (MapThing thing in this.party.Map.ThingsAt(at))
        {
            if (thing.Kind == MapThingKind.Door)
            {
                mark = DungeonMapMark.Door;
            }
            else if (thing.Kind == MapThingKind.SavePoint)
            {
                mark = DungeonMapMark.SavePoint;
            }
        }

        return mark;
    }
}
