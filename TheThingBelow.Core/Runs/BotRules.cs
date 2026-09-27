using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The bot rules file: the start maps of the bot runs, the story flag that ends a bot run as
/// complete, and the tick budget of a run of each policy (D-1181, D-1184, D-1185, D-1259). The file
/// is `content/rules/bots.json`.
/// </summary>
/// <remarks>
/// No rule of the game reads the file. The headless runner of Tools reads it, and Core holds its
/// record, as it holds the record of every content file (D-517). The story content moves the goal
/// flag as the game grows, and PR-15 measures the budget (G-14).
/// </remarks>
public sealed class BotRules
{
    /// <summary>The path of the file under the content folder (D-1181).</summary>
    public const string Path = "rules/bots.json";

    /// <summary>The field of the budget of a greedy run (D-1259).</summary>
    public const string GreedyBudgetField = "greedy_tick_budget";

    /// <summary>The field of the budget of a random run (D-1259).</summary>
    public const string RandomBudgetField = "random_tick_budget";

    private BotRules(string file, IReadOnlyList<ContentId> startMaps, ContentId goalFlag, long greedyBudget, long randomBudget)
    {
        this.File = file;
        this.StartMaps = startMaps;
        this.GoalFlag = goalFlag;
        this.GreedyBudget = greedyBudget;
        this.RandomBudget = randomBudget;
    }

    /// <summary>The path of the file, for an error (T-2).</summary>
    public string File { get; }

    /// <summary>The maps where the bot runs start, in turn by seed: the seed modulo the count picks the map (D-1185).</summary>
    public IReadOnlyList<ContentId> StartMaps { get; }

    /// <summary>The story flag that ends a bot run as complete when it is on (D-1181).</summary>
    public ContentId GoalFlag { get; }

    /// <summary>
    /// The count of ticks after which a greedy run with no other end ends as budget: three times the
    /// longest greedy run to the goal (D-1184, D-1259).
    /// </summary>
    public long GreedyBudget { get; }

    /// <summary>
    /// The count of ticks after which a random run with no other end ends as budget (D-1259). A random
    /// run seldom reaches the goal, so its budget bounds the time of the bot job alone.
    /// </summary>
    public long RandomBudget { get; }

    /// <summary>Reads the bot rules file, and refuses each budget below one tick (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The rules.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or repeated, or the budget is below one tick (G-6, T-2).</exception>
    public static BotRules Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<ContentId>? starts = null;
        ContentId? goal = null;
        long? greedyBudget = null;
        long? randomBudget = null;
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "start_maps":
                    starts = ReadStartMaps(ref reader);
                    break;
                case "goal_flag":
                    goal = reader.ReadContentId(FlagList.Kind);
                    break;
                case GreedyBudgetField:
                    greedyBudget = reader.ReadLong();
                    break;
                case RandomBudgetField:
                    randomBudget = reader.ReadLong();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        long readGreedy = RequireBudget(ref reader, greedyBudget, depth, GreedyBudgetField);
        long readRandom = RequireBudget(ref reader, randomBudget, depth, RandomBudgetField);

        List<ContentId> readStarts = reader.Require(starts, depth, "start_maps");
        if (readStarts.Count == 0)
        {
            throw reader.RefuseField(depth, "start_maps", "the list is empty, and a bot run starts on one map at least (D-1185)");
        }

        var rules = new BotRules(file, readStarts, reader.Require(goal, depth, "goal_flag"), readGreedy, readRandom);
        reader.ReadFileEnd();
        return rules;
    }

    /// <summary>Gives the start map of the run of a seed (D-1185).</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>The id of the map.</returns>
    public ContentId StartOf(ulong seed) => this.StartMaps[(int)(seed % (ulong)this.StartMaps.Count)];

    /// <summary>
    /// Fails when a start map is not a map of the content (D-1185, T-2). The runner checks it
    /// before a play, because a content set of a test can hold no map.
    /// </summary>
    /// <param name="maps">The maps of this build.</param>
    /// <exception cref="ContentException">The content holds no map with a start id.</exception>
    public void RequireStartsOf(MapSet maps)
    {
        ArgumentNullException.ThrowIfNull(maps);

        for (int index = 0; index < this.StartMaps.Count; index += 1)
        {
            if (!maps.TryFind(this.StartMaps[index], out _))
            {
                throw ContentException.ForField(this.File, $"start_maps[{index}]", $"the map '{this.StartMaps[index].Value}' is no map of the content (D-1185)");
            }
        }
    }

    /// <summary>Fails when the flag file does not declare the goal flag (D-542, T-2).</summary>
    /// <param name="flags">The flag file of this build.</param>
    /// <exception cref="ContentException">The flag file holds no such id.</exception>
    public void RequireGoalOf(FlagList flags)
    {
        ArgumentNullException.ThrowIfNull(flags);

        flags.RequireDeclared(this.GoalFlag, this.File, "goal_flag");
    }

    /// <summary>Gives one budget, and refuses an absent budget or a budget below one tick (D-1184, T-2).</summary>
    private static long RequireBudget(ref ContentReader reader, long? budget, int depth, string field)
    {
        long read = reader.RequireValue(budget, depth, field);
        if (read < 1)
        {
            throw reader.RefuseField(depth, field, $"the budget is {read} ticks, and a bot run takes one tick at least (D-1184)");
        }

        return read;
    }

    private static List<ContentId> ReadStartMaps(ref ContentReader reader)
    {
        List<ContentId> maps = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, maps.Count))
        {
            maps.Add(reader.ReadContentId(GameMap.IdKind));
        }

        return maps;
    }
}
