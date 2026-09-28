using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Tools.Worldgen;

/// <summary>The rows and the things that the generator writes into the map file of the overworld (D-1295).</summary>
/// <param name="Terrain">The terrain rows, one character for each tile.</param>
/// <param name="Zones">The rows of the zone grid, one key for each walkable tile and `.` for each blocked tile.</param>
/// <param name="Things">The tile of each thing of the settings, in the order of the settings.</param>
public sealed record GeneratedOverworld(IReadOnlyList<string> Terrain, IReadOnlyList<string> Zones, IReadOnlyList<(ContentId Id, TilePoint At)> Things);

/// <summary>
/// Makes the land of the overworld from its settings: basins of land with irregular coasts, rock
/// ridges that split the middle of each basin, rivers and lakes, a winding pass one tile wide
/// between two basins, a road that bends with the land and crosses water on a bridge, the gorge,
/// the cover, and each thing (D-1276, D-1293, D-1295, D-1300 to D-1302).
/// </summary>
/// <remarks>
/// Every value is an integer, and each random draw comes from a PCG stream of the seed, so each
/// CI leg makes the same map (D-502, D-1296). A value of the noise runs from 0 to
/// <see cref="One"/>. A pocket of land that a ridge or a river cuts off joins the land again
/// through a gap or a bridge, which makes the side routes of D-1301, and a pocket too small to
/// visit turns to rock. The generator checks the rules of the layout and fails with the seed and
/// the rule, so it never writes a map that breaks a gate (D-1281, T-2).
/// </remarks>
public sealed class OverworldGenerator
{
    /// <summary>The scale of a fraction: 1024 means one whole.</summary>
    public const int One = 1024;

    /// <summary>The fewest tiles of a pocket of land that the generator joins to the land, and not fills with rock.</summary>
    public const int SmallestPocket = 25;

    private const int Lattice = 64;
    private const long Unreached = long.MaxValue;
    private const long WaterCost = 1200;
    private const long RidgeCost = 2400;

    private static readonly TilePoint[] Four = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
    private static readonly TilePoint[] Eight = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1), new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)];
    private static readonly int[] ThreeSteps = [12, 6, 3];
    private static readonly OverworldRole[] Landmarks = [OverworldRole.BrokenWaystone, OverworldRole.DeadMineHead, OverworldRole.WarGraves, OverworldRole.BanditLookout];

    private readonly OverworldPlan plan;
    private readonly int width;
    private readonly int height;
    private readonly Pcg32 turns;
    private readonly SortedDictionary<int, int[]> lattices = [];
    private readonly string?[] basinOf;
    private readonly string?[] areaOf;
    private readonly bool[] walk;
    private readonly bool[] water;
    private readonly bool[] river;
    private readonly HashSet<int> road = [];
    private readonly HashSet<int> gorge = [];
    private readonly char[] tiles;
    private readonly SortedDictionary<string, TilePoint> placed = new(StringComparer.Ordinal);

    private OverworldGenerator(OverworldPlan plan)
    {
        this.plan = plan;
        this.width = plan.Width;
        this.height = plan.Height;
        this.turns = Pcg32.FromSeed(plan.Seed, 1);
        this.basinOf = new string?[this.width * this.height];
        this.areaOf = new string?[this.width * this.height];
        this.walk = new bool[this.width * this.height];
        this.water = new bool[this.width * this.height];
        this.river = new bool[this.width * this.height];
        this.tiles = new char[this.width * this.height];
    }

    /// <summary>Makes the rows and the things of the overworld of one set of settings.</summary>
    /// <param name="plan">The settings.</param>
    /// <returns>The rows and the things.</returns>
    /// <exception cref="InvalidOperationException">The land breaks a rule of the layout, and the message names the seed and the rule (T-2).</exception>
    public static GeneratedOverworld Generate(OverworldPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var build = new OverworldGenerator(plan);
        return build.Run();
    }

    private GeneratedOverworld Run()
    {
        this.LayBasins();
        List<TilePoint> lowPass = this.Canyon(this.plan.Passes[0]);
        List<TilePoint> highPass = this.Canyon(this.plan.Passes[1]);
        TilePoint iceFoot = this.Nearest(this.Near(OverworldRole.IceCrossing).X, 0, this.BasinTiles(this.plan.Passes[1].To));
        List<TilePoint> icePath = this.Line([iceFoot, new TilePoint(iceFoot.X, iceFoot.Y - 2)]);
        TilePoint southFoot = this.Nearest(this.plan.SouthX, this.height, this.BasinTiles(this.plan.Passes[0].From));
        List<TilePoint> southPath = this.Line([new TilePoint(southFoot.X, this.height - 1), southFoot]);
        foreach (List<TilePoint> path in new[] { lowPass, highPass, icePath, southPath })
        {
            foreach (TilePoint at in path)
            {
                this.walk[this.Index(at)] = true;
                this.road.Add(this.Index(at));
            }
        }

        this.LayRidges(lowPass, highPass, icePath, southPath);
        this.LayGorge();
        this.LayRivers();
        this.LayLakes();

        List<TilePoint> main = [.. this.Route(southPath[^1], lowPass[0]), .. lowPass];
        main.AddRange(this.Route(lowPass[^1], highPass[0]));
        main.AddRange(highPass);
        main.AddRange(this.Route(highPass[^1], icePath[0]));

        TilePoint town = this.Nearest(this.Near(OverworldRole.Town), this.OnLand(main, OverworldLand.Valley));
        TilePoint fort = this.Nearest(this.Near(OverworldRole.Fort), this.OnLand(main, OverworldLand.Pass));
        TilePoint spawn = this.Nearest(this.Near(OverworldRole.Spawn), this.OnLand(main, OverworldLand.Low));
        this.LayTerrain(lowPass);
        TilePoint refuge = this.Nearest(this.Near(OverworldRole.Refuge), this.AllBasinTiles());
        this.JoinPockets(spawn, refuge, lowPass);
        HashSet<int> fromTown = this.Reach(town, []);
        (TilePoint mineMouth, TilePoint mineGate, TilePoint mine) = this.Notch(this.Near(OverworldRole.Mine), -1, fromTown);
        (_, TilePoint door, TilePoint gallery) = this.Notch(this.Near(OverworldRole.Gallery), 1, fromTown);
        _ = this.Route(town, mineMouth);
        this.road.Add(this.Index(mineGate));
        this.tiles[this.Index(mineGate)] = TileKinds.RoadCharacter;
        this.OpenNotchToGorge(gallery);
        this.LayOutcrops(lowPass);
        TilePoint village = this.Nearest(this.Near(OverworldRole.Village), this.BesideRoad(OverworldLand.Low));

        this.Place(OverworldRole.IceCrossing, icePath[^1], lowPass);
        this.Place(OverworldRole.Fort, fort, lowPass);
        this.Place(OverworldRole.Town, town, lowPass);
        this.Place(OverworldRole.MineGate, mineGate, lowPass);
        this.Place(OverworldRole.Mine, mine, lowPass);
        this.Place(OverworldRole.SealedDoor, door, lowPass);
        this.Place(OverworldRole.Gallery, gallery, lowPass);
        this.Place(OverworldRole.Refuge, refuge, lowPass);
        this.Place(OverworldRole.Village, village, lowPass);
        this.Place(OverworldRole.Spawn, spawn, lowPass);
        this.Place(OverworldRole.RoadUp, this.Nearest(this.Near(OverworldRole.RoadUp), this.Cuts(highPass, town, fort)), lowPass);
        List<TilePoint> lowCuts = this.Cuts(lowPass, spawn, town);
        TilePoint townGate = this.Nearest(this.Near(OverworldRole.TownRoad), lowCuts);
        this.Place(OverworldRole.TownRoad, townGate, lowPass);

        // The gate of the way down lies past the town gate, so a lead from the south meets the town
        // gate first and a lead from the north meets the way down first (D-1281).
        HashSet<int> southOfTownGate = this.Reach(spawn, [townGate]);
        List<TilePoint> pastTownGate = lowCuts.FindAll(at => at != townGate && !southOfTownGate.Contains(this.Index(at)));
        this.Place(OverworldRole.VillageRoad, this.Nearest(this.Near(OverworldRole.VillageRoad), pastTownGate), lowPass);
        List<TilePoint> offRoad = this.OffRoad(spawn);
        foreach (OverworldRole landmark in Landmarks)
        {
            this.Place(landmark, this.Nearest(this.Near(landmark), offRoad), lowPass);
        }

        foreach (OverworldFix fix in this.plan.Fixes)
        {
            this.tiles[this.Index(fix.At)] = TileKinds.CharacterOf(fix.Tile);
        }

        this.Check(spawn, town, refuge);
        return new GeneratedOverworld(this.Rows(), this.ZoneRows(lowPass), this.ThingsInOrder());
    }

    // ---- noise --------------------------------------------------------------------------

    private int[] LatticeOf(int layer)
    {
        if (!this.lattices.TryGetValue(layer, out int[]? values))
        {
            Pcg32 stream = Pcg32.FromSeed(this.plan.Seed, (ulong)(100 + layer));
            values = new int[Lattice * Lattice];
            for (int index = 0; index < values.Length; index += 1)
            {
                values[index] = (int)(stream.Next() % One);
            }

            this.lattices[layer] = values;
        }

        return values;
    }

    /// <summary>Gives the smooth value noise of one layer at a point in 1024ths of a tile.</summary>
    private int Noise(long x, long y, int layer, int step)
    {
        int[] values = this.LatticeOf(layer);
        long gx = FloorDiv(x, step);
        long gy = FloorDiv(y, step);
        long i = FloorDiv(gx, One);
        long j = FloorDiv(gy, One);
        long fx = gx - (i * One);
        long fy = gy - (j * One);
        long sx = fx * fx * ((3 * One) - (2 * fx)) / (One * One);
        long sy = fy * fy * ((3 * One) - (2 * fy)) / (One * One);
        long a = (Value(values, i, j) * (One - sx)) + (Value(values, i + 1, j) * sx);
        long b = (Value(values, i, j + 1) * (One - sx)) + (Value(values, i + 1, j + 1) * sx);
        return (int)(((a * (One - sy)) + (b * sy)) / (One * One));
    }

    /// <summary>Gives the sum of the noise of each step of one layer, the finer step at half the weight, at a tile.</summary>
    private int Fbm(int x, int y, int layer, int[] steps)
    {
        long total = 0;
        long weights = 0;
        long weight = 1L << steps.Length;
        for (int index = 0; index < steps.Length; index += 1)
        {
            total += this.Noise((long)x * One, (long)y * One, (layer * 8) + index, steps[index]) * weight;
            weights += weight;
            weight /= 2;
        }

        return (int)(total / weights);
    }

    private static long Value(int[] values, long i, long j) => values[(int)(Mod(i, Lattice) * Lattice) + (int)Mod(j, Lattice)];

    private static long FloorDiv(long value, long divisor) => value >= 0 ? value / divisor : -((-value + divisor - 1) / divisor);

    private static long Mod(long value, long divisor) => ((value % divisor) + divisor) % divisor;

    // ---- basins and ridges --------------------------------------------------------------

    private void LayBasins()
    {
        for (int y = 4; y < this.height - 4; y += 1)
        {
            for (int x = 5; x < this.width - 5; x += 1)
            {
                for (int index = 0; index < this.plan.Basins.Count; index += 1)
                {
                    if (this.InBasin(this.plan.Basins[index], index + 1, x, y))
                    {
                        this.basinOf[this.Index(x, y)] = this.plan.Basins[index].Name;
                        break;
                    }
                }
            }
        }

        this.SmoothCoasts();
        this.SeparateBasins();
        this.KeepLargestPieces();
        for (int index = 0; index < this.walk.Length; index += 1)
        {
            this.walk[index] = this.basinOf[index] is not null;
            this.areaOf[index] = this.basinOf[index];
        }
    }

    /// <summary>Tells whether a tile lies inside a basin whose center and whose edge the noise moves.</summary>
    private bool InBasin(OverworldBasin basin, int layer, int x, int y)
    {
        long wx = ((long)x * One) + (14L * (this.Fbm(x, y, 40 + layer, [24, 12, 6]) - (One / 2)));
        long wy = ((long)y * One) + (10L * (this.Fbm(x, y, 50 + layer, [24, 12, 6]) - (One / 2)));
        long reach = 717 + (614L * this.Fbm(x, y, layer, [24, 12, 6]) / One);
        long dx = (wx - ((long)basin.X * One)) * basin.RadiusY;
        long dy = (wy - ((long)basin.Y * One)) * basin.RadiusX;
        long edge = reach * basin.RadiusX * basin.RadiusY;
        return (dx * dx) + (dy * dy) < edge * edge;
    }

    private void SmoothCoasts()
    {
        for (int pass = 0; pass < 2; pass += 1)
        {
            string?[] next = (string?[])this.basinOf.Clone();
            for (int y = 2; y < this.height - 2; y += 1)
            {
                for (int x = 2; x < this.width - 2; x += 1)
                {
                    string? mine = this.basinOf[this.Index(x, y)];
                    List<string> around = this.NamedAround(x, y);
                    if (mine is not null && Count(around, mine) <= 2)
                    {
                        next[this.Index(x, y)] = null;
                    }
                    else if (mine is null && around.Count >= 6 && Count(around, around[0]) == around.Count)
                    {
                        next[this.Index(x, y)] = around[0];
                    }
                }
            }

            Array.Copy(next, this.basinOf, next.Length);
        }
    }

    /// <summary>Keeps two tiles of rock between two basins, so a pass alone joins them.</summary>
    private void SeparateBasins()
    {
        string?[] before = (string?[])this.basinOf.Clone();
        for (int y = 0; y < this.height; y += 1)
        {
            for (int x = 0; x < this.width; x += 1)
            {
                string? mine = before[this.Index(x, y)];
                if (mine is null)
                {
                    continue;
                }

                for (int dy = -2; dy <= 2; dy += 1)
                {
                    for (int dx = -2; dx <= 2; dx += 1)
                    {
                        if (this.Inside(x + dx, y + dy) && before[this.Index(x + dx, y + dy)] is string other && string.CompareOrdinal(other, mine) != 0)
                        {
                            this.basinOf[this.Index(x, y)] = null;
                        }
                    }
                }
            }
        }
    }

    /// <summary>Keeps the largest piece of each basin, so no basin holds an island of its own land.</summary>
    private void KeepLargestPieces()
    {
        foreach (OverworldBasin basin in this.plan.Basins)
        {
            List<List<int>> pieces = [];
            var seen = new HashSet<int>();
            for (int index = 0; index < this.basinOf.Length; index += 1)
            {
                if (this.IsBasin(index, basin.Name) && seen.Add(index))
                {
                    pieces.Add(this.PieceFrom(index, seen, at => this.IsBasin(at, basin.Name)));
                }
            }

            pieces.Sort(static (first, second) => second.Count != first.Count ? second.Count.CompareTo(first.Count) : first[0].CompareTo(second[0]));
            for (int piece = 1; piece < pieces.Count; piece += 1)
            {
                foreach (int index in pieces[piece])
                {
                    this.basinOf[index] = null;
                }
            }
        }
    }

    /// <summary>
    /// Turns a thin band of the noise inside each basin into rock, so ridges and spurs split the
    /// middle of the land (D-1300). No ridge falls on a pass or on the road in and out.
    /// </summary>
    private void LayRidges(params List<TilePoint>[] kept)
    {
        var keep = new HashSet<int>();
        foreach (List<TilePoint> path in kept)
        {
            foreach (TilePoint at in path)
            {
                foreach (TilePoint step in Eight)
                {
                    if (this.Inside(at.X + step.X, at.Y + step.Y))
                    {
                        keep.Add(this.Index(at.X + step.X, at.Y + step.Y));
                    }
                }

                keep.Add(this.Index(at));
            }
        }

        for (int y = 0; y < this.height; y += 1)
        {
            for (int x = 0; x < this.width; x += 1)
            {
                int index = this.Index(x, y);
                if (this.basinOf[index] is null || keep.Contains(index))
                {
                    continue;
                }

                bool ridge = Math.Abs(this.Fbm(x, y, 70, [20, 10]) - (One / 2)) < this.plan.Ridge;
                bool rock = this.plan.Rocks > 0 && this.Fbm(x, y, 75, [14, 7]) > this.plan.Rocks;
                if (ridge || rock)
                {
                    this.walk[index] = false;
                }
            }
        }
    }

    private List<int> PieceFrom(int first, HashSet<int> seen, Func<int, bool> member)
    {
        List<int> piece = [first];
        var todo = new Stack<int>([first]);
        while (todo.Count > 0)
        {
            TilePoint at = this.PointOf(todo.Pop());
            foreach (TilePoint step in Four)
            {
                int x = at.X + step.X;
                int y = at.Y + step.Y;
                if (this.Inside(x, y) && member(this.Index(x, y)) && seen.Add(this.Index(x, y)))
                {
                    piece.Add(this.Index(x, y));
                    todo.Push(this.Index(x, y));
                }
            }
        }

        return piece;
    }

    // ---- passes, notches, and the gorge ---------------------------------------------------

    /// <summary>Makes a winding pass one tile wide from the top of one basin to the bottom of the next.</summary>
    private List<TilePoint> Canyon(OverworldPass pass)
    {
        TilePoint foot = this.Nearest(pass.X, 0, this.BasinTiles(pass.From));
        TilePoint top = this.Nearest(pass.X + pass.Zig, this.height, this.BasinTiles(pass.To));
        int middle = (foot.Y + top.Y) / 2;
        return this.Line(
        [
            foot,
            new TilePoint(foot.X, foot.Y - 1),
            new TilePoint(foot.X + pass.Zig, foot.Y - 1),
            new TilePoint(foot.X + pass.Zig, middle),
            new TilePoint(foot.X - (pass.Zig / 2), middle),
            new TilePoint(foot.X - (pass.Zig / 2), top.Y + 1),
            new TilePoint(top.X, top.Y + 1),
            top,
        ]);
    }

    /// <summary>
    /// Finds a walkable tile of the valley at its west edge or its east edge whose next three tiles
    /// outward are rock, with no walkable tile on either side, and opens the two tiles: the gate, and the mark past it.
    /// The mouth lies on land that the town reaches, so no repair of a pocket cuts it off.
    /// </summary>
    private (TilePoint Mouth, TilePoint Gate, TilePoint Mark) Notch(TilePoint hint, int side, HashSet<int> fromTown)
    {
        List<TilePoint> candidates = [];
        foreach (TilePoint at in this.BasinTiles(this.plan.Passes[0].To))
        {
            if (fromTown.Contains(this.Index(at)) && !this.road.Contains(this.Index(at)) && this.NotchFits(at, side))
            {
                candidates.Add(at);
            }
        }

        if (candidates.Count == 0)
        {
            throw this.Broken($"the valley holds no tile with room for a notch to the {(side < 0 ? "west" : "east")}");
        }

        TilePoint mouth = this.Nearest(hint, candidates);
        var gate = new TilePoint(mouth.X + side, mouth.Y);
        var mark = new TilePoint(mouth.X + (2 * side), mouth.Y);
        foreach (TilePoint at in new[] { gate, mark })
        {
            this.walk[this.Index(at)] = true;
            this.tiles[this.Index(at)] = TileKinds.SnowfieldCharacter;
            this.gorge.Remove(this.Index(at));
            this.areaOf[this.Index(at)] = this.plan.Passes[0].To;
        }

        return (mouth, gate, mark);
    }

    private bool NotchFits(TilePoint at, int side)
    {
        for (int step = 1; step <= 3; step += 1)
        {
            int x = at.X + (step * side);
            if (x < 2 || x >= this.width - 2 || this.Walkable(x, at.Y) || this.gorge.Contains(this.Index(x, at.Y)))
            {
                return false;
            }

            if (this.Walkable(x, at.Y - 1) || this.Walkable(x, at.Y + 1))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Turns the three tiles past the gallery into the gorge, so the notch opens onto the drop.</summary>
    private void OpenNotchToGorge(TilePoint gallery)
    {
        for (int x = gallery.X + 1; x <= gallery.X + 3 && x < this.width - 1; x += 1)
        {
            if (!this.walk[this.Index(x, gallery.Y)])
            {
                this.gorge.Add(this.Index(x, gallery.Y));
                this.tiles[this.Index(x, gallery.Y)] = TileKinds.GorgeCharacter;
            }
        }
    }

    private void LayGorge()
    {
        for (int y = this.plan.Gorge.Y; y <= this.plan.GorgeBottom; y += 1)
        {
            long center = ((long)this.plan.Gorge.X * One) + (20L * (this.Fbm(0, y * 2, 5, ThreeSteps) - (One / 2)));
            long half = (2L * One) + (4L * this.Fbm(y * 2, 0, 6, ThreeSteps));
            for (long x = FloorDiv(center - half, One); x <= FloorDiv(center + half, One); x += 1)
            {
                if (x > 0 && x < this.width - 1 && this.areaOf[this.Index((int)x, y)] is null)
                {
                    this.gorge.Add(this.Index((int)x, y));
                }
            }
        }
    }

    // ---- water --------------------------------------------------------------------------

    /// <summary>Lays each river as a winding line of water over the land of the basins (D-1301).</summary>
    private void LayRivers()
    {
        foreach (OverworldRiver line in this.plan.Rivers)
        {
            TilePoint from = this.Nearest(line.From, this.AreaTiles());
            TilePoint to = this.Nearest(line.To, this.AreaTiles());
            foreach (TilePoint at in this.RiverLine(from, to))
            {
                int index = this.Index(at);
                if (!this.road.Contains(index))
                {
                    this.walk[index] = false;
                    this.water[index] = true;
                    this.river[index] = true;
                }
            }
        }
    }

    /// <summary>
    /// Joins the two ends of a river through two points pushed to each side of the straight line,
    /// so the river swings across its valley, and bends each leg with the noise.
    /// </summary>
    private List<TilePoint> RiverLine(TilePoint from, TilePoint to)
    {
        long dx = to.X - from.X;
        long dy = to.Y - from.Y;
        List<TilePoint> area = this.AreaTiles();
        var first = new TilePoint((int)(from.X + (dx / 3) - (dy / 5)), (int)(from.Y + (dy / 3) + (dx / 5)));
        var second = new TilePoint((int)(from.X + (2 * dx / 3) + (dy / 5)), (int)(from.Y + (2 * dy / 3) - (dx / 5)));
        List<TilePoint> line = [];
        TilePoint last = from;
        foreach (TilePoint point in new[] { this.Nearest(first, area), this.Nearest(second, area), to })
        {
            line.AddRange(this.Meander(last, point));
            last = point;
        }

        return line;
    }

    /// <summary>Finds a winding line over the land of the basins, which the noise bends.</summary>
    private List<TilePoint> Meander(TilePoint start, TilePoint goal)
    {
        var best = new Dictionary<int, long> { [this.Index(start)] = 0 };
        var from = new Dictionary<int, int>();
        var queue = new PriorityQueue<TilePoint, (long Cost, int Order)>();
        queue.Enqueue(start, (0, this.Index(start)));
        while (queue.TryDequeue(out TilePoint at, out (long Cost, int Order) priority))
        {
            if (at == goal)
            {
                break;
            }

            if (priority.Cost > best[this.Index(at)])
            {
                continue;
            }

            foreach (TilePoint step in Four)
            {
                var next = new TilePoint(at.X + step.X, at.Y + step.Y);
                if (!this.Inside(next.X, next.Y) || this.areaOf[this.Index(next)] is null || this.road.Contains(this.Index(next)))
                {
                    continue;
                }

                long bend = this.Fbm(next.X, next.Y, 80, [10, 5]);
                long cost = priority.Cost + 30 + (3000L * bend * bend / (One * One));
                long known = best.TryGetValue(this.Index(next), out long found) ? found : Unreached;
                if (cost < known)
                {
                    best[this.Index(next)] = cost;
                    from[this.Index(next)] = this.Index(at);
                    queue.Enqueue(next, (cost, this.Index(next)));
                }
            }
        }

        return this.PathBack(start, goal, from, "the river");
    }

    private void LayLakes()
    {
        for (int index = 0; index < this.plan.Lakes.Count; index += 1)
        {
            OverworldBasin lake = this.plan.Lakes[index];
            for (int y = 0; y < this.height; y += 1)
            {
                for (int x = 0; x < this.width; x += 1)
                {
                    int at = this.Index(x, y);
                    if (this.areaOf[at] is null || this.road.Contains(at))
                    {
                        continue;
                    }

                    long reach = 768 + (461L * this.Fbm(x, y, 41 + index, [5, 3]) / One);
                    long dx = ((long)x - lake.X) * lake.RadiusY * One;
                    long dy = ((long)y - lake.Y) * lake.RadiusX * One;
                    long edge = reach * lake.RadiusX * lake.RadiusY;
                    if ((dx * dx) + (dy * dy) < edge * edge)
                    {
                        this.walk[at] = false;
                        this.water[at] = true;
                    }
                }
            }
        }
    }

    // ---- the road -----------------------------------------------------------------------

    /// <summary>
    /// Tells whether the road can take a tile: open land, a river, or the rock of a ridge inside a
    /// basin, which the road cuts through at a high cost. A lake and the gorge never take the road.
    /// </summary>
    private bool Routable(int x, int y)
    {
        if (!this.Inside(x, y))
        {
            return false;
        }

        int index = this.Index(x, y);
        bool ridge = this.areaOf[index] is not null && !this.water[index] && !this.gorge.Contains(index);
        return this.walk[index] || this.river[index] || ridge;
    }

    private long CostOf(TilePoint at)
    {
        if (this.road.Contains(this.Index(at)))
        {
            return 35;
        }

        int edge = 0;
        foreach (TilePoint step in Four)
        {
            edge += this.walk[this.Index(at.X + step.X, at.Y + step.Y)] ? 0 : 1;
        }

        long bend = this.Fbm(at.X, at.Y, 9, [7, 4]);
        long cost = 100 + (300L * edge) + (900L * bend * bend / (One * One));
        if (this.river[this.Index(at)])
        {
            return cost + WaterCost;
        }

        return this.walk[this.Index(at)] ? cost : cost + RidgeCost;
    }

    /// <summary>
    /// Finds the cheapest eight-way route over the open tiles and the rivers, and turns each
    /// diagonal step into two four-way steps, so the road climbs in a staircase and bends with the
    /// land. A road tile on a river becomes a bridge (D-1302).
    /// </summary>
    private List<TilePoint> Route(TilePoint start, TilePoint goal)
    {
        var best = new Dictionary<int, long> { [this.Index(start)] = 0 };
        var from = new Dictionary<int, int>();
        var queue = new PriorityQueue<TilePoint, (long Cost, int Order)>();
        queue.Enqueue(start, (0, this.Index(start)));
        while (queue.TryDequeue(out TilePoint at, out (long Cost, int Order) priority))
        {
            if (at == goal)
            {
                break;
            }

            if (priority.Cost > best[this.Index(at)])
            {
                continue;
            }

            foreach (TilePoint step in Eight)
            {
                var next = new TilePoint(at.X + step.X, at.Y + step.Y);
                bool diagonal = step.X != 0 && step.Y != 0;
                if (!this.Routable(next.X, next.Y) || (diagonal && !this.Routable(next.X, at.Y) && !this.Routable(at.X, next.Y)))
                {
                    continue;
                }

                long cost = priority.Cost + (diagonal ? this.CostOf(next) * 141 / 100 : this.CostOf(next));
                long known = best.TryGetValue(this.Index(next), out long found) ? found : Unreached;
                if (cost < known)
                {
                    best[this.Index(next)] = cost;
                    from[this.Index(next)] = this.Index(at);
                    queue.Enqueue(next, (cost, this.Index(next)));
                }
            }
        }

        return this.Staircase(this.PathBack(start, goal, from, "the road"));
    }

    private List<TilePoint> PathBack(TilePoint start, TilePoint goal, Dictionary<int, int> from, string what)
    {
        if (start != goal && !from.ContainsKey(this.Index(goal)))
        {
            throw this.Broken($"{what} finds no way from {start} to {goal}");
        }

        List<TilePoint> path = [goal];
        while (path[^1] != start)
        {
            path.Add(this.PointOf(from[this.Index(path[^1])]));
        }

        path.Reverse();
        return path;
    }

    private List<TilePoint> Staircase(List<TilePoint> path)
    {
        List<TilePoint> steps = [path[0]];
        for (int index = 1; index < path.Count; index += 1)
        {
            TilePoint last = steps[^1];
            TilePoint next = path[index];
            if (next.X != last.X && next.Y != last.Y)
            {
                var across = new TilePoint(next.X, last.Y);
                var down = new TilePoint(last.X, next.Y);
                bool acrossFirst = this.Routable(across.X, across.Y) && (!this.Routable(down.X, down.Y) || this.Fbm(next.X, next.Y, 61, ThreeSteps) < One / 2);
                steps.Add(acrossFirst ? across : down);
            }

            steps.Add(next);
        }

        foreach (TilePoint at in steps)
        {
            int index = this.Index(at);
            this.road.Add(index);
            this.walk[index] = true;
            if (this.tiles[index] != '\0')
            {
                this.tiles[index] = this.water[index] ? TileKinds.BridgeCharacter : TileKinds.RoadCharacter;
            }
        }

        return steps;
    }

    /// <summary>Makes a path of four-way steps through the corner points, with a random turn at each step.</summary>
    private List<TilePoint> Line(IReadOnlyList<TilePoint> corners)
    {
        List<TilePoint> path = [];
        for (int index = 0; index + 1 < corners.Count; index += 1)
        {
            TilePoint at = corners[index];
            TilePoint end = corners[index + 1];
            while (at != end)
            {
                if (path.Count == 0 || path[^1] != at)
                {
                    path.Add(at);
                }

                bool across = at.X != end.X && (at.Y == end.Y || this.turns.Next() % 2 == 0);
                at = across ? new TilePoint(at.X + Math.Sign(end.X - at.X), at.Y) : new TilePoint(at.X, at.Y + Math.Sign(end.Y - at.Y));
            }
        }

        path.Add(corners[^1]);
        return path;
    }

    // ---- terrain ------------------------------------------------------------------------

    private OverworldLand LandAt(int x, int y, List<TilePoint> lowPass)
    {
        if (this.areaOf[this.Index(x, y)] is string name)
        {
            return this.plan.BasinOf(name).Land;
        }

        OverworldBasin low = this.plan.BasinOf(this.plan.Passes[0].From);
        if (lowPass.Contains(new TilePoint(x, y)) || y > low.Y - low.RadiusY - 4)
        {
            return OverworldLand.Low;
        }

        return y > this.SnowLine() ? OverworldLand.Valley : OverworldLand.Pass;
    }

    private int SnowLine()
    {
        OverworldBasin pass = this.plan.BasinOf(this.plan.Passes[1].To);
        return pass.Y + pass.RadiusY + 3;
    }

    private char RockAt(int x, int y)
    {
        int snowLine = this.SnowLine();
        bool high = y < snowLine + (6 * (this.Fbm(x, y, 11, ThreeSteps) - (One / 2)) / One)
            || (y < snowLine + (13 * this.height / 64) && this.Fbm(x, y, 12, [5, 3]) > 737);
        return high ? TileKinds.SnowPeakCharacter : TileKinds.MountainCharacter;
    }

    private char CoverAt(int x, int y, List<TilePoint> lowPass)
    {
        int forest = this.Fbm(x, y, 21, [8, 4, 2]);
        int snow = this.Fbm(x, y, 33, [6, 3]);
        return this.LandAt(x, y, lowPass) switch
        {
            OverworldLand.Low => forest > 614 ? TileKinds.ForestCharacter : snow > 696 ? TileKinds.SnowfieldCharacter : TileKinds.GrassCharacter,
            OverworldLand.Valley => forest > 594 ? TileKinds.ForestCharacter : snow > 717 ? TileKinds.GrassCharacter : TileKinds.SnowfieldCharacter,
            _ => forest > 717 ? TileKinds.ForestCharacter : TileKinds.SnowfieldCharacter,
        };
    }

    private void LayTerrain(List<TilePoint> lowPass)
    {
        for (int y = 0; y < this.height; y += 1)
        {
            for (int x = 0; x < this.width; x += 1)
            {
                int index = this.Index(x, y);
                if (this.road.Contains(index))
                {
                    this.tiles[index] = this.water[index] ? TileKinds.BridgeCharacter : TileKinds.RoadCharacter;
                }
                else if (this.water[index])
                {
                    this.tiles[index] = TileKinds.WaterCharacter;
                }
                else if (!this.walk[index])
                {
                    this.tiles[index] = this.gorge.Contains(index) ? TileKinds.GorgeCharacter : this.RockAt(x, y);
                }
                else
                {
                    this.tiles[index] = this.CoverAt(x, y, lowPass);
                }
            }
        }
    }

    /// <summary>
    /// Joins each pocket of land that a ridge or the water cut off: a pocket of
    /// <see cref="SmallestPocket"/> tiles or more gets a gap through the rock or a bridge over the
    /// river, inside the land of its own basin, and a smaller pocket turns to rock (D-1301). The
    /// pockets of the refuge join the refuge, and never the land that the lead walks (D-1280).
    /// </summary>
    private void JoinPockets(TilePoint spawn, TilePoint refuge, List<TilePoint> lowPass)
    {
        string refugeArea = this.areaOf[this.Index(refuge)] ?? throw this.Broken($"the refuge at {refuge} lies in no basin");
        foreach (TilePoint root in new[] { spawn, refuge })
        {
            for (int round = 0; round < 200; round += 1)
            {
                HashSet<int> reached = this.Reach(root, []);
                List<int>? pocket = this.NextPocket(reached, root == refuge ? refugeArea : null, refugeArea);
                if (pocket is null)
                {
                    break;
                }

                if (pocket.Count < SmallestPocket)
                {
                    foreach (int index in pocket)
                    {
                        TilePoint at = this.PointOf(index);
                        this.walk[index] = false;
                        this.tiles[index] = this.RockAt(at.X, at.Y);
                    }

                    continue;
                }

                this.Bridge(pocket, reached, lowPass);
            }
        }
    }

    /// <summary>Gives the first pocket of walkable land, in reading order, that the lead does not reach from the root.</summary>
    private List<int>? NextPocket(HashSet<int> reached, string? onlyArea, string refugeArea)
    {
        var seen = new HashSet<int>();
        for (int index = 0; index < this.tiles.Length; index += 1)
        {
            TilePoint at = this.PointOf(index);
            if (!this.Walkable(at.X, at.Y) || reached.Contains(index) || !seen.Add(index))
            {
                continue;
            }

            string? area = this.areaOf[index];
            bool refugeSide = area is not null && string.CompareOrdinal(area, refugeArea) == 0;
            List<int> pocket = this.PieceFrom(index, seen, other => this.Walkable(this.PointOf(other).X, this.PointOf(other).Y));
            if (onlyArea is null ? !refugeSide : refugeSide)
            {
                return pocket;
            }
        }

        return null;
    }

    /// <summary>Opens the shortest way from a pocket to the reached land, through rock or over a river of the same basin.</summary>
    private void Bridge(List<int> pocket, HashSet<int> reached, List<TilePoint> lowPass)
    {
        string? area = this.areaOf[pocket[0]];
        var from = new Dictionary<int, int>();
        var todo = new Queue<int>(pocket);
        var seen = new HashSet<int>(pocket);
        int end = -1;
        while (todo.Count > 0 && end < 0)
        {
            TilePoint at = this.PointOf(todo.Dequeue());
            foreach (TilePoint step in Four)
            {
                int x = at.X + step.X;
                int y = at.Y + step.Y;
                if (!this.Inside(x, y) || x == 0 || y == 0 || x == this.width - 1 || y == this.height - 1)
                {
                    continue;
                }

                int next = this.Index(x, y);
                bool sameArea = this.areaOf[next] is string other && area is not null && string.CompareOrdinal(other, area) == 0;
                if (!sameArea || this.gorge.Contains(next) || !seen.Add(next))
                {
                    continue;
                }

                from[next] = this.Index(at);
                if (reached.Contains(next))
                {
                    end = next;
                    break;
                }

                todo.Enqueue(next);
            }
        }

        if (end < 0)
        {
            throw this.Broken($"the pocket of {pocket.Count} tiles at {this.PointOf(pocket[0])} finds no way to the land inside its basin");
        }

        // Each gap through the rock takes two tiles side by side, so it reads as a small valley and
        // not as a cut. A bridge stays one tile wide.
        for (int at = from[end]; from.ContainsKey(at); at = from[at])
        {
            TilePoint point = this.PointOf(at);
            this.Open(at, lowPass);
            TilePoint beside = from[at] == at + 1 || from[at] == at - 1 ? new TilePoint(point.X, point.Y + 1) : new TilePoint(point.X + 1, point.Y);
            int side = this.Index(beside);
            bool sameArea = this.areaOf[side] is string other && string.CompareOrdinal(other, area) == 0;
            if (!this.water[at] && sameArea && !this.water[side] && !this.gorge.Contains(side) && beside.X < this.width - 1 && beside.Y < this.height - 1)
            {
                this.Open(side, lowPass);
            }
        }
    }

    private void Open(int index, List<TilePoint> lowPass)
    {
        TilePoint point = this.PointOf(index);
        this.tiles[index] = this.water[index] ? TileKinds.BridgeCharacter : this.CoverAt(point.X, point.Y, lowPass);
        this.walk[index] = true;
    }

    /// <summary>Puts a lone rock on some open tiles whose eight neighbors are open ground, off the road.</summary>
    private void LayOutcrops(List<TilePoint> lowPass)
    {
        for (int y = 1; y < this.height - 1; y += 1)
        {
            for (int x = 1; x < this.width - 1; x += 1)
            {
                if (!IsCover(this.tiles[this.Index(x, y)]) || this.Fbm(x, y, 51, [4, 2]) <= 737 || this.turns.Next() % 10 >= 6)
                {
                    continue;
                }

                bool clear = true;
                foreach (TilePoint step in Eight)
                {
                    clear &= IsCover(this.tiles[this.Index(x + step.X, y + step.Y)]);
                }

                if (clear)
                {
                    this.tiles[this.Index(x, y)] = this.LandAt(x, y, lowPass) == OverworldLand.Pass ? TileKinds.SnowPeakCharacter : TileKinds.MountainCharacter;
                    this.walk[this.Index(x, y)] = false;
                }
            }
        }
    }

    private static bool IsCover(char tile) => tile == TileKinds.GrassCharacter || tile == TileKinds.SnowfieldCharacter || tile == TileKinds.ForestCharacter;

    // ---- things -------------------------------------------------------------------------

    private TilePoint Near(OverworldRole role) =>
        this.plan.Of(role).Near ?? throw new InvalidOperationException($"The role '{OverworldPlan.NameOf(role)}' holds no tile, and the generator reads one (D-1295).");

    /// <summary>Puts a thing on its tile, and turns the tile into open ground when the cover took it.</summary>
    private void Place(OverworldRole role, TilePoint at, List<TilePoint> lowPass)
    {
        int index = this.Index(at);
        char tile = this.tiles[index];
        if (tile != TileKinds.GrassCharacter && tile != TileKinds.SnowfieldCharacter && tile != TileKinds.RoadCharacter)
        {
            this.tiles[index] = this.LandAt(at.X, at.Y, lowPass) == OverworldLand.Low ? TileKinds.GrassCharacter : TileKinds.SnowfieldCharacter;
        }

        this.placed[this.plan.Of(role).Id.Value] = at;
    }

    /// <summary>Gives the tiles of a pass whose closure cuts one tile from another, in the order of the pass.</summary>
    private List<TilePoint> Cuts(List<TilePoint> pass, TilePoint from, TilePoint to)
    {
        List<TilePoint> cuts = [];
        foreach (TilePoint at in pass)
        {
            if (!this.Reach(from, [at]).Contains(this.Index(to)))
            {
                cuts.Add(at);
            }
        }

        if (cuts.Count == 0)
        {
            throw this.Broken($"no tile of the pass from {pass[0]} to {pass[^1]} cuts {from} from {to}");
        }

        return cuts;
    }

    private List<TilePoint> BesideRoad(OverworldLand land)
    {
        List<TilePoint> beside = [];
        foreach (TilePoint at in this.AllBasinTiles())
        {
            if (this.plan.BasinOf(this.areaOf[this.Index(at)]!).Land != land || this.road.Contains(this.Index(at)) || !IsCover(this.tiles[this.Index(at)]))
            {
                continue;
            }

            foreach (TilePoint step in Four)
            {
                if (this.road.Contains(this.Index(at.X + step.X, at.Y + step.Y)))
                {
                    beside.Add(at);
                    break;
                }
            }
        }

        return beside;
    }

    /// <summary>Gives each open tile that the lead reaches with no road within three tiles, for a landmark (D-1299).</summary>
    private List<TilePoint> OffRoad(TilePoint spawn)
    {
        HashSet<int> reached = this.Reach(spawn, []);
        List<TilePoint> off = [];
        foreach (int index in reached)
        {
            TilePoint at = this.PointOf(index);
            if (!IsCover(this.tiles[index]) || this.placed.ContainsValue(at))
            {
                continue;
            }

            bool far = true;
            for (int dy = -3; dy <= 3 && far; dy += 1)
            {
                for (int dx = -3; dx <= 3 && far; dx += 1)
                {
                    far = !(this.Inside(at.X + dx, at.Y + dy) && this.road.Contains(this.Index(at.X + dx, at.Y + dy)));
                }
            }

            if (far)
            {
                off.Add(at);
            }
        }

        return off;
    }

    private IReadOnlyList<(ContentId Id, TilePoint At)> ThingsInOrder()
    {
        List<(ContentId Id, TilePoint At)> things = [];
        foreach (OverworldPlacement placement in this.plan.Things)
        {
            things.Add((placement.Id, this.placed[placement.Id.Value]));
        }

        return things;
    }

    // ---- checks -------------------------------------------------------------------------

    /// <summary>Checks the rules of the layout, and fails with the seed and the rule (D-1276, D-1280, D-1281, T-2).</summary>
    private void Check(TilePoint spawn, TilePoint town, TilePoint refuge)
    {
        HashSet<int> every = this.Reach(spawn, []);
        foreach (OverworldPlacement placement in this.plan.Things)
        {
            TilePoint at = this.placed[placement.Id.Value];
            if (placement.Role != OverworldRole.Refuge && !every.Contains(this.Index(at)))
            {
                throw this.Broken($"the lead cannot reach '{placement.Id.Value}' at {at}");
            }
        }

        if (every.Contains(this.Index(refuge)))
        {
            throw this.Broken("a walkable tile reaches the refuge, which the gallery alone reaches (D-1280)");
        }

        HashSet<int> shut = this.Reach(spawn, this.GateTiles());
        foreach (OverworldRole role in new[] { OverworldRole.Town, OverworldRole.Mine, OverworldRole.Gallery, OverworldRole.Fort, OverworldRole.IceCrossing })
        {
            if (shut.Contains(this.Index(this.PlacedOf(role))))
            {
                throw this.Broken($"the lead reaches the {OverworldPlan.NameOf(role)} with each gate shut (D-1281)");
            }
        }

        TilePoint townGate = this.PlacedOf(OverworldRole.TownRoad);
        TilePoint villageGate = this.PlacedOf(OverworldRole.VillageRoad);
        if (townGate == villageGate || this.Reach(spawn, [townGate]).Contains(this.Index(villageGate)))
        {
            throw this.Broken("the gate of the way down does not lie north of the gate of the road to the town (D-1281)");
        }

        HashSet<int> fromTown = this.Reach(town, [.. this.GateTiles()]);
        foreach (OverworldRole role in new[] { OverworldRole.Fort, OverworldRole.Mine, OverworldRole.Gallery })
        {
            if (fromTown.Contains(this.Index(this.PlacedOf(role))))
            {
                throw this.Broken($"the lead reaches the {OverworldPlan.NameOf(role)} from the town past its gate (D-1281)");
            }
        }

        this.CheckEdges();
    }

    private void CheckEdges()
    {
        for (int x = 0; x < this.width; x += 1)
        {
            foreach (int y in new[] { 0, this.height - 1 })
            {
                if (this.Walkable(x, y) && !this.road.Contains(this.Index(x, y)))
                {
                    throw this.Broken($"the open tile ({x}, {y}) of the top or the bottom edge is not on the road");
                }
            }
        }

        for (int y = 0; y < this.height; y += 1)
        {
            foreach (int x in new[] { 0, this.width - 1 })
            {
                if (this.Walkable(x, y))
                {
                    throw this.Broken($"the tile ({x}, {y}) of a side edge is open");
                }
            }
        }
    }

    private List<TilePoint> GateTiles()
    {
        List<TilePoint> gates = [];
        foreach (OverworldRole role in new[] { OverworldRole.TownRoad, OverworldRole.VillageRoad, OverworldRole.RoadUp, OverworldRole.MineGate, OverworldRole.SealedDoor })
        {
            gates.Add(this.PlacedOf(role));
        }

        return gates;
    }

    private TilePoint PlacedOf(OverworldRole role) => this.placed[this.plan.Of(role).Id.Value];

    /// <summary>Gives each tile that the lead reaches from a start with four-way steps, past no shut tile.</summary>
    private HashSet<int> Reach(TilePoint start, IReadOnlyList<TilePoint> shut)
    {
        var closed = new HashSet<int>();
        foreach (TilePoint at in shut)
        {
            closed.Add(this.Index(at));
        }

        var seen = new HashSet<int> { this.Index(start) };
        var todo = new Queue<TilePoint>([start]);
        while (todo.Count > 0)
        {
            TilePoint at = todo.Dequeue();
            foreach (TilePoint step in Four)
            {
                int x = at.X + step.X;
                int y = at.Y + step.Y;
                if (this.Walkable(x, y) && !closed.Contains(this.Index(x, y)) && seen.Add(this.Index(x, y)))
                {
                    todo.Enqueue(new TilePoint(x, y));
                }
            }
        }

        return seen;
    }

    private bool Walkable(int x, int y)
    {
        if (!this.Inside(x, y))
        {
            return false;
        }

        char tile = this.tiles[this.Index(x, y)];
        return tile != '\0' ? TileKinds.TryOf(tile, out TileKind kind) && TileKinds.CanWalk(kind) : this.walk[this.Index(x, y)];
    }

    private InvalidOperationException Broken(string rule) =>
        new($"The overworld of the seed {this.plan.Seed} breaks a rule of its layout: {rule}. Change the settings of '{OverworldPlan.Path}' (D-1295, T-2).");

    // ---- zones and rows -----------------------------------------------------------------

    private IReadOnlyList<string> ZoneRows(List<TilePoint> lowPass)
    {
        var nearThing = new HashSet<int>();
        foreach (TilePoint at in this.placed.Values)
        {
            foreach (TilePoint step in Eight)
            {
                if (this.Inside(at.X + step.X, at.Y + step.Y))
                {
                    nearThing.Add(this.Index(at.X + step.X, at.Y + step.Y));
                }
            }

            nearThing.Add(this.Index(at));
        }

        List<string> rows = [];
        for (int y = 0; y < this.height; y += 1)
        {
            var row = new char[this.width];
            for (int x = 0; x < this.width; x += 1)
            {
                char tile = this.tiles[this.Index(x, y)];
                bool onRoad = tile == TileKinds.RoadCharacter || tile == TileKinds.BridgeCharacter;
                row[x] = !this.Walkable(x, y) ? EncounterZone.NoZone
                    : onRoad || nearThing.Contains(this.Index(x, y)) ? 'r'
                    : this.LandAt(x, y, lowPass) switch
                    {
                        OverworldLand.Pass => 'p',
                        OverworldLand.Valley => tile == TileKinds.ForestCharacter ? 'w' : 'v',
                        _ => tile == TileKinds.ForestCharacter ? 'f' : 'l',
                    };
            }

            rows.Add(new string(row));
        }

        return rows;
    }

    private IReadOnlyList<string> Rows()
    {
        List<string> rows = [];
        for (int y = 0; y < this.height; y += 1)
        {
            rows.Add(new string(this.tiles, y * this.width, this.width));
        }

        return rows;
    }

    // ---- small helpers ------------------------------------------------------------------

    private int Index(int x, int y) => (y * this.width) + x;

    private int Index(TilePoint at) => this.Index(at.X, at.Y);

    private TilePoint PointOf(int index) => new(index % this.width, index / this.width);

    private bool Inside(int x, int y) => x >= 0 && y >= 0 && x < this.width && y < this.height;

    private bool IsBasin(int index, string name) => this.basinOf[index] is string mine && string.CompareOrdinal(mine, name) == 0;

    private List<TilePoint> BasinTiles(string name)
    {
        List<TilePoint> tiles = [];
        for (int index = 0; index < this.basinOf.Length; index += 1)
        {
            if (this.IsBasin(index, name) && this.walk[index])
            {
                tiles.Add(this.PointOf(index));
            }
        }

        if (tiles.Count == 0)
        {
            throw this.Broken($"the basin '{name}' holds no tile");
        }

        return tiles;
    }

    private List<TilePoint> AreaTiles()
    {
        List<TilePoint> tiles = [];
        for (int index = 0; index < this.areaOf.Length; index += 1)
        {
            if (this.areaOf[index] is not null)
            {
                tiles.Add(this.PointOf(index));
            }
        }

        return tiles;
    }

    private List<TilePoint> AllBasinTiles()
    {
        List<TilePoint> tiles = [];
        for (int index = 0; index < this.areaOf.Length; index += 1)
        {
            if (this.areaOf[index] is not null && this.Walkable(this.PointOf(index).X, this.PointOf(index).Y) && !this.road.Contains(index))
            {
                tiles.Add(this.PointOf(index));
            }
        }

        return tiles;
    }

    private List<TilePoint> OnLand(List<TilePoint> path, OverworldLand land)
    {
        List<TilePoint> on = [];
        foreach (TilePoint at in path)
        {
            if (this.areaOf[this.Index(at)] is string name && !this.water[this.Index(at)] && this.plan.BasinOf(name).Land == land)
            {
                on.Add(at);
            }
        }

        return on;
    }

    private List<string> NamedAround(int x, int y)
    {
        List<string> around = [];
        foreach (TilePoint step in Eight)
        {
            if (this.basinOf[this.Index(x + step.X, y + step.Y)] is string name)
            {
                around.Add(name);
            }
        }

        return around;
    }

    private static int Count(List<string> names, string name)
    {
        int count = 0;
        foreach (string entry in names)
        {
            count += string.CompareOrdinal(entry, name) == 0 ? 1 : 0;
        }

        return count;
    }

    private TilePoint Nearest(int x, int y, List<TilePoint> tiles) => this.Nearest(new TilePoint(x, y), tiles);

    /// <summary>Gives the tile nearest a point, and the tile of the lower row, then of the lower column, on a tie.</summary>
    private TilePoint Nearest(TilePoint to, List<TilePoint> tiles)
    {
        if (tiles.Count == 0)
        {
            throw this.Broken($"no tile of its kind lies near {to}");
        }

        TilePoint best = tiles[0];
        long bestDistance = long.MaxValue;
        foreach (TilePoint at in tiles)
        {
            long dx = at.X - to.X;
            long dy = at.Y - to.Y;
            long distance = (dx * dx) + (dy * dy);
            if (distance < bestDistance || (distance == bestDistance && (at.Y < best.Y || (at.Y == best.Y && at.X < best.X))))
            {
                best = at;
                bestDistance = distance;
            }
        }

        return best;
    }
}
